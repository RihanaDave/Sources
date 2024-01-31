using GPAS.Workspace.Entities;
using System.Collections.Generic;

namespace GPAS.Workspace.Presentation.Observers.Properties
{
    public class PropertiesChangedArgs
    {
        public PropertiesChangedArgs(IEnumerable<KWProperty> addedProperties, IEnumerable<KWProperty> removedProperties)
        {
            AddedProperties = addedProperties;
            RemovedProperties = removedProperties;
        }

        public IEnumerable<KWProperty> AddedProperties { get; protected set; }
        public IEnumerable<KWProperty> RemovedProperties { get; protected set; }
    }
}
