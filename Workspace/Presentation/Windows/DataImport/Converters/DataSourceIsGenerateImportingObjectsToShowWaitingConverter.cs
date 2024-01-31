using GPAS.Workspace.Presentation.Controls.DataImport.Model;
using GPAS.Workspace.Presentation.Controls.Waiting;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Windows.DataImport.Converters
{
    public class DataSourceIsGenerateImportingObjectsToShowWaitingConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length >= 4 &&
                values[0] is IDataSource dataSource &&
                values[1] is DataSourceImportStatus ImportingObjectsGenerationStatus &&
                values[2] is DataSourceImportStatus importStatus &&
                values[3] is WaitingControl waitingControl)
            {
                if ((dataSource is ITabularDataSource && 
                    (ImportingObjectsGenerationStatus == DataSourceImportStatus.Importing || 
                    ImportingObjectsGenerationStatus == DataSourceImportStatus.InQueue ||
                    ImportingObjectsGenerationStatus == DataSourceImportStatus.Transforming ||
                    ImportingObjectsGenerationStatus == DataSourceImportStatus.Publishing)) ||
                    (importStatus == DataSourceImportStatus.Importing || 
                    importStatus == DataSourceImportStatus.InQueue ||
                    importStatus == DataSourceImportStatus.Transforming ||
                    importStatus == DataSourceImportStatus.Publishing))
                {
                    if (importStatus == DataSourceImportStatus.Importing)
                    {
                        waitingControl.Message = "Uploading";
                    }
                    else if(importStatus == DataSourceImportStatus.Transforming || 
                        importStatus == DataSourceImportStatus.InQueue ||
                        importStatus == DataSourceImportStatus.Publishing)
                    {
                        waitingControl.Message = importStatus.ToString();
                    }
                    else
                    {
                        waitingControl.Message = "Generating Objects";
                    }
                    waitingControl.TaskIncrement();
                    return Visibility.Visible;
                }
                else
                {
                    int c = waitingControl.TasksCount;
                    for (int i = c; i > 0; i--)
                    {
                        waitingControl.TaskDecrement();
                    }

                    waitingControl.Message = "";
                    return Visibility.Collapsed;
                }
            }

            return Visibility.Visible;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
