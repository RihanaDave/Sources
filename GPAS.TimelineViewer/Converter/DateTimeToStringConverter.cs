using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.TimelineViewer.Converter
{
    internal class DateTimeToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if (parameter is string)
                    return ((DateTime)value).ToString(parameter.ToString());

                return ((DateTime)value).ToString();
            }
            catch
            {
                return ((DateTime)value).ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}