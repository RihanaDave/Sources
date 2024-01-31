using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.PropertyValueTemplates
{
    public class PropertyBooleanValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result;
            if (value == null || !bool.TryParse(value.ToString(), out result)) return false;

            if (parameter != null && bool.Parse(parameter.ToString()))
            {
                return result;
            }

            return !result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool result = value != null && (bool) value;

            if (parameter != null && bool.Parse(parameter.ToString()))
            {
                return result.ToString();
            }

            var falseResult = !result;
            return falseResult.ToString();
        }
    }
}
