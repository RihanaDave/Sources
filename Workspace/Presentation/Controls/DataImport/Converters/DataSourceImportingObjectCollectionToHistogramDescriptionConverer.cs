using GPAS.Workspace.Presentation.Controls.DataImport.Model;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Converters
{
    public class DataSourceImportingObjectCollectionToHistogramDescriptionConverer : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int objectCount = 0;
            int propertiesCount = 0;

            if (value is ObservableCollection<ImportingObject> importingObjectCollection)
            {
                objectCount = importingObjectCollection.Count;
                propertiesCount = importingObjectCollection.Sum(x => x.Properties.Count);
            }

            return $"Contain {objectCount} objects and {propertiesCount} properties";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
