using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Helpers.Graph
{
    public class MiliSecondsToSpeedSliderValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || !(parameter is double[]) || (parameter as double[]).Length != 2)
            {
                return value;
            }

            int val = (int)value;
            double min = (parameter as double[])[0];
            double max = (parameter as double[])[1];

            return max - val + min;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || !(parameter is double[]) || (parameter as double[]).Length != 2)
            {
                return value;
            }

            double val = (double)value;
            double min = (parameter as double[])[0];
            double max = (parameter as double[])[1];

            return (int)(max - val + min);
        }
    }
}