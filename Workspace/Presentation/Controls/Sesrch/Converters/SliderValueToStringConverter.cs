using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.Sesrch.Converters
{
    public class SliderValueToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value!=null && (double)value!=0)
            {
                return Math.Ceiling((double)value);
            }
            else
            {
                return 0;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (double)value != 0)
            {
                return Math.Ceiling((double)value);
            }
            else
            {
                return 0;
            }
        }
    }
}
