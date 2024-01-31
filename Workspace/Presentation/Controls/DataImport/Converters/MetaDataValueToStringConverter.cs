using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Converters
{
    public class MetaDataValueToStringConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length == 0)
                return values;

            object value = values[0];
            bool needsRecalculation = false;
            if (values.Length >= 2 && values[1] is bool)
                needsRecalculation = (bool)values[1];

            if (value == null)
                return value;

            if (value is string[])
                return string.Join(", ", ((string[])value));

            if (value is long && needsRecalculation)
                return "> " + value;

            return value.ToString();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
