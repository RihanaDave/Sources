using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.Common.Converters
{
    public class ValueTypeEqualityWithParameterTypeToVisibilityConverter : IValueConverter
    {
        private bool FindTypeInAncestorType(Type baseType, Type item)
        {
            if (baseType == item)
                return true;

            if (baseType == null)
                return false;

            return FindTypeInAncestorType(baseType.BaseType, item);
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null && parameter == null)
                return Visibility.Visible;

            if (value == null || parameter == null)
                return Visibility.Collapsed;

            Type paramType = parameter is Type ? (Type)parameter : parameter.GetType();
            Type valueType = value.GetType();

            return FindTypeInAncestorType(valueType, paramType) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
