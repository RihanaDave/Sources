using MaterialDesignThemes.Wpf;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Converters
{
    public class BooleanToBorderBrushConverter : IValueConverter
    {
        PaletteHelper palettehelper = new PaletteHelper();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ITheme theme = palettehelper.GetTheme();
            if (value!=null && value is bool isValid)
            {
                if (isValid)
                {
                    return new SolidColorBrush(theme.Body);
                }               
            }
            return new SolidColorBrush(Colors.Red);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
