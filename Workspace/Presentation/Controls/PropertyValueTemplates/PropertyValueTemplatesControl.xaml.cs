using GPAS.Dispatch.Entities.Concepts.Geo;
using GPAS.Ontology;
using GPAS.PropertiesValidation;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Controls.PropertyValueTemplates
{
    /// <summary>
    /// Interaction logic for PropertyValueTemplatesControl.xaml
    /// </summary>
    public partial class PropertyValueTemplatesControl
    {
        public PropertyValueTemplatesControl()
        {
            InitializeComponent();
        }

        private GeoTimeEntityRawData geoTimeEntityRawData;
        public KWProperty Property;

        public event EventHandler<ValidationResultEventArgs> CheckValidation;
        public event EventHandler<EventArgs> EnterKeyHit;
        public event EventHandler PropertyValueChanged;

        private void OnCheckValidation(ValidationStatus validationStatus)
        {
            CheckValidation?.Invoke(this, new ValidationResultEventArgs(validationStatus));
        }

        private void OnEnterKeyHit()
        {
            EnterKeyHit?.Invoke(this, new EventArgs());

        }

        private void OnPropertyValueChanged()
        {
            PropertyValueChanged?.Invoke(this, EventArgs.Empty);
        }

        public BaseDataTypes DataType
        {
            get => (BaseDataTypes)GetValue(DataTypeProperty);
            set => SetValue(DataTypeProperty, value);
        }

        public static readonly DependencyProperty DataTypeProperty = DependencyProperty.Register(nameof(DataType),
            typeof(BaseDataTypes), typeof(PropertyValueTemplatesControl),
            new PropertyMetadata(BaseDataTypes.None, OnSetDataTypeChanged));

        private static void OnSetDataTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PropertyValueTemplatesControl)d).OnSetDataTypeChanged(e);
        }

        private void OnSetDataTypeChanged(DependencyPropertyChangedEventArgs e)
        {
            if (DataType == BaseDataTypes.None)
                IsEnabled = false;
            else
                IsEnabled = true;

            SetDefaultValue();
            OnCheckValidation(Validation(PropertyValue));
        }

        public string PropertyValue
        {
            get => (string)GetValue(PropertyValueProperty);
            set => SetValue(PropertyValueProperty, value);
        }

        public static readonly DependencyProperty PropertyValueProperty = DependencyProperty.Register(nameof(PropertyValue),
            typeof(string), typeof(PropertyValueTemplatesControl), new PropertyMetadata(string.Empty,
                OnSetPropertyValueChanged));

        private static void OnSetPropertyValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PropertyValueTemplatesControl)d).OnSetPropertyValueChanged(e);
        }

        private void OnSetPropertyValueChanged(DependencyPropertyChangedEventArgs e)
        {
            Validation(PropertyValue);
            OnPropertyValueChanged();
        }

        public bool ReadOnly
        {
            get => (bool)GetValue(ReadOnlyProperty);
            set => SetValue(ReadOnlyProperty, value);
        }

        public static readonly DependencyProperty ReadOnlyProperty = DependencyProperty.Register(nameof(ReadOnly),
            typeof(bool), typeof(PropertyValueTemplatesControl), new PropertyMetadata(false));

        public ValidationStatus ValidationStatus
        {
            get => (ValidationStatus)GetValue(ValidationStatusProperty);
            set => SetValue(ValidationStatusProperty, value);
        }

        // Using a DependencyProperty as the backing store for ValidationStatus.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValidationStatusProperty =
            DependencyProperty.Register(nameof(ValidationStatus), typeof(ValidationStatus), typeof(PropertyValueTemplatesControl),
                new PropertyMetadata(ValidationStatus.Invalid));

        public string ValidationMessage
        {
            get => (string)GetValue(ValidationMessageProperty);
            set => SetValue(ValidationMessageProperty, value);
        }

        // Using a DependencyProperty as the backing store for ValidationMessage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ValidationMessageProperty =
            DependencyProperty.Register(nameof(ValidationMessage), typeof(string), typeof(PropertyValueTemplatesControl),
                new PropertyMetadata(string.Empty));


        public bool SearchMode
        {
            get { return (bool)GetValue(SearchModeProperty); }
            set { SetValue(SearchModeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SearchMode.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SearchModeProperty =
            DependencyProperty.Register(nameof(SearchMode), typeof(bool), typeof(PropertyValueTemplatesControl),
                new PropertyMetadata(false));



        /// <summary>
        /// آماده‌سازی کنترل برای استفاده
        /// نوع ویژگی را دریافت می‌کند و قالب مناسب با آن را نمایش می‌دهد
        /// </summary>
        /// <param name="dataType">نوع ویژگی</param>
        public void PrepareControl(BaseDataTypes dataType)
        {
            ResetControl();
            DataType = dataType;
        }

        /// <summary>
        /// مقدار ویژگی را بر‌می‌گرداند به صورت رشته 
        /// </summary>
        /// <returns></returns>
        public string GetPropertyValue()
        {
            string stringValue = string.Empty;

            if (DataType == BaseDataTypes.GeoTime)
            {
                geoTimeEntityRawData = DateRangeAndLocationControl.GetInvariantGeoTimeValue();
                if (GeoTime.IsValidGeoTime(GeoTime.GetGeoTimeStringValue(geoTimeEntityRawData), out _, null, null, null, null).Status == ValidationStatus.Valid)
                {
                    stringValue = GeoTime.GetGeoTimeStringValue(DateRangeAndLocationControl.GetGeoTimeValue());
                }
            }
            else if (DataType == BaseDataTypes.GeoPoint)
            {
                if (GeoPoint.IsValidGeoPoint(LocationPropertyValueControl.InvariantValue, out _).Status == ValidationStatus.Valid)
                {
                    stringValue = GeoPoint.GetGeoPointStringValue(new GeoLocationEntityRawData()
                    {
                        Latitude = LocationPropertyValueControl.Latitude,
                        Longitude = LocationPropertyValueControl.Longitude
                    });
                }
            }
            else
            {
                ValueBaseValidation.TryParsePropertyValue(DataType, PropertyValue, out var parsedValue, CultureInfo.CurrentCulture);
                stringValue = string.Format(CultureInfo.CurrentCulture, "{0}", parsedValue);
            }

            return stringValue;
        }

        /// <summary>
        /// کنترل را به حالت اولیه بر‌می‌گرداند
        /// </summary>
        public void ResetControl()
        {
            SetDefaultValue();

            DataType = BaseDataTypes.None;
            ValidationStatus = ValidationStatus.Valid;
            WarningInvalidDateTextBlock.Visibility = Visibility.Collapsed;
        }

        private void SetDefaultValue()
        {
            if (DataType == BaseDataTypes.GeoPoint)
            {
                LocationPropertyValueControl.ResetValues();
            }
            else if (DataType == BaseDataTypes.GeoTime)
            {
                DateRangeAndLocationControl.ResetValues();
            }
            else if (DataType == BaseDataTypes.Boolean)
            {
                PropertyValue = false.ToString();
            }
            else if (DataType == BaseDataTypes.Int || DataType == BaseDataTypes.Long || DataType == BaseDataTypes.Double)
            {
                PropertyValue = (0).ToString();
            }
            else if (DataType == BaseDataTypes.DateTime)
            {
                PropertyValue = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).ToString(CultureInfo.CurrentCulture);
            }
            else if(DataType == BaseDataTypes.HdfsURI || DataType == BaseDataTypes.String)
            {
                PropertyValue = string.Empty;
            }
        }

        /// <summary>
        /// برای انتساب مقدار اولیه ویژگی از این متد استفاده
        /// می‌شود
        /// </summary>
        /// <param name="property">ویژگی</param>
        /// <param name="readOnly">خاصیت فقط خواندنی</param>
        public void SetPropertyValue(KWProperty property, bool readOnly)
        {
            ResetControl();

            ReadOnly = readOnly;
            DataType = OntologyProvider.GetBaseDataTypeOfProperty(property.TypeURI);
            Property = property;

            ValidationStatus validationStatus = Validation(property.Value);
            if (validationStatus != ValidationStatus.Valid)
            {
                InvalidValueTextBlock.Text = property.Value;
                InvalidValueStackPanel.Visibility = Visibility.Visible;
                OnCheckValidation(validationStatus);
                return;
            }

            if (DataType == BaseDataTypes.GeoTime)
            {
                SetGeoTimePropertyValue(property as GeoTimeKWProperty);
            }
            else if (DataType == BaseDataTypes.GeoPoint)
            {
                SetGeoPointPropertyValue(property as GeoPointKWProperty);
            }
            else
            {
                SetOtherPropertyValue(property.Value);
            }
        }

        /// <summary>
        /// برای انتساب مقدار اولیه ویژگی از این متد استفاده
        /// می‌شود
        /// </summary>
        /// <param name="propertyValue">مقدار ویژگی</param>
        /// <param name="typeUri">نوع ویژگی</param>
        public void SetPropertyValue(string propertyValue, string typeUri)
        {
            if (string.IsNullOrEmpty(typeUri) || string.IsNullOrWhiteSpace(typeUri))
            {
                DataType = BaseDataTypes.None;
            }
            else
            {
                DataType = OntologyProvider.GetBaseDataTypeOfProperty(typeUri);

                ValidationStatus validationStatus = Validation(propertyValue);
                if (validationStatus != ValidationStatus.Valid)
                {
                    InvalidValueTextBlock.Text = propertyValue;
                    InvalidValueStackPanel.Visibility = Visibility.Visible;
                    OnCheckValidation(validationStatus);
                    return;
                }

                SetOtherPropertyValue(propertyValue);
            }
        }

        private void SetOtherPropertyValue(string propertyValue)
        {
            ValueBaseValidation.TryParsePropertyValue(DataType, propertyValue, out var parsedValue, CultureInfo.CurrentCulture);

            PropertyValue = string.Format(CultureInfo.CurrentCulture, "{0}", parsedValue);
        }

        private void SetGeoTimePropertyValue(GeoTimeKWProperty property)
        {
            DateRangeAndLocationControl.SetDateRangeAndLocationValues
                (property.GeoTimeValue.Latitude, property.GeoTimeValue.Longitude,
                property.GeoTimeValue.TimeBegin, property.GeoTimeValue.TimeEnd);
        }

        private void SetGeoPointPropertyValue(GeoPointKWProperty property)
        {
            LocationPropertyValueControl.Latitude = property.GeoLocationValue.Latitude;
            LocationPropertyValueControl.Longitude = property.GeoLocationValue.Longitude;
        }

        private void PropertyValueTxtBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            OnCheckValidation(Validation(((TextBox)sender).Text));
        }

        private void DateTimeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (DataType != BaseDataTypes.DateTime)
                return;
            OnCheckValidation(Validation(((TextBox)sender).Text));
        }

        private void TrueRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            OnCheckValidation(Validation(((RadioButton)sender).IsChecked.ToString()));
        }

        private void FalseRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            OnCheckValidation(Validation(((RadioButton)sender).IsChecked.ToString()));
        }

        private void DateRangeAndLocationControl_ValidationChecked(object sender, DateRangeAndLocationControl.ValidationEventArgs e)
        {
            ValidationMessage = e.ValidationResult.ToString();
            ValidationStatus = e.ValidationResult;
            OnCheckValidation(e.ValidationResult);
        }

        /// <summary>
        /// اعتبار سنجی مقدار ویژگی
        /// </summary>
        /// <param name="value">مقدار مورد نظر</param>
        /// <returns></returns>
        private ValidationStatus Validation(string value)
        {
            InvalidValueStackPanel.Visibility = Visibility.Collapsed;
            ValidationProperty validationResult = new ValidationProperty() { Message = "", Status = ValidationStatus.Valid };

            if (DataType != BaseDataTypes.None)
            {

                if (string.IsNullOrEmpty(value) && DataType == BaseDataTypes.String)
                {
                    WarningInvalidDateTextBlock.Text = "Invalid .. not empty value to save.";
                    WarningInvalidDateTextBlock.Visibility = Visibility.Visible;
                }
                try
                {
                    if (DataType == BaseDataTypes.GeoPoint)
                        validationResult = LocationPropertyValueControl.Validation;
                    else
                        validationResult = PropertyManager.IsPropertyValid(DataType, value, CultureInfo.CurrentCulture);
                }
                catch (Exception ex)
                {
                    validationResult = new ValidationProperty()
                    {
                        Status = ValidationStatus.Invalid,
                        Message = ex.Message
                    };
                }
                WarningInvalidDateTextBlock.Text = validationResult.Message;

                switch (validationResult.Status)
                {
                    case ValidationStatus.Invalid:
                        WarningInvalidDateTextBlock.Visibility = Visibility.Visible;
                        break;

                    case ValidationStatus.Valid:
                        WarningInvalidDateTextBlock.Visibility = Visibility.Collapsed;
                        break;

                    case ValidationStatus.Warning:
                        WarningInvalidDateTextBlock.Visibility = Visibility.Visible;
                        break;
                }
            }

            ValidationMessage = validationResult.Message;
            ValidationStatus = validationResult.Status;
            return validationResult.Status;
        }

        /// <summary>
        /// مدیریت رویداد کلید‌های
        /// Enter و
        /// Ctrl + Enter 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainGrid_OnKeyDown(object sender, KeyEventArgs e)
        {
            HandelEnterKey(e);
        }

        private void DateTimeTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            HandelEnterKey(e);
        }

        private void HandelEnterKey(KeyEventArgs eventArgs)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && eventArgs.Key == Key.Enter)
            {
                var caretIndex = PropertyValueTxtBox.CaretIndex;
                PropertyValueTxtBox.Text = PropertyValueTxtBox.Text.Insert(caretIndex, Environment.NewLine);
                PropertyValueTxtBox.CaretIndex = caretIndex + 1;
                eventArgs.Handled = true;
            }
            else if (eventArgs.Key == Key.Enter)
            {
                if (ValidationStatus == ValidationStatus.Valid)
                {
                    OnEnterKeyHit();
                }

                eventArgs.Handled = true;
            }
        }

        private void DateTimeDatePicker_OnSelectedDateChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (DataType != BaseDataTypes.DateTime)
                return;
            OnCheckValidation(Validation(((DateTimePicker.DateTimePicker)sender).Text));
        }

        private void LocationPropertyValueControl_ValidationChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ValidationMessage = LocationPropertyValueControl?.Validation?.Message;
            ValidationStatus = LocationPropertyValueControl.Validation.Status;
            OnCheckValidation(LocationPropertyValueControl.Validation.Status);
        }

        private void LocationPropertyValueControl_ValueChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            PropertyValue = LocationPropertyValueControl.Value;
            OnCheckValidation(Validation(LocationPropertyValueControl.Value));
        }
    }
}
