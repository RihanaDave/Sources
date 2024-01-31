using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.Link.Converters
{
    public class LinkTypeToEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return false;

            if (value.ToString().Equals(Properties.Resources.Select_A_Type) ||
                value.ToString().Equals(Properties.Resources.Not_Initialized) ||
                value.ToString().Equals(string.Empty))
            {
                return false;
            }

            return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
