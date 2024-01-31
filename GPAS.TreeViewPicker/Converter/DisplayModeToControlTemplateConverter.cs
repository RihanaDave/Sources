using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace GPAS.TreeViewPicker.Converter
{
    internal class DisplayModeToControlTemplateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter is ControlTemplate[] templates)
            {
                if ((DisplayMode)value == DisplayMode.List && templates.Length >= 2)
                {
                    return templates[1];
                }
                else if (templates.Length >= 1)
                {
                    return templates[0];
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
