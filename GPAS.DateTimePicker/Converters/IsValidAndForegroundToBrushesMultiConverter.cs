using GPAS.PropertiesValidation;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.DateTimePicker.Converters
{
    public class IsValidAndForegroundToBrushesMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] != null)
            {
                ValidationProperty Isvalid = (ValidationProperty)values[1];

                if (Isvalid?.Status == ValidationStatus.Valid)
                {
                    return values[0];
                }
                else if (Isvalid?.Status == ValidationStatus.Warning)
                {
                    return Brushes.OrangeRed;
                }
                else
                {
                    return Brushes.Red;
                }
            }
            return Brushes.Red;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
