using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace GPAS.Workspace.Presentation.Controls.OntologyPickers.Converters
{
    public class WorkspaceDisplayModeToTreeViewPickerDisplayModeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DisplayMode workspaceDisplayMode)
            {
                if (workspaceDisplayMode == DisplayMode.List)
                    return TreeViewPicker.DisplayMode.List;
            }

            return TreeViewPicker.DisplayMode.DropDown;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TreeViewPicker.DisplayMode treeViewPickerDisplayMode)
            {
                if (treeViewPickerDisplayMode == TreeViewPicker.DisplayMode.List)
                    return DisplayMode.List;
            }

            return DisplayMode.DropDown;
        }
    }
}
