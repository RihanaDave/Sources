using GraphX;
using GraphX.Controls.Animations;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GPAS.Graph.GraphViewer.Foundations
{
    public class FlowPathVM : INotifyPropertyChanged
    {
        public FlowPathVM()
        {
            SetBindingPropertiesAnimateShape();
            pathOrderedEdges.CollectionChanged += PathOrderedEdges_CollectionChanged;
        }

        private void SetBindingPropertiesAnimateShape()
        {
            AnimateShape.SetBinding(FlowShapeAnimate.FlowColorProperty, new Binding("AnimateShapeFlowColor")
            {
                Mode = BindingMode.TwoWay,
                Source = this,
            });

            AnimateShape.SetBinding(FlowShapeAnimate.FlowSpeedInMiliSecondsProperty, new Binding("FlowSpeedInMiliSeconds")
            {
                Mode = BindingMode.TwoWay,
                Source = this,
            });

            AnimateShape.SetBinding(FlowShapeAnimate.TypeProperty, new Binding("Type")
            {
                Mode = BindingMode.TwoWay,
                Source = this,
            });
        }

        private void SetBindingPropertiesStaticShapes(FlowShapeStatic staticShape)
        {
            if (staticShape == null)
                return;

            staticShape.SetBinding(FlowShapeStatic.FlowColorProperty, new Binding("StaticShapeFlowColor")
            {
                Mode = BindingMode.TwoWay,
                Source = this,
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event EventHandler PathGeometryChanged;
        public void OnPathGeometryChanged()
        {
            PathGeometryChanged?.Invoke(this, new EventArgs());
        }

        private double weight;
        public double Weight
        {
            get
            {
                return weight;
            }
            set
            {
                if (weight != value)
                {
                    weight = value;
                    NotifyPropertyChanged();

                    UpdateShapeSize(weight, includeWeightInFlow);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
                    UpdateStaticShapes();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
                }
            }
        }

        private double minFlowPathWeight = 0;
        public double MinFlowPathWeight
        {
            get
            {
                return minFlowPathWeight;
            }
            set
            {
                if(minFlowPathWeight != value)
                {
                    minFlowPathWeight = value;
                    NotifyPropertyChanged();

                    UpdateShapeSize(weight, includeWeightInFlow);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
                    UpdateStaticShapes();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
                }
            }
        }

        private double maxFlowPathWeight = double.MaxValue;
        public double MaxFlowPathWeight
        {
            get
            {
                return maxFlowPathWeight;
            }
            set
            {
                if(maxFlowPathWeight != value)
                {
                    maxFlowPathWeight = value;
                    NotifyPropertyChanged();

                    UpdateShapeSize(weight, includeWeightInFlow);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
                    UpdateStaticShapes();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
                }
            }
        }

        private ObservableCollection<EdgeControl> pathOrderedEdges = new ObservableCollection<EdgeControl>();

        public void UpdatePathOrderedEdges(IEnumerable<EdgeControl> edgeControls)
        {
            pathOrderedEdges.Clear();
            staticShapes.Clear();

            if(edgeControls != null)
            {
                foreach (var edgeControl in edgeControls)
                {
                    pathOrderedEdges.Add(edgeControl);
                }
            }

            //UpdateFlowShape();
        }

        private void PathOrderedEdges_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (AnimateShape == null)
                return;
            
            if(e.NewItems != null)
            {
                foreach (EdgeControl edgeControl in e.NewItems)
                {
                    staticShapes[edgeControl] = new FlowShapeStatic();
                    staticShapes[edgeControl].Direction = (edgeControl.Edge as Edge).Direction;
                    SetBindingPropertiesStaticShapes(staticShapes[edgeControl]);

                    edgeControl.SizeChanged -= EdgeControl_SizeChanged;
                    edgeControl.SizeChanged += EdgeControl_SizeChanged;
                }
            }

            if(e.OldItems != null)
            {
                foreach (EdgeControl edgeControl in e.OldItems)
                {
                   // edgeControl.SizeChanged -= EdgeControl_SizeChanged;
                }
            }
        }

        private void EdgeControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            OnPathGeometryChanged();
        }

        public async void UpdateFlowShape()
        {
            if (Parent != null && Parent.IsFlowsShown)
            {
                if (VisualStyle == FlowVisualStyle.Animated)
                {
                    await HideStaticShapes();
                    await UpdateAnimateShape();
                    await ShowAnimateShape();
                }
                else if (VisualStyle == FlowVisualStyle.Static)
                {
                    await HideAnimateShape();
                    await UpdateStaticShapes();
                    await ShowStaticShapes();
                }
                else if (VisualStyle == FlowVisualStyle.None)
                {
                    await HideStaticShapes();
                    await HideAnimateShape();
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
        }

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        private async Task ShowAnimateShape()
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            AnimateShape.CanShow = IsShown;
        }

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        private async Task HideAnimateShape()
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            AnimateShape.CanShow = false;
        }

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        private async Task ShowStaticShapes()
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            foreach (var shape in staticShapes.Values)
            {
                shape.CanShow = IsShown;
            }
        }

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        private async Task HideStaticShapes()
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            foreach (var shape in staticShapes.Values)
            {
                shape.CanShow = false;
            }
        }

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        private async Task UpdateStaticShapes()
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            foreach (var item in staticShapes)
            {
                EdgeControl edge = item.Key;
                FlowShapeStatic shape = item.Value;
                shape.SetPosition(GetEdgeCenterPoint(edge), GetEdgeAngle(edge));
            }
        }

        private double GetEdgeAngle(EdgeControl edge)
        {
            PathGeometry lineEdge = edge.GetEdgePathGeometry();

            if (lineEdge == null)
                return 0;

            PathFigure lineEdgeFigure = lineEdge.Figures[0];
            LineSegment lineEdgeSegment = lineEdgeFigure.Segments[0] as LineSegment;

            var p1 = lineEdgeFigure.StartPoint;
            var p2 = lineEdgeSegment.Point;

            double xDiff = p2.X - p1.X;
            double yDiff = p2.Y - p1.Y;
            return Math.Atan2(yDiff, xDiff) * (180 / Math.PI);
        }

        private Point GetEdgeCenterPoint(EdgeControl edge)
        {
            PathGeometry lineEdge = edge.GetEdgePathGeometry();

            if (lineEdge == null)
                return new Point(double.NaN, double.NaN);

            PathFigure lineEdgeFigure = lineEdge.Figures[0];
            LineSegment lineEdgeSegment = lineEdgeFigure.Segments[0] as LineSegment;

            var startPoint = lineEdgeFigure.StartPoint;
            var endPoint = lineEdgeSegment.Point;

            return new Point((startPoint.X + endPoint.X) / 2, (startPoint.Y + endPoint.Y) / 2);
        }

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        private async Task UpdateAnimateShape()
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            List<Geometry> geometries = new List<Geometry>();

            if (pathOrderedEdges == null || pathOrderedEdges.Count == 0 || pathOrderedEdges[0].Edge == null)
            {
                AnimateShape.UpdateFlowPath(geometries);
                return;
            }

            for (int i = 0; i < pathOrderedEdges.Count; i++)
            {
                EdgeControl edge = pathOrderedEdges[i];
                EdgeControl nextEdge = pathOrderedEdges.Count > i + 1 ? pathOrderedEdges[i + 1] : null;
                geometries.AddRange(GetEdgeGeometry(edge, nextEdge));
            }

            AnimateShape.UpdateFlowPath(geometries);
        }

        private IEnumerable<Geometry> GetEdgeGeometry(EdgeControl edge, EdgeControl nextEdge)
        {
            List<Geometry> geometries = new List<Geometry>();
            PathGeometry lineEdge = edge.GetEdgePathGeometry();

            if (edge.IsDirectedToSource)
            {
                lineEdge = InverteLineGeometry(lineEdge);
            }
            else if(nextEdge !=null && !edge.IsDirectedToSource && !edge.IsDirectedToTarget) //لینک بعدی وجود داشته باشد و لینک دوطرفه باشد.
            {
                if(edge.Source == nextEdge.Source || edge.Source == nextEdge.Target)
                {
                    lineEdge = InverteLineGeometry(lineEdge);
                }
            }

            if (lineEdge != null)
            {
                geometries.Add(lineEdge);

                if (nextEdge != null)
                {
                    PathGeometry nextLineEdge = nextEdge.GetEdgePathGeometry();
                    if (nextLineEdge != null)
                    {
                        if (nextEdge.IsDirectedToSource)
                        {
                            nextLineEdge = InverteLineGeometry(nextLineEdge);
                        }

                        PathFigure nextLineEdgeFigure = nextLineEdge.Figures[0];
                        LineSegment LineEdgeSegments = lineEdge.Figures[0].Segments[0] as LineSegment;

                        LineGeometry betweenLineEdge = new LineGeometry(LineEdgeSegments.Point, nextLineEdgeFigure.StartPoint);
                        geometries.Add(betweenLineEdge);
                    }
                }
            }

            return geometries;
        }

        private PathGeometry InverteLineGeometry(PathGeometry lineEdge)
        {
            if (lineEdge == null)
                return null;

            PathFigure pathFigure = lineEdge.Figures[0];
            LineSegment lineSegment = pathFigure.Segments[0] as LineSegment;
            var p1 = lineSegment.Point;
            var p2 = pathFigure.StartPoint;

            PathGeometry pathGeometry = new PathGeometry();
            PathFigure linePathFigure = new PathFigure();
            linePathFigure.StartPoint = p1;
            linePathFigure.Segments.Add(new LineSegment(p2, true));
            pathGeometry.Figures.Add(linePathFigure);

            return pathGeometry;
        }

        private bool isShown;
        public bool IsShown
        {
            get
            {
                return isShown;
            }
            set
            {
                if (isShown != value)
                {
                    isShown = value;

                    AnimateShape.CanShow = isShown && VisualStyle == FlowVisualStyle.Animated;
                    foreach (var shape in StaticShapes.Values)
                    {
                        shape.CanShow = isShown && VisualStyle == FlowVisualStyle.Static;
                    }

                    NotifyPropertyChanged();
                }
            }
        }

        private PathType type;
        public PathType Type
        {
            get
            {
                return type;
            }
            set
            {
                if (type != value)
                {
                    type = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private FlowVisualStyle visualStyle = FlowVisualStyle.Animated;
        public FlowVisualStyle VisualStyle
        {
            get
            {
                return visualStyle;
            }
            set
            {
                if (visualStyle != value)
                {
                    visualStyle = value;
                    NotifyPropertyChanged();

                    //UpdateFlowShape(VisualStyle);
                }
            }
        }

        private FlowShapeAnimate animateShape = new FlowShapeAnimate();
        public FlowShapeAnimate AnimateShape
        {
            get
            {
                return animateShape;
            }
            set
            {
                if (animateShape != value)
                {
                    animateShape = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Dictionary<EdgeControl, FlowShapeStatic> staticShapes = new Dictionary<EdgeControl, FlowShapeStatic>();
        public Dictionary<EdgeControl, FlowShapeStatic> StaticShapes
        {
            get
            {
                return staticShapes;
            }
            set
            {
                if(staticShapes != value)
                {
                    staticShapes = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private bool includeWeightInFlow = true;
        public bool IncludeWeightInFlow
        {
            get
            {
                return includeWeightInFlow;
            }
            set
            {
                if (includeWeightInFlow != value)
                {
                    includeWeightInFlow = value;
                    NotifyPropertyChanged();

                    UpdateShapeSize(weight, includeWeightInFlow);
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
                    UpdateStaticShapes();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
                }
            }
        }

        private void UpdateShapeSize(double weight, bool includeWeightInFlow)
        {
            if (includeWeightInFlow)
            {
                if (MaxFlowPathWeight == MinFlowPathWeight)
                {
                    AnimateShape.FlowSize = FlowShapeAnimate.MiddleSize;
                    foreach (var shape in staticShapes.Values)
                    {
                        shape.FlowSize = FlowShapeStatic.MiddleSize;
                    }
                }
                else
                {
                    double animateWeight = ((weight - MinFlowPathWeight) / (MaxFlowPathWeight - MinFlowPathWeight)) *
                        (FlowShapeAnimate.MaxSize - FlowShapeAnimate.MinSize) + FlowShapeAnimate.MinSize;
                    AnimateShape.FlowSize = animateWeight;

                    double staticWeight = ((weight - MinFlowPathWeight) / (MaxFlowPathWeight - MinFlowPathWeight)) *
                        (FlowShapeStatic.MaxSize - FlowShapeStatic.MinSize) + FlowShapeStatic.MinSize;
                    foreach (var shape in staticShapes.Values)
                    {
                        shape.FlowSize = staticWeight;
                    }
                }
            }
            else
            {
                AnimateShape.FlowSize = FlowShapeAnimate.MiddleSize;
                foreach (var shape in staticShapes.Values)
                {
                    shape.FlowSize = FlowShapeStatic.MiddleSize;
                }
            }
        }

        private Brush animateShapeflowColor;
        public Brush AnimateShapeFlowColor
        {
            get
            {
                return animateShapeflowColor;
            }
            set
            {
                if (animateShapeflowColor != value)
                {
                    animateShapeflowColor = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private Brush staticShapeflowColor;
        public Brush StaticShapeFlowColor
        {
            get
            {
                return staticShapeflowColor;
            }
            set
            {
                if (staticShapeflowColor != value)
                {
                    staticShapeflowColor = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private int flowSpeedInMiliSeconds = 4000;
        public int FlowSpeedInMiliSeconds
        {
            get
            {
                return flowSpeedInMiliSeconds;
            }
            set
            {
                if (flowSpeedInMiliSeconds != value)
                {
                    flowSpeedInMiliSeconds = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private object tag;
        public object Tag
        {
            get
            {
                return tag;
            }
            set
            {
                if(tag != value)
                {
                    tag = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private GraphViewer parent;
        public GraphViewer Parent
        {
            get
            {
                return parent;
            }
            set
            {
                if(parent != value)
                {
                    parent = value;
                    NotifyPropertyChanged();
                }
            }
        }
    }
}
