using System;
using System.Globalization;
using System.Windows.Data;
using GPAS.Workspace.Entities.KWLinks;

namespace GPAS.Workspace.Presentation.Controls.Link.Converters
{
    /// <summary>
    /// این مبدل دکمه رادیویی انتخاب شده را به جهت لینک
    /// متناظر تبدیل می‌کند و برعکس
    /// </summary>
    public class DirectionToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return parameter != null && (value != null && (LinkDirection)value == (LinkDirection)parameter);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(bool) value) return null;
            return (LinkDirection?) parameter;
        }
    }
}
