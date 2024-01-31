using GMap.NET;
using GPAS.Dispatch.Entities.Concepts.Geo;
using GPAS.MapViewer;
using GPAS.Ontology;
using GPAS.PropertiesValidation;
using GPAS.PropertiesValidation.Geo;
using GPAS.Workspace.Entities.Geo;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Controls
{
    /// <summary>
    /// Interaction logic for LocationPropertyValueControl.xaml
    /// </summary>
    public partial class LocationPropertyValueControl
    {
        #region dependencies

        public string Latitude
        {
            get { return (string)GetValue(LatitudeProperty); }
            set { SetValue(LatitudeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Latitude.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LatitudeProperty =
            DependencyProperty.Register(nameof(Latitude), typeof(string), typeof(LocationPropertyValueControl),
                new PropertyMetadata("0", OnSetLatitudeChanged));

        private static void OnSetLatitudeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LocationPropertyValueControl)d).OnSetLatitudeChanged(e);
        }

        private void OnSetLatitudeChanged(DependencyPropertyChangedEventArgs e)
        {
            InvariantLatitude = ConvertCurrentCultureValueToInvariant(Latitude);
            Validation = GetValidation(Latitude, Longitude, Radius, SearchMode);
            Value = GenerateValue(Latitude, Longitude, Radius, SearchMode);
        }

        public string Longitude
        {
            get { return (string)GetValue(LongitudeProperty); }
            set { SetValue(LongitudeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Longitude.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty LongitudeProperty =
            DependencyProperty.Register(nameof(Longitude), typeof(string), typeof(LocationPropertyValueControl),
                new PropertyMetadata("0", OnSetLongitudeChanged));

        private static void OnSetLongitudeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LocationPropertyValueControl)d).OnSetLongitudeChanged(e);
        }

        private void OnSetLongitudeChanged(DependencyPropertyChangedEventArgs e)
        {
            InvariantLongitude = ConvertCurrentCultureValueToInvariant(Longitude);
            Validation = GetValidation(Latitude, Longitude, Radius, SearchMode);
            Value = GenerateValue(Latitude, Longitude, Radius, SearchMode);
        }

        public ValidationProperty Validation
        {
            get { return (ValidationProperty)GetValue(ValidationProperty); }
            set { SetValue(ValidationProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Validation.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValidationProperty =
            DependencyProperty.Register(nameof(Validation), typeof(ValidationProperty), typeof(LocationPropertyValueControl),
                new PropertyMetadata(null, OnSetValidationChanged));

        private static void OnSetValidationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LocationPropertyValueControl)d).OnSetValidationChanged(e);
        }

        private void OnSetValidationChanged(DependencyPropertyChangedEventArgs e)
        {
            ValidationProperty oldValidation = (ValidationProperty)e.OldValue;
            if (oldValidation?.Status != Validation?.Status || oldValidation.Message != Validation?.Message)
                OnValidationChanged(e);
        }


        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            protected set { SetValue(ValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Value.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(string), typeof(LocationPropertyValueControl),
                new PropertyMetadata(string.Empty, OnSetValueChanged));

        private static void OnSetValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LocationPropertyValueControl)d).OnSetValueChanged(e);
        }

        private void OnSetValueChanged(DependencyPropertyChangedEventArgs e)
        {
            InvariantValue = GenerateValue(InvariantLatitude, InvariantLongitude, InvariantRadius, SearchMode);

            if (Validation.Status != ValidationStatus.Invalid)
            {
                if (!SearchMode)
                    ShowMarkerOnMap(double.Parse(InvariantLatitude), double.Parse(InvariantLongitude));
            }

            OnValueChanged(e);
        }

        public string InvariantValue
        {
            get { return (string)GetValue(InvariantValueProperty); }
            protected set { SetValue(InvariantValueProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InvariantValue.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InvariantValueProperty =
            DependencyProperty.Register(nameof(InvariantValue), typeof(string), typeof(LocationPropertyValueControl),
                new PropertyMetadata(string.Empty));



        public string InvariantLatitude
        {
            get { return (string)GetValue(InvariantLatitudeProperty); }
            protected set { SetValue(InvariantLatitudeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InvariantLatitude.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InvariantLatitudeProperty =
            DependencyProperty.Register(nameof(InvariantLatitude), typeof(string), typeof(LocationPropertyValueControl),
                new PropertyMetadata("0"));



        public string InvariantLongitude
        {
            get { return (string)GetValue(InvariantLongitudeProperty); }
            set { SetValue(InvariantLongitudeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InvariantLongitude.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InvariantLongitudeProperty =
            DependencyProperty.Register(nameof(InvariantLongitude), typeof(string), typeof(LocationPropertyValueControl),
                new PropertyMetadata("0"));



        public bool SearchMode
        {
            get { return (bool)GetValue(SearchModeProperty); }
            set { SetValue(SearchModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SearchMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SearchModeProperty =
            DependencyProperty.Register(nameof(SearchMode), typeof(bool), typeof(LocationPropertyValueControl),
                new PropertyMetadata(false, OnSetSearchModeChanged));

        private static void OnSetSearchModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LocationPropertyValueControl)d).OnSetSearchModeChanged(e);
        }

        private void OnSetSearchModeChanged(DependencyPropertyChangedEventArgs e)
        {
            if (SearchMode)
                mainMapViewer.RemoveAllMarkers();

            Value = GenerateValue(Latitude, Longitude, Radius, SearchMode);
        }



        public string Radius
        {
            get { return (string)GetValue(RadiusProperty); }
            set { SetValue(RadiusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Radius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RadiusProperty =
            DependencyProperty.Register(nameof(Radius), typeof(string), typeof(LocationPropertyValueControl),
                new PropertyMetadata("5000", OnSetRadiusChanged));

        private static void OnSetRadiusChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((LocationPropertyValueControl)d).OnSetRadiusChanged(e);
        }

        private void OnSetRadiusChanged(DependencyPropertyChangedEventArgs e)
        {
            InvariantRadius = ConvertCurrentCultureValueToInvariant(Radius);
            Validation = GetValidation(Latitude, Longitude, Radius, SearchMode);
            Value = GenerateValue(Latitude, Longitude, Radius, SearchMode);
        }



        public string InvariantRadius
        {
            get { return (string)GetValue(InvariantRadiusProperty); }
            set { SetValue(InvariantRadiusProperty, value); }
        }

        // Using a DependencyProperty as the backing store for InvariantRadius.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty InvariantRadiusProperty =
            DependencyProperty.Register(nameof(InvariantRadius), typeof(string), typeof(LocationPropertyValueControl),
                new PropertyMetadata("5000"));


        #endregion

        #region Methods

        private string SelectedMapTileSource = string.Empty;

        public LocationPropertyValueControl()
        {
            InitializeComponent();
            Validation = GetValidation(Latitude, Longitude, Radius, SearchMode);
            Value = GenerateValue(Latitude, Longitude, Radius, SearchMode);

            SelectedMapTileSource = Logic.Geo.GetMapTileSources().FirstOrDefault();
        }

        private ValidationProperty GetValidation(string latitude, string longitude, string radius, bool searchMode)
        {
            ValidationProperty pointValidation = GetValidationPoint(latitude, longitude);
            if (!searchMode)
                return pointValidation;

            ValidationProperty radiusValidation = GetValidationRadius(radius);

            ValidationProperty result = new ValidationProperty() { Status = ValidationStatus.Valid, Message = string.Empty };

            if (pointValidation.Status == ValidationStatus.Invalid || radiusValidation.Status == ValidationStatus.Invalid)
                result.Status = ValidationStatus.Invalid;
            else if (pointValidation.Status == ValidationStatus.Warning || radiusValidation.Status == ValidationStatus.Warning)
                result.Status = ValidationStatus.Warning;

            if (pointValidation.Status != ValidationStatus.Valid && radiusValidation.Status != ValidationStatus.Valid)
                result.Message = pointValidation.Message + Environment.NewLine + radiusValidation.Message;
            else if (pointValidation.Status != ValidationStatus.Valid)
                result.Message = pointValidation.Message;
            else if (radiusValidation.Status != ValidationStatus.Valid)
                result.Message = radiusValidation.Message;

            return result;
        }

        private ValidationProperty GetValidationRadius(string radius)
        {
            ValidationProperty result =
                ValueBaseValidation.TryParsePropertyValue(BaseDataTypes.Double, radius, out var r, CultureInfo.CurrentCulture);
            if (result.Status != ValidationStatus.Valid)
                result.Message = "Radius must be numerical";
            else
            {
                if (double.Parse(r.ToString()) <= 0)
                {
                    result.Status = ValidationStatus.Invalid;
                    result.Message = "Radius must be greater than zero (Radius > 0)";
                }
            }

            return result;
        }

        private ValidationProperty GetValidationPoint(string latitude, string longitude)
        {
            string geoPointStringValue = GeoPoint.GetGeoPointStringValue(new GeoLocationEntityRawData() { Latitude = latitude, Longitude = longitude });
            return GeoPoint.IsValidGeoPoint(geoPointStringValue, CultureInfo.CurrentCulture, out _);
        }

        private string GenerateValue(string latitude, string longitude, string radius, bool searchMode)
        {
            if (searchMode)
                return GeoCircle.GetGeoCircleStringValue(
                    new GeoCircleEntityRawData() { Latitude = latitude, Longitude = longitude, Radius = radius });
            else
                return GeoPoint.GetGeoPointStringValue(new GeoLocationEntityRawData() { Latitude = latitude, Longitude = longitude });
        }

        private string ConvertCurrentCultureValueToInvariant(string input)
        {
            ValueBaseValidation.TryParsePropertyValue(BaseDataTypes.Double, input, out var parsedValue,
                CultureInfo.CurrentCulture);
            return Convert.ToString(parsedValue, CultureInfo.InvariantCulture);
        }

        public void ResetValues()
        {
            Latitude = "0";
            Longitude = "0";
            Radius = "5000";
        }

        private void ShowMapBoxButton_Click(object sender, RoutedEventArgs e)
        {
            MapPopupBox.IsPopupOpen = true;
        }

        private void MapPopupBox_Opened(object sender, RoutedEventArgs e)
        {
            mainMapViewer.Position = new PointLatLng(double.Parse(InvariantLatitude), double.Parse(InvariantLongitude));

            if (SearchMode)
                mainMapViewer.ShapeType = DrawingShape.Circle;
        }

        private void MapPopupBox_Closed(object sender, RoutedEventArgs e)
        {
            if (SearchMode)
                ClearCircle();
        }

        private void ClearCircle()
        {
            mainMapViewer.ShapeType = DrawingShape.None;
            if (mainMapViewer.SelectedAdvanceShape != null)
            {
                mainMapViewer.SelectedAdvanceShape?.Delete();
                mainMapViewer.SelectedAdvanceShape = null;
            }
        }

        private void MainMapViewer_MapTileImageNeeded(object sender, MapViewer.MapTileImageNeededEventArgs e)
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

        private void MainMapViewer_MapMouseDown(object sender, MapViewer.MapMouseButtonEventArgs e)
        {
            if (!SearchMode)
            {
                if (e.MouseButtonEventArgs.LeftButton == MouseButtonState.Pressed && e.MouseButtonEventArgs.ClickCount == 2)
                {
                    Latitude = e.Point.Lat.ToString(CultureInfo.CurrentCulture);
                    Longitude = e.Point.Lng.ToString(CultureInfo.CurrentCulture);
                }
            }
        }

        private void ShowMarkerOnMap(double lat, double lng)
        {
            PointLatLng point = new PointLatLng(lat, lng);
            mainMapViewer.RemoveAllMarkers();
            mainMapViewer.ShowMarkers(new List<MapViewer.SimpleMarker>() { new MapViewer.SimpleMarker(point) });
        }

        private void MainMapViewer_CircleDrawn(object sender, CircleDrawnEventArgs e)
        {
            if (SearchMode && mainMapViewer.ShapeType == DrawingShape.Circle)
            {
                Latitude = e.Center.Lat.ToString(CultureInfo.CurrentCulture);
                Longitude = e.Center.Lng.ToString(CultureInfo.CurrentCulture);
                Radius = e.RadiusInMeters.ToString(CultureInfo.CurrentCulture);
            }
        }

        #endregion

        #region Events

        public event DependencyPropertyChangedEventHandler ValidationChanged;
        private void OnValidationChanged(DependencyPropertyChangedEventArgs e)
        {
            ValidationChanged?.Invoke(this, e);
        }

        public event DependencyPropertyChangedEventHandler ValueChanged;
        private void OnValueChanged(DependencyPropertyChangedEventArgs e)
        {
            ValueChanged?.Invoke(this, e);
        }

        #endregion
    }
}
