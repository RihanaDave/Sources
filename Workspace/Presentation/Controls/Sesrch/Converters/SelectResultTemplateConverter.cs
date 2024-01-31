using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.Sesrch.Converters
{
    public class SelectResultTemplateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
       
            string ConditionTerm = ((string)value).ToUpper();
            if ((string)ConditionTerm == "TXT" || (string)ConditionTerm == "PDF" || (string)ConditionTerm == "DOC" || (string)ConditionTerm == "DOCX" || (string)ConditionTerm == "HTM"
                || (string)ConditionTerm == "HTML" || (string)ConditionTerm == "PPT" || (string)ConditionTerm == "PPTX" || (string)ConditionTerm == "RTF" || (string)ConditionTerm == "XHTML")
            {
                return Application.Current.Resources["TXT"];
            }

            //return Application.Current.Resources["All"];
            return Application.Current.Resources["TXT"];
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
