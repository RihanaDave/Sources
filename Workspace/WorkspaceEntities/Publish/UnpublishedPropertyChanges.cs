using System.Collections.Generic;
using GPAS.Workspace.Entities;

namespace GPAS.Workspace.Entities
{
    public class UnpublishedPropertyChanges
    {
        public UnpublishedPropertyChanges()
        {
        }

        public IEnumerable<KWProperty> AddedProperties { get; set; }
        public IEnumerable<KWProperty> ModifiedProperties { get; set; }
    }
}