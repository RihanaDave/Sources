using GPAS.Workspace.Presentation.Windows;
using MaterialDesignThemes.Wpf;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Applications.Convertors
{
    public class BooleanToSelectionBackgroundColorConverter : IMultiValueConverter
    {
        PaletteHelper paletteHelper = new PaletteHelper();
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            ITheme theme = paletteHelper.GetTheme();
            IBaseTheme baseTheme = null;

            if (values[1] is ThemeApplication && (ThemeApplication)values[1] == ThemeApplication.Dark)
            {
                baseTheme = new MaterialDesignDarkTheme();
                theme.SetBaseTheme(baseTheme);
            }
            else
            {
                baseTheme = new MaterialDesignLightTheme();
                theme.SetBaseTheme(baseTheme);

                theme.Paper = (Color)ColorConverter.ConvertFromString("#EFEFEF");
                theme.CardBackground = (Color)ColorConverter.ConvertFromString("#DFDFDF");
            }
            if (values[0] is bool)
            {
                if ((bool)values[0])
                {
                    return new SolidColorBrush(theme.PrimaryMid.Color);
                }
                else
                {
                    return new SolidColorBrush(theme.Paper);
                }
            }
            return new SolidColorBrush(theme.Paper);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
