using GPAS.Dispatch.Entities.Concepts.Geo;
using GPAS.Ontology;
using GPAS.PropertiesValidation;
using MaterialDesignThemes.Wpf;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Controls
{
    /// <summary>
    /// Interaction logic for DateRangeAndLocationControl.xaml
    /// </summary>
    public partial class DateRangeAndLocationControl
    {
        #region متغیرهای سراسری
        protected SolidColorBrush AcceptBrush = new SolidColorBrush();
        protected SolidColorBrush RejectBrush = new SolidColorBrush();
        protected GeoTimeEntityRawData InvariantGeoTimeEntityRawData = new GeoTimeEntityRawData();
        public Brush NewBackground
        {
            get { return (Brush)GetValue(NewBackgroundProperty); }
            set { SetValue(NewBackgroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NewBackground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NewBackgroundProperty =
            DependencyProperty.Register(nameof(NewBackground), typeof(Brush), typeof(DateRangeAndLocationControl),
                new PropertyMetadata(Brushes.Black));

        public Brush NewForeground
        {
            get { return (Brush)GetValue(NewForegroundProperty); }
            set { SetValue(NewForegroundProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NewForeground.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NewForegroundProperty =
            DependencyProperty.Register(nameof(NewForeground), typeof(Brush), typeof(DateRangeAndLocationControl),
                new PropertyMetadata(Brushes.Black));
        #endregion

        #region مدیریت رخدادها
        public class ValidationEventArgs
        {
            public ValidationEventArgs(ValidationStatus validationResult, GeoTimeEntityRawData geoTimeEntityRawData)
            {
                ValidationResult = validationResult;
                GeoTimeEntityRawData = geoTimeEntityRawData;
            }

            public ValidationStatus ValidationResult
            {
                get;
            }

            public GeoTimeEntityRawData GeoTimeEntityRawData
            {
                get;
            }

        }

        public event EventHandler<ValidationEventArgs> ValidationChecked;
        protected virtual void OnValidationChecked(ValidationStatus validationResult, GeoTimeEntityRawData geoTimeEntityRawData)
        {
            ValidationChecked?.Invoke(this, new ValidationEventArgs(validationResult, geoTimeEntityRawData));
        }

        #endregion

        public DateRangeAndLocationControl()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            AcceptBrush = new SolidColorBrush(Colors.Black);
            RejectBrush = new SolidColorBrush(Colors.Red);

            ResetValues();
        }

        private void LatitudeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckDateRangeAndLocationValidation(LatitudeTextBox?.Text, LongitudeTextBox?.Text,
               BeginDateTimePicker?.Text, EndDateTimePicker?.Text);
        }

        private void LongitudeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CheckDateRangeAndLocationValidation(LatitudeTextBox?.Text, LongitudeTextBox?.Text,
                BeginDateTimePicker?.Text, EndDateTimePicker?.Text);
        }

        private void SetRejectForegroundToElements()
        {
           // NewForeground = RejectBrush;
            //LatitudeTextBox.Foreground =
            //    LongitudeTextBox.Foreground =
            //    BeginDateTimePicker.Foreground =
            //    EndDateTimePicker.Foreground = RejectBrush;
        }

        private void SetAcceptForegroundToElements()
        {
           // NewForeground = AcceptBrush;
            //LatitudeTextBox.Foreground =
            //    LongitudeTextBox.Foreground =
            //    BeginDateTimePicker.Foreground =
            //    EndDateTimePicker.Foreground = AcceptBrush;
        }

        private void CheckDateRangeAndLocationValidation(string lat, string lon, string begin, string end)
        {
            InvariantGeoTimeEntityRawData = new GeoTimeEntityRawData()
            {
                Latitude = lat,
                Longitude = lon,
                TimeBegin = begin,
                TimeEnd = end
            };

            string geoTimeStringValue = GeoTime.GetGeoTimeStringValue(InvariantGeoTimeEntityRawData);

            var geoTimeValidation = GeoTime.IsValidGeoTime(geoTimeStringValue, CultureInfo.CurrentCulture, out _, null, null, null, null);

            OnValidationChecked(geoTimeValidation.Status, InvariantGeoTimeEntityRawData);
        }

        public GeoTimeEntityRawData GetGeoTimeValue()
        {
            return new GeoTimeEntityRawData()
            {
                Latitude = LatitudeTextBox.Text,
                Longitude = LongitudeTextBox.Text,
                TimeBegin = BeginDateTimePicker.SelectedDateTime?.ToString(CultureInfo.CurrentCulture),
                TimeEnd = EndDateTimePicker.SelectedDateTime?.ToString(CultureInfo.CurrentCulture)
            };
        }

        public GeoTimeEntityRawData GetInvariantGeoTimeValue()
        {
            ValueBaseValidation.TryParsePropertyValue(BaseDataTypes.Double, LatitudeTextBox.Text, out var parsedValue,
                CultureInfo.CurrentCulture);
            string latitude = Convert.ToString(parsedValue, CultureInfo.InvariantCulture);

            ValueBaseValidation.TryParsePropertyValue(BaseDataTypes.Double, LongitudeTextBox.Text, out parsedValue,
                CultureInfo.CurrentCulture);
            string longitude = Convert.ToString(parsedValue, CultureInfo.InvariantCulture);

            ValueBaseValidation.TryParsePropertyValue(BaseDataTypes.DateTime,
                BeginDateTimePicker.SelectedDateTime?.ToString(CultureInfo.CurrentCulture), out parsedValue,
                CultureInfo.CurrentCulture);
            string timeBegin = Convert.ToString(parsedValue, CultureInfo.InvariantCulture);

            ValueBaseValidation.TryParsePropertyValue(BaseDataTypes.DateTime,
                EndDateTimePicker.SelectedDateTime?.ToString(CultureInfo.CurrentCulture), out parsedValue,
                CultureInfo.CurrentCulture);
            string timeEnd = Convert.ToString(parsedValue, CultureInfo.InvariantCulture);

            InvariantGeoTimeEntityRawData = new GeoTimeEntityRawData()
            {
                Latitude = latitude,
                Longitude = longitude,
                TimeBegin = timeBegin,
                TimeEnd = timeEnd
            };

            return InvariantGeoTimeEntityRawData;
        }

        public void SetDateRangeAndLocationValues(string lat, string lon, string begin, string end)
        {
            LatitudeTextBox.Text = lat;
            LongitudeTextBox.Text = lon;

            if (begin != null && DateTime.TryParse(begin, CultureInfo.CurrentCulture, DateTimeStyles.None, out var beginDateTime))
                BeginDateTimePicker.SelectedDateTime = beginDateTime;

            if (end != null && DateTime.TryParse(end, CultureInfo.CurrentCulture, DateTimeStyles.None, out var endDateTime))
                EndDateTimePicker.SelectedDateTime = endDateTime;

            CheckDateRangeAndLocationValidation(LatitudeTextBox?.Text, LongitudeTextBox?.Text,
                BeginDateTimePicker?.SelectedDateTime?.ToString(CultureInfo.CurrentCulture),
                EndDateTimePicker?.SelectedDateTime?.ToString(CultureInfo.CurrentCulture));

        }

        public void ResetValues()
        {
            BeginDateTimePicker.SelectedDateTime = DateTime.Now;
            EndDateTimePicker.SelectedDateTime = DateTime.Now;
            LatitudeTextBox.Text = "0";
            LongitudeTextBox.Text = "0";
        }

        private void BeginDateTimePicker_TextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            CheckDateRangeAndLocationValidation(LatitudeTextBox?.Text, LongitudeTextBox?.Text,
               BeginDateTimePicker?.Text, EndDateTimePicker?.Text);
        }

        private void EndDateTimePicker_TextChanged(object sender, System.Windows.DependencyPropertyChangedEventArgs e)
        {
            CheckDateRangeAndLocationValidation(LatitudeTextBox?.Text, LongitudeTextBox?.Text,
               BeginDateTimePicker?.Text, EndDateTimePicker?.Text);
        }
    }
}
