using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.TreeViewPicker.Converter
{
    internal class SelectedItemToHeaderDefaultTitleConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            TreeViewPickerItem selectedItem = new TreeViewPickerItem();
            if(values[0] is TreeViewPickerItem)
            {
                selectedItem = values[0] as TreeViewPickerItem;
            }
            else
            {
                if (values.Length > 1 && values[1] is string)
                {
                    selectedItem.Title = values[1].ToString();
                }
            }

            return selectedItem.Title;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
