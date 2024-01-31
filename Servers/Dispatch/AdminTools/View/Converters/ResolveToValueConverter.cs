using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Dispatch.AdminTools.View.Converters
{
    public class ResolveToValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? "Not Resolved" : value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
