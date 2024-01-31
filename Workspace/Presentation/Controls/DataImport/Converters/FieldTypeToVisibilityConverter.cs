using GPAS.Workspace.Presentation.Controls.DataImport.Model.DataSourceField;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Converters
{
    public class FieldTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
                return Visibility.Collapsed;

            if ((FieldType)parameter == FieldType.Const)
            {
                return (FieldType)value == FieldType.Const ? Visibility.Visible : Visibility.Collapsed;
            }

            return (FieldType)value != FieldType.Const ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
