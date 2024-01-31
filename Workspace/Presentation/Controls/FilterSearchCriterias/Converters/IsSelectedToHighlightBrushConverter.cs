using MaterialDesignThemes.Wpf;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Controls.FilterSearchCriterias.Converters
{
    public class IsSelectedToHighlightBrushConverter : IMultiValueConverter
    {
        private readonly PaletteHelper paletteHelper = new PaletteHelper();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            ITheme theme = paletteHelper.GetTheme();

            if (values[0] is bool selected)
            {
                return selected ? new SolidColorBrush(theme.PrimaryMid.Color) : new SolidColorBrush(theme.Body);
            }

            return new SolidColorBrush(theme.Body);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
