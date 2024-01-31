using GPAS.Workspace.Presentation.Observers.Base;
using System.Collections.Generic;

namespace GPAS.Workspace.Presentation.Observers.ObjectsRemoving
{
    public class ObjectsRemovingObserver : ObserverBase<IObjectsRemovableListener, ObjectsRemovingArgs>
    {
        protected override void WakeupListeners(IEnumerable<IObjectsRemovableListener> listener, ObjectsRemovingArgs arguments)
        {
            foreach (var item in listener)
            {
                item.RemoveObjects(arguments.ObjectsToRemove);
            }
        }
    }
}
