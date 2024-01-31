using System;
using System.Globalization;
using System.Windows.Data;
using GPAS.Ontology;

namespace GPAS.Workspace.Presentation.Controls.Link.Converters
{
    /// <summary>
    /// این مبدل نوع لینک انتخاب شده برمی‌گرداند
    /// </summary>
    public class TreeNodeToTypeUri :IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException(nameof(value));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? string.Empty : ((OntologyNode) value).TypeUri;
        }
    }
}
