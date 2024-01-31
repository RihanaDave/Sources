using GPAS.Workspace.Entities;
using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.Browser
{
    public class ObjectDisplayNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (((KWProperty)value).Value.Length > 25)
                {
                    return ((KWProperty)value).Value.Substring(0, 25) + "...";
                }

                return ((KWProperty)value).Value;
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
