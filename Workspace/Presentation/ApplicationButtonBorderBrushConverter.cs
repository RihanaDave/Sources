using MaterialDesignThemes.Wpf;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation
{
    public class ApplicationButtonBorderBrushConverter : IMultiValueConverter
    {
        private PaletteHelper palettehelper = new PaletteHelper();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] == null || values[1] == null)
                return Brushes.Transparent;

            ITheme theme = palettehelper.GetTheme();

            return (PresentationApplications)values[0] == (PresentationApplications)values[1] ?
                new SolidColorBrush(theme.PrimaryMid.Color) : new SolidColorBrush(theme.Body);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
