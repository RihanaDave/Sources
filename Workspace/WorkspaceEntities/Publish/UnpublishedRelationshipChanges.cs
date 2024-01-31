using System.Collections.Generic;
using GPAS.Workspace.Entities;

namespace GPAS.Workspace.Entities
{
    public class UnpublishedRelationshipChanges
    {
        public UnpublishedRelationshipChanges()
        {
        }

        public IEnumerable<RelationshipEntity> AddedRelationships { get; set; }
        //public IEnumerable<RelationshipEntity> ModifiedRelationships { get; set; }

        public class RelationshipEntity
        {
            public KWObject Source;
            public KWObject Target;
            public KWRelationship Relationship;
        }
    }
}