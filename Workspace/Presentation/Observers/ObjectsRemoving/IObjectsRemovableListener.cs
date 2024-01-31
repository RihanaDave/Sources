using GPAS.Workspace.Entities;
using System.Collections.Generic;

namespace GPAS.Workspace.Presentation.Observers.ObjectsRemoving
{
    public interface IObjectsRemovableListener
    {
        void RemoveObjects(IEnumerable<KWObject> objectsToRemove);
    }
}
