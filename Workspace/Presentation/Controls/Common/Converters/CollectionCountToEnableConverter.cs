using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.Common.Converters
{
    public class CollectionCountToEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }

            if (value is int listCount)
            {
                return listCount > 0;
            }

            return ((IList)value).Count > 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
