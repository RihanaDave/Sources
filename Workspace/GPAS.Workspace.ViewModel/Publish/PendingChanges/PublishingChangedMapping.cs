using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.ViewModel.Publish.PendingChanges
{
    interface IPublishingChangedMapping<TEntity, TUnPublishConecptBaseClass> 
        where TEntity : class 
        where TUnPublishConecptBaseClass :class
    {
         TUnPublishConecptBaseClass ConvertEntityToUnPublishConecptBaseClass(TEntity entity,ConceptChangeType status);
         TEntity ConvertUnPublishConecptBaseClassToEntity(TUnPublishConecptBaseClass unPublishConcept);
    }
}
