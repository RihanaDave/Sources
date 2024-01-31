using GPAS.FilterSearch;
using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.FilterSearchCriterias.Converters
{
    public class PropertyCriteriaOperatorToComboBoxEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //درصورتی که value 
            //از نوع EmptyPropertyCriteriaOperatorValuePair
            //باشد مقدار false برمی گرداند
            return !(value is EmptyPropertyCriteriaOperatorValuePair);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
