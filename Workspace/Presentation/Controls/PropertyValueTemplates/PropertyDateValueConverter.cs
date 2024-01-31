using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.PropertyValueTemplates
{
    public class PropertyDateValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime dateValue;

            if (value != null && DateTime.TryParse(value.ToString(), CultureInfo.CurrentCulture, DateTimeStyles.None, out dateValue))
                return dateValue;

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string nn = value != null ? ((DateTime)value).ToString(CultureInfo.CurrentCulture) : string.Empty;
            return nn;
        }
    }
}
