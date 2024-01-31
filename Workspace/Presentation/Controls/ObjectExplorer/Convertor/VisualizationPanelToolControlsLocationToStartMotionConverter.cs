using System;
using System.Windows.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Controls.ObjectExplorer.Convertor
{
    class VisualizationPanelToolControlsLocationToStartMotionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object para, CultureInfo culture)
        {
            try
            {
                if ((VisualizationPanelToolControlsLocation)value == VisualizationPanelToolControlsLocation.TopPanel)
                    return true;
                else
                    return false;
            }
            catch
            {
                return false;
            }
        }
        public object ConvertBack(object value, Type targetType, object para, CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}
