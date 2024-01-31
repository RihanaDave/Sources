using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.Common.Converters
{
    public class CollectionCountToVisibilityConverter : IValueConverter
    {
        public Visibility ZeroValue { get; set; } = Visibility.Collapsed;
        public Visibility MoreThanZero { get; set; } = Visibility.Visible;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int listCount)
            {
                if (listCount>0)
                {
                    return MoreThanZero;
                }
            }
            return ZeroValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
