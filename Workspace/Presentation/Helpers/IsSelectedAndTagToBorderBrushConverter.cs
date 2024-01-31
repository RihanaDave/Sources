using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using MaterialDesignThemes.Wpf;

namespace GPAS.Workspace.Presentation.Helpers
{
    public class IsSelectedAndTagToBorderBrushConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            PaletteHelper paletteHelper = new PaletteHelper();
            ITheme theme = paletteHelper.GetTheme();

            if (values?[0] == null || values[1] == null)
                return new SolidColorBrush(theme.CardBackground);

            bool myBool;
            if (Boolean.TryParse(values[1].ToString(), out myBool))
            {
                if ((bool)values[0] && myBool)
                {
                    return new SolidColorBrush(theme.PrimaryMid.Color);
                }
            }

            return new SolidColorBrush(theme.CardBackground);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
