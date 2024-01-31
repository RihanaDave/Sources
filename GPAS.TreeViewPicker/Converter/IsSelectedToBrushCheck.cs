using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.TreeViewPicker.Converter
{
    public class IsSelectedToBrushCheck : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Brush selectedBrush = Brushes.LightSeaGreen;
            Brush deSelectedBrush = new SolidColorBrush(Color.FromRgb(221, 221, 221));
            if (((bool)value == true))
                return selectedBrush;

            return deSelectedBrush;
;        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
