using GPAS.Workspace.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.ViewModel.Publish.PendingChanges
{
    public class PublishingChangedMediaVm : PublishingChangedConceptsVm, IPublishingChangedMapping<KWMedia, PublishingChangedMediaVm>
    {
        public PublishingChangedMediaVm ConvertEntityToUnPublishConecptBaseClass(KWMedia entity, ConceptChangeType status)
        {
            PublishingChangedMediaVm unPublishConcept = new PublishingChangedMediaVm
            {
                Id= entity.ID,
                TypeURI = "--------",
                Value = entity.MediaUri.DisplayName,
                Check = true,
                ChangeType = status,
                RowType = RowType.Media

            };
            return unPublishConcept;
        }

        public KWMedia ConvertUnPublishConecptBaseClassToEntity(PublishingChangedMediaVm unPublishConcept)
        {
            throw new Exception();
        }

        public long ReturnId(PublishingChangedLinksVm unPublishConcept)
        {
            return unPublishConcept.Id;
        }

        
        
    }
}
