using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Emgu.CV
{
    ///<summary> 
    /// Class Histogram 
    ///</summary>
    ///<remarks><B>This is a Beta version</B></remarks>
    //TODO: Improve the histogram class
    public class Histogram : UnmanagedObject
    {
        private int _dimension;
        private int[] _binSize;

        /// <summary>
        /// Creates a uniform histogram of the specified size
        /// </summary>
        /// <param name="binSizes">The length of this array is the dimension of the histogram. The values of the array contains the number of bins in each dimension. The total number of bins eaquals the multiplication of all numbers in the array</param>
        /// <param name="min">the lower boundaries of the bins</param>
        /// <param name="max">the upper boundaries of the bins</param>
        public Histogram(int[] binSizes, float[] min, float[] max)
        {
            _binSize = binSizes;
            _dimension = binSizes.Length;

            Debug.Assert(
                min.Length == _dimension && max.Length == _dimension,
                "incompatible dimension");

            GCHandle[] r = new GCHandle[Dimension];

            float[][] ranges = new float[Dimension][];

            for (int i = 0; i < _dimension; i++)
            {
                ranges[i] = new float[2] { min[i], max[i] };
                r[i] = GCHandle.Alloc(ranges[i], GCHandleType.Pinned);
            }

            _ptr = CvInvoke.cvCreateHist(
                _dimension, 
                binSizes, 
                Emgu.CV.CvEnum.HIST_TYPE.CV_HIST_ARRAY, 
                Array.ConvertAll<GCHandle, IntPtr>( r, delegate(GCHandle h) { return h.AddrOfPinnedObject();}),
                true); 

            foreach (GCHandle h in r)
                h.Free();
        }

        ///<summary> 
        /// Clear this histogram
        ///</summary>
        public void Clear()
        {
            CvInvoke.cvClearHist(_ptr);
        }

        ///<summary> 
        /// Project the images to the histogram bins 
        ///</summary>
        ///<param name="imgs">image to project</param>
        public void Accumulate<D>(Image<Gray, D>[] imgs)
        {
            Accumulate<D>(imgs, null);
        }

        /// <summary>
        /// Project the images to the histogram bins 
        /// </summary>
        /// <typeparam name="D">The depth of the image</typeparam>
        /// <param name="imgs">image to project</param>
        /// <param name="mask">The operation mask, determines what pixels of the source images are counted</param>
        public void Accumulate<D>(Image<Gray, D>[] imgs, Image<Gray, Byte> mask)
        {
            Debug.Assert(imgs.Length == Dimension, "incompatible dimension");

            IntPtr[] imgPtrs =
                System.Array.ConvertAll<Image<Gray, D>, IntPtr>(
                    imgs,
                    delegate(Image<Gray, D> img) { return img.Ptr; });

            IntPtr maskPtr = mask == null ? IntPtr.Zero : mask.Ptr;

            CvInvoke.cvCalcHist(imgPtrs, _ptr, true, maskPtr);
        }

        /// <summary>
        /// Project the images to the histogram bins 
        /// </summary>
        /// <param name="imgs"></param>
        public void Accumulate(IImage[] imgs)
        {
            Debug.Assert(imgs.Length == Dimension, "incompatible dimension");
            IntPtr[] imgPtrs =
                System.Array.ConvertAll<IImage, IntPtr>(
                    imgs,
                    delegate(IImage img) { return img.Ptr; });
            CvInvoke.cvCalcHist(imgPtrs, _ptr, true, IntPtr.Zero);
        }

        ///<summary> 
        /// Back project the histogram into an gray scale image
        ///</summary>
        public Image<Gray, D> BackProject<D>(Image<Gray, D>[] srcs)
        {
            Debug.Assert(srcs.Length == _dimension, "incompatible dimension");

            IntPtr[] imgPtrs = 
                System.Array.ConvertAll<Image<Gray,D>, IntPtr>(
                    srcs, 
                    delegate(Image<Gray, D> img) { return img.Ptr; });

            Image<Gray, D> res = srcs[0].BlankClone();
            CvInvoke.cvCalcBackProject(imgPtrs, res.Ptr, _ptr);
            return res;
        }

        ///<summary>
        ///Clears histogram bins that are below the specified threshold.
        ///</summary>
        ///<param name="thresh">The threshold used to clear the bins</param>
        public void Threshold(double thresh)
        {
            CvInvoke.cvThreshHist(_ptr, thresh);
        }

        ///<summary> Retrieve item counts for the specific bin </summary>
        public double Query(params int[] binIndex)
        {
            Debug.Assert(binIndex.Length > 0, "Please specify at least one index");
            Debug.Assert(binIndex.Length == Dimension, "incompatible dimension");
          
            switch (binIndex.Length)
            {
                case 1:
                    return CvInvoke.cvQueryHistValue_1D(_ptr, binIndex[0]);
                case 2:
                    return CvInvoke.cvQueryHistValue_2D(_ptr, binIndex[0], binIndex[1]);
                case 3:
                    return CvInvoke.cvQueryHistValue_3D(_ptr, binIndex[0], binIndex[1], binIndex[2]);
                default:
                    throw new NotImplementedException(String.Format("Retrive from {0} dimensional histogram is not implemented", binIndex.Length));
            }
        }

        /// <summary>
        /// The number of dimensions for the histogram
        /// </summary>
        public int Dimension { get { return _dimension; } }

        public int[] BinSize { get { return _binSize; } }

        /// <summary>
        /// Release the histogram and all memory associate with it
        /// </summary>
        protected override void DisposeObject()
        {
            CvInvoke.cvReleaseHist(ref _ptr);
        }

        /// <summary>
        ///  normalizes the histogram bins by scaling them, such that the sum of the bins becomes equal to factor
        /// </summary>
        /// <param name="factor">the sum of the bins after normalization</param>
        public void Normalize(double factor)
        {
            CvInvoke.cvNormalizeHist(Ptr, factor);
        }

        /// <summary>
        /// Get the MCvHistogram structure from Ptr
        /// </summary>
        public MCvHistogram MCvHistogram
        {
            get
            {
                return (MCvHistogram)Marshal.PtrToStructure(Ptr, typeof(MCvHistogram));
            }
        }
    }
}
