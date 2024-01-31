using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.Common.Converters
{
    public class ValuesEqualityToVisibilityConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null || values[1] == null)
                return Visibility.Hidden;

            return values[0].Equals(values[1]) ? Visibility.Visible : Visibility.Hidden;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
