using GPAS.Workspace.Presentation.Controls.DataImport.Model;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Windows.DataImport.Converters
{
    public class DataSourceImportStatusToIconVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DataSourceImportStatus importStatus)
            {
                if (importStatus == DataSourceImportStatus.Completed ||
                    importStatus == DataSourceImportStatus.Failure ||
                    importStatus == DataSourceImportStatus.Pause ||
                    importStatus == DataSourceImportStatus.Stop ||
                    importStatus == DataSourceImportStatus.Incomplete ||
                    importStatus == DataSourceImportStatus.Warning)
                {
                    return Visibility.Visible;
                }
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
