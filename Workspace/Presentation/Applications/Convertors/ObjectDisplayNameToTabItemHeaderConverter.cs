using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Applications.Convertors
{
    public class ObjectDisplayNameToTabItemHeaderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return null;

            if (value.ToString().Length > 15)
            {
                return value.ToString().Substring(0, 15) + "...";
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
