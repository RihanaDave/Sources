using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.FilterSearchCriterias.Converters
{
    public class KeywordToCorrectKeywordConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return CorrectKeyword(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return CorrectKeyword(value.ToString());
        }

        private string CorrectKeyword(string keyword)
        {
            return keyword.Replace('"', '”');
        }
    }
}
