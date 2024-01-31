using GPAS.PropertiesValidation;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.FilterSearchCriterias.Converters
{
    public class ValidationStatusToVisibilityErrorBoxConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ValidationStatus)
            {
                ValidationStatus validationStatus = (ValidationStatus)value;
                if (validationStatus == ValidationStatus.Valid)
                {
                    return Visibility.Collapsed;
                }
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
