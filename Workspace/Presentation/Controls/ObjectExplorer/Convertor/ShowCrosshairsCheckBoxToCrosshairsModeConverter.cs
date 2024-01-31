using GPAS.BarChartViewer;
using System;
using System.Globalization;
using System.Windows.Data;
namespace GPAS.Workspace.Presentation.Controls.ObjectExplorer.Convertor
{
    public class ShowCrosshairsCheckBoxToCrosshairsModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object para, CultureInfo culture)
        {
            if ((bool)value)
                return OrientationStatus.Both;
            else return OrientationStatus.None;
        }
        public object ConvertBack(object value, Type targetType, object para, CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}
