using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Utility
{
    class BoolToTextDecorationsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object para, CultureInfo culture)
        {
            if ((bool)value == true)
                return TextDecorations.Strikethrough;
            else return null;
        }
        public object ConvertBack(object value, Type targetType, object para, CultureInfo culture)
        {
            return new NotImplementedException();
        }
    }
}
