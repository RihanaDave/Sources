using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Dispatch.AdminTools.View.Converters.UserAccountsControlConverter
{
    public class DeleteGroupVisibilityConverter :IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (value.ToString().Equals("Administrators") || value.ToString().Equals("EveryOne")))
            {
                return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
