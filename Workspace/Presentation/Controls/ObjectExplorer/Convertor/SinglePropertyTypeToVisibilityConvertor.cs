using GPAS.Workspace.Presentation.Controls.ObjectExplorer.Histograms;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.ObjectExplorer.Convertor
{
    public class SinglePropertyTypeToVisibilityConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object para, CultureInfo culture)
        {
            if ((HistogramSuperCategoryType)value == HistogramSuperCategoryType.SingleProperty)
                return Visibility.Visible;
            else return Visibility.Collapsed;
        }
        public object ConvertBack(object value, Type targetType, object para, CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}
