using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Emgu.CV;
using Emgu.Reflection;

namespace Emgu.CV.UI
{
    public partial class ImageProperty : UserControl
    {
        private ImageBox _imageBox;

        /// <summary>
        /// The parent imagebox for this property panel
        /// </summary>
        public ImageBox ImageBox
        {
            get { return _imageBox; }
            set { _imageBox = value; }
        }

        /// <summary>
        /// Create a ImageProperty control
        /// </summary>
        public ImageProperty()
        {
            InitializeComponent();
            cSharpOperationStackView.Language = Emgu.Utils.ProgrammingLanguage.CSharp;
            cPlusPlusoperationStackView.Language = Emgu.Utils.ProgrammingLanguage.CPlusPlus;
        }

        /// <summary>
        /// Set the width of the image
        /// </summary>
        public int ImageWidth
        {
            set
            {
                widthTextbox.Text = value.ToString();
            }
        }

        /// <summary>
        /// Set the height of the image
        /// </summary>
        public int ImageHeight
        {
            set
            {
                heightTextBox.Text = value.ToString();
            }
        }

        /// <summary>
        /// Set the Type of the color
        /// </summary>
        public Type TypeOfColor
        {
            set
            {
                Object[] colorAttributes = value.GetCustomAttributes(typeof(ColorInfoAttribute), true);
                if (colorAttributes.Length > 0)
                {
                    ColorInfoAttribute info = (ColorInfoAttribute)colorAttributes[0];
                    typeOfColorTexbox.Text = info.ConversionCodeName;
                }
                else
                {
                    typeOfColorTexbox.Text = "Unknown";
                }
            }
        }

        /// <summary>
        /// Set the mouse position over the image
        /// </summary>
        public Point2D<int> MousePositionOnImage
        {
            set
            {
                mousePositionTextbox.Text = String.Format("[{0}, {1}]", value.X, value.Y);
            }
        }

        /// <summary>
        /// Set the color intensity of the pixel on the image where is mouse is at
        /// </summary>
        public ColorType ColorIntensity
        {
            set
            {
                colorIntensityTextbox.Text = String.Format("[{0}]",
                    value == null ? "" : String.Join(",", System.Array.ConvertAll<double, String>(value.Coordinate, System.Convert.ToString)));
            }
        }

        /// <summary>
        /// Set the Depth of the image
        /// </summary>
        public Type TypeOfDepth
        {
            set
            {
                typeOfDepthTextBox.Text = value.Name;
            }
        }

        /// <summary>
        /// Set the description of the operation stack
        /// </summary>
        public Stack<Operation<IImage>> OperationStack
        {
            set
            {
                cSharpOperationStackView.DisplayOperationStack(value);
                cPlusPlusoperationStackView.DisplayOperationStack(value);
            }
        }

        /// <summary>
        /// Set the frame rate
        /// </summary>
        public int FramesPerSecondText
        {
            set
            {
                fpsTextBox.Text = value.ToString();
            }
        }

        private void clearStackBtn_Click(object sender, EventArgs e)
        {
            _imageBox.ClearOperation();
        }

        private void popStackButton_Click(object sender, EventArgs e)
        {
            _imageBox.PopOperation();
        }

        private void showHistogramButton_Click(object sender, EventArgs e)
        {
            IImage image = _imageBox.DisplayedImage;
            if (image.TypeOfDepth == typeof(Byte))
            {
                IImage[] channels = image.Split();
                HistogramViewer hviewer = new HistogramViewer();

                int i = 0;
                foreach (IImage channel in channels)
                {
                    int[] binSize = new int[1] { 256 };
                    float[] min = new float[1] { 0.0f };
                    float[] max = new float[1] { 255.0f };

                    Histogram hist = new Histogram(binSize, min, max);
                    hist.Clear(); //this is required since the initial histogram might contains random values
                    hist.Accumulate(channels);

                    //all the values of the histogram for the specific color channel
                    Point2D<int>[] pts = new Point2D<int>[binSize[0]];

                    int sum = 0;
                    for (int j = 0; j < binSize[0]; j++)
                    {
                        //TODO: find out why some time hist.query return negative number (hence Math.Max(0, ...) is required) 
                        pts[j] = new Point2D<int>(j, (int)hist.Query(j) );
                        sum += pts[j].Y;
                    }
                    
                    hviewer.HistogramCtrl.AddHistogram("Channel " + i++, System.Drawing.Color.Black, pts);
                    
                }
                hviewer.Show();

            }
            else
            {
                //TODO: handle non-byte depth
            }
        }
    }
}
