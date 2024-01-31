using System;
using System.Linq;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.Common.Converters
{
    public class AnyValuesAreNotNullToBooleanConverter : IMultiValueConverter
    {
        public bool TrueValue { get; set; } = true;
        public bool FalseValue { get; set; } = false;

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Any(value => value != null))
                return TrueValue;
            else
                return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
