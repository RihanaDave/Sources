using GPAS.Workspace.Presentation.Controls.DataImport.Model.Map;
using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Converters
{
    public class WarningTypeToBooleanConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || parameter == null)
                return false;

            if (values[0] is MapWarningModel mapWarning)
            {
                if (mapWarning.WarningType == (MapWarningType)parameter && mapWarning.RelatedElement.Equals(values[1]))
                {
                    return true;
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
