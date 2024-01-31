using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.TimelineViewer.Converter
{
    internal class BinSizesEnumToDoubleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is BinSizesEnum)
            {
                BinSizesEnum binSizesEnum = (BinSizesEnum)value;
                return (int)(binSizesEnum);
            }
            return (int)BinSizes.Default.BinScale;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int val = int.Parse(Math.Round((double)value).ToString());
            return (BinSizesEnum)val;
        }
    }
}
