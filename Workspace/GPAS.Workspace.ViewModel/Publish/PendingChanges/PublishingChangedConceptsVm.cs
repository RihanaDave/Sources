using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.ViewModel.Publish.PendingChanges
{
    public enum ConceptChangeType
    {
        Added,
        Modified,
        Deleteted,
        Unchanged
    }

    public enum RowType
    {
        Property,
        Media,   
        Link,
    }


    public abstract class PublishingChangedConceptsVm : INotifyPropertyChanged
    {
        protected PublishingChangedConceptsVm()
        {
            OwnerObjectId = new List<long>();
        }

        protected PublishingChangedConceptsVm(string type, string value, ConceptChangeType changeType, bool isAccepted)
        {
            TypeURI = type;
            Value = value;
            ChangeType = changeType;
            OwnerObjectId = new List<long>();
        }
        public string TypeURI { get; set; }
        public string Value { get; set; }
        public ConceptChangeType ChangeType { get; set; }
    
        public long Id { get; set; }
        public RowType RowType { get; set; }

        private bool check = true;
        public bool Check
        {
            get { return this.check; }
            set
            {
                if (this.check != value)
                {
                    this.check = value;
                    this.NotifyPropertyChanged("Check");
                }
            }
        }

        public List<long> OwnerObjectId { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

    }
}
