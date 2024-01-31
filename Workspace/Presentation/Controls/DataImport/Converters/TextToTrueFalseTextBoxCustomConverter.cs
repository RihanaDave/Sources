using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Converters
{
    public class TextToTrueFalseTextBoxCustomConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length == 2)
            {
                if (values[1]?.ToString()== "Custom")
                {
                    if (values[0]?.ToString()!=string.Empty)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else               
                  return true;
            }
            else
            {
                return false;
            }
            
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
