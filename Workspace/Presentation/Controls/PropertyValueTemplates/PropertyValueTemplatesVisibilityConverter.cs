using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using GPAS.Ontology;

namespace GPAS.Workspace.Presentation.Controls.PropertyValueTemplates
{
    public class PropertyValueTemplatesVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && (parameter != null && (BaseDataTypes)parameter == (BaseDataTypes)value))
            {
                return Visibility.Visible;
            }

            if (parameter == null || (BaseDataTypes) parameter != BaseDataTypes.String)
                return Visibility.Collapsed;

            if (value != null && ((BaseDataTypes)value == BaseDataTypes.Double ||
                                  (BaseDataTypes)value == BaseDataTypes.HdfsURI ||
                                  (BaseDataTypes)value == BaseDataTypes.Int ||
                                  (BaseDataTypes)value == BaseDataTypes.Long))
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
