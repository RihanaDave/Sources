using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.Waiting
{
    public class ProgressShapeToVisibilityConverter : IValueConverter
    {
        public Visibility CircularValue { get; set; } = Visibility.Visible;
        public Visibility LinearValue { get; set; } = Visibility.Collapsed;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ProgressShape progressShape)
            {
                if (progressShape == ProgressShape.Circular)
                    return CircularValue;
                else if (progressShape == ProgressShape.Linear)
                    return LinearValue;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
