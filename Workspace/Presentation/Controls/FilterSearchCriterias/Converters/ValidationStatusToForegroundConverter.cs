using GPAS.PropertiesValidation;
using MaterialDesignThemes.Wpf;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Controls.FilterSearchCriterias.Converters
{
    public class ValidationStatusToForegroundConverter : IMultiValueConverter
    {
        private readonly PaletteHelper paletteHelper = new PaletteHelper();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            ITheme theme = paletteHelper.GetTheme();

            if (values[0] is ValidationStatus validationStatus)
            {               
                if (validationStatus == ValidationStatus.Valid)
                    return new SolidColorBrush(theme.Body);

                if (validationStatus == ValidationStatus.Invalid)
                    return new SolidColorBrush(Colors.Red);
                else
                    return new SolidColorBrush(Colors.OrangeRed);
            }

            return new SolidColorBrush(Colors.Red);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
