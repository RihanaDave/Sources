using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Converters
{
    public class HeaderContainCommaToCanSortConveretr : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DataView dataView)
            {
                foreach (var item in dataView.Table.Columns)
                {
                    if (item.ToString().Contains(','))
                    {
                        return false;
                    }
                }       
            }
            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
