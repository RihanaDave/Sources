using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.Common.Converters
{
    public class ObjectToEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
