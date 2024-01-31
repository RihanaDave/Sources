using GPAS.Graph.GraphViewer.Foundations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;

namespace GPAS.Graph.GraphViewer
{
    public partial class GraphViewer
    {
        public bool IsFlowsShown
        {
            get { return (bool)GetValue(IsFlowsShownProperty); }
            set { SetValue(IsFlowsShownProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsFlowsShown.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsFlowsShownProperty =
            DependencyProperty.Register("IsFlowsShown", typeof(bool), typeof(GraphViewer), new PropertyMetadata(false,
                OnSetIsFlowsShownChanged));

        private static void OnSetIsFlowsShownChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as GraphViewer).OnSetIsFlowsShownChanged(e);
        }

        private void OnSetIsFlowsShownChanged(DependencyPropertyChangedEventArgs e)
        {
            SetVisibilityFlowLayer();
        }

        public ObservableCollection<FlowPathVM> TotalFlowPathes
        {
            get { return (ObservableCollection<FlowPathVM>)GetValue(TotalFlowPathesProperty); }
            set { SetValue(TotalFlowPathesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for TotalFlowPathes.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TotalFlowPathesProperty =
            DependencyProperty.Register("TotalFlowPathes", typeof(ObservableCollection<FlowPathVM>), typeof(GraphViewer), new PropertyMetadata(null,
                                            OnSetTotalFlowPathesChanged));

        private static void OnSetTotalFlowPathesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as GraphViewer).OnSetTotalFlowPathesChanged(e);
        }

        private void OnSetTotalFlowPathesChanged(DependencyPropertyChangedEventArgs e)
        {
            if (TotalFlowPathes == null)
                TotalFlowPathes = new ObservableCollection<FlowPathVM>();
        }

        public double MinFlowPathWeight
        {
            get { return (double)GetValue(MinFlowPathWeightProperty); }
            set { SetValue(MinFlowPathWeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MinFlowPathWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MinFlowPathWeightProperty =
            DependencyProperty.Register("MinFlowPathWeight", typeof(double), typeof(GraphViewer), new PropertyMetadata(0.0));

        public double MaxFlowPathWeight
        {
            get { return (double)GetValue(MaxFlowPathWeightProperty); }
            set { SetValue(MaxFlowPathWeightProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MaxFlowPathWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxFlowPathWeightProperty =
            DependencyProperty.Register("MaxFlowPathWeight", typeof(double), typeof(GraphViewer), new PropertyMetadata(double.MaxValue));

        private void SetVisibilityFlowLayer()
        {
            if(IsFlowsShown)
            {
                flowLayerCanvas.Visibility = Visibility.Visible;
                RedrawFlows();
            }
            else
            {
                flowLayerCanvas.Visibility = Visibility.Collapsed;
            }
        }

        CancellationTokenSource RedrawFlowsCTS = new CancellationTokenSource();
        public async void RedrawFlows()
        {
            RedrawFlowsCTS.Cancel();
            AssignMinAndMaxFlowPathWeight();
            RedrawFlowsCTS = new CancellationTokenSource();

            await Dispatcher.BeginInvoke(new Action(async() => await RedrawFlows(RedrawFlowsCTS)), DispatcherPriority.Normal);
           // await RedrawFlows(RedrawFlowsCTS);
        }

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        public async Task RedrawFlows(CancellationTokenSource cts)
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            if (IsFlowsShown)
            {
                flowLayerCanvas.Children.Clear();

                if (TotalFlowPathes == null)
                    return;

                foreach (var flowPath in TotalFlowPathes)
                {
                    if (cts.IsCancellationRequested)
                        return;

                    AddFlowPath(flowPath, cts);
                }
            }
        }

        private void InitFlowZoomControl()
        {
            if (flowZoomControl.ViewFinder != null)
                flowZoomControl.ViewFinder.Visibility = Visibility.Collapsed;

            flowZoomControl.SetBinding(GraphX.Controls.ZoomControl.ModifierModeProperty, new Binding("ModifierMode")
            {
                Source = zoomControl,
                Mode = BindingMode.TwoWay
            });

            flowZoomControl.SetBinding(GraphX.Controls.ZoomControl.ModeProperty, new Binding("Mode")
            {
                Source = zoomControl,
                Mode = BindingMode.TwoWay
            });

            flowZoomControl.SetBinding(GraphX.Controls.ZoomControl.TranslateXProperty, new Binding("TranslateX")
            {
                Source = zoomControl,
                Mode = BindingMode.TwoWay
            });

            flowZoomControl.SetBinding(GraphX.Controls.ZoomControl.TranslateYProperty, new Binding("TranslateY")
            {
                Source = zoomControl,
                Mode = BindingMode.TwoWay
            });

            flowZoomControl.SetBinding(GraphX.Controls.ZoomControl.MinZoomProperty, new Binding("MinZoom")
            {
                Source = zoomControl,
                Mode = BindingMode.TwoWay
            });

            flowZoomControl.SetBinding(GraphX.Controls.ZoomControl.MaxZoomProperty, new Binding("MaxZoom")
            {
                Source = zoomControl,
                Mode = BindingMode.TwoWay
            });

            flowZoomControl.SetBinding(GraphX.Controls.ZoomControl.ZoomBoxProperty, new Binding("ZoomBox")
            {
                Source = zoomControl,
                Mode = BindingMode.TwoWay
            });

            flowZoomControl.SetBinding(GraphX.Controls.ZoomControl.ZoomSensitivityProperty, new Binding("ZoomSensitivity")
            {
                Source = zoomControl,
                Mode = BindingMode.TwoWay
            });

            flowZoomControl.SetBinding(GraphX.Controls.ZoomControl.ZoomStepProperty, new Binding("ZoomStep")
            {
                Source = zoomControl,
                Mode = BindingMode.TwoWay
            });

            flowZoomControl.SetBinding(GraphX.Controls.ZoomControl.ZoomProperty, new Binding("Zoom")
            {
                Source = zoomControl,
                Mode = BindingMode.TwoWay
            });
        }

        private void AssignMinAndMaxFlowPathWeight()
        {
            if (TotalFlowPathes == null)
                return;

            if (TotalFlowPathes.Count > 0)
            {
                MinFlowPathWeight = TotalFlowPathes.Select(fp => fp.Weight).Min();
                MaxFlowPathWeight = TotalFlowPathes.Select(fp => fp.Weight).Max();
            }

            foreach (var flowPath in TotalFlowPathes)
            {
                flowPath.MaxFlowPathWeight = MaxFlowPathWeight;
                flowPath.MinFlowPathWeight = MinFlowPathWeight;
            }
        }

        private void AddFlowPath(FlowPathVM flowPath, CancellationTokenSource cts)
        {
            if (flowPath.AnimateShape == null)
            {
                flowPath.AnimateShape = new FlowShapeAnimate();
            }

            if (flowPath.StaticShapes == null)
            {
                flowPath.StaticShapes = new Dictionary<EdgeControl, FlowShapeStatic>();
            }

            if (!flowLayerCanvas.Children.Contains(flowPath.AnimateShape))
            {
                flowLayerCanvas.Children.Add(flowPath.AnimateShape);
            }

            foreach (var shape in flowPath.StaticShapes.Values)
            {
                if (cts.IsCancellationRequested)
                    return;

                if (!flowLayerCanvas.Children.Contains(shape))
                {
                    flowLayerCanvas.Children.Add(shape);
                }
            }

            flowPath.UpdateFlowShape();
        }
    }
}
