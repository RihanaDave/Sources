using System.Collections.Generic;
using GPAS.Workspace.Entities;

namespace GPAS.Workspace.Entities
{
    public class UnpublishedObjectChanges
    {
        public UnpublishedObjectChanges()
        {
        }

        public IEnumerable<KWObject> AddedObjects { get; set; }
    }
}