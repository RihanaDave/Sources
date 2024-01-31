using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Windows.DataImport.Converters
{
    public class DataSourcesCountToBooleanConverter :IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values[0] is int selectedCount && values[1] is int allCount)
            {
                if (selectedCount == 0)
                {
                    return false;
                }

                if (allCount == selectedCount)
                {
                    return true;
                }

                if (allCount > selectedCount)
                {
                    return null;
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
