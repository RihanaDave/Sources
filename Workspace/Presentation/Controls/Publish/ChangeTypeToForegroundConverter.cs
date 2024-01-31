using GPAS.Workspace.Presentation.Windows;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Controls.Publish
{
    public class ChangeTypeToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return new SolidColorBrush(Colors.Green);

            switch ((ChangeType)value)
            {
                case ChangeType.Added:
                    return new SolidColorBrush(Colors.Green);
                case ChangeType.Changed:
                    return new SolidColorBrush(Colors.Orange);
                case ChangeType.Deleted:
                    return new SolidColorBrush(Colors.Red);
            }

            return new SolidColorBrush(Colors.Green);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
