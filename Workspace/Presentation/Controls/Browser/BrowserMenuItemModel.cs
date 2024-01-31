using MaterialDesignThemes.Wpf;
using System.Collections.Generic;

namespace GPAS.Workspace.Presentation.Controls.Browser
{
    public class BrowserMenuItemModel : BaseModel
    {
        private string text;
        public string Text
        {
            get => text;
            set
            {
                SetValue(ref text, value);
            }
        }

        private bool isSelected;
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                SetValue(ref isSelected, value);
            }
        }

        private PackIconKind icon;
        public PackIconKind Icon
        {
            get => icon;
            set
            {
                SetValue(ref icon, value);
            }
        }

        private BrowseMenuItemTypes type;
        public BrowseMenuItemTypes Type
        {
            get => type;
            set
            {
                SetValue(ref type, value);
            }
        }

        private List<BrowserMenuItemModel> items;
        public List<BrowserMenuItemModel> Items
        {
            get => items;
            set
            {
                SetValue(ref items, value);
            }
        }
    }
}
