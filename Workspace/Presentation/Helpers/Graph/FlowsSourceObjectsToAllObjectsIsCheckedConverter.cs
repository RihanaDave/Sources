using GPAS.Workspace.Presentation.Controls.Graph.Flows;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Helpers.Graph
{
    public class FlowsSourceObjectsToAllObjectsIsCheckedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is SourceObjects))
                return false;

            SourceObjects sourceObjects = (SourceObjects)value;
            if (sourceObjects == SourceObjects.AllObjects)
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool? && (bool?)value == true)
                return SourceObjects.AllObjects;

            return DependencyProperty.UnsetValue;
        }
    }
}