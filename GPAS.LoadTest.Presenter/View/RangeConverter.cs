using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.LoadTest.Presenter.View
{
    public class RangeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null) return Math.Log10((long)value) - 1;

            throw new ArgumentNullException(nameof(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null) return Math.Pow(10, (int)value + 1);

            throw new ArgumentNullException(nameof(value));
        }
    }
}
