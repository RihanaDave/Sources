using System.Collections.ObjectModel;
using System.ComponentModel;

namespace GPAS.Workspace.Presentation.Windows
{
    public class UnpublishObjectComponent: TreeViewItemBase
    {
        public UnpublishObjectComponent()
        {
            IsExpanded = true;
            ComponentValues = new ObservableCollection<UnpublishedComponentValue>();
        }
        private ComponentType componentType;
        public ComponentType ComponentType
        {
            get { return this.componentType; }
            set
            {
                if (this.componentType != value)
                {
                    this.componentType = value;
                    this.NotifyPropertyChanged("ComponentType");
                }
            }
        }
        public ObservableCollection<UnpublishedComponentValue> ComponentValues { get; set; }
    }
}
