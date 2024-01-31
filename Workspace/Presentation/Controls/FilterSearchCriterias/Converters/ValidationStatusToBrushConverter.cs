using GPAS.PropertiesValidation;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Controls.FilterSearchCriterias.Converters
{
    public class ValidationStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush validBrush = new SolidColorBrush(Colors.Transparent);
            Brush inValidBrush = new SolidColorBrush(Colors.OrangeRed);
            Brush WarningBrush = new SolidColorBrush(Colors.Orange);

            if (value is ValidationStatus)
            {
                ValidationStatus validationStatus = (ValidationStatus)value;
                if (validationStatus == ValidationStatus.Valid)
                    return validBrush;
                else if (validationStatus == ValidationStatus.Invalid)
                    return inValidBrush;
                else
                    return WarningBrush;
            }

            return inValidBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
