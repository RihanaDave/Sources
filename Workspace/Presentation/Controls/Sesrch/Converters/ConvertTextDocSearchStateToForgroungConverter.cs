using GPAS.Workspace.Presentation.Controls.Sesrch.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Controls.Sesrch.Converters
{
  public  class ConvertTextDocSearchStateToForgroungConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((SearchState)value == SearchState.TextDoc)
            {
                var converter = new System.Windows.Media.BrushConverter();
                return (Brush)converter.ConvertFromString("#C4EAEC");
            }
            else
            {
                var converter = new System.Windows.Media.BrushConverter();
                return (Brush)converter.ConvertFromString("#00A3AD");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

      
    }
}
