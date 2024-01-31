using GPAS.Workspace.Presentation.Windows;
using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.Publish
{
    public class ChangeTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return MaterialDesignThemes.Wpf.PackIconKind.PlusBox;

            switch ((ChangeType)value)
            {
                case ChangeType.Added:
                    return MaterialDesignThemes.Wpf.PackIconKind.PlusBoxOutline;
                case ChangeType.Changed:
                    return MaterialDesignThemes.Wpf.PackIconKind.SquareEditOutline;
                case ChangeType.Deleted:
                    return MaterialDesignThemes.Wpf.PackIconKind.MinusBoxOutline;
            }

            return MaterialDesignThemes.Wpf.PackIconKind.PlusBox;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
