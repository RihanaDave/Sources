using GPAS.Workspace.Presentation.Controls.DataImport.Model;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Converters
{
    public class GroupDataSourceTypeToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || parameter == null)
            {
                return Visibility.Collapsed;
            }
            
            if (bool.Parse(parameter.ToString()))
            {
                return value is GroupDataSourceModel ? Visibility.Visible : Visibility.Collapsed;
            }

            return value is GroupDataSourceModel ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
