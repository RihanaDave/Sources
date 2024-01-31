using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.ObjectExplorer.Convertor
{
    public class ScreenStatusToToolTipStringConverter : IValueConverter
    {
        ResourceDictionary iconsResource = new ResourceDictionary();
        public object Convert(object value, Type targetType, object para, CultureInfo culture)
        {
            if ((ScreenStatus)value == ScreenStatus.fullScreen)
            {
                return "Normal";
            }
            else if ((ScreenStatus)value == ScreenStatus.normal)
            {
                return "Full screen";
            }
            else
            {
                throw new Exception();
            }
        }
        public object ConvertBack(object value, Type targetType, object para, CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}
