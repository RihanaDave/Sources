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

namespace GPAS.Graph.GraphViewer.Foundations
{
    /// <summary>
    /// Interaction logic for FlowShapeStatic.xaml
    /// </summary>
    public partial class FlowShapeStatic : UserControl
    {
        public FlowShapeStatic()
        {
            InitializeComponent();
            DataContext = this;
        }

        public EdgeDirection Direction
        {
            get { return (EdgeDirection)GetValue(DirectionProperty); }
            set { SetValue(DirectionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Direction.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DirectionProperty =
            DependencyProperty.Register("Direction", typeof(EdgeDirection), typeof(FlowShapeStatic), new PropertyMetadata(null));

        public double FlowSize
        {
            get { return (double)GetValue(FlowSizeProperty); }
            set { SetValue(FlowSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FlowSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FlowSizeProperty =
            DependencyProperty.Register("FlowSize", typeof(double), typeof(FlowShapeStatic), new PropertyMetadata(50.0));

        public Brush FlowColor
        {
            get { return (Brush)GetValue(FlowColorProperty); }
            set { SetValue(FlowColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FlowColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FlowColorProperty =
            DependencyProperty.Register("FlowColor", typeof(Brush), typeof(FlowShapeStatic), new PropertyMetadata(null));

        public bool CanShow
        {
            get { return (bool)GetValue(CanShowProperty); }
            set { SetValue(CanShowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanShow.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanShowProperty =
            DependencyProperty.Register("CanShow", typeof(bool), typeof(FlowShapeStatic), new PropertyMetadata(true));


        public void SetPosition(Point point, double angle)
        {
            Point cp = new Point(FlowSize / 2, FlowSize / 2);

            Canvas.SetLeft(this, point.X - cp.X);
            Canvas.SetTop(this, point.Y - cp.Y);

            this.RenderTransform = new RotateTransform(angle, cp.X, cp.Y);
        }

        public static double MiddleSize { get; } = 80;
        public static double MinSize { get; } = 40;
        public static double MaxSize { get; } = 120;
    }
}
