using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Utility
{
    public class DoubleToDistanceConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string result = string.Empty;
            if(value is double)
            {
                string convertedValue = string.Empty;
                string unit = string.Empty;
                if ((double)value < 1000)
                {
                    convertedValue = ((double)value).ToString("N2");
                    unit = "m";
                }
                else
                {
                    convertedValue = ((double)value / 1000).ToString("N2");
                    unit = "km";
                }
                result = $"{convertedValue} {unit}";
            }
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
