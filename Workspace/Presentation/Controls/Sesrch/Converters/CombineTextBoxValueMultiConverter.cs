using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.Sesrch.Converters
{
    class CombineTextBoxValueMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            bool Temp = false;
            foreach (var item in values)
            {
                if (!string.IsNullOrEmpty(item.ToString()))
                {
                    Temp = true;
                    break;
                }
            }

            return Temp;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
