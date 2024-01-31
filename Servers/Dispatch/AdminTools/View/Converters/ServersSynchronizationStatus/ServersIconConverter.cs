using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Dispatch.AdminTools.View.Converters.ServersSynchronizationStatus
{
    public class ServersIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (bool)value)
            {
                return MaterialDesignThemes.Wpf.PackIconKind.Server;
            }

            return MaterialDesignThemes.Wpf.PackIconKind.ServerOff;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
