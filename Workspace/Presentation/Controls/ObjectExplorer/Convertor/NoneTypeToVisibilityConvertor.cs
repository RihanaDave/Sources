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
    public class NoneTypeToVisibilityConvertor : IValueConverter
    {
        public object Convert(object value, Type targetType, object para, CultureInfo culture)
        {
            if ((HistogramSuperCategoryType)value == HistogramSuperCategoryType.None)
                return Visibility.Collapsed;
            else return Visibility.Visible;
        }
        public object ConvertBack(object value, Type targetType, object para, CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}
