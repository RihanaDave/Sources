using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Converters
{
    public class TextToColorTextBoxCustomConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (value.ToString()!=string.Empty)
                {
                    return ((Brush)parameter);
                }
                else
                {
                    return Brushes.Red;
                }
            }
            else
            {
                return Brushes.Red;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
            {
                if (value.ToString() != string.Empty)
                {
                    return Brushes.Black;
                }
                else
                {
                    return Brushes.Red;
                }
            }
            else
            {
                return Brushes.Red;
            }
        }
    }
}
