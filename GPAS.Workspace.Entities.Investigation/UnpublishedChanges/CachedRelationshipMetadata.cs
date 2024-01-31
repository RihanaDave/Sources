using GPAS.Dispatch.Entities.Concepts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Entities.Investigation.UnpublishedChanges
{
    public class CachedRelationshipMetadata
    {
        public KRelationship CachedRelationship;
        public long RelationshipSourceId;
        public long RelationshipTargetId;
        public bool IsPublished;
        public string TypeURI;
    }
}
