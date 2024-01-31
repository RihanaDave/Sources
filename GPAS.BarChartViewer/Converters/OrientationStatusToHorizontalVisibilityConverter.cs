using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GPAS.BarChartViewer.Converters
{
    public class OrientationStatusToHorizontalVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((OrientationStatus)value == OrientationStatus.None || (OrientationStatus)value == OrientationStatus.Vertical)
            {
                return Visibility.Collapsed;
            }
            else if ((OrientationStatus)value == OrientationStatus.Horizontal || (OrientationStatus)value == OrientationStatus.Both)
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
