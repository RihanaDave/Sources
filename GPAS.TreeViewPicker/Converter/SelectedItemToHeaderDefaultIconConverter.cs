using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace GPAS.TreeViewPicker.Converter
{
    internal class SelectedItemToHeaderDefaultIconConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            TreeViewPickerItem selectedItem = new TreeViewPickerItem();
            if (values[0] is TreeViewPickerItem)
            {
                selectedItem = values[0] as TreeViewPickerItem;
            }
            else
            {
                if (values.Length > 1 && values[1] is BitmapSource)
                {
                    selectedItem.Icon = values[1] as BitmapSource;
                }
            }

            return selectedItem.Icon;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
