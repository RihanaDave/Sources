using GPAS.HistogramViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.Histograms
{
    public class IntToHistogramPropertyNodeOrderByConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is int)
            {
                if ((int)value == 0)
                    return HistogramPropertyNodeOrderBy.Count;
                else if ((int)value == 1)
                    return HistogramPropertyNodeOrderBy.Title;
            }
            return HistogramPropertyNodeOrderBy.Count;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            switch ((HistogramPropertyNodeOrderBy)value)
            {
                case HistogramPropertyNodeOrderBy.Count:
                    return 0;
                case HistogramPropertyNodeOrderBy.Title:
                    return 1;
            }
            return 0;
        }
    }
}
