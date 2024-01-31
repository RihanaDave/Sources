using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.DateTimePicker.Converters
{
    public class DateTimeToStrintCoverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                return value.ToString();
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new Exception();
        }
    }
}
