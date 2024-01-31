using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Dispatch.AdminTools.View.Converters
{
    public class CollectionCountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return Visibility.Collapsed;
            }

            if (value is int listCount)
            {
                return listCount > 0 ? Visibility.Collapsed : Visibility.Visible;
            }

            return ((IList)value).Count > 0 ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
