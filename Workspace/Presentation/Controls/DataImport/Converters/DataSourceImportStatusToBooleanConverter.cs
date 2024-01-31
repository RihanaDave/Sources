using GPAS.Workspace.Presentation.Controls.DataImport.Model;
using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Converters
{
    public class DataSourceImportStatusToBooleanConverter : IValueConverter
    {
        public bool EqualValue { get; set; } = true;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is DataSourceImportStatus importStatusValue && parameter is DataSourceImportStatus importStatusParam)
            {
                if (importStatusValue == importStatusParam)
                    return EqualValue;
            }

            return !EqualValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
