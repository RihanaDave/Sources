using GPAS.Workspace.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.ViewModel.Publish.PendingChanges
{
    public class PublishingChangedPropertyVm : PublishingChangedConceptsVm, IPublishingChangedMapping<KWProperty, PublishingChangedPropertyVm>
    {
        public PublishingChangedPropertyVm ConvertEntityToUnPublishConecptBaseClass(KWProperty entity, ConceptChangeType status)
        {
            var unPublishConcept = new PublishingChangedPropertyVm
            {              
                Id= entity.ID,
                TypeURI = entity.TypeURI,
                Value = entity.Value,
                Check = true,
                ChangeType = status,
                RowType = RowType.Property


            };
            return unPublishConcept;
        }

        public KWProperty ConvertUnPublishConecptBaseClassToEntity(PublishingChangedPropertyVm unPublishConcept)
        {
            throw new NotImplementedException();
        }

        public long ReturnId(PublishingChangedLinksVm unPublishConcept)
        {
            return unPublishConcept.Id;
        }
    }
}
