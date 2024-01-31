using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Windows.MessageBoxes
{
    public class MessageBoxButtonToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MessageBoxButton button && parameter is MessageBoxResult resultParameter)
            {
                switch (resultParameter)
                {
                    case MessageBoxResult.OK:
                        return button == MessageBoxButton.OK || button == MessageBoxButton.OKCancel ?
                            Visibility.Visible : Visibility.Collapsed;
                    case MessageBoxResult.Cancel:
                        return button == MessageBoxButton.OKCancel || button == MessageBoxButton.YesNoCancel ?
                            Visibility.Visible : Visibility.Collapsed;
                    case MessageBoxResult.Yes:
                        return button == MessageBoxButton.YesNo || button == MessageBoxButton.YesNoCancel ?
                            Visibility.Visible : Visibility.Collapsed;
                    case MessageBoxResult.No:
                        return button == MessageBoxButton.YesNo || button == MessageBoxButton.YesNoCancel ?
                            Visibility.Visible : Visibility.Collapsed;
                }
            }

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
