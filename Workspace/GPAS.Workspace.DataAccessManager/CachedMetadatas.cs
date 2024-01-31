using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GPAS.Workspace.DataAccessManager.LinkManager;
using static GPAS.Workspace.DataAccessManager.MediaManager;
using static GPAS.Workspace.DataAccessManager.ObjectManager;
using static GPAS.Workspace.DataAccessManager.PropertyManager;

namespace GPAS.Workspace.DataAccessManager
{
    internal class CachedMetadatas
    {
        public List<CachedObjectMetadata> objectMetadatas { get; set; }
        public List<CachedPropertyMetadata> propertyMetadatas { get; set; }
        public List<CachedMediaMetadata> mediaMetadatas { get; set; }
        public List<CachedRelationshipMetadata> relationshipMetadatas { get; set; }
    }
}
