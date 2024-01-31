using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Windows.MessageBoxes
{
    public class MessageBoxImageToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MessageBoxImage messageBoxImage)
            {
                switch (messageBoxImage)
                {
                    case MessageBoxImage.Question:
                        return MaterialDesignThemes.Wpf.PackIconKind.QuestionMark;
                    case MessageBoxImage.Error:
                        return MaterialDesignThemes.Wpf.PackIconKind.CloseOctagon;
                    case MessageBoxImage.Warning:
                        return MaterialDesignThemes.Wpf.PackIconKind.Warning;
                    case MessageBoxImage.Information:
                    case MessageBoxImage.None:
                        return MaterialDesignThemes.Wpf.PackIconKind.Information;
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
