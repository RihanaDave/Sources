using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace GPAS.ObjectExplorerHistogramViewer.Convertors
{
    public class KeyValueTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object para, CultureInfo culture)
        {
            if ((HisogramKeyValueType)value == HisogramKeyValueType.KeyValue)
                return Visibility.Visible;
            else return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object para, CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}
