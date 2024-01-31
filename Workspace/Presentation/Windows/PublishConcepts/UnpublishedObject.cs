using GPAS.Workspace.Entities;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.Windows
{
    public class UnpublishedObject : TreeViewItemBase
    {
        public UnpublishedObject()
        {
            UnpublishObjectComponents = new ObservableCollection<UnpublishObjectComponent>();
            IsExpanded = true;
        }

        private bool isUnpublishedObjectSelected;
        public bool IsUnpublishedObjectSelected
        {
            get { return this.isUnpublishedObjectSelected; }
            set
            {
                if (this.isUnpublishedObjectSelected != value)
                {
                    this.isUnpublishedObjectSelected = value;
                    this.NotifyPropertyChanged("IsUnpublishedObjectSelected");
                }
            }
        }

        private bool enableSelection;
        public bool EnableSelection
        {
            get { return this.enableSelection; }
            set
            {
                if (this.enableSelection != value)
                {
                    this.enableSelection = value;
                    this.NotifyPropertyChanged("EnableSelection");
                }
            }
        }

        public ViewModel.Publish.UnpublishedObject relatedViewModel { get; set; }
        public KWObject relatedKWObject { get; set; }

        private BitmapImage unpublishedObjectIcon;
        public BitmapImage UnpublishedObjectIcon
        {
            get { return this.unpublishedObjectIcon; }
            set
            {
                if (this.unpublishedObjectIcon != value)
                {
                    this.unpublishedObjectIcon = value;
                    this.NotifyPropertyChanged("UnpublishedObjectIcon");
                }
            }
        }

        private string unpublishedObjectDisplayName;
        public string UnpublishedObjectDisplayName
        {
            get { return this.unpublishedObjectDisplayName; }
            set
            {
                if (this.unpublishedObjectDisplayName != value)
                {
                    this.unpublishedObjectDisplayName = value;
                    this.NotifyPropertyChanged("UnpublishedObjectDisplayName");
                }
            }
        }

        private string unpublishedObjectTypeURI;
        public string UnpublishedObjectTypeURI
        {
            get { return this.unpublishedObjectTypeURI; }
            set
            {
                if (this.unpublishedObjectTypeURI != value)
                {
                    this.unpublishedObjectTypeURI = value;
                    this.NotifyPropertyChanged("UnpublishedObjectTypeURI");
                }
            }
        }

        private ChangeType changeType;
        public ChangeType ChangeType
        {
            get { return this.changeType; }
            set
            {
                if (this.changeType != value)
                {
                    this.changeType = value;
                    this.NotifyPropertyChanged("ChangeType");
                }
            }
        }
        public ObservableCollection<UnpublishObjectComponent> UnpublishObjectComponents { get; set; }               
    }
}
