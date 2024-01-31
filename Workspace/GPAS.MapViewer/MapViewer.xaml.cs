using GMap.NET;
using GMap.NET.WindowsPresentation;
using GPAS.MapViewer.Heatmap;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GPAS.MapViewer
{
    /// <summary>
    /// Interaction logic for MapViewer.xaml
    /// </summary>
    public partial class MapViewer : UserControl
    {
        public MapViewer()
        {
            InitializeComponent();
            Reset();
            gmapControl.MapProvider = mapProvider;
            gmapControl.IgnoreMarkerOnMouseWheel = true;
            gmapControl.Manager.PrimaryCache.DeleteOlderThan(DateTime.Now, null);
            gmapControl.Manager.MemoryCache.Clear();
            gmapControl.OnTileLoadStart += GmapControl_OnTileLoadStart;
            gmapControl.OnTileLoadComplete += GmapControl_OnTileLoadComplete;
            gmapControl.DisableAltForSelection = true;
            gmapControl.ShowCenter = false;
            // Hide GMap built-in Selection
            gmapControl.SelectionPen.Thickness = 0;
            gmapControl.SelectedAreaFill = Brushes.Transparent;
            MapShapes.CollectionChanged += MapShapes_CollectionChanged;
            HeatMapLayers.CollectionChanged += HeatMapLayers_CollectionChanged;

            mapProvider.MapTileImageNeeded += (sender, e) =>
            {
                OnMapTileImageNeeded(ref e);
            };

            isClickPointOnMarkers = false;
            gmapControl.ShowTileGridLines = false;

            // Multiselection-Polygon Initialization
            MultiselectionPolygon = new GMapPolygon(new PointLatLng[4] {
                new PointLatLng(), new PointLatLng(), new PointLatLng(), new PointLatLng()});
            MultiselectionPolygon.RegenerateShape(gmapControl);
            ((System.Windows.Shapes.Path)MultiselectionPolygon.Shape).StrokeThickness = 2;
        }

        private void GmapControl_OnTileLoadComplete(long ElapsedMilliseconds)
        {
            OnMapTileLoadCompleted();
        }

        public event EventHandler MapTileLoadCompleted;
        protected void OnMapTileLoadCompleted()
        {
            MapTilesLoading = false;
            MapTileLoadCompleted?.Invoke(this, EventArgs.Empty);
        }

        private void GmapControl_OnTileLoadStart()
        {
            OnMapTileLoadStarted();
        }

        public event EventHandler MapTileLoadStarted;
        protected void OnMapTileLoadStarted()
        {
            if (MapTilesLoading)
            {
                OnMapTileLoadAborted();
            }

            MapTilesLoading = true;
            MapTileLoadStarted?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler MapTileLoadAborted;
        protected void OnMapTileLoadAborted()
        {
            MapTilesLoading = false;
            MapTileLoadAborted?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<MapMouseButtonEventArgs> MapMouseDown;
        protected void OnMapMouseDown(MapMouseButtonEventArgs e)
        {
            MapMouseDown?.Invoke(this, e);
        }

        public readonly double EasternMostDegree = 180, WesternMostDegree = -180, NorthernMostDegree = 85.0511287798066, SouthernMostDegree = -85.0511287798066;

        protected MapProvider mapProvider = new MapProvider();

        private PointLatLng MouseDownPosition;
        private Point MouseDownLocalPosition;
        private bool IsMultiSelectionInprogress = false;
        private GMapPolygon MultiselectionPolygon;

        private DrawingShape shapeType = DrawingShape.None;
        public DrawingShape ShapeType
        {
            get { return shapeType; }
            set
            {
                if (value == DrawingShape.None)
                {
                    EnableDrawMode = false;
                }
                else
                {
                    EnableDrawMode = true;
                }
                shapeType = value;
            }
        }

        public bool MapTilesLoading { get; protected set; } = false;

        public IAdvanceShape SelectedAdvanceShape { get; set; }

        public IMapShape SelectedMapShape { get; set; }

        Boolean enableDrawMode = false;
        public bool EnableDrawMode { get { return enableDrawMode; } set { enableDrawMode = value; } }

        /// <summary>
        /// مجموعه ای از شکل ها ویژه که می تواند شامل چندضلعی، دایره و مسیر باشد و روی نقشه نمایش داده می شوند.
        /// </summary>
        public ObservableCollection<IAdvanceShape> AdvanceShapes = new ObservableCollection<IAdvanceShape>();

        /// <summary>
        /// مجموعه ای از شکل ها که می تواند شامل چندضلعی، دایره و مسیر باشد و روی نقشه نمایش داده می شوند.
        /// </summary>
        public ObservableCollection<IMapShape> MapShapes = new ObservableCollection<IMapShape>();

        private void MapShapes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (IMapShape item in e.NewItems)
                {
                    item.Draw();
                }
            }

            if (e.OldItems != null)
            {
                foreach (IMapShape item in e.OldItems)
                {
                    item.Erase();
                }
            }
        }

        private ObservableCollection<HeatmapMarker> HeatMapLayers = new ObservableCollection<HeatmapMarker>();

        private void HeatMapLayers_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (HeatmapMarker item in e.NewItems)
                {
                    item.Map = this;

                    item.Draw();
                }
            }

            if (e.OldItems != null)
            {
                foreach (HeatmapMarker item in e.OldItems)
                {
                    item.Erase();
                }
            }

            if (HeatMapLayers?.Count > 0)
            {
                SelectedHeatmapLayer = HeatMapLayers.Last();
            }
            else
            {
                SelectedHeatmapLayer = null;
            }
        }

        HeatmapMarker SelectedHeatmapLayer = null;

        /// <summary>
        /// افزودن شکل های بدون کنترلر به نقشه.
        /// </summary>
        /// <param name="shape"></param>
        public void AddMapShape(IMapShape shape)
        {
            if (shape is PolygonLatLng)
            {
                AddPolygonToMapShapes(shape as PolygonLatLng);
            }
            if (shape is RouteLatLng)
            {
                AddRouteToMapShapes(shape as RouteLatLng);
            }
            if (shape is CircleLatLng)
            {
                AddCircleToMapShapes(shape as CircleLatLng);
            }
        }

        /// <summary>
        /// تبدیل شکل های معمولی (بدون کنترلر) به شکل های ویژه و نمایش آن روی نقشه.
        /// </summary>
        /// <param name="shape">شکل معمولی</param>
        public void ConvertMapShapeToAdvanceShape(IMapShape shape)
        {
            if (shape is PolygonLatLng)
            {
                PolygonLatLng polygon = shape as PolygonLatLng;
                AdvancePolygon advancePolygon = polygon.ConvertToAdvancePolygon();
                DeleteMapShape(polygon);
                AddAdvanceShap(advancePolygon);
            }
            if (shape is RouteLatLng)
            {
                RouteLatLng route = shape as RouteLatLng;
                AdvanceRoute advanceRoute = route.ConvertToAdvanceRoute();
                DeleteMapShape(route);
                AddAdvanceShap(advanceRoute);
            }
            if (shape is CircleLatLng)
            {
                CircleLatLng circle = shape as CircleLatLng;
                AdvanceCircle advanceCircle = circle.ConvertToAdvanceCircle();
                DeleteMapShape(circle);
                AddAdvanceShap(advanceCircle);
            }
        }

        /// <summary>
        /// حذف شکل های معمولی از روی نقشه.
        /// </summary>
        /// <param name="shape">شکل معمولی</param>
        public void DeleteMapShape(IMapShape shape)
        {
            shape.Delete();
            MapShapes?.Remove(shape);

            if (MapShapes?.Count > 0)
            {
                SelectedMapShape = MapShapes.Last();
            }
            else
            {
                SelectedMapShape = null;
            }
        }

        /// <summary>
        /// تبدیل شکل ویژه به شکل معمولی و نمایش آن رو نقشه.
        /// </summary>
        /// <param name="adv">شکل ویژه</param>
        public void ConvertAdvanceShapeToMapShape(IAdvanceShape adv)
        {
            if (adv is AdvancePolygon)
            {
                AdvancePolygon advancePolygon = adv as AdvancePolygon;
                PolygonLatLng polygon = advancePolygon?.ConvertToPolygonLatLng();
                advancePolygon.Delete();
                AddPolygonToMapShapes(polygon);
            }
            if (adv is AdvanceRoute)
            {
                AdvanceRoute advanceRoute = adv as AdvanceRoute;
                RouteLatLng route = advanceRoute?.ConvertToRouteLatLng();
                advanceRoute.Delete();
                AddRouteToMapShapes(route);
            }
            if (adv is AdvanceCircle)
            {
                AdvanceCircle advanceCircle = adv as AdvanceCircle;
                CircleLatLng circle = advanceCircle?.ConvertToCircleLatLng();
                advanceCircle.Delete();
                AddCircleToMapShapes(circle);
            }
        }

        /// <summary>
        /// افزودن دایره معمولی به نقشه.
        /// </summary>
        /// <param name="circle">دایره معمولی</param>
        private void AddCircleToMapShapes(CircleLatLng circle)
        {
            circle.Map = gmapControl;
            circle.Style = FindResource("LockCircle") as Style;
            circle.MouseDown += MapShape_MouseDown;
            MapShapes.Add(circle);
            SelectedMapShape = circle;
        }

        /// <summary>
        /// افزودن چندضلعی معمولی به نقشه.
        /// </summary>
        /// <param name="polygon">چندضلعی معمولی</param>
        private void AddPolygonToMapShapes(PolygonLatLng polygon)
        {
            polygon.Map = gmapControl;
            polygon.Style = FindResource("LockPolygon") as Style;
            polygon.MouseDown += MapShape_MouseDown;
            MapShapes.Add(polygon);
            SelectedMapShape = polygon;
        }

        /// <summary>
        /// افزودن مسیر معمولی به نقشه.
        /// </summary>
        /// <param name="route">مسیر معمولی</param>
        private void AddRouteToMapShapes(RouteLatLng route)
        {
            route.Map = gmapControl;
            route.Style = FindResource("LockRoute") as Style;
            route.MouseDown += MapShape_MouseDown;
            MapShapes.Add(route);
            SelectedMapShape = route;
        }

        public void StartDrawingShape(DrawingShape shape)
        {

        }

        public void ClearDrawnShape()
        {

        }

        private void GmapControl_MouseMove(object sender, MouseEventArgs e)
        {
            //--------------------------------------
            // برگرفته از کد مبدا GMapControl
            Point p = e.GetPosition(this);
            var currentPoint = gmapControl.FromLocalToLatLng((int)p.X, (int)p.Y);

            // انتخاب محدوده به صورت دستی
            if (IsMultiSelectionInprogress)
            {
                //--------------------------------------
                MultiselectionPolygon.Points[1] = new PointLatLng(currentPoint.Lat, MouseDownPosition.Lng);
                MultiselectionPolygon.Points[2] = currentPoint;
                MultiselectionPolygon.Points[3] = new PointLatLng(MouseDownPosition.Lat, currentPoint.Lng);
                MultiselectionPolygon.RegenerateShape(gmapControl);
            }

            if (SelectedHeatmapLayer != null && SelectedHeatmapLayer.Shape is Image)
            {
                Image heatmapImage = SelectedHeatmapLayer.Shape as Image;
                if (heatmapImage.Visibility == Visibility.Visible)
                {
                    Point pp = e.GetPosition(heatmapImage);

                    Color color = SelectedHeatmapLayer.GetPixel(pp);
                    HeatmapHintedPixel = (HeatmapMostDensity - HeatmapLeastDensity) * color.ScA + (HeatmapLeastDensity * color.ScA);
                }
            }
        }

        private void gmapControl_MouseLeave(object sender, MouseEventArgs e)
        {
            HeatmapHintedPixel = 0.0;
        }

        private void GmapControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            //--------------------------------------
            // برگرفته از کد مبدا GMapControl
            Point p = e.GetPosition(this);
            MouseDownPosition = gmapControl.FromLocalToLatLng((int)p.X, (int)p.Y);
            //--------------------------------------

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (ShapeType != DrawingShape.None)
                {
                    if (ShapeType == DrawingShape.Polygon)
                    {
                        if ((SelectedAdvanceShape == null ||
                            SelectedAdvanceShape.DrawMode == DrawingMode.End) &&
                            AdvanceShapes.Count == 0)
                        {
                            AdvancePolygon polygon = new AdvancePolygon(new List<PointLatLng>() { MouseDownPosition });
                            polygon.PolygonDrawn += Polygon_PolygonDrawn;
                            polygon.DrawMode = DrawingMode.SetPoint;
                            polygon.Controller.CheckCoincidentLines = true;
                            polygon.Controller.CheckCrossPoints = true;
                            AddAdvanceShap(polygon);
                        }
                    }
                    else if (ShapeType == DrawingShape.Route)
                    {
                        if ((SelectedAdvanceShape == null ||
                            SelectedAdvanceShape.DrawMode == DrawingMode.End) &&
                            AdvanceShapes.Count == 0)
                        {
                            AdvanceRoute route = new AdvanceRoute(new List<PointLatLng>() { MouseDownPosition }, DefaultDrawingPathWidthInMeters);
                            route.RouteDrawn += Route_RouteDrawn;
                            route.DrawMode = DrawingMode.SetPoint;
                            AddAdvanceShap(route);
                        }
                    }
                    else if (ShapeType == DrawingShape.Circle)
                    {
                        if ((SelectedAdvanceShape == null ||
                            SelectedAdvanceShape.DrawMode == DrawingMode.End) &&
                            AdvanceShapes.Count == 0)
                        {
                            AdvanceCircle circle = new AdvanceCircle(MouseDownPosition, 0);
                            circle.CircleDrawn += Circle_CircleDrawn;
                            circle.DrawMode = DrawingMode.SetPoint;
                            AddAdvanceShap(circle);
                        }
                    }
                }
            }
            // انتخاب محدوده به صورت دستی
            if (IsMultiselectionModifierKeysPressed(e.ChangedButton))
            {
                MultiselectionPolygon.Points[0]
                    = MultiselectionPolygon.Points[1]
                    = MultiselectionPolygon.Points[2]
                    = MultiselectionPolygon.Points[3]
                    = MouseDownPosition;
                MultiselectionPolygon.RegenerateShape(gmapControl);
                gmapControl.Markers.Add(MultiselectionPolygon);
                IsMultiSelectionInprogress = true;
            }
            else
            {
                IsMultiSelectionInprogress = false;
            }

            MouseDownLocalPosition = p;
            OnMapMouseDown(new MapMouseButtonEventArgs(e, MouseDownPosition));
        }

        public void SetBufferSizeForSelectedRouteShape(double buffer)
        {
            DefaultDrawingPathWidthInMeters = buffer;
            if (SelectedAdvanceShape != null && SelectedAdvanceShape is AdvanceRoute)
            {
                (SelectedAdvanceShape as AdvanceRoute).Radius = buffer;
                (SelectedAdvanceShape as AdvanceRoute).CreateShape();
            }
        }

        private void Circle_CircleDrawn(object sender, CircleDrawnEventArgs e)
        {
            OnCircleDrawn(e);
        }

        public void AddHeatmapLayer(bool changePointsSourceType)
        {
            List<HeatPointLatLng> points = new List<HeatPointLatLng>();
            Random rnd = new Random();
            List<SimpleMarker> markers;
            if (HeatmapTarget == TargetPoints.AllMarkers)
            {
                markers = GetShowingMarkers().ToList();
            }
            else
            {
                markers = GetSelectedMarkers();
            }

            if (!HasChangedSimpleMarkers(markers) && !changePointsSourceType)
            {
                return;
            }

            foreach (var simpleMarker in markers)
            {
                points.Add(new HeatPointLatLng(simpleMarker.Position, (long)simpleMarker.Weight));
            }

            gmapControl.Markers.Remove(SelectedHeatmapLayer);

            SelectedHeatmapLayer = new HeatmapMarker(points) { Map = this, Radius = HeatmapDensityRadiusInMeters, ArealUnitAreaInSquareMeters = HeatmapArealUnitInSquareMeters, PointsSource = HeatmapPointsSource, Status = HeatmapStatus };
            SelectedHeatmapLayer.Draw();

            HeatmapWarningArealUnitNotMatchRadius = SelectedHeatmapLayer.WarningArealUnitNotMatchRadius;
            //HeatmapPointsCount = SelectedHeatmapLayer.Points.Count;
        }

        /// <summary>
        /// افزودن شکل ویژه به نقشه.
        /// </summary>
        /// <param name="advance">شکل ویژه</param>
        public void AddAdvanceShap(IAdvanceShape advance)
        {
            advance.Map = this;
            advance.CreateShape();
            advance.MouseDown += AdvanceShape_MouseDown;
            AdvanceShapes.Add(advance);
            SelectedAdvanceShape = advance;
        }

        private void Route_RouteDrawn(object sender, PathDrawnEventArgs e)
        {
            OnRouteDrawn(e, (AdvanceRoute)sender);
        }

        private void Polygon_PolygonDrawn(object sender, PolygonDrawnEventArgs e)
        {
            OnPolygonDrawn(e);
        }

        private void AdvanceShape_MouseDown(object sender, MouseButtonEventArgs e)
        {
            IAdvanceShape shape = sender as IAdvanceShape;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                e.Handled = true;
                SelectedAdvanceShape = shape;
                SendToFront(shape);
            }
        }

        private void MapShape_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                e.Handled = true;

                SelectedMapShape = sender as IMapShape;

                if (e.ClickCount == 2)
                {
                    //ConvertMapShapeToAdvanceShape(sender as IMapShape);
                }
            }
        }

        /// <summary>
        /// شکل ویژه مورد نظر در ترتیب نمایش در بالاترین سطح قرار می گیرد.
        /// </summary>
        /// <param name="shape"></param>
        public void SendToFront(IAdvanceShape shape)
        {
            int i = 0;
            foreach (var adv in AdvanceShapes)
            {
                if (adv != shape)
                    i = adv.SendToFront(i);
            }
            shape.SendToFront(i);
        }

        private void GmapControl_MouseUp(object sender, MouseButtonEventArgs e)
        {
            gmapControl.Markers.Remove(MultiselectionPolygon);

            //--------------------------------------
            // برگرفته از کد مبدا GMapControl
            Point p = e.GetPosition(this);
            var mouseUpPosition = gmapControl.FromLocalToLatLng((int)p.X, (int)p.Y);
            //--------------------------------------

            // حذف محدوده انتخاب چندگانه درصورت درحال نمایش بودن
            if (gmapControl.Markers.Contains(MultiselectionPolygon))


                if (MouseDownPosition == mouseUpPosition)
                {
                    DeselectAllMarkers();
                    return;
                }

            if (IsZoomSelectionModifierKeysPressed())
            {
                gmapControl.SetZoomToFitRect(RectLatLng.FromLTRB(
                    Math.Min(MouseDownPosition.Lng, mouseUpPosition.Lng),
                    Math.Min(MouseDownPosition.Lat, mouseUpPosition.Lat),
                    Math.Max(MouseDownPosition.Lng, mouseUpPosition.Lng),
                    Math.Max(MouseDownPosition.Lat, mouseUpPosition.Lat)));
                e.Handled = true;
                return;
            }

            // انتخاب محدوده به صورت دستی
            if (IsMultiSelectionInprogress && IsMultiselectionModifierKeysPressed(e.ChangedButton) && !isClickPointOnMarkers)
            {
                IsMultiSelectionInprogress = false;
                OnMultiSelectionCompleted(RectLatLng.FromLTRB(
                    Math.Min(MouseDownPosition.Lng, mouseUpPosition.Lng),
                    Math.Min(MouseDownPosition.Lat, mouseUpPosition.Lat),
                    Math.Max(MouseDownPosition.Lng, mouseUpPosition.Lng),
                    Math.Max(MouseDownPosition.Lat, mouseUpPosition.Lat)));
            }

            // انتخاب محدوده با استفاده از قابلیت داخلی انتخاب در جی.مپ
            // TODO: کدها یبه رخداد گردان رخداد SelectionChanged منتقل شوند
            //if (GetNumberOfMarkersInsideSelectedArea() == 0 && !isClickPointOnMarkers)
            //{
            //    DeselectAllMarkers();
            //}
            //if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            //{

            //    if (Math.Abs((gmapControl.SelectedArea.Right - gmapControl.SelectedArea.Left)) > 0)
            //    {
            //        InvertSelectionOfSelectedAreaMarkers(gmapControl.SelectedArea);
            //    }
            //}
            //List<SimpleMarker> selectedMarkers = GetSelectedMarkers();
            //OnMarkersSelectionChanged(selectedMarkers);
            isClickPointOnMarkers = false;
        }

        public class MarkersSelectionChangedArgs
        {
            public MarkersSelectionChangedArgs(IEnumerable<SimpleMarker> currentlySelectedMarkers)
            {
                CurrentlySelectedMarkers = currentlySelectedMarkers;
            }

            public IEnumerable<SimpleMarker> CurrentlySelectedMarkers
            {
                get;
                private set;
            }
        }

        public event EventHandler<MarkersSelectionChangedArgs> MarkersSelectionChanged;

        protected void OnMarkersSelectionChanged()
        {
            var markers = GetSelectedMarkers();
            if (markers == null)
                throw new ArgumentNullException("currentlySelectedMarkers");

            MarkersSelectionChanged?.Invoke(this, new MarkersSelectionChangedArgs(markers));
        }

        public event EventHandler<MapTileImageNeededEventArgs> MapTileImageNeeded;
        protected void OnMapTileImageNeeded(ref MapTileImageNeededEventArgs e)
        {
            MapTileImageNeeded?.Invoke(this, e);
        }

        public IEnumerable<SimpleMarker> GetShowingMarkers()
        {
            return gmapControl.Markers
                .Where(m => m is SimpleMarker)
                .Select(m => m as SimpleMarker);
        }

        public List<SimpleMarker> GetSelectedMarkers()
        {
            List<SimpleMarker> currentlySelectedMarkers = new List<SimpleMarker>();
            foreach (var item in GetShowingMarkers())
            {
                // TODO: در بررسی وضعیت انتخاب/عدم انتخاب، استفاده از ویژگی کلاس منطقی به جای کلاس نمایشی بهتر است
                if ((item.Shape as SimpleMarkerShape).IsHighlighted)
                {
                    currentlySelectedMarkers.Add(item);
                }
            }
            return currentlySelectedMarkers;
        }

        public void SelectAllMarkers()
        {
            foreach (var item in GetShowingMarkers())
            {
                SimpleMarker.SelectMarker(item);
            }
            OnMarkersSelectionChanged();
        }

        public void DeselectAllMarkers()
        {
            foreach (var item in GetShowingMarkers())
            {
                SimpleMarker.DeselectMarker(item);
            }
            OnMarkersSelectionChanged();
        }

        public void SelectMarkers(IEnumerable<SimpleMarker> markersToSelect)
        {
            foreach (SimpleMarker marker in markersToSelect)
            {
                SimpleMarker.SelectMarker(marker);
            }
            OnMarkersSelectionChanged();
        }

        protected static void InvertSelect(SimpleMarker marker)
        {
            if (!marker.IsSelected)
            {
                SimpleMarker.SelectMarker(marker);
            }
            else
            {
                SimpleMarker.DeselectMarker(marker);
            }
        }

        protected static void Select(SimpleMarker marker)
        {
            SimpleMarker.SelectMarker(marker);
        }

        public void InvertSelectionMarkers()
        {
            foreach (SimpleMarker marker in GetShowingMarkers())
            {
                InvertSelect(marker);
            }
        }

        private void InvertSelectionOfSelectedAreaMarkers(RectLatLng selectedArea)
        {
            foreach (var item in GetShowingMarkers())
            {
                if (IsAreaCoversPoint(selectedArea, item.Position))
                {
                    InvertSelect(item);
                }
            }
            OnMarkersSelectionChanged();
        }

        private bool IsAreaCoversPoint(RectLatLng area, PointLatLng position)
        {
            return
                (position.Lat >= area.Top) &&
                (position.Lat <= area.Bottom) &&
                (position.Lng >= area.Left) &&
                (position.Lng <= area.Right);
        }

        private int GetNumberOfMarkersInsideSelectedArea()
        {
            int count = 0;
            RectLatLng selectedArea = gmapControl.SelectedArea;
            foreach (var item in GetShowingMarkers())
            {
                if (selectedArea.Contains(item.Position.Lat, item.Position.Lng))
                {
                    count = count + 1;
                }
            }
            return count;
        }

        public bool isClickPointOnMarkers;

        private void OnMultiSelectionCompleted(RectLatLng selectionRect)
        {
            var beforeOperationSelectedMarkers = GetSelectedMarkers();

            InvertSelectionOfSelectedAreaMarkers(selectionRect);

            var afterOperationSelectedMarkers = GetSelectedMarkers();
            ReportSelectionChanged(beforeOperationSelectedMarkers, afterOperationSelectedMarkers);
        }

        private bool IsMultiselectionModifierKeysPressed(MouseButton mouseChangedButton)
        {
            return
                (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                && mouseChangedButton == MouseButton.Left;
        }

        private bool IsZoomSelectionModifierKeysPressed()
        {
            return (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt));
        }

        private void ShowMarker(SimpleMarker marker)
        {
            SimpleMarker.SelectMarker(marker);
            marker.Shape.MouseUp += ShowingMarkerShape_MouseUp;
            marker.Shape.MouseDown += ShowingMarkerShape_MouseDown;
            gmapControl.Markers.Add(marker);
        }

        public void ShowMarkers(List<SimpleMarker> markersToShow)
        {
            foreach (var currentMarker in markersToShow)
            {
                ShowMarker(currentMarker);
            }

            SetShowMapPointsAndLabelsStatus(HeatmapShowMapPointsAndLabels);
        }

        public Boolean HasChangedSimpleMarkers(List<SimpleMarker> showingMarkers)
        {
            if (SelectedHeatmapLayer == null || SelectedHeatmapLayer.Points == null || showingMarkers.Count != SelectedHeatmapLayer.Points.Count)
                return true;

            if (HeatmapPointsSource == PointsValueSourceType.MarkersCount)
            {
                if (showingMarkers.Any(m => !SelectedHeatmapLayer.Points.Select(p => p.Point).Contains(m.Position)))
                    return true;
            }
            else
            {
                for (int i = 0; i < showingMarkers.Count; i++)
                {
                    var m = showingMarkers[i];
                    var p = SelectedHeatmapLayer.Points[i];
                    if (m.Position != p.Point || m.Weight != p.Intensity)
                        return true;
                }
            }

            return false;
        }

        public void ReportSelectionChanged(List<SimpleMarker> beforeOperationSelectedMarkers, List<SimpleMarker> afterOperationSelectedMarkers)
        {
            if (afterOperationSelectedMarkers.Count != beforeOperationSelectedMarkers.Count
                || afterOperationSelectedMarkers.Any(m => !beforeOperationSelectedMarkers.Contains(m)))
            {
                OnMarkersSelectionChanged();
            }
        }

        private void ShowingMarkerShape_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var beforeOperationSelectedMarkers = GetSelectedMarkers();

            isClickPointOnMarkers = true;
            SimpleMarker clickedMarker = ((sender as SimpleMarkerShape).Tag as SimpleMarker);
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                InvertSelect(clickedMarker);
            }
            else
            {
                foreach (var item in GetShowingMarkers())
                {
                    if (item != clickedMarker)
                    {
                        SimpleMarker.DeselectMarker(item);
                    }
                    else
                    {
                        //InvertSelect(clickedMarker);
                        Select(clickedMarker);
                    }
                }
            }

            var afterOperationSelectedMarkers = GetSelectedMarkers();
            ReportSelectionChanged(beforeOperationSelectedMarkers, afterOperationSelectedMarkers);

            //e.Handled = true;
        }

        public void RemoveAllMarkers()
        {
            foreach (var item in GetShowingMarkers())
            {
                //item.Shape.MouseDown -= ShowingMarkerShape_MouseDown;
                item.Shape.MouseUp -= ShowingMarkerShape_MouseUp;
            }

            HeatMapLayers.Remove(SelectedHeatmapLayer);
            gmapControl.Markers.Clear();

            OnMarkersSelectionChanged();
        }

        public void RemoveSelectedMarkers()
        {
            List<SimpleMarker> selectedMarkers = GetSelectedMarkers();
            foreach (var currentSelectedMarker in selectedMarkers)
            {
                //currentSelectedMarker.Shape.MouseDown -= ShowingMarkerShape_MouseDown;
                currentSelectedMarker.Shape.MouseUp -= ShowingMarkerShape_MouseUp;
                gmapControl.Markers.Remove(currentSelectedMarker);
            }

            OnMarkersSelectionChanged();
        }

        public void RemoveMarker(SimpleMarker marker)
        {
            //marker.Shape.MouseDown -= ShowingMarkerShape_MouseDown;
            marker.Shape.MouseUp -= ShowingMarkerShape_MouseUp;
            HeatMapLayers.Remove(SelectedHeatmapLayer);
            Dispatcher.Invoke(() =>
            {
                gmapControl.Markers.Remove(marker);
            });
        }

        private void SetShowMapPointsAndLabelsStatus(LayerShowState status)
        {
            var markers = GetShowingMarkers();

            if (HeatmapStatus == LayerShowState.Hidden || (HeatmapStatus == LayerShowState.Shown && status == LayerShowState.Shown))
            {
                foreach (var m in markers)
                {
                    m.Shape.Visibility = Visibility.Visible;
                }
            }
            else
            {
                foreach (var m in markers)
                {
                    m.Shape.Visibility = Visibility.Hidden;
                }
            }
        }

        private void ShowingMarkerShape_MouseDown(object sender, MouseButtonEventArgs e)
        {

            if (e.ClickCount == 2)
            {
                OnMarkerDoubleClick(((SimpleMarkerShape)sender).RelatedObject);
            }
            else
            {
                //MessageBox.Show("Hello Single");
            }



            /*
            var beforeOperationSelectedMarkers = GetSelectedMarkers();

            isClickPointOnMarkers = true;
            SimpleMarker clickedMarker = ((sender as SimpleMarkerShape).Tag as SimpleMarker);
            if ((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
            {
                InvertSelect(clickedMarker);
            }
            else
            {
                foreach (var item in GetShowingMarkers())
                {
                    if (item != clickedMarker)
                    {
                        SimpleMarker.DeselectMarker(item);
                    }
                    else
                    {
                        //InvertSelect(clickedMarker);
                        Select(clickedMarker);
                    }
                }
            }

            var afterOperationSelectedMarkers = GetSelectedMarkers();
            ReportSelectionChanged(beforeOperationSelectedMarkers, afterOperationSelectedMarkers);

            e.Handled = true;
            */
        }

        public event EventHandler<MarkerDoubleClickEventArgs> MarkerDoubleClick;

        protected virtual void OnMarkerDoubleClick(object marker)
        {
            MarkerDoubleClick?.Invoke(this, new MarkerDoubleClickEventArgs(marker));
        }

        public event EventHandler<PolygonDrawnEventArgs> PolygonDrawn;

        protected virtual void OnPolygonDrawn(PolygonDrawnEventArgs args)
        {
            PolygonDrawn?.Invoke(this, args);
        }

        public event EventHandler<PathDrawnEventArgs> RouteDrawn;
        protected virtual void OnRouteDrawn(PathDrawnEventArgs args, AdvanceRoute drawnRoute)
        {
            RouteDrawn?.Invoke(this, args);
        }

        public event EventHandler<CircleDrawnEventArgs> CircleDrawn;
        protected virtual void OnCircleDrawn(CircleDrawnEventArgs args)
        {
            CircleDrawn?.Invoke(this, args);
        }

        public double DefaultDrawingPathWidthInMeters = 300;

        public event EventHandler<PathDrawnEventArgs> PathDrawn;
        protected virtual void OnPathDrawn(PathDrawnEventArgs args)
        {
            PathDrawn?.Invoke(this, args);
        }

        private void gmapControl_OnMapZoomChanged()
        {

        }

        public void ClearTilesCache()
        {
            // TODO: به جای مدیریت دستی میانگیری در زمان تغییر منابع تصاویر نقشه بهتر است از
            // (GMap.)MapProvider
            // استفاده شود
            gmapControl.Manager.CancelTileCaching();
            gmapControl.Manager.PrimaryCache.DeleteOlderThan(DateTime.Now.AddHours(1), null);
            gmapControl.Manager.MemoryCache.Clear();
            gmapControl.ReloadMap();
        }


        public LayerShowState HeatmapStatus
        {
            get { return (LayerShowState)GetValue(HeatmapStatusProperty); }
            set { SetValue(HeatmapStatusProperty, value); }
        }
        public static readonly DependencyProperty HeatmapStatusProperty =
            DependencyProperty.Register("HeatmapStatus", typeof(LayerShowState), typeof(MapViewer), new PropertyMetadata(LayerShowState.Hidden, new PropertyChangedCallback(OnSetHeatmapStatusChanged)));

        private static void OnSetHeatmapStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MapViewer mv = d as MapViewer;
            mv.OnSetHeatmapStatusChanged(e);
        }

        private void OnSetHeatmapStatusChanged(DependencyPropertyChangedEventArgs e)
        {
            var value = (LayerShowState)e.NewValue;

            if (SelectedHeatmapLayer != null)
                SelectedHeatmapLayer.Status = value;

            SetShowMapPointsAndLabelsStatus(HeatmapShowMapPointsAndLabels);
        }

        public TargetPoints HeatmapTarget
        {
            get { return (TargetPoints)GetValue(HeatmapTargetProperty); }
            set { SetValue(HeatmapTargetProperty, value); }
        }
        public static readonly DependencyProperty HeatmapTargetProperty =
            DependencyProperty.Register("HeatmapTarget", typeof(TargetPoints), typeof(MapViewer), new PropertyMetadata(TargetPoints.AllMarkers, new PropertyChangedCallback(OnSetHeatmapTargetChanged)));

        private static void OnSetHeatmapTargetChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MapViewer mv = d as MapViewer;
            mv.OnSetHeatmapTargetChanged(e);
        }

        private void OnSetHeatmapTargetChanged(DependencyPropertyChangedEventArgs e)
        {
            AddHeatmapLayer(false);
        }

        public PointsValueSourceType HeatmapPointsSource
        {
            get { return (PointsValueSourceType)GetValue(HeatmapPointsSourceProperty); }
            set { SetValue(HeatmapPointsSourceProperty, value); }
        }
        public static readonly DependencyProperty HeatmapPointsSourceProperty =
            DependencyProperty.Register("HeatmapPointsSource", typeof(PointsValueSourceType), typeof(MapViewer), new PropertyMetadata(PointsValueSourceType.MarkersCount, new PropertyChangedCallback(OnSetHeatmapPointsSource)));

        private static void OnSetHeatmapPointsSource(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MapViewer mv = d as MapViewer;
            mv.OnSetHeatmapPointsSource(e);
        }

        private void OnSetHeatmapPointsSource(DependencyPropertyChangedEventArgs e)
        {
            if (SelectedHeatmapLayer != null)
                SelectedHeatmapLayer.PointsSource = (PointsValueSourceType)e.NewValue;

            AddHeatmapLayer(true);
        }

        public double MarkersMaximumWeight
        {
            get { return (double)GetValue(MarkersMaximumWeightProperty); }
            set { SetValue(MarkersMaximumWeightProperty, value); }
        }
        public static readonly DependencyProperty MarkersMaximumWeightProperty =
            DependencyProperty.Register("MarkersMaximumWeight", typeof(double), typeof(MapViewer), new PropertyMetadata(1.0));



        public int HeatmapDensityRadiusInMeters
        {
            get { return (int)GetValue(HeatmapDensityRadiusInMetersProperty); }
            set { SetValue(HeatmapDensityRadiusInMetersProperty, value); }
        }
        public static readonly DependencyProperty HeatmapDensityRadiusInMetersProperty =
            DependencyProperty.Register("HeatmapDensityRadiusInMeters", typeof(int), typeof(MapViewer), new PropertyMetadata(10000, new PropertyChangedCallback(OnSetHeatmapDensityRadiusInMetersChanged)));

        private static void OnSetHeatmapDensityRadiusInMetersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MapViewer mv = d as MapViewer;
            mv.OnSetHeatmapDensityRadiusInMetersChanged(e);
        }

        private void OnSetHeatmapDensityRadiusInMetersChanged(DependencyPropertyChangedEventArgs e)
        {
            if (SelectedHeatmapLayer != null)
            {
                SelectedHeatmapLayer.Radius = (int)e.NewValue;
                HeatmapWarningArealUnitNotMatchRadius = SelectedHeatmapLayer.WarningArealUnitNotMatchRadius;
            }
        }

        public LinearGradientBrush HeatmapColorSpectrum
        {
            get { return (LinearGradientBrush)GetValue(HeatmapColorSpectrumProperty); }
            set
            {
                SetValue(HeatmapColorSpectrumProperty, value);
            }
        }
        public static readonly DependencyProperty HeatmapColorSpectrumProperty =
            DependencyProperty.Register("HeatmapColorSpectrum", typeof(LinearGradientBrush), typeof(MapViewer), new PropertyMetadata(null, new PropertyChangedCallback(OnSetHeatmapColorSpectrumChanged)));

        private static void OnSetHeatmapColorSpectrumChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MapViewer mv = d as MapViewer;
            mv.OnSetHeatmapColorSpectrumChanged(e);
        }

        private void OnSetHeatmapColorSpectrumChanged(DependencyPropertyChangedEventArgs e)
        {
            if (SelectedHeatmapLayer != null)
            {
                SelectedHeatmapLayer.Gradient = (Brush)e.NewValue;
            }
        }

        public double HeatmapOpacity
        {
            get { return (double)GetValue(HeatmapOpacityProperty); }
            set { SetValue(HeatmapOpacityProperty, value); }
        }
        public static readonly DependencyProperty HeatmapOpacityProperty =
            DependencyProperty.Register("HeatmapOpacity", typeof(double), typeof(MapViewer), new PropertyMetadata(0.8, new PropertyChangedCallback(OnSetHeatmapOpacityChanged)));

        private static void OnSetHeatmapOpacityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MapViewer mv = d as MapViewer;
            mv.OnSetHeatmapOpacityChanged(e);
        }

        private void OnSetHeatmapOpacityChanged(DependencyPropertyChangedEventArgs e)
        {
            if (SelectedHeatmapLayer != null)
            {
                SelectedHeatmapLayer.Opacity = (double)e.NewValue;
            }
        }


        public double HeatmapArealUnitInSquareMeters
        {
            get { return (double)GetValue(HeatmapArealUnitInSquareMetersProperty); }
            set { SetValue(HeatmapArealUnitInSquareMetersProperty, value); }
        }
        public static readonly DependencyProperty HeatmapArealUnitInSquareMetersProperty =
            DependencyProperty.Register("HeatmapArealUnitInSquareMeters", typeof(double), typeof(MapViewer), new PropertyMetadata(1000000.0, new PropertyChangedCallback(OnSetHeatmapArealUnitInSquareMetersChanged)));

        private static void OnSetHeatmapArealUnitInSquareMetersChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MapViewer mv = d as MapViewer;
            mv.OnSetHeatmapArealUnitInSquareMetersChanged(e);
        }

        private void OnSetHeatmapArealUnitInSquareMetersChanged(DependencyPropertyChangedEventArgs e)
        {
            if (SelectedHeatmapLayer != null)
            {
                SelectedHeatmapLayer.ArealUnitAreaInSquareMeters = (double)e.NewValue;
                HeatmapWarningArealUnitNotMatchRadius = SelectedHeatmapLayer.WarningArealUnitNotMatchRadius;

                if (HeatmapPointsSource == PointsValueSourceType.MarkersCount)
                {
                    HeatmapPointsCount = SelectedHeatmapLayer.Points.Count;
                    HeatmapLeastDensity = SelectedHeatmapLayer.MinIntensityCount;
                    HeatmapMostDensity = SelectedHeatmapLayer.MaxIntensityCount;
                }
                else
                {
                    HeatmapPointsCount = SelectedHeatmapLayer.Points.Count;
                    HeatmapLeastDensity = SelectedHeatmapLayer.MinIntensityWeight;
                    HeatmapMostDensity = SelectedHeatmapLayer.MaxIntensityWeight;
                }
            }
        }

        public long HeatmapPointsCount
        {
            get { return (long)GetValue(HeatmapPointsCountProperty); }
            set { SetValue(HeatmapPointsCountProperty, value); }
        }

        public static readonly DependencyProperty HeatmapPointsCountProperty =
            DependencyProperty.Register("HeatmapPointsCount", typeof(long), typeof(MapViewer));



        public bool HeatmapWarningArealUnitNotMatchRadius
        {
            get { return (bool)GetValue(HeatmapWarningArealUnitNotMatchRadiusProperty); }
            set { SetValue(HeatmapWarningArealUnitNotMatchRadiusProperty, value); }
        }

        public static readonly DependencyProperty HeatmapWarningArealUnitNotMatchRadiusProperty =
            DependencyProperty.Register("HeatmapWarningArealUnitNotMatchRadius", typeof(bool), typeof(MapViewer), new PropertyMetadata(false));



        public double HeatmapLeastDensity
        {
            get { return (double)GetValue(HeatmapLeastDensityProperty); }
            set { SetValue(HeatmapLeastDensityProperty, value); }
        }

        public static readonly DependencyProperty HeatmapLeastDensityProperty =
            DependencyProperty.Register("HeatmapLeastDensity", typeof(double), typeof(MapViewer), new PropertyMetadata(0.0));



        public double HeatmapMostDensity
        {
            get { return (double)GetValue(HeatmapMostDensityProperty); }
            set { SetValue(HeatmapMostDensityProperty, value); }
        }

        public static readonly DependencyProperty HeatmapMostDensityProperty =
            DependencyProperty.Register("HeatmapMostDensity", typeof(double), typeof(MapViewer), new PropertyMetadata(0.0));

        public double HeatmapProgressPercent
        {
            get { return (double)GetValue(HeatmapProgressPercentProperty); }
            set { SetValue(HeatmapProgressPercentProperty, value); }
        }

        public static readonly DependencyProperty HeatmapProgressPercentProperty =
            DependencyProperty.Register("HeatmapProgressPercent", typeof(double), typeof(MapViewer), new PropertyMetadata(0.0));

        public LayerShowState HeatmapShowMapPointsAndLabels
        {
            get { return (LayerShowState)GetValue(HeatmapShowMapPointsAndLabelsProperty); }
            set { SetValue(HeatmapShowMapPointsAndLabelsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeatmapShowMapPointsAndLabels.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeatmapShowMapPointsAndLabelsProperty =
            DependencyProperty.Register("HeatmapShowMapPointsAndLabels", typeof(LayerShowState), typeof(MapViewer), 
                new PropertyMetadata(LayerShowState.Shown, new PropertyChangedCallback(OnSetHeatmapShowMapPointsAndLabelsChanged)));

        private static void OnSetHeatmapShowMapPointsAndLabelsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MapViewer mv = d as MapViewer;
            mv.OnSetHeatmapShowMapPointsAndLabelsChanged(e);
        }

        private void OnSetHeatmapShowMapPointsAndLabelsChanged(DependencyPropertyChangedEventArgs e)
        {
            SetShowMapPointsAndLabelsStatus((LayerShowState)e.NewValue);
        }


        public PointLatLng Position
        {
            get { return (PointLatLng)GetValue(PositionProperty); }
            set { SetValue(PositionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Position.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PositionProperty =
            DependencyProperty.Register(nameof(Position), typeof(PointLatLng), typeof(MapViewer),
                new PropertyMetadata(new PointLatLng(0, 0), OnSetPositionChanged));

        private static void OnSetPositionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((MapViewer)d).OnSetPositionChanged(e);
        }

        private void OnSetPositionChanged(DependencyPropertyChangedEventArgs e)
        {
            gmapControl.Position = Position;
        }

        private void GmapControl_OnPositionChanged(PointLatLng point)
        {
            Position = point;
        }

        /// <summary>
        /// مقدار عددی نقطه مشخص شده در تصویر هیت مپ 
        /// </summary>
        public double HeatmapHintedPixel
        {
            get { return (double)GetValue(HeatmapHintedPixelProperty); }
            set { SetValue(HeatmapHintedPixelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeatmapHintedPixel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeatmapHintedPixelProperty =
            DependencyProperty.Register("HeatmapHintedPixel", typeof(double), typeof(MapViewer), new PropertyMetadata(0.0));

        public void Reset()
        {
            gmapControl.Zoom = 2;
            gmapControl.Position = new PointLatLng(0, 0);
            HeatmapStatus = LayerShowState.Hidden;
            HeatmapTarget = TargetPoints.AllMarkers;
            HeatmapPointsSource = PointsValueSourceType.MarkersCount;
            HeatmapDensityRadiusInMeters = 10000;
            HeatmapArealUnitInSquareMeters = 1000000.0;
            HeatmapColorSpectrum = null;
            HeatmapOpacity = .8;
            HeatmapShowMapPointsAndLabels = LayerShowState.Shown;
        }
    }
}