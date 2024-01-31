using System;
using System.Globalization;
using System.Windows.Data;
using GPAS.Workspace.Entities.KWLinks;

namespace GPAS.Workspace.Presentation.Windows.Graph.Converters
{
    /// <summary>
    /// این مبدل برای این است که در پنجره های ساخت لینک
    /// تا وقتی جهت لینک انتخاب نشده باشد دکمه
    /// ok
    /// غیر فعال بماند
    /// </summary>
    public class DirectionToButtonEnableConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return false;
            switch ((LinkDirection)value)
            {
                case LinkDirection.Bidirectional:
                case LinkDirection.SourceToTarget:
                case LinkDirection.TargetToSource:
                    return true;
                default:
                    return false;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
