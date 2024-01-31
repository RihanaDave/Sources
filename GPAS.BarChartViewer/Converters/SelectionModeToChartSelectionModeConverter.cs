using System;
using System.Globalization;
using System.Windows.Data;
using Telerik.Windows.Controls;

namespace GPAS.BarChartViewer.Converters
{
    public class SelectionModeToChartSelectionModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is SelectionMode)
            {
                SelectionMode selectionMode = (SelectionMode)value;
                if (selectionMode == SelectionMode.Multiple)
                    return ChartSelectionMode.Multiple;
                else if (selectionMode == SelectionMode.Single)
                    return ChartSelectionMode.Single;
                else if (selectionMode == SelectionMode.None)
                    return ChartSelectionMode.None;
                else
                    throw new NotSupportedException();
            }

            return ChartSelectionMode.Multiple;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {

            if (value is ChartSelectionMode)
            {
                ChartSelectionMode chartSelectionMode = (ChartSelectionMode)value;
                if (chartSelectionMode == ChartSelectionMode.Multiple)
                    return SelectionMode.Multiple;
                else if (chartSelectionMode == ChartSelectionMode.Single)
                    return SelectionMode.Single;
                else if (chartSelectionMode == ChartSelectionMode.None)
                    return SelectionMode.None;
                else
                    throw new NotSupportedException();
            }
            return SelectionMode.Multiple;
        }
    }
}
