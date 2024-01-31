using GPAS.Workspace.Presentation.Observers.Base;
using System.Collections.Generic;

namespace GPAS.Workspace.Presentation.Observers
{
    public class ObjectsShowingObserver
        : ObserverBase<IObjectsShowableListener, ObjectsShowingArgs>
    {
        protected async override void WakeupListeners(IEnumerable<IObjectsShowableListener> listener, ObjectsShowingArgs arguments)
        {
            foreach (IObjectsShowableListener item in listener)
            {
                await item.ShowObjectsAsync((arguments as ObjectsShowingArgs).ObjectsToShow);
            }
        }
    }
}
