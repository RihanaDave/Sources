using GPAS.Workspace.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.ViewModel.Publish.PendingChanges
{
    public class PublishingChangedLinksVm : PublishingChangedConceptsVm, IPublishingChangedMapping<KWLink, PublishingChangedLinksVm>
    {
        //public bool Check { get; set; }
        public PublishingChangedLinksVm ConvertEntityToUnPublishConecptBaseClass(KWLink entity, ConceptChangeType status, long KWLinkId)
        {
            PublishingChangedLinksVm unPublishConcept = new PublishingChangedLinksVm
            {
                Id = KWLinkId,
                Value = entity.TypeURI,
                TypeURI = "--------",
                Check = true,
                ChangeType = status,
                RowType = RowType.Link

            };
            return unPublishConcept;
        }


        public KWLink ConvertUnPublishConecptBaseClassToEntity(PublishingChangedLinksVm unPublishConcept)
        {
            throw new NotImplementedException();
        }
        public long ReturnId(PublishingChangedLinksVm unPublishConcept)
        {
            return unPublishConcept.Id;
        }


        public PublishingChangedLinksVm ConvertEntityToUnPublishConecptBaseClass(KWLink entity, ConceptChangeType status)
        {
            throw new NotImplementedException();
        }
    }
}
