using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Dispatch.AdminTools.View
{
    public class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && parameter != null && (SidebarItems)parameter == (SidebarItems)value)
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
