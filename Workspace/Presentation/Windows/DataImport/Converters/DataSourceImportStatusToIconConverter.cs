using GPAS.Workspace.Presentation.Controls.DataImport.Model;
using MaterialDesignThemes.Wpf;
using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Windows.DataImport.Converters
{
    public class DataSourceImportStatusToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DataSourceImportStatus importStatus)
            {
                switch (importStatus)
                {
                    case DataSourceImportStatus.Completed:
                        return PackIconKind.CheckCircle;
                        break;
                    case DataSourceImportStatus.Pause:
                        return PackIconKind.Pause;
                        break;
                    case DataSourceImportStatus.Stop:
                        return PackIconKind.Stop;
                        break;
                    case DataSourceImportStatus.Failure:
                        return PackIconKind.AlertOctagon;
                        break;
                    case DataSourceImportStatus.Incomplete:
                        return PackIconKind.SettingsTransfer;
                        break;
                    case DataSourceImportStatus.Warning:
                        return PackIconKind.Warning;
                        break;
                    case DataSourceImportStatus.Ready:
                    case DataSourceImportStatus.Importing:
                    case DataSourceImportStatus.InQueue:
                    case DataSourceImportStatus.Publishing:
                    case DataSourceImportStatus.Transforming:
                    default:
                        break;
                }
            }

            return PackIconKind.None;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
