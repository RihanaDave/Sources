using GPAS.Graph.GraphViewer.Foundations;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Helpers.Graph
{
    public class FlowsVisualStyleToAnimatedIsCheckedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is FlowVisualStyle))
                return false;

            FlowVisualStyle visualStyle = (FlowVisualStyle)value;
            if (visualStyle == FlowVisualStyle.Animated)
                return true;
            else
                return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool? && (bool?)value == true)
                return FlowVisualStyle.Animated;

            return DependencyProperty.UnsetValue;
        }
    }
}