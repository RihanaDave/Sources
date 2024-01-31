using GPAS.Workspace.Entities;
using System.Collections.Generic;
using System.ComponentModel;

namespace GPAS.Workspace.Presentation.Windows
{
    public class UnpublishedComponentValue : INotifyPropertyChanged
    {
        public UnpublishedComponentValue()
        {
            Parents = new List<UnpublishedObject>();
        }
        private bool isUnpublishedComponentValueSelected;
        public bool IsUnpublishedComponentValueSelected
        {
            get { return this.isUnpublishedComponentValueSelected; }
            set
            {
                if (this.isUnpublishedComponentValueSelected != value)
                {
                    this.isUnpublishedComponentValueSelected = value;
                    this.NotifyPropertyChanged("IsUnpublishedComponentValueSelected");
                }
            }
        }

        private string value;
        public string Value
        {
            get { return this.value; }
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    this.NotifyPropertyChanged("Value");
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

        public ComponentType UnpublishedComponentType { get; set; }

        public List<UnpublishedObject> Parents { get; set; }

        public KWMedia relatedMedia { get; set; }

        public KWProperty relatedProperty { get; set; }

        public KWRelationship relatedRelationship { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
