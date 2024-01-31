using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.Waiting
{
    public class ProgressDisplayModeToVisibilityConverter : IValueConverter
    {
        public Visibility PercentageValue { get; set; } = Visibility.Visible;
        public Visibility NumericValue { get; set; } = Visibility.Collapsed;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is ProgressValueDisplayMode progressDisplayMode)
            {
                if (progressDisplayMode == ProgressValueDisplayMode.Numeric)
                    return NumericValue;
                else if (progressDisplayMode == ProgressValueDisplayMode.Percentage)
                    return PercentageValue;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
