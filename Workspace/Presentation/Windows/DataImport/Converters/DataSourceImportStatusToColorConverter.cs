using GPAS.Workspace.Presentation.Controls.DataImport.Model;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Windows.DataImport.Converters
{
    public class DataSourceImportStatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DataSourceImportStatus importStatus)
            {
                switch (importStatus)
                {
                    case DataSourceImportStatus.Completed:
                        return Brushes.YellowGreen;
                    case DataSourceImportStatus.Pause:
                        return Brushes.DimGray;
                    case DataSourceImportStatus.Stop:
                        return Brushes.Violet;
                    case DataSourceImportStatus.Failure:
                        return Brushes.Red;
                    case DataSourceImportStatus.Incomplete:
                        return Brushes.LightSkyBlue;
                    case DataSourceImportStatus.Warning:
                        return Brushes.Orange;
                    case DataSourceImportStatus.Ready:
                    case DataSourceImportStatus.Importing:
                    case DataSourceImportStatus.InQueue:
                    case DataSourceImportStatus.Transforming:
                    case DataSourceImportStatus.Publishing:
                    default:
                        break;
                }
            }

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
