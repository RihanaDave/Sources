using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Histogram.Converters
{
    public class HistogramItemToMaxValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 2)
            {
                if (values[0] is HistogramItem histogramItem)
                    return (double)histogramItem.MaxValue;
                else if(values[1] is long maxValue)
                    return (double)maxValue;
            }
            else if (values.Length == 1 && values[0] is HistogramItem histogramItem)
            {
                return histogramItem.MaxValue;
            }

            return 100.0;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
