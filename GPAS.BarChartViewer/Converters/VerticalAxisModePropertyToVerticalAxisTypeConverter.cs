using System;
using System.Globalization;
using System.Windows.Data;
using Telerik.Windows.Controls.ChartView;

namespace GPAS.BarChartViewer.Converters
{
    public class VerticalAxisModePropertyToVerticalAxisTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if((LinearAxisMode)value == LinearAxisMode.Normal)
            {
                return new LinearAxis();
            }
            else if ((LinearAxisMode)value == LinearAxisMode.Logarithmic)
            {
                return new LogarithmicAxis() { LogarithmBase = 10 };
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
