using GPAS.PropertiesValidation;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Controls.PropertyValueTemplates
{
    public class ValidationStatusToForegroundConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is ValidationStatus validationStatus)
            {
                switch (validationStatus)
                {
                    case ValidationStatus.Valid:
                        return (SolidColorBrush)values[1];
                    case ValidationStatus.Warning:
                        return new SolidColorBrush(Colors.OrangeRed);
                    case ValidationStatus.Invalid:
                        return new SolidColorBrush(Colors.Red);
                }
            }

            return new SolidColorBrush(Colors.Red);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
