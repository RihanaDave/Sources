using GPAS.PropertiesValidation;
using MaterialDesignThemes.Wpf;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.DateTimePicker.Converters
{
    public class IsValidToBrushConverter : IValueConverter
    {
        readonly PaletteHelper paletteHelper = new PaletteHelper();
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {   
            ValidationProperty Isvalid = (ValidationProperty)value;
            if (Isvalid?.Status == ValidationStatus.Valid)
            {
                ITheme theme = paletteHelper.GetTheme();
                return new SolidColorBrush(theme.Body);
            }
            else if (Isvalid?.Status == ValidationStatus.Warning)
            {
                return Brushes.Orange;
            }
            else 
            {
                return Brushes.Red;
            }
           
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
