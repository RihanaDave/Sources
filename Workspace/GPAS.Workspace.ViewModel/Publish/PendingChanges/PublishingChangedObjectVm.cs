using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPAS.Workspace.Entities;

namespace GPAS.Workspace.ViewModel.Publish.PendingChanges
{
    public class PublishingChangedObjectVm : IPublishingChangedMapping<KWObject, PublishingChangedObjectVm>,
        INotifyPropertyChanged
    {
        public PublishingChangedObjectVm()
        {
            publishingChangedConceptsVm= new ObservableCollection<PublishingChangedConceptsListVm>();
        }
        public long ObjectId { get; set; }
        public  string TypeURI{ get; set; }
        public string DisplayName { get; set; }
        public ConceptChangeType ObjectChangeType { get; set; }
        public PublishingChangeType TotalPublishingChangeType { get; set; }

        private bool _IsAccepted;

        public bool IsAccepted
        {
            get { return _IsAccepted; }
            set
            {
                _IsAccepted = value;
                OnPropertyChanged("IsAccepted");
            }
        }
        
        public ObservableCollection<PublishingChangedConceptsListVm> publishingChangedConceptsVm { get; set; }

        public PublishingChangedObjectVm ConvertEntityToUnPublishConecptBaseClass(KWObject entity, ConceptChangeType status)
        {
            PublishingChangedObjectVm unPublishConcept = new PublishingChangedObjectVm
            {
                ObjectId = entity.ID,
                IsAccepted = true,
                TypeURI = entity.TypeURI,
                DisplayName = entity.GetObjectLabel(),
                ObjectChangeType = status
            };
            return unPublishConcept;
        }

        public KWObject ConvertUnPublishConecptBaseClassToEntity(PublishingChangedObjectVm unPublishConcept)
        {
            throw new NotImplementedException();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string property)
        {
           //event
            NotifyPropertyChanged(property);
        }

        //private void OnAcceptanceChanged()
        //{
        //    if (AcceptanceChanged != null)
        //        AcceptanceChanged(this, EventArgs.Empty);
        //}

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        //public void UpdateAcceptanceByLinksChecks(object sender, PropertyChangedEventArgs e)
        //{
        //    foreach (var item in publishingChangedConceptsVm)
        //    {
        //        if (item is PublishingChangedLinksVm)
        //        {
        //            foreach (var it in item.PublishingChangedConceptsVms)
        //            {
        //                if (it.Check == false)
        //                {

        //                }
        //            }
        //        }
        //    }
        //}
    }
}
