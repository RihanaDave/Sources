using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using GPAS.Workspace.Presentation.Controls.Sesrch.Enum;

namespace GPAS.Workspace.Presentation.Controls.Sesrch.Converters
{
    public class AsceOrDesConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (((SortOrder)value)== SortOrder.SortAscending)
            {
                return MaterialDesignThemes.Wpf.PackIconKind.SortDescending;
            }
            else
            {
                return MaterialDesignThemes.Wpf.PackIconKind.SortAscending;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
