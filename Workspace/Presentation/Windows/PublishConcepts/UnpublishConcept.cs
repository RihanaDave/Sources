using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Windows
{
    public class UnpublishConcept : TreeViewItemBase
    {
        public UnpublishConcept()
        {
            IsExpanded = true;
        }
        private ConceptType conceptType;
        public ConceptType ConceptType
        {
            get { return this.conceptType; }
            set
            {
                if (this.conceptType != value)
                {
                    this.conceptType = value;
                    this.NotifyPropertyChanged("ConceptType");
                }
            }
        }
        public ObservableCollection<UnpublishedObject> UnpublishedObjects { get; set; }
        
    }
}
