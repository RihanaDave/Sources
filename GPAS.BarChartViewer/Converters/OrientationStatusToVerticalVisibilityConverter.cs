using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GPAS.BarChartViewer.Converters
{
    public class OrientationStatusToVerticalVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((OrientationStatus)value == OrientationStatus.None || (OrientationStatus)value == OrientationStatus.Horizontal)
            {
                return Visibility.Collapsed;
            }
            else if ((OrientationStatus)value == OrientationStatus.Vertical || (OrientationStatus)value == OrientationStatus.Both)
            {
                return Visibility.Visible;
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
