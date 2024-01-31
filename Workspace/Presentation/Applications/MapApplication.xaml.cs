using GMap.NET;
using GPAS.FilterSearch;
using GPAS.GeoSearch;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Entities.Investigation;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Controls.Map;
using GPAS.Workspace.Presentation.Controls.Map.GeoSearch;
using GPAS.Workspace.Presentation.Observers;
using GPAS.Workspace.Presentation.Observers.ObjectsRemoving;
using GPAS.Workspace.Presentation.Observers.Properties;
using GPAS.Workspace.Presentation.Windows;
using MaterialDesignThemes.Wpf;
using Notifications.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Applications
{
    /// <summary>
    /// Interaction logic for MapApplication.xaml
    /// </summary>
    public partial class MapApplication : IObjectsFilterableListener, IObjectsShowableListener,
        IObjectsSelectableListener, IObjectsRemovableListener, IPropertiesChangeableListener
    {
        public class ObjectsSelectionChangedArgs
        {
            public ObjectsSelectionChangedArgs(IEnumerable<KWObject> currentlySelectedObjects)
            {
                CurrentlySelectedObjects = currentlySelectedObjects;
            }

            public IEnumerable<KWObject> CurrentlySelectedObjects
            {
                get;
                private set;
            }
        }

        public event EventHandler<ObjectsSelectionChangedArgs> ObjectsSelectionChanged;

        protected void OnObjectsSelectionChanged(IEnumerable<KWObject> currentlySelectedObjects)
        {
            if (currentlySelectedObjects == null)
                throw new ArgumentNullException("currentlySelectedObjects");

            ObjectsSelectionChanged?.Invoke(this, new ObjectsSelectionChangedArgs(currentlySelectedObjects));
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

        public event NotifyCollectionChangedEventHandler ObjectsPropertiesChanged;
        protected void OnObjectsPropertiesChanged(NotifyCollectionChangedEventArgs e)
        {
            ObjectsPropertiesChanged?.Invoke(this, e);
        }

        /// <summary>
        /// رخداد «دریافت درخواست نمایش (براوز) اشیا»
        /// </summary>
        public event EventHandler<GraphApplication.BrowseRequestedEventArgs> MarkerBrowseRequested;

        protected void OnMarkerBrowseRequested(IEnumerable<KWObject> objectsRequestedForBrowse)
        {
            if (objectsRequestedForBrowse == null)
                throw new ArgumentNullException(nameof(objectsRequestedForBrowse));

            MarkerBrowseRequested?.Invoke(this, new GraphApplication.BrowseRequestedEventArgs(objectsRequestedForBrowse));
        }

        public event EventHandler<SnapshotRequestEventArgs> SnapshotRequested;
        public void OnSnapshotRequested(SnapshotRequestEventArgs args)
        {
            SnapshotRequested?.Invoke(this, args);
        }


        public MapApplication()
        {
            InitializeComponent();
            mainMapControl.mainMapViewer.PolygonDrawn += MainMapViewer_PolygonDrawn;
            mainMapControl.mainMapViewer.CircleDrawn += MainMapViewer_CircleDrawn;
            mainMapControl.mainMapViewer.RouteDrawn += MainMapViewer_RouteDrawn;
            mainMapControl.ObjectsSelectionChanged += MainMapControl_ObjectsSelectionChanged;
            App.WorkspaceInitializationCompleted += App_WorkspaceInitializationCompleted;
        }

        private void App_WorkspaceInitializationCompleted(object sender, EventArgs e)
        {
            App.MainWindow.LocationChanged += MainWindow_LocationChanged;
            App.MainWindow.CurrentThemeChanged += MainWindow_CurrentThemeChanged;
        }

        internal PaletteHelper paletteHelper = new PaletteHelper();
        private void MainWindow_CurrentThemeChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ITheme theme = paletteHelper.GetTheme();
        }

        private void MainMapViewer_PolygonDrawn(object sender, MapViewer.PolygonDrawnEventArgs e)
        {
            if (PolygonSearchPopup.IsOpen)
            {
                polygonSearchControl.Points = e.Points.Count;
                polygonSearchControl.Perimeter = e.perimeterInMeters;
                polygonSearchControl.IsDrawnPolygonCrossed = e.isAnyVectorCrossed;
            }
        }

        private void MainMapViewer_RouteDrawn(object sender, MapViewer.PathDrawnEventArgs e)
        {
            if (RouteSearchPopup.IsOpen)
            {
                routeSearchControl.Points = e.Points.Count;
                routeSearchControl.Length = e.LengthInMeters;
            }
        }

        private void MainMapViewer_CircleDrawn(object sender, MapViewer.CircleDrawnEventArgs e)
        {
            if (CircleSearchPopup.IsOpen)
            {
                circleSearchControl.CenterPointLat = e.Center.Lat;
                circleSearchControl.CenterPointLng = e.Center.Lng;
                circleSearchControl.Radious = e.RadiusInMeters;
            }
        }

        private void MainMapControl_ObjectsSelectionChanged(object sender, Controls.Map.ObjectsSelectionChangedArgs e)
        {
            OnObjectsSelectionChanged(e.CurrentlySelectedObjects);
        }

        private void MainMapControl_SimpleMarkerDoubleClick(object sender, SimpleMarkerDoubleClickEventArgs e)
        {
            List<KWObject> doubleClickedObjects = new List<KWObject>
            {
                e.SimpleMarker
            };

            OnMarkerBrowseRequested(doubleClickedObjects);
        }

        IEnumerable<KWObject> ObjectsToShow = null;
        public async Task ShowObjectsAsync(IEnumerable<KWObject> objectsToShow)
        {
            ObjectsToShow = objectsToShow;
            await mainMapControl.ShowObjects(objectsToShow);
        }

        private void ClearMapContent()
        {
            mainMapControl.ClearMapContent();
        }

        public void RemoveSelectedMapContent()
        {
            mainMapControl.RemoveSelectedMarkers();
            OnObjectsSelectionChanged(mainMapControl.GetSelectedObjects());
        }

        private void ClearMapButton_Click(object sender, RoutedEventArgs e)
        {
            CloseAllPopups();
            RemoveSelectedMapContentByAskQuestion();
        }

        private void RemoveSelectedMapContentByAskQuestion()
        {
            if (mainMapControl.GetSelectedObjects().Any())
            {
                switch (KWMessageBox.Show(Properties.Resources.Do_You_Want_To_Remove_All_Objects_From_Map,
                    MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.Yes))
                {
                    case MessageBoxResult.Yes:
                        RemoveSelectedMapContent();
                        break;
                    case MessageBoxResult.No:
                        break;
                }
            }
        }

        public override void PerformCommandForShortcutKey(SupportedShortCutKey commandShortCutKey)
        {
            switch (commandShortCutKey)
            {
                case SupportedShortCutKey.Ctrl_A:
                    try
                    { mainMapControl.SelectAllObjects(); }
                    catch (Exception ex)
                    { KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error); }
                    break;
            }
        }

        public void SelectObjects(IEnumerable<KWObject> objectsToSelect)
        {
            mainMapControl.SelectObjects(objectsToSelect);
        }


        private void RouteSearchMapButton_Click(object sender, RoutedEventArgs e)
        {
            CloseAllPopups();
            RouteSearchPopup.IsOpen = true;
            mainMapControl.SetBufferSizeForSelectedRouteShape(routeSearchControl.RaiseBufferChangedEvent(), routeSearchControl.SelectedScale);
            mainMapControl.DrawRoute();
        }

        private void PolygonSearchMapButton_Click(object sender, RoutedEventArgs e)
        {
            CloseAllPopups();
            PolygonSearchPopup.IsOpen = true;
            mainMapControl.DrawPolygon();
        }
        private void CircleSearchMapButton_Click(object sender, RoutedEventArgs e)
        {
            CloseAllPopups();
            CircleSearchPopup.IsOpen = true;
            mainMapControl.DrawCircle();
        }

        private void SnapshotButton_Click(object sender, RoutedEventArgs e)
        {
            mainMapControl.TakeSnapshot();
        }

        public void CloseAllPopups()
        {
            mainMapControl.ClearDrawing();
            RouteSearchPopup.IsOpen = false;
            PolygonSearchPopup.IsOpen = false;
            CircleSearchPopup.IsOpen = false;
        }

        private void polygonSearchControl_Canceled(object sender, EventArgs e)
        {
            mainMapControl.ClearDrawing();
            PolygonSearchPopup.IsOpen = false;
        }
        private void circleSearchControl_Canceled(object sender, EventArgs e)
        {
            mainMapControl.ClearDrawing();
            CircleSearchPopup.IsOpen = false;
        }

        private void routeSearchControl_Canceled(object sender, EventArgs e)
        {
            mainMapControl.ClearDrawing();
            RouteSearchPopup.IsOpen = false;
        }

        private async void routeSearchControl_SearchRequested(object sender, EventArgs e)
        {
            List<PolygonSearchCriteria> arguments = new List<PolygonSearchCriteria>(mainMapControl.LastPathDrawnEventArgs.Points.Count - 1);
            foreach (List<PointLatLng> polygonPoints in mainMapControl.LastPathDrawnEventArgs.InnerPolygons)
            {
                PolygonSearchCriteria polygonSearchCriteria = new PolygonSearchCriteria()
                {
                    Vertices = ConvertPointToGeoPoint(polygonPoints)
                };
                arguments.Add(polygonSearchCriteria);
            }
            List<KWObject> geoSearchResults;
            try
            {
                WaitingControl.Message = Properties.Resources.Performing_Geographical_Search_;
                WaitingControl.TaskIncrement();
                geoSearchResults = await Geo.PerformGeoPolygonSearchAsync(arguments);
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message);
                geoSearchResults = new List<KWObject>();
            }
            finally
            {
                WaitingControl.TaskDecrement();
            }

            await ShowObjectsAsync(geoSearchResults);

            ShowNotification(Properties.Resources.Unpublish_Concepts_Notification,
                Properties.Resources.Unpublished_concepts_can_not_be_searched);


        }

        private List<GeoPoint> ConvertPointToGeoPoint(List<PointLatLng> points)
        {
            List<GeoPoint> result = new List<GeoPoint>();
            foreach (var currentPoint in points)
            {
                result.Add(new GeoPoint()
                {
                    Lat = currentPoint.Lat,
                    Lng = currentPoint.Lng
                });
            }
            return result;
        }

        private async void polygonSearchControl_SearchRequested(object sender, EventArgs e)
        {


            PolygonSearchCriteria polygonSearchCriteria = new PolygonSearchCriteria()
            {
                Vertices = ConvertPointToGeoPoint(mainMapControl.LastPolygonDrawnEventArgs.Points),
                perimeterInMeters = mainMapControl.LastPolygonDrawnEventArgs.perimeterInMeters,
                isAnyVectorCoincident = mainMapControl.LastPolygonDrawnEventArgs.isAnyVectorCoincident,
                isAnyVectorCrossed = mainMapControl.LastPolygonDrawnEventArgs.isAnyVectorCrossed
            };
            List<KWObject> geoSearchResults;
            try
            {
                WaitingControl.Message = Properties.Resources.Performing_Geographical_Search_;
                WaitingControl.TaskIncrement();
                geoSearchResults = await Geo.PerformGeoPolygonSearchAsync(polygonSearchCriteria);
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message);
                geoSearchResults = new List<KWObject>();
            }
            finally
            {
                WaitingControl.TaskDecrement();
            }

            await ShowObjectsAsync(geoSearchResults);

            ShowNotification(Properties.Resources.Unpublish_Concepts_Notification,
                Properties.Resources.Unpublished_concepts_can_not_be_searched);
        }
        private void ShowNotification(string title, string message)
        {
            var notification = new NotificationContent
            {
                Title = title,
                Message = message
            };
            mainMapControl.ShowNotification(notification);
        }

        private async void circleSearchControl_SearchRequested(object sender, EventArgs e)
        {
            CircleSearchCriteria circleSearchCriteria = new CircleSearchCriteria()
            {
                Center = new GeoPoint()
                {
                    Lat = Math.Round(mainMapControl.LastCircleDrawnEventArgs.Center.Lat, 4),
                    Lng = Math.Round(mainMapControl.LastCircleDrawnEventArgs.Center.Lng, 4)
                },
                RediusInKiloMeters = Math.Round(mainMapControl.LastCircleDrawnEventArgs.RadiusInMeters / 1000, 4)
            };
            List<KWObject> geoSearchResults;
            try
            {
                WaitingControl.Message = Properties.Resources.Performing_Geographical_Search_;
                WaitingControl.TaskIncrement();
                geoSearchResults = await Geo.PerformGeoCircleSearchAsync(circleSearchCriteria);


                await ShowObjectsAsync(geoSearchResults);
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message);
                geoSearchResults = new List<KWObject>();
            }
            finally
            {
                WaitingControl.TaskDecrement();
            }

            ShowNotification(Properties.Resources.Unpublish_Concepts_Notification,
                Properties.Resources.Unpublished_concepts_can_not_be_searched);
        }

        private void routeSearchControl_BufferChanged(object sender, BufferChangedEventArgs e)
        {
            mainMapControl.SetBufferSizeForSelectedRouteShape(e.Buffer, e.Scale);
        }

        private void innerControlsViewBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateSearchPopupPositions();
        }
        private void UpdateSearchPopupPositions()
        {
            UpdatePopupControlPosition(CircleSearchPopup);
            UpdatePopupControlPosition(PolygonSearchPopup);
            UpdatePopupControlPosition(RouteSearchPopup);
        }
        private void UpdatePopupControlPosition(Popup popupControl)
        {
            if (popupControl.IsOpen)
            {
                popupControl.IsOpen = false;
                popupControl.IsOpen = true;
            }
        }

        private void MainWindow_LocationChanged(object sender, EventArgs e)
        {
            UpdateSearchPopupPositions();
        }

        private void CircleSearchPopup_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void RouteSearchPopup_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void PolygonSearchPopup_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }
        public bool IsDragging { get; set; }

        Query IObjectsFilterableListener.CurrentFilter
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        private void mainMapControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed &&
                !((Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))) &&
                !(CircleSearchPopup.IsOpen || PolygonSearchPopup.IsOpen || RouteSearchPopup.IsOpen))
            {
                List<KWObject> selectedObjects = mainMapControl.GetSelectedObjects();
                if (selectedObjects.Count > 0)
                {
                    GiveFeedbackEventHandler handler = new GiveFeedbackEventHandler(DragSource_GiveFeedback);
                    GiveFeedback += handler;
                    IsDragging = true;
                    DataObject data = new DataObject();
                    data.SetData("SelectedMapObjects", selectedObjects);
                    DragDrop.DoDragDrop(this, data, DragDropEffects.Copy | DragDropEffects.Move);
                    GiveFeedback -= handler;
                    IsDragging = false;
                }
            }
        }

        void DragSource_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            try
            {
                Mouse.SetCursor(((TextBlock)Resources["DragDropCursor"]).Cursor);
                e.UseDefaultCursors = false;
                e.Handled = true;
            }
            finally { }
        }

        private void ClearMapButton_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("SelectedMapObjects"))
            {
                List<KWObject> selectedObjects = e.Data.GetData("SelectedMapObjects") as List<KWObject>;
                RemoveSelectedMapContentByAskQuestion();
            }
        }

        public async Task ApplyFilter(ObjectsFilteringArgs filterToApply)
        {
            //TODO 
            await Task.Delay(0);
        }

        private void routeSearchControl_BufferChanged(object sender, object e)
        {

        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            OpenSelectedNodesMenu();
        }

        private void OpenSelectedNodesMenu()
        {
            //try
            //{ btnSelect.ContextMenu.IsOpen = true; }
            //catch (Exception ex)
            //{ KWMessageBox.Show(ex.Message, Properties.Resources.Map_Application, MessageBoxButton.OK, MessageBoxImage.Error); }
        }

        private void SelectAllObjectsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mainMapControl.SelectAllObjects();
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeselectAllObjectsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                mainMapControl.DeselectAllObjects();
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void InvertSelectionObjectsMenuItem_Click(object sender, RoutedEventArgs e)
        {

            try
            {
                mainMapControl.InvertSelectionObjects();
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        internal MapApplicationStatus GetMapApplicationStatus()
        {
            MapApplicationStatus mapAppStatus = new MapApplicationStatus()
            {
                SelectedObjectIds = mainMapControl.GetSelectedObjects()?.Select(o => o.ID).ToList(),
                ShowingObjectIds = mainMapControl.GetShowingObjects()?.Select(o => o.ID).ToList(),
                HeatMapStatus = mainMapControl.GetHeatMapStatus()
            };
            return mapAppStatus;
        }

        internal async Task SetMapApplicationStatus(MapApplicationStatus mapAppStatus)
        {
            if (mapAppStatus.ShowingObjectIds != null &&
                mapAppStatus.SelectedObjectIds != null)
            {
                ClearMapContent();
                await ShowObjectsAsync(await ObjectManager.GetObjectsById(mapAppStatus.ShowingObjectIds));

                mainMapControl.SetHeatMapStatus(mapAppStatus.HeatMapStatus);
            }
        }

        private void mainMapControl_SnapshotRequested(object sender, SnapshotRequestEventArgs e)
        {
            OnSnapshotRequested(e);
        }

        public void RemoveObjects(IEnumerable<KWObject> objectsToRemove)
        {
            mainMapControl.RemoveMarkersOfObjects(objectsToRemove);
        }

        public void ChangeProperties(PropertiesChangedArgs args)
        {
            if (args == null)
                return;

            if (args.RemovedProperties?.Count() > 0)
                mainMapControl.RemoveMarkers(args.RemovedProperties);

            IEnumerable<KWProperty> allPropertiesChanged = GetAllPropertiesChanged(args);
            if (allPropertiesChanged == null)
                return;

            var showingObjects = mainMapControl.GetShowingObjects();

            List<KWObject> ObjectsWhosePropertiesHaveChanged = allPropertiesChanged.Select(p => p.Owner).Intersect(showingObjects).ToList();

            if (ObjectsWhosePropertiesHaveChanged?.Count > 0)
            {
                IEnumerable<KWObject> added = new List<KWObject>();
                IEnumerable<KWObject> removed = new List<KWObject>();
                if (args.AddedProperties != null)
                    added = args.AddedProperties.Select(p => p.Owner);

                if (args.RemovedProperties != null)
                    removed = args.RemovedProperties.Select(p => p.Owner);

                OnObjectsPropertiesChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, added, removed));
            }
        }

        private IEnumerable<KWProperty> GetAllPropertiesChanged(PropertiesChangedArgs args)
        {
            if (args.AddedProperties == null && args.RemovedProperties == null)
                return null;
            else if (args.AddedProperties == null)
                return args.RemovedProperties;
            else if (args.RemovedProperties == null)
                return args.AddedProperties;
            else
                return args.AddedProperties.Union(args.RemovedProperties);
        }

        private void MainMapControl_ObjectsAdded(object sender, EventArgs e)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
            OnObjectsAdded();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
        }

        private void MainMapControl_ObjectsRemoved(object sender, EventArgs e)
        {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
            OnObjectsRemoved();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed. Consider applying the 'await' operator to the result of the call.
        }

        public override void Reset()
        {
            routeSearchControl.Reset();
            ClearMapContent();
        }
    }
}
