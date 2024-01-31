using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.Link.Converters
{
    /// <summary>
    /// این مبدل برای تبدیل یک شی به لیستی از اشیاء است
    /// کنترلی که کار انتخاب نوع لینک را انجام می‌دهد شی مبداء
    /// را به عنوان یک لیست قبول می‌کند
    /// </summary>
    public class SourceTypeToSourceTypeCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? new ObservableCollection<string> { value.ToString() } : new ObservableCollection<string>();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<string> collection)
            {
                return collection.FirstOrDefault();
            }

            return string.Empty;
        }
    }
}
