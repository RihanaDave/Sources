using MaterialDesignThemes.Wpf;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Converters
{
    public class ChangeForegrandTitleWithIsValisConverter : IMultiValueConverter
    {
        PaletteHelper PaletteHelper = new PaletteHelper();
      
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
             ITheme theme = PaletteHelper.GetTheme();
            if (values[0] != null && values[1] != null)
            {
                if (values[0] is bool isvalid && values[1] is bool mapIsvalid)
                {
                    if (isvalid && mapIsvalid)
                    {
                        return new SolidColorBrush(theme.Body);
                    }
                }
            }
            return new SolidColorBrush(Colors.Red);
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
