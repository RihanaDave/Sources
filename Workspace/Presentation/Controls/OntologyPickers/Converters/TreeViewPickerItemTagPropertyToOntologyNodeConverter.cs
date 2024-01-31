using GPAS.TreeViewPicker;
using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.OntologyPickers.Converters
{
    internal class TreeViewPickerItemTagPropertyToOntologyNodeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TreeViewPickerItem)
            {
                TreeViewPickerItem treeViewPickerItem = value as TreeViewPickerItem;
                return treeViewPickerItem.Tag;
            }

            return value;
        }
    }
}
