using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Dispatch.AdminTools.View.Converters.ServersStatus
{
    public class ServerStatusMessageIconConverter :IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null && (bool)value
                ? MaterialDesignThemes.Wpf.PackIconKind.CheckCircleOutline
                : MaterialDesignThemes.Wpf.PackIconKind.MinusCircleOutline;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
