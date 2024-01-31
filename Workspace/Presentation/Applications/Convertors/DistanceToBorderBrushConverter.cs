using System;
using System.Globalization;
using System.Windows.Data;
using Brushes = System.Windows.Media.Brushes;

namespace GPAS.Workspace.Presentation.Applications.Convertors
{
    public class DistanceToBorderBrushConverter :IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value!=null && value is double numberofAnalysPic)
            {
                if (numberofAnalysPic<=1.2)
                {
                    return Brushes.Green;
                }
                else if (numberofAnalysPic>1.2 && numberofAnalysPic<1.3)
                {
                    return Brushes.Yellow;
                }
            }
            return Brushes.Red;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
