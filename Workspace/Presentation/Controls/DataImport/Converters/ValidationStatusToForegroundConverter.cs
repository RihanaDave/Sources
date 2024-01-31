using GPAS.PropertiesValidation;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Converters
{
    public class ValidationStatusToForegroundConverter :IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            switch ((ValidationStatus)value)
            {
                case ValidationStatus.Invalid:
                    return new SolidColorBrush(Colors.OrangeRed);
                case ValidationStatus.Warning:
                    return new SolidColorBrush(Colors.DarkOrange);
                case ValidationStatus.Valid:
                    return new SolidColorBrush(Colors.Green);
                default:
                    return new SolidColorBrush(Colors.OrangeRed);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
