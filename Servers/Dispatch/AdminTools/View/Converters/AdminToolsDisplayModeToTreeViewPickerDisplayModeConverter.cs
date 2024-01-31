using GPAS.Dispatch.AdminTools.View.UserControls.OntologyPicker;
using System;
using System.Globalization;
using System.Windows.Data;

namespace GPAS.Dispatch.AdminTools.View.Converters
{
    public class AdminToolsDisplayModeToTreeViewPickerDisplayModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DisplayMode adminToolsDisplayMode)
            {
                if (adminToolsDisplayMode == DisplayMode.List)
                {
                    return TreeViewPicker.DisplayMode.List;
                }
            }

            return DisplayMode.DropDown;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TreeViewPicker.DisplayMode treeViewPickerDisplayMode)
            {
                if (treeViewPickerDisplayMode == TreeViewPicker.DisplayMode.List)
                {
                    return DisplayMode.List;
                }
            }

            return DisplayMode.DropDown;
        }
    }
}
