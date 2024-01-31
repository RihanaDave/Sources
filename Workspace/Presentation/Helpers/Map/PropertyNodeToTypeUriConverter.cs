using GPAS.Ontology;
using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Helpers.Map
{
    internal class PropertyNodeToTypeUriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PropertyNode)
                return ((PropertyNode)value).TypeUri;

            return string.Empty;
        }
    }
}