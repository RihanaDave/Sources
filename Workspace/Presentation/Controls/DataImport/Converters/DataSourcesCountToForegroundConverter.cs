using MaterialDesignThemes.Wpf;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Converters
{
    public class DataSourcesCountToForegroundConverter : IValueConverter
    {
        readonly PaletteHelper paletteHelper = new PaletteHelper();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return new SolidColorBrush(Colors.Red);
            }

            ITheme theme = paletteHelper.GetTheme();

            if ((int)value == 0)
            {
                return new SolidColorBrush(Colors.Red);
            }

            return new SolidColorBrush(theme.PrimaryMid.Color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
