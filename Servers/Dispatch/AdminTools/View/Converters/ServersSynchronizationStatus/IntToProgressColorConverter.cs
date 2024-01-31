using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.Dispatch.AdminTools.View.Converters.ServersSynchronizationStatus
{
    public class IntToProgressColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object para, CultureInfo culture)
        {
            return value != null && (int)value > 5000000 ? Brushes.DarkRed : Brushes.Indigo;
        }
        public object ConvertBack(object value, Type targetType, object para, CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}
