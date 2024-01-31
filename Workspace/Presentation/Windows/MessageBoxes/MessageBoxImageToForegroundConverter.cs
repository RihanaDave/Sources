using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Windows.MessageBoxes
{
    public class MessageBoxImageToForegroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MessageBoxImage messageBoxImage)
            {
                switch (messageBoxImage)
                {
                    case MessageBoxImage.Error:
                        return new SolidColorBrush(Colors.Red);
                    case MessageBoxImage.Warning:
                        return new SolidColorBrush(Colors.Orange);
                    case MessageBoxImage.Question:
                    case MessageBoxImage.Information:
                    case MessageBoxImage.None:
                       return new SolidColorBrush(Colors.DodgerBlue);
                }
            }

            return MaterialDesignThemes.Wpf.PackIconKind.Information;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
