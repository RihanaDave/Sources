using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Graph.GraphViewer.Converters
{
    public class NumberOfInnerVerticesToDisplayBadgeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int number && number < 2)
                return string.Empty;

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
