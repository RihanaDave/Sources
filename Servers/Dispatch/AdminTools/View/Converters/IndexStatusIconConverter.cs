using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Dispatch.AdminTools.View.Converters
{
    public class IndexStatusIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return MaterialDesignThemes.Wpf.PackIconKind.ServerOff;
            }

            if ((bool)value)
            {
                return MaterialDesignThemes.Wpf.PackIconKind.TickCircleOutline;
            }

            return MaterialDesignThemes.Wpf.PackIconKind.CloseCircleOutline;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
