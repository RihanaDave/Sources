using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.TreeViewPicker.Converter
{
    internal class IsSelectableToOpacitySelectedItemConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double highlightOpacity = .18;
            if ((bool)value == false) //در صورتی که گزینه قابل انتخاب نباشد شفافیت هایلایت انتخاب به نصف حالت عادی کاهش می یابد.
            {
                return highlightOpacity / 2;
            }
            else
            {
                return highlightOpacity;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
