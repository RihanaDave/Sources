using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Converters
{
    public class CollectionCountIToVisibilityConverter : IValueConverter
    {
        public Visibility ZeroCountValue { get; set; } = Visibility.Collapsed;
        public Visibility PositiveCountValue { get; set; } = Visibility.Visible;
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return ZeroCountValue;
            }

            if (value is int count && count > 0)
            {
                return PositiveCountValue;
            }

            return ((IList)value).Count > 0 ? PositiveCountValue : ZeroCountValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
