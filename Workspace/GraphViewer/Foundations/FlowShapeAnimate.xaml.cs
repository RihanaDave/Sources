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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GPAS.Graph.GraphViewer.Foundations
{
    /// <summary>
    /// Interaction logic for FlowAnimateShape.xaml
    /// </summary>
    public partial class FlowShapeAnimate : UserControl
    {
        public int FlowSpeedInMiliSeconds
        {
            get { return (int)GetValue(FlowSpeedInMiliSecondsProperty); }
            set { SetValue(FlowSpeedInMiliSecondsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FlowsSpeedInMiliSeconds.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FlowSpeedInMiliSecondsProperty =
            DependencyProperty.Register("FlowSpeedInMiliSeconds", typeof(int), typeof(FlowShapeAnimate),
                                            new PropertyMetadata(4000, OnSetFlowsSpeedInMiliSecondsChanged));

        private static void OnSetFlowsSpeedInMiliSecondsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FlowShapeAnimate).OnSetFlowsSpeedInMiliSecondsChanged(e);
        }

        private void OnSetFlowsSpeedInMiliSecondsChanged(DependencyPropertyChangedEventArgs e)
        {
            UpdateAnimation();
        }

        public double FlowSize
        {
            get { return (double)GetValue(FlowSizeProperty); }
            set { SetValue(FlowSizeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FlowSize.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FlowSizeProperty =
            DependencyProperty.Register("FlowSize", typeof(double), typeof(FlowShapeAnimate), new PropertyMetadata(25.0));

        public Brush FlowColor
        {
            get { return (Brush)GetValue(FlowColorProperty); }
            set { SetValue(FlowColorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for FlowColor.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FlowColorProperty =
            DependencyProperty.Register("FlowColor", typeof(Brush), typeof(FlowShapeAnimate), new PropertyMetadata(null));

        public DoubleAnimationUsingPath Animation
        {
            get { return (DoubleAnimationUsingPath)GetValue(AnimationProperty); }
            set { SetValue(AnimationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Animation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AnimationProperty =
            DependencyProperty.Register("Animation", typeof(DoubleAnimationUsingPath), typeof(FlowShapeAnimate), new PropertyMetadata(null));

        public PathType Type
        {
            get { return (PathType)GetValue(TypeProperty); }
            set { SetValue(TypeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Type.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TypeProperty =
            DependencyProperty.Register("Type", typeof(PathType), typeof(FlowShapeAnimate), new PropertyMetadata(PathType.OneWay,
                OnSetTypeChanged));

        private static void OnSetTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as FlowShapeAnimate).OnSetTypeChanged(e);
        }

        private void OnSetTypeChanged(DependencyPropertyChangedEventArgs e)
        {
            UpdateAnimation();
        }

        public bool CanShow
        {
            get { return (bool)GetValue(CanShowProperty); }
            set { SetValue(CanShowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CanShow.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanShowProperty =
            DependencyProperty.Register("CanShow", typeof(bool), typeof(FlowShapeAnimate), new PropertyMetadata(true));


        public FlowShapeAnimate()
        {
            InitializeComponent();
            DataContext = this;

            Animation = new DoubleAnimationUsingPath()
            {
                RepeatBehavior = RepeatBehavior.Forever,
            };            
        }

        private PathGeometry FlowPathGeometry;

        public void UpdateFlowPath(List<Geometry> geometries)
        {
            if (FlowPathGeometry == null)
                FlowPathGeometry = new PathGeometry();

            FlowPathGeometry.Clear();

            foreach (var geo in geometries)
            {
                if (geo == null)
                    continue;

                FlowPathGeometry.AddGeometry(geo);
            }

            UpdateAnimation();
        }

        public void UpdateAnimation()
        {
            Animation.PathGeometry = FlowPathGeometry;

            if (Type == PathType.OneWay)
            {
                Animation.AutoReverse = false;
                Animation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, FlowSpeedInMiliSeconds));
            }
            else if (Type == PathType.TwoWay)
            {
                Animation.AutoReverse = true;
                Animation.Duration = new Duration(new TimeSpan(0, 0, 0, 0, FlowSpeedInMiliSeconds / 2));
            }
            else
            {
                throw new NotSupportedException();
            }

            Animation.Source = PathAnimationSource.X;
            this.BeginAnimation(Canvas.LeftProperty, Animation);
            Animation.Source = PathAnimationSource.Y;
            this.BeginAnimation(Canvas.TopProperty, Animation);
        }

        public static double MiddleSize { get; } = 40;
        public static double MinSize { get; } = 20;
        public static double MaxSize { get; } = 60;
    }
}
