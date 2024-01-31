using GPAS.Workspace.Entities;
using System;
using System.Collections.Generic;

namespace GPAS.Workspace.Presentation.Applications.EventsArgs
{
    public class RemoveObjectFromMapRequestEventArgs
    {
        public List<KWObject> ObjectsRequestedToRemoveFromMap { get; }

        public RemoveObjectFromMapRequestEventArgs(List<KWObject> objectsRequestedToRemoveFromMap)
        {
            if (objectsRequestedToRemoveFromMap == null)
            {
                throw new ArgumentNullException(nameof(objectsRequestedToRemoveFromMap));
            }

            ObjectsRequestedToRemoveFromMap = objectsRequestedToRemoveFromMap;
        }
    }
}
