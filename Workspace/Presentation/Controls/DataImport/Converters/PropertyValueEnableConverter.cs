using GPAS.Workspace.Presentation.Controls.DataImport.Model.Map;
using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Converters
{
    public class PropertyValueEnableConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?[0] is ObjectMapModel && values?[1] is PropertyMapModel propertyMap)
                return propertyMap;

            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
