using GPAS.Workspace.Entities.ImageProcessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GPAS.Workspace.Presentation.Controls.ImageProcessing
{
    /// <summary>
    /// Interaction logic for BoundingBoxControl.xaml
    /// </summary>
    public partial class BoundingBoxControl : UserControl
    {
        #region متغیرهای سراسری
        private KWBoundingBox box;
        public KWBoundingBox Box
        {
            get { return box; }
            set
            {
                box = value;

                GenerateRectangle();

                box.SelectionChanged += Box_SelectionChanged;

                if (Box.IsSelected)
                {
                    Box.Select();
                }
                else
                {
                    Box.DeSelect();
                }
            }
        }

        Size scaleSize = new Size(1, 1);

        public Size ScaleSize
        {
            get { return scaleSize; }
            set
            {
                scaleSize = value;
                GenerateRectangle();
                SetLocation();
            }
        }

        private void SetLocation()
        {
            double left = Box.TopLeft.X * ScaleSize.Width;
            double top = Box.TopLeft.Y * ScaleSize.Height;

            Margin = new Thickness(left, top, 0, 0);
        }

        string caption;

        public string Caption
        {
            get
            {
                return caption;
            }

            set
            {
                caption = value;
                lblCaption.ToolTip = value;
                textBlockCaption.Text = value;

                if (string.IsNullOrEmpty(caption) || string.IsNullOrWhiteSpace(caption))
                {
                    lblCaption.Visibility = Visibility.Hidden;
                }
                else
                {
                    lblCaption.Visibility = Visibility.Visible;
                }
            }
        }

        public Style ShapeSelectedStyle { get; set; }

        public Style ShapeDeselectedStyle { get; set; }


        #endregion

        public BoundingBoxControl()
        {
            InitializeComponent();
            Init();
        }

        #region توابع

        public void Init()
        {
            DataContext = this;
            ShapeDeselectedStyle = Resources["BoxStyle"] as Style;
            ShapeSelectedStyle = Resources["SelectedBoxStyle"] as Style;
        }

        private void GenerateRectangle()
        {
            rectangle.Width = Box.Width * scaleSize.Width;
            rectangle.Height = Box.Height * scaleSize.Height;
        }

        private void Box_SelectionChanged(object source, SelectEventArgs e)
        {
            if (e.Select)
            {
                rectangle.Style = ShapeSelectedStyle;
            }
            else
            {
                rectangle.Style = ShapeDeselectedStyle;
            }
        }
        #endregion
    }
}
