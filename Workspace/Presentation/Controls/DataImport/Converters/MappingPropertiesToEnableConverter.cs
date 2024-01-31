using GPAS.Workspace.Presentation.Controls.DataImport.Model;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.Map;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Converters
{
    public class MappingPropertiesToEnableConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null)
                return false;

            if (values[2] is DataSourceImportStatus importStatus && importStatus != DataSourceImportStatus.Ready)
                return false;

            if (!(values[0] is ObservableCollection<PropertyMapModel> propertyMapModels))
                return false;

            return propertyMapModels.Count != 0 && propertyMapModels.Any(propertyMapModel => propertyMapModel.Editable);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
