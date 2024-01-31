using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace GPAS.Dispatch.AdminTools.View.Converters
{
    public class PageCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null) return value.ToString();
            throw new ArgumentNullException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToInt32(((ComboBoxItem)value)?.Content);
        }
    }
}
