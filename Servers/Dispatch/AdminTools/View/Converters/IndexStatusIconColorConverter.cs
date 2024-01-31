using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.Dispatch.AdminTools.View.Converters
{
    public class IndexStatusIconColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null )
            {
                return new SolidColorBrush(Colors.DimGray);
            }

            if ((bool)value)
            {
                return new SolidColorBrush(Colors.Green);
            }

            return new SolidColorBrush(Colors.Red);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
