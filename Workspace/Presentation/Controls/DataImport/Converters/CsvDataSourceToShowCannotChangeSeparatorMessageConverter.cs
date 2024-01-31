using GPAS.Workspace.Presentation.Controls.DataImport.Model;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Converters
{
    public class CsvDataSourceToShowCannotChangeSeparatorMessageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is CsvDataSourceModel csvDataSource)
            {
                return csvDataSource.CanSeparatorChange() ? Visibility.Collapsed : Visibility.Visible;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
