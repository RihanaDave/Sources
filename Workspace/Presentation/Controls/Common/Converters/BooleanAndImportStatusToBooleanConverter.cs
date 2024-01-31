using GPAS.Workspace.Presentation.Controls.DataImport.Model;
using System;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.Common.Converters
{
    public class BooleanAndImportStatusToBooleanConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return values[0] is bool booleanValue && booleanValue && values[1] is DataSourceImportStatus importStatus &&
                importStatus == DataSourceImportStatus.Ready;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
