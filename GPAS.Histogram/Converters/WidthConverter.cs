using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Histogram.Converters
{
    public class WidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return 0;

            double.TryParse(value.ToString(), NumberStyles.Float, culture, out double val);
            double.TryParse(parameter.ToString(), NumberStyles.Float, culture, out double par);

            return val * par;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
