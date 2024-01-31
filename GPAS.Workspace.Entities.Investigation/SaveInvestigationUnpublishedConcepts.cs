using GPAS.Workspace.Entities.Investigation.UnpublishedChanges;
using System.Collections.Generic;

namespace GPAS.Workspace.Entities.Investigation
{
    public class SaveInvestigationUnpublishedConcepts
    {
        public List<CachedObjectMetadata> unpublishedObjectChanges { get; set; }
        public List<CachedPropertyMetadata> unpublishedPropertyChanges { get; set; }
        public List<CachedMediaMetadata> unpublishedMediaChanges { get; set; }
        public List<CachedRelationshipMetadata> unpublishedRelationshipChanges { get; set; }
    }
}
