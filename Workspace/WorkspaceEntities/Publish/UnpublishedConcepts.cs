using System.Collections.Generic;

namespace GPAS.Workspace.Entities
{
    public class UnpublishedConcepts
    {
        public List<KWObject> unpublishedObjectChanges { get; set; }
        public List<KWProperty>unpublishedPropertyChanges { get; set; }
        public List<KWMedia> unpublishedMediaChanges { get; set; }
        public List<KWRelationship> unpublishedRelationshipChanges { get; set; }
    }
}
