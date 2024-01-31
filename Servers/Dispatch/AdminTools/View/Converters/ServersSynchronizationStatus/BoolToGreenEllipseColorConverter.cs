using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.Dispatch.AdminTools.View.Converters.ServersSynchronizationStatus
{
    public class BoolToGreenEllipseColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object para, CultureInfo culture)
        {
            if ((bool)value)
                return Brushes.Green;

            else return Brushes.White;
        }
        public object ConvertBack(object value, Type targetType, object para, CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}
