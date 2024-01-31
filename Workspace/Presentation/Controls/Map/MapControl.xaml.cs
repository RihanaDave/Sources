using GMap.NET;
using GPAS.Dispatch.Entities.Concepts.Geo;
using GPAS.MapViewer;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Entities.Geo;
using GPAS.Workspace.Entities.Investigation;
using GPAS.Workspace.Presentation.Controls.Map;
using GPAS.Workspace.Presentation.Controls.Map.GeoSearch;
using GPAS.Workspace.Presentation.Controls.Map.Heatmap;
using Notifications.Wpf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using GPAS.PropertiesValidation;

namespace GPAS.Workspace.Presentation.Controls
{
    public class LayerShowStateMapControlToLayerShowStateMapViewerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((LayerShowState)value)
            {
                case LayerShowState.Hidden:
                    return MapViewer.Heatmap.LayerShowState.Hidden;
                case LayerShowState.Shown:
                default:
                    return MapViewer.Heatmap.LayerShowState.Shown;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((MapViewer.Heatmap.LayerShowState)value)
            {
                case MapViewer.Heatmap.LayerShowState.Hidden:
                    return LayerShowState.Hidden;
                case MapViewer.Heatmap.LayerShowState.Shown:
                default:
                    return LayerShowState.Shown;
            }
        }
    }

    public class TargetPointsMapControlToTargetPointsMapViewerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((TargetPoints)value)
            {
                case TargetPoints.SelectedDataPoints:
                    return MapViewer.Heatmap.TargetPoints.SelectedMarkers;
                case TargetPoints.AllDataPoints:
                default:
                    return MapViewer.Heatmap.TargetPoints.AllMarkers;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((MapViewer.Heatmap.TargetPoints)value)
            {
                case MapViewer.Heatmap.TargetPoints.SelectedMarkers:
                    return TargetPoints.SelectedDataPoints;
                case MapViewer.Heatmap.TargetPoints.AllMarkers:
                default:
                    return TargetPoints.AllDataPoints;
            }
        }
    }

    public class PointsValueSourceTypeMapControlToPointsValueSourceTypeMapViewerConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((PointsValueSourceType)value)
            {
                case PointsValueSourceType.ValueOfSelectedProperty:
                    return MapViewer.Heatmap.PointsValueSourceType.MarkersWeight;
                case PointsValueSourceType.ObjectsCount:
                default:
                    return MapViewer.Heatmap.PointsValueSourceType.MarkersCount;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((MapViewer.Heatmap.PointsValueSourceType)value)
            {
                case MapViewer.Heatmap.PointsValueSourceType.MarkersWeight:
                    return PointsValueSourceType.ValueOfSelectedProperty;
                case MapViewer.Heatmap.PointsValueSourceType.MarkersCount:
                default:
                    return PointsValueSourceType.ObjectsCount;
            }
        }
    }

    /// <summary>
    /// Interaction logic for MapControl.xaml
    /// </summary>
    public partial class MapControl
    {
        private readonly NotificationManager NotificationManager = new NotificationManager();
        public event EventHandler<ObjectsSelectionChangedArgs> ObjectsSelectionChanged;
        protected void OnMarkersSelectionChanged(IEnumerable<KWObject> CurrentlySelectedObjects)
        {
            if (CurrentlySelectedObjects == null)
                throw new ArgumentNullException("CurrentlySelectedObjects");

            if (ObjectsSelectionChanged != null)
                ObjectsSelectionChanged(this, new ObjectsSelectionChangedArgs(CurrentlySelectedObjects));
        }

        public event EventHandler<SimpleMarkerDoubleClickEventArgs> SimpleMarkerDoubleClick;

        protected virtual void OnSimpleMarkerDoubleClick(object simpleMarker)
        {
            if (simpleMarker == null)
            {
                throw new ArgumentException(nameof(simpleMarker));
            }

            SimpleMarkerDoubleClick?.Invoke(this, new SimpleMarkerDoubleClickEventArgs((KWObject)simpleMarker));
        }

        public event EventHandler ObjectsAdded;
#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        protected async Task OnObjectsAdded()
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            ObjectsAdded?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ObjectsRemoved;
#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        protected async Task OnObjectsRemoved()
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            ObjectsRemoved?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler<Windows.SnapshotRequestEventArgs> SnapshotRequested;
        public void OnSnapshotRequested(Windows.SnapshotRequestEventArgs args)
        {
            SnapshotRequested?.Invoke(this, args);
        }

        public MapControl()
        {
            InitializeComponent();

            DataContext = this;

            mainMapViewer.MarkersSelectionChanged += MainMapViewer_MarkersSelectionChanged;
            mainMapViewer.MarkerDoubleClick += MainMapViewer_MarkerDoubleClick;
            mainMapViewer.MapTileLoadStarted += (sender, e) => { OnMapTileLoadStarted(); };
            mainMapViewer.MapTileLoadCompleted += (sender, e) => { OnMapTileLoadCompleted(); };
            mainMapViewer.MapTileLoadAborted += (sender, e) => { OnMapTileLoadAborted(); };
        }

        private void MainMapViewer_MarkerDoubleClick(object sender, MarkerDoubleClickEventArgs e)
        {
            OnSimpleMarkerDoubleClick(e.Marker);
        }

        private void MainMapViewer_MarkersSelectionChanged(object sender, MapViewer.MapViewer.MarkersSelectionChangedArgs e)
        {
            var selectedKWObjects = new HashSet<KWObject>();
            foreach (SimpleMarker selectedMarker in e.CurrentlySelectedMarkers)
            {
                KWObject markerOwner = ShowingObjectsPerMarker[selectedMarker];
                if (!selectedKWObjects.Contains(markerOwner))
                    selectedKWObjects.Add(markerOwner);
            }

            if (mainMapViewer.HasChangedSimpleMarkers(e.CurrentlySelectedMarkers.ToList()))
            {
                AddHeatmapLayer();
            }

            OnMarkersSelectionChanged(selectedKWObjects);
        }

        Dictionary<KWObject, List<SimpleMarker>> ShowingMarkersPerObject = new Dictionary<KWObject, List<SimpleMarker>>();
        Dictionary<SimpleMarker, KWObject> ShowingObjectsPerMarker = new Dictionary<SimpleMarker, KWObject>();

        public void ShowNotification(NotificationContent notificationContent)
        {
            NotificationManager.Show(notificationContent, "WindowArea");
        }

        public async Task ShowObject(KWObject objectToShow)
        {
            if (objectToShow == null)
                throw new ArgumentNullException("objectToShow");

            await ShowObjects(new KWObject[] { objectToShow });
        }

        public async Task ShowObjects(IEnumerable<KWObject> objectsToShow)
        {
            if (objectsToShow == null)
                throw new ArgumentNullException("objectsToShow");

            mainMapViewer.DeselectAllMarkers();
            var beforeOperationSelectedMarkers = GetSelectedMarkers();
            WaitingControl.Message = Properties.Resources.Loading_objects_geo_loacations;
            WaitingControl.TaskIncrement();
            Dictionary<KWObject, HashSet<GeoLocationEntity>> objectPositions = await Logic.Geo.GetGeoDataForObjectAsync(objectsToShow);

            if (objectPositions.Count < objectsToShow.Count() && objectsToShow.Count() != 0)
            {
                NotificationContent notificationContent = new NotificationContent()
                {
                    Title = Properties.Resources.Map_Application,
                    Message = string.Format(Properties.Resources.The_Number_Of_Objects_With_GeoTime_Property_Is_Less_Than_Objects_Retrieved, objectsToShow.Count(), objectPositions.Count)
                };

                ShowNotification(notificationContent);
            }
            WaitingControl.TaskDecrement();

            WaitingControl.Message = Properties.Resources.Showing_objects_on_map_application;
            WaitingControl.TaskIncrement();
            await Task.Factory.StartNew(() =>
            {
                List<SimpleMarker> markersToShow = new List<SimpleMarker>();
                foreach (var objPos in objectPositions)
                {
                    KWObject showingObject = objPos.Key;
                    if (ShowingMarkersPerObject.ContainsKey(showingObject))
                        RemoveMarkersOfObject(showingObject);
                    HashSet<GeoLocationEntity> positions = objPos.Value;

                    foreach (var pos in positions)
                    {
                        if (pos.Equals(GeoLocationEntity.Empty))
                            continue;
                        SimpleMarker marker = null;
                        Dispatcher.Invoke(() =>
                        {
                            marker = new SimpleMarker(ConvertGlobalPositionToGMapPosition(pos));
                            ((SimpleMarkerShape)marker.Shape).RelatedObject = showingObject;

                            if (!string.IsNullOrWhiteSpace(showingObject.GetObjectLabel()))
                            {
                                ((SimpleMarkerShape)marker.Shape).ToolTip = showingObject.GetObjectLabel();
                            }
                        });
                        AddNewObjectPositionToDictionaries(marker, showingObject, ref ShowingMarkersPerObject, ref ShowingObjectsPerMarker);
                        markersToShow.Add(marker);
                        //mainMapViewer.ShowMarker(marker, showingObject.GetObjectLabel());
                    }
                }
                Dispatcher.Invoke(() =>
                {
                    mainMapViewer.ShowMarkers(markersToShow);
                });
            });
            WaitingControl.TaskDecrement();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
            OnObjectsAdded();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
            //mainMapViewer.ShowMarkers(markersToShow);

            List<SimpleMarker> markers;
            if (HeatmapTarget == TargetPoints.AllDataPoints)
            {
                markers = mainMapViewer.GetShowingMarkers().ToList();
            }
            else
            {
                markers = mainMapViewer.GetSelectedMarkers();
            }

            if (mainMapViewer.HasChangedSimpleMarkers(markers))
            {
                AddHeatmapLayer();
            }

            var afterOperationSelectedMarkers = GetSelectedMarkers();
            ReportSelectionChanged(beforeOperationSelectedMarkers, afterOperationSelectedMarkers);
        }

        public void TakeSnapshot()
        {
            OnSnapshotRequested(new Windows.SnapshotRequestEventArgs(mainGrid, $"Map Application {DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss")}"));
        }

        private void ReportSelectionChanged(List<SimpleMarker> beforeOperationSelectedMarkers, List<SimpleMarker> afterOperationSelectedMarkers)
        {
            mainMapViewer.ReportSelectionChanged(beforeOperationSelectedMarkers, afterOperationSelectedMarkers);
        }

        private static void AddNewObjectPositionToDictionaries
            (SimpleMarker marker, KWObject positionOwner
            , ref Dictionary<KWObject, List<SimpleMarker>> mappingDictionary
            , ref Dictionary<SimpleMarker, KWObject> reverseMappingDictionary)
        {
            if (!mappingDictionary.ContainsKey(positionOwner))
            {
                mappingDictionary.Add(positionOwner, new List<SimpleMarker>());
            }
            //if (!mappingDictionary[positionOwner].Contains(marker))
            //{
            mappingDictionary[positionOwner].Add(marker);
            //}
            reverseMappingDictionary.Add(marker, positionOwner);
        }

        internal bool IsAnyObjectShown()
        {
            return ShowingMarkersPerObject.Keys.Any();
        }

        internal void DrawPolygon()
        {
            // TODO: روش فراخوانی طبق قرارداد فنی اصلاح شود
            //mainMapViewer.StartDrawingShape(DrawingShape.Polygon);
            mainMapViewer.ShapeType = DrawingShape.Polygon;
        }
        internal void DrawCircle()
        {
            // TODO: روش فراخوانی طبق قرارداد فنی اصلاح شود
            //mainMapViewer.StartDrawingShape(DrawingShape.Polygon);
            mainMapViewer.ShapeType = DrawingShape.Circle;
        }
        internal void DrawRoute()
        {
            // TODO: روش فراخوانی طبق قرارداد فنی اصلاح شود
            //mainMapViewer.StartDrawingShape(DrawingShape.Polygon);
            mainMapViewer.ShapeType = DrawingShape.Route;
        }

        internal void ClearDrawing()
        {
            // TODO: روش فراخوانی طبق قرارداد فنی اصلاح شود
            //mainMapViewer.ClearDrawnShape();
            mainMapViewer.ShapeType = DrawingShape.None;
            if (mainMapViewer.SelectedAdvanceShape != null)
            {
                mainMapViewer.SelectedAdvanceShape?.Delete();
                mainMapViewer.SelectedAdvanceShape = null;
            }
        }

        public void SelectAllObjects()
        {
            mainMapViewer.SelectAllMarkers();
        }

        private List<KWObject> GetObjectsFromSimpleMarkers(List<SimpleMarker> simpleMarkers)
        {
            List<KWObject> kWObjects = new List<KWObject>();
            foreach (var currentMarker in simpleMarkers)
            {
                if (ShowingObjectsPerMarker.ContainsKey(currentMarker))
                {
                    KWObject relatedKWObject = null;
                    ShowingObjectsPerMarker.TryGetValue(currentMarker, out relatedKWObject);
                    if (relatedKWObject != null && !kWObjects.Contains(relatedKWObject))
                    {
                        kWObjects.Add(relatedKWObject);
                    }
                }
            }

            return kWObjects;
        }

        internal List<KWObject> GetSelectedObjects()
        {
            List<SimpleMarker> selectedMarkers = mainMapViewer.GetSelectedMarkers();
            return GetObjectsFromSimpleMarkers(selectedMarkers);
        }

        internal List<KWObject> GetShowingObjects()
        {
            List<SimpleMarker> showingMarkers = mainMapViewer.GetShowingMarkers().ToList();
            return GetObjectsFromSimpleMarkers(showingMarkers);
        }

        internal async Task SetMarkersWeight()
        {
            List<KWObject> KWObjectList = null;
            if (HeatmapTarget == TargetPoints.AllDataPoints)
            {
                KWObjectList = GetShowingObjects();
            }
            else
            {
                KWObjectList = GetSelectedObjects();
            }

            Dictionary<KWObject, double> objectsWeights = new Dictionary<KWObject, double>(KWObjectList.Count);
            foreach (KWObject obj in KWObjectList)
            {
                objectsWeights.Add(obj, 0);
            }

            if (HeatmapPointsSource == PointsValueSourceType.ValueOfSelectedProperty)
            {
                var weightSourceProperties = await Logic.PropertyManager.GetPropertiesOfObjectsAsync(KWObjectList, new string[] { HeatmapPropertyBasedPointsSelectedTypeUri });
                foreach (KWProperty prop in weightSourceProperties)
                {
                    if (!objectsWeights.ContainsKey(prop.Owner))
                        continue;

                    Ontology.BaseDataTypes propBaseType = Logic.OntologyProvider.GetBaseDataTypeOfProperty(prop.TypeURI);
                    object propValueObject;
                    if (PropertiesValidation.ValueBaseValidation.TryParsePropertyValue(propBaseType, prop.Value, out propValueObject).Status == PropertiesValidation.ValidationStatus.Invalid)
                        continue;

                    double propValue = 0;
                    double.TryParse(propValueObject.ToString(), out propValue);

                    if (propValue > objectsWeights[prop.Owner]) //set maximum value
                        objectsWeights[prop.Owner] = propValue;
                }
            }

            foreach (var item in objectsWeights)
            {
                foreach (var m in ShowingMarkersPerObject[item.Key])
                {
                    m.Weight = item.Value;
                }
            }

            List<SimpleMarker> markers;
            if (HeatmapTarget == TargetPoints.AllDataPoints)
            {
                markers = mainMapViewer.GetShowingMarkers().ToList();
            }
            else if (HeatmapTarget == TargetPoints.SelectedDataPoints)
            {
                markers = mainMapViewer.GetSelectedMarkers();
            }
            else
            {
                throw new NotImplementedException();
            }

            if (mainMapViewer.HasChangedSimpleMarkers(markers))
            {
                AddHeatmapLayer();
            }
        }

        internal List<SimpleMarker> GetSelectedMarkers()
        {
            return mainMapViewer.GetSelectedMarkers();
        }

        internal void SelectObjects(IEnumerable<KWObject> objectsToSelect, bool deselectAllBeforeSelection = true)
        {
            if (deselectAllBeforeSelection)
            {
                mainMapViewer.DeselectAllMarkers();
            }
            List<SimpleMarker> markersToSelect = new List<SimpleMarker>(objectsToSelect.Count());
            foreach (KWObject obj in objectsToSelect)
            {
                markersToSelect.AddRange(ShowingMarkersPerObject[obj]);
            }
            mainMapViewer.SelectMarkers(markersToSelect);
        }

        public void DeselectAllObjects()
        {
            mainMapViewer.DeselectAllMarkers();
        }

        public void RemoveMarkersOfObjects(IEnumerable<KWObject> objectsToRemove)
        {
            foreach (var objectToRemove in objectsToRemove)
            {
                RemoveMarkersOfObject(objectToRemove);
            }
        }

        public void RemoveMarkers(IEnumerable<KWProperty> properties)
        {
            foreach (var property in properties)
            {
                List<SimpleMarker> objectMarkers;
                if (ShowingMarkersPerObject.TryGetValue(property.Owner, out objectMarkers))
                {
                    foreach (var marker in from marker in objectMarkers
                        let geoTimeEntityRawData = GeoTime.GeoTimeEntityRawData(property.Value)
                        where marker.Position.Lat.ToString(CultureInfo.InvariantCulture) ==
                              geoTimeEntityRawData.Latitude &&
                              marker.Position.Lng.ToString(CultureInfo.InvariantCulture) ==
                              geoTimeEntityRawData.Longitude
                        select marker)
                    {
                        mainMapViewer.RemoveMarker(marker);
                        ShowingObjectsPerMarker.Remove(marker);
                    }
                }
            }
        }

        public void InvertSelectionObjects()
        {
            mainMapViewer.InvertSelectionMarkers();
        }

        private void RemoveMarkersOfObject(KWObject obj)
        {
            List<SimpleMarker> objectMarkers;
            if (ShowingMarkersPerObject.TryGetValue(obj, out objectMarkers))
            {
                foreach (SimpleMarker marker in objectMarkers)
                {
                    mainMapViewer.RemoveMarker(marker);
                    ShowingObjectsPerMarker.Remove(marker);
                }

                ShowingMarkersPerObject.Remove(obj);
            }
        }

        private PointLatLng ConvertGlobalPositionToGMapPosition(GeoLocationEntity value)
        {
            return new PointLatLng(value.Latitude, value.Longitude);
        }

        /// <summary>
        /// تمام اشیا در حال نمایش روی نقشه را حذف می‌کند
        /// </summary>
        public void ClearMapContent()
        {
            mainMapViewer.RemoveAllMarkers();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
            OnObjectsRemoved();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.

            ShowingMarkersPerObject.Clear();
            ShowingObjectsPerMarker.Clear();
        }

        public void RemoveSelectedMarkers()
        {
            List<KWObject> selectedObjects = GetSelectedObjects();
            List<SimpleMarker> selectedMarkers = GetSelectedMarkers();

            foreach (var currentObject in selectedObjects)
            {
                ShowingMarkersPerObject.Remove(currentObject);
            }
            foreach (var currentMarker in selectedMarkers)
            {
                ShowingObjectsPerMarker.Remove(currentMarker);
            }

            mainMapViewer.RemoveSelectedMarkers();

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
            OnObjectsRemoved();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
        }

        private void mainMapViewer_MapTileImageNeeded(object sender, MapTileImageNeededEventArgs e)
        {
            e.TileImage = GetMapTileImageFromRemoteService(e.ZoomLevel, ConvertGPointToMapTilePosition(e.TilePosition));
        }

        private byte[] GetMapTileImageFromRemoteService(int zoomLevel, MapTilePosition mapTilePosition)
        {
            if (!string.IsNullOrWhiteSpace(SelectedMapTileSource))
            {
                return Logic.Geo.GetMapTileImage(SelectedMapTileSource, zoomLevel, mapTilePosition);
            }
            else
            {
                return null;
            }
        }

        private MapTilePosition ConvertGPointToMapTilePosition(GPoint tilePosition)
        {
            return new MapTilePosition() { X = tilePosition.X, Y = tilePosition.Y };
        }

        public void AddHeatmapLayer()
        {
            mainMapViewer.AddHeatmapLayer(false);
        }

        public PolygonDrawnEventArgs LastPolygonDrawnEventArgs { get; set; }
        private void mainMapViewer_PolygonDrawn(object sender, PolygonDrawnEventArgs e)
        {
            LastPolygonDrawnEventArgs = e;
        }
        public CircleDrawnEventArgs LastCircleDrawnEventArgs { get; set; }
        private void mainMapViewer_CircleDrawn(object sender, CircleDrawnEventArgs e)
        {
            LastCircleDrawnEventArgs = e;
            //if (mainMapViewer.SelectedAdvanceShape == null &&
            //    mainMapViewer.SelectedAdvanceShape.DrawMode == DrawingMode.End)
            //{

            //}
        }
        public PathDrawnEventArgs LastPathDrawnEventArgs { get; set; }

        private void mainMapViewer_RouteDrawn(object sender, PathDrawnEventArgs e)
        {
            LastPathDrawnEventArgs = e;
        }

        internal void SetBufferSizeForSelectedRouteShape(double buffer, Scales scale)
        {
            if (scale == Scales.m)
            {
                mainMapViewer.SetBufferSizeForSelectedRouteShape(buffer);
            }
            else if (scale == Scales.km)
            {
                mainMapViewer.SetBufferSizeForSelectedRouteShape(buffer * 1000);
            }


        }

        private void PresentationControl_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        public string SelectedMapTileSource { get; private set; }

        private string[] MapTileImageSources = null;

        internal string[] GetMapTileImageSources()
        {
            if (MapTileImageSources != null)
            {
                return MapTileImageSources;
            }
            else
            {
                MapTileImageSources = Logic.Geo.GetMapTileSources();
                if (MapTileImageSources == null || MapTileImageSources.Length == 0)
                    throw new InvalidOperationException(Properties.Resources.No_map_tile_source_defined);
                SelectedMapTileSource = MapTileImageSources[0];
                return MapTileImageSources;
            }
        }

        internal void ChangeMapTileSource(string tileSource)
        {
            SelectedMapTileSource = tileSource;
            mainMapViewer.ClearTilesCache();
        }

        public event EventHandler MapTileLoadAborted;
        protected void OnMapTileLoadAborted()
        {
            MapTileLoadAborted?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler MapTileLoadCompleted;
        private void OnMapTileLoadCompleted()
        {
            MapTileLoadCompleted?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler MapTileLoadStarted;
        private void OnMapTileLoadStarted()
        {
            MapTileLoadStarted?.Invoke(this, EventArgs.Empty);
        }

        public LayerShowState HeatmapStatus
        {
            get { return (LayerShowState)GetValue(HeatmapStatusProperty); }
            set { SetValue(HeatmapStatusProperty, value); }
        }
        public static readonly DependencyProperty HeatmapStatusProperty =
            DependencyProperty.Register("HeatmapStatus", typeof(LayerShowState), typeof(MapControl), new PropertyMetadata(LayerShowState.Hidden, OnSetHeatmapStatusChanged));

        private static void OnSetHeatmapStatusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MapControl mc = d as MapControl;
            mc.OnSetHeatmapStatusChanged(e);
        }

        private void OnSetHeatmapStatusChanged(DependencyPropertyChangedEventArgs e)
        {
            SetShowHeatmapScaleOnMap(HeatmapShowScaleOnMap);
        }

        public TargetPoints HeatmapTarget
        {
            get { return (TargetPoints)GetValue(HeatmapTargetProperty); }
            set { SetValue(HeatmapTargetProperty, value); }
        }
        public static readonly DependencyProperty HeatmapTargetProperty =
            DependencyProperty.Register("HeatmapTarget", typeof(TargetPoints), typeof(MapControl), new PropertyMetadata(TargetPoints.AllDataPoints));


        public PointsValueSourceType HeatmapPointsSource
        {
            get { return (PointsValueSourceType)GetValue(HeatmapPointsSourceProperty); }
            set { SetValue(HeatmapPointsSourceProperty, value); }
        }
        public static readonly DependencyProperty HeatmapPointsSourceProperty =
            DependencyProperty.Register("HeatmapPointsSource", typeof(PointsValueSourceType), typeof(MapControl), new PropertyMetadata(PointsValueSourceType.ObjectsCount));


        public double HeatmapPropertyBasedPointsMaximumValue
        {
            get { return (double)GetValue(HeatmapPropertyBasedPointsMaximumValueProperty); }
            set { SetValue(HeatmapPropertyBasedPointsMaximumValueProperty, value); }
        }
        public static readonly DependencyProperty HeatmapPropertyBasedPointsMaximumValueProperty =
            DependencyProperty.Register("MarkersMaximumWeight", typeof(double), typeof(MapControl), new PropertyMetadata(1.0));


        public string HeatmapPropertyBasedPointsSelectedTypeUri
        {
            get { return (string)GetValue(HeatmapPropertyBasedPointsSelectedTypeUriProperty); }
            set { SetValue(HeatmapPropertyBasedPointsSelectedTypeUriProperty, value); }
        }
        public static readonly DependencyProperty HeatmapPropertyBasedPointsSelectedTypeUriProperty =
            DependencyProperty.Register("HeatmapPropertyBasedPointsSelectedTypeUri", typeof(string), typeof(MapControl), new PropertyMetadata(string.Empty, OnSetHeatmapPropertyBasedPointsSelectedTypeUriChanged));

        private static void OnSetHeatmapPropertyBasedPointsSelectedTypeUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MapControl mc = d as MapControl;
            mc.OnSetHeatmapPropertyBasedPointsSelectedTypeUriChange(e);
        }

        private async void OnSetHeatmapPropertyBasedPointsSelectedTypeUriChange(DependencyPropertyChangedEventArgs e)
        {
            await SetMarkersWeight();
        }

        public int HeatmapDensityRadiusInMeters
        {
            get { return (int)GetValue(HeatmapDensityRadiusInMetersProperty); }
            set { SetValue(HeatmapDensityRadiusInMetersProperty, value); }
        }
        public static readonly DependencyProperty HeatmapDensityRadiusInMetersProperty =
            DependencyProperty.Register("HeatmapDensityRadiusInMeters", typeof(int), typeof(MapControl), new PropertyMetadata(10000));

        public LinearGradientBrush HeatmapColorSpectrum
        {
            get { return (LinearGradientBrush)GetValue(HeatmapColorSpectrumProperty); }
            set { SetValue(HeatmapColorSpectrumProperty, value); }
        }
        public static readonly DependencyProperty HeatmapColorSpectrumProperty =
            DependencyProperty.Register("HeatmapColorSpectrum", typeof(LinearGradientBrush), typeof(MapControl), new PropertyMetadata());

        public double HeatmapOpacity
        {
            get { return (double)GetValue(HeatmapOpacityProperty); }
            set { SetValue(HeatmapOpacityProperty, value); }
        }
        public static readonly DependencyProperty HeatmapOpacityProperty =
            DependencyProperty.Register("HeatmapOpacity", typeof(double), typeof(MapControl), new PropertyMetadata(0.8));

        public double HeatmapArealUnitInSquareMeters
        {
            get { return (double)GetValue(HeatmapArealUnitInSquareMetersProperty); }
            set { SetValue(HeatmapArealUnitInSquareMetersProperty, value); }
        }
        public static readonly DependencyProperty HeatmapArealUnitInSquareMetersProperty =
            DependencyProperty.Register("HeatmapArealUnitInSquareMeters", typeof(double), typeof(MapControl), new PropertyMetadata(1000000.0));


        public long HeatmapPointsCount
        {
            get { return (long)GetValue(HeatmapPointsCountProperty); }
            set { SetValue(HeatmapPointsCountProperty, value); }
        }

        public static readonly DependencyProperty HeatmapPointsCountProperty =
            DependencyProperty.Register("HeatmapPointsCount", typeof(long), typeof(MapControl), new PropertyMetadata((long)0, OnSetHeatmapPointsCountChanged));

        private static void OnSetHeatmapPointsCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MapControl mc = d as MapControl;
            mc.OnSetHeatmapPointsCountChanged(e);
        }

        private void OnSetHeatmapPointsCountChanged(DependencyPropertyChangedEventArgs e)
        {
            sscSpectrum.ValueMode = (long)e.NewValue > 0;

            if ((long)e.NewValue == 0)
            {
                sscSpectrum.TickVisibility = Visibility.Collapsed;
            }
        }

        public bool HeatmapWarningArealUnitNotMatchRadius
        {
            get { return (bool)GetValue(HeatmapWarningArealUnitNotMatchRadiusProperty); }
            set { SetValue(HeatmapWarningArealUnitNotMatchRadiusProperty, value); }
        }

        public static readonly DependencyProperty HeatmapWarningArealUnitNotMatchRadiusProperty =
            DependencyProperty.Register("HeatmapWarningArealUnitNotMatchRadius", typeof(bool), typeof(MapControl), new PropertyMetadata(false));



        public double HeatmapLeastDensity
        {
            get { return (double)GetValue(HeatmapLeastDensityProperty); }
            set { SetValue(HeatmapLeastDensityProperty, value); }
        }

        public static readonly DependencyProperty HeatmapLeastDensityProperty =
            DependencyProperty.Register("HeatmapLeastDensity", typeof(double), typeof(MapControl), new PropertyMetadata(0.0));



        public double HeatmapMostDensity
        {
            get { return (double)GetValue(HeatmapMostDensityProperty); }
            set { SetValue(HeatmapMostDensityProperty, value); }
        }

        public static readonly DependencyProperty HeatmapMostDensityProperty =
            DependencyProperty.Register("HeatmapMostDensity", typeof(double), typeof(MapControl), new PropertyMetadata(0.0));

        public double HeatmapProgressPercent
        {
            get { return (double)GetValue(HeatmapProgressPercentProperty); }
            set { SetValue(HeatmapProgressPercentProperty, value); }
        }

        public static readonly DependencyProperty HeatmapProgressPercentProperty =
            DependencyProperty.Register("HeatmapProgressPercent", typeof(double), typeof(MapControl), new PropertyMetadata(0.0));


        public LayerShowState HeatmapShowMapPointsAndLabels
        {
            get { return (LayerShowState)GetValue(HeatmapShowMapPointsAndLabelsProperty); }
            set { SetValue(HeatmapShowMapPointsAndLabelsProperty, value); }
        }
        public static readonly DependencyProperty HeatmapShowMapPointsAndLabelsProperty =
            DependencyProperty.Register("HeatmapShowMapPointsAndLabels", typeof(LayerShowState), typeof(MapControl), new PropertyMetadata(LayerShowState.Shown));


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
            DependencyProperty.Register("HeatmapHintedPixel", typeof(double), typeof(MapControl), new PropertyMetadata(0.0, OnSetHeatmapHintedPixelChanged));

        private static void OnSetHeatmapHintedPixelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MapControl mc = d as MapControl;
            mc.OnSetHeatmapHintedPixelChanged(e);
        }

        private void OnSetHeatmapHintedPixelChanged(DependencyPropertyChangedEventArgs e)
        {
            double val = (double)e.NewValue;

            double rVal = Math.Round(val);

            if (rVal < 10)
            {
                sscSpectrum.Value = Math.Round(val, 10);
            }
            else if (rVal < 1000)
            {
                sscSpectrum.Value = Math.Round(val, 5);
            }
            else if (rVal < 100000)
            {
                sscSpectrum.Value = Math.Round(val, 2);
            }
            else
            {
                sscSpectrum.Value = Math.Round(val, 1);
            }

            if (val > 0)
            {
                sscSpectrum.TickVisibility = Visibility.Visible;
            }
            else
            {
                sscSpectrum.TickVisibility = Visibility.Collapsed;
            }
        }

        public LayerShowState HeatmapShowScaleOnMap
        {
            get { return (LayerShowState)GetValue(HeatmapShowScaleOnMapProperty); }
            set { SetValue(HeatmapShowScaleOnMapProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HeatmapShowScaleOnMap.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HeatmapShowScaleOnMapProperty =
            DependencyProperty.Register("HeatmapShowScaleOnMap", typeof(LayerShowState), typeof(MapControl), new PropertyMetadata(LayerShowState.Shown, OnSetHeatmapShowScaleOnMapChanged));

        private static void OnSetHeatmapShowScaleOnMapChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            MapControl mc = d as MapControl;
            mc.OnSetHeatmapShowScaleOnMapChanged(e);
        }

        private void OnSetHeatmapShowScaleOnMapChanged(DependencyPropertyChangedEventArgs e)
        {
            SetShowHeatmapScaleOnMap((LayerShowState)e.NewValue);
        }

        private void SetShowHeatmapScaleOnMap(LayerShowState state)
        {
            if (HeatmapStatus == LayerShowState.Shown)
            {
                if (state == LayerShowState.Hidden)
                {
                    sscSpectrum.Visibility = Visibility.Collapsed;
                }
                else if (state == LayerShowState.Shown)
                {
                    sscSpectrum.Visibility = Visibility.Visible;
                }
                else
                {

                }
            }
            else if (HeatmapStatus == LayerShowState.Hidden)
            {
                sscSpectrum.Visibility = Visibility.Collapsed;
            }
            else
            {

            }
        }

        public HeatMapStatus GetHeatMapStatus()
        {
            HeatMapStatus heatMapStatus = new HeatMapStatus();

            // HeatmapStatus
            if (HeatmapStatus == LayerShowState.Hidden)
            {
                heatMapStatus.IsHide = true;
            }
            else
            {
                heatMapStatus.IsHide = false;
            }

            // HeatmapTarget
            if (HeatmapTarget == TargetPoints.AllDataPoints)
            {
                heatMapStatus.IsAllDataPoint = true;
            }
            else
            {
                heatMapStatus.IsAllDataPoint = false;
            }

            // HeatmapShowMapPointsAndLabels
            if (HeatmapShowMapPointsAndLabels == LayerShowState.Shown)
            {
                heatMapStatus.IsShowMapPoint = true;
            }
            else
            {
                heatMapStatus.IsShowMapPoint = false;
            }

            // HeatmapShowScaleOnMap
            if (HeatmapShowScaleOnMap == LayerShowState.Shown)
            {
                heatMapStatus.IsScaleOnMap = true;
            }
            else
            {
                heatMapStatus.IsScaleOnMap = false;
            }

            // HeatmapDensityRadiusInMeters
            heatMapStatus.DensityRedius = HeatmapDensityRadiusInMeters;


            // HeatmapArealUnitInSquareMeters
            heatMapStatus.ArealUnits = HeatmapArealUnitInSquareMeters;

            // HeatmapOpacity
            heatMapStatus.Opacity = HeatmapOpacity;

            // HeatmapPointsSource && HeatmapPropertyBasedPointsSelectedTypeUri
            if (HeatmapPointsSource == PointsValueSourceType.ObjectsCount)
            {
                heatMapStatus.ShowByCountOrValue = HeatMapStatus.ShowByCount;
            }
            else
            {
                heatMapStatus.ShowByCountOrValue = HeatmapPropertyBasedPointsSelectedTypeUri;
            }

            return heatMapStatus;
        }

        public void SetHeatMapStatus(HeatMapStatus heatMapStatus)
        {
            // HeatmapStatus
            if (heatMapStatus.IsHide)
            {
                HeatmapStatus = LayerShowState.Hidden;
            }
            else
            {
                HeatmapStatus = LayerShowState.Shown;
            }

            // HeatmapTarget
            if (heatMapStatus.IsAllDataPoint)
            {
                HeatmapTarget = TargetPoints.AllDataPoints;
            }
            else
            {
                HeatmapTarget = TargetPoints.SelectedDataPoints;
            }

            // HeatmapShowMapPointsAndLabels
            if (heatMapStatus.IsShowMapPoint)
            {
                HeatmapShowMapPointsAndLabels = LayerShowState.Shown;
            }
            else
            {
                HeatmapShowMapPointsAndLabels = LayerShowState.Hidden;
            }

            // HeatmapShowScaleOnMap
            if (heatMapStatus.IsScaleOnMap)
            {
                HeatmapShowScaleOnMap = LayerShowState.Shown;
            }
            else
            {
                HeatmapShowScaleOnMap = LayerShowState.Hidden;
            }

            // HeatmapDensityRadiusInMeters
            HeatmapDensityRadiusInMeters = heatMapStatus.DensityRedius;


            // HeatmapArealUnitInSquareMeters
            HeatmapArealUnitInSquareMeters = heatMapStatus.ArealUnits;

            // HeatmapOpacity
            HeatmapOpacity = heatMapStatus.Opacity;

            // HeatmapPointsSource && HeatmapPropertyBasedPointsSelectedTypeUri
            if (heatMapStatus.ShowByCountOrValue.Equals("Count"))
            {
                HeatmapPointsSource = PointsValueSourceType.ObjectsCount;
            }
            else
            {
                HeatmapPointsSource = PointsValueSourceType.ValueOfSelectedProperty;
                HeatmapPropertyBasedPointsSelectedTypeUri = heatMapStatus.ShowByCountOrValue;
            }
        }

        public void Reset()
        {
            HeatmapPropertyBasedPointsSelectedTypeUri = string.Empty;
            HeatmapShowScaleOnMap = LayerShowState.Shown;
            mainMapViewer.Reset();
        }
    }
}