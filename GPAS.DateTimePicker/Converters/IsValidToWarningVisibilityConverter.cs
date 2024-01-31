using GPAS.PropertiesValidation;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GPAS.DateTimePicker.Converters
{
    class IsValidToWarningVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ValidationProperty Isvalid = (ValidationProperty)value;
            if (Isvalid?.Status == ValidationStatus.Invalid || Isvalid?.Status == ValidationStatus.Warning)
            {
                return Visibility.Visible;
            }
            else
            {
                return Visibility.Collapsed;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}