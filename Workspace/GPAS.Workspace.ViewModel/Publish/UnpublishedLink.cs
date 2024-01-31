using GPAS.Workspace.Entities;
using System;
using System.ComponentModel;

namespace GPAS.Workspace.ViewModel.Publish
{
    public class UnpublishedLink
    {
        public string TypeURI { get; set; }

        public string Description { get; set; }

        public virtual DateTime? TimeBegin { get; set; }

        public virtual DateTime? TimeEnd { get; set; }
                      
        public UnpublishConceptChangeType ChangeType { get; set; }
        public KWRelationship relatedKWRelationship { get; set; }
        public KWObject relatedSource { get; set; }
        public KWObject relatedTarget { get; set; }

    }
}
