using GPAS.Workspace.Presentation.Controls.DataImport.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Converters
{
    public class CollectionImportStatusToBooleanConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[2] is IEnumerable<IDataSource> collection)
            {
                if (collection.Count() == 1)
                {
                    return values[0] is bool booleanValue && booleanValue && values[1] is DataSourceImportStatus importStatus &&
                        importStatus == DataSourceImportStatus.Ready;
                }
                else
                {
                    return values[0] is bool booleanValue && booleanValue && 
                        collection.Any(x => x.ImportStatus == DataSourceImportStatus.Ready);
                }
            }

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
