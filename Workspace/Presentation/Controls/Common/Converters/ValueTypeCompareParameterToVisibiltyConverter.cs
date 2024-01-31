using System;
using System.Linq;
using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace GPAS.Workspace.Presentation.Controls.Common.Converters
{
    public class ValueTypeCompareParameterToVisibiltyConverter : IValueConverter
    {
        public Visibility EqualValue { get; set; } = Visibility.Visible;
        public Visibility NotEqualValue { get; set; } = Visibility.Collapsed;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null && parameter == null)
                return EqualValue;

            if (value == null || !(parameter is Type))
                return NotEqualValue;

            Type valueType = value.GetType();
            Type paramType = parameter as Type;

            if (InheritsFrom(valueType, paramType))
                return EqualValue;
            else
                return NotEqualValue;
        }

        private bool InheritsFrom(Type type, Type baseType)
        {
            // null does not have base type
            if (type == null)
            {
                return false;
            }

            // only interface or object can have null base type
            if (baseType == null)
            {
                return type.IsInterface || type == typeof(object);
            }

            // check implemented interfaces
            if (baseType.IsInterface)
            {
                return type.GetInterfaces().Contains(baseType);
            }

            // check all base types
            var currentType = type;
            while (currentType != null)
            {
                if (currentType.BaseType == baseType)
                {
                    return true;
                }

                currentType = currentType.BaseType;
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
