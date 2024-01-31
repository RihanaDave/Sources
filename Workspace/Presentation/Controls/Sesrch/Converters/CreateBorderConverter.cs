using GPAS.Workspace.Presentation.Controls.Sesrch.Model;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.Sesrch.Converters
{
    public class CreateBorderConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int index = ((int)(SearchState)value)-1;
            if (index<0)
            {
                return new Thickness(0, 0, 0, 0);
            }

            return new Thickness(10 + (80 * index), 60, 0, 0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}