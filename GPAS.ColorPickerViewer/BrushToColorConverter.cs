using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.ColorPickerViewer
{
    public class BrushToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush solidColorBrush = value as SolidColorBrush;
            if (solidColorBrush != null)
            {
                return solidColorBrush.Color;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            SolidColorBrush solidColorBrush = new SolidColorBrush();
            if (value != null)
            {
                solidColorBrush.Color = (Color)value;
            }
            return solidColorBrush;
        }
    }
}
