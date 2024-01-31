using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.ObjectExplorer.Convertor
{
    class ScreenStatusToIconPathConvertor : IValueConverter
    {
        ResourceDictionary iconsResource = new ResourceDictionary();        
        public object Convert(object value, Type targetType, object para, CultureInfo culture)
        {
            if ((ScreenStatus)value == ScreenStatus.fullScreen)
            {
                return MaterialDesignThemes.Wpf.PackIconKind.FullscreenExit;
            }
            else if ((ScreenStatus)value == ScreenStatus.normal)
            {
                return MaterialDesignThemes.Wpf.PackIconKind.Fullscreen;
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
