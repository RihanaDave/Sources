using GPAS.Workspace.Presentation.Observers.Base;
using System.Collections.Generic;

namespace GPAS.Workspace.Presentation.Observers
{
    /// <summary>
    /// ناظر اعمال فیلتر روی اشیاء
    /// </summary>
    public class ObjectsFilteringObserver
        : ObserverBase<IObjectsFilterableListener, ObjectsFilteringArgs>
    {
        /// <summary>
        /// گوش‌دهنده‌های ناظر را هوشیار می‌کند
        /// </summary>
        protected async override void WakeupListeners(IEnumerable<IObjectsFilterableListener> listener, ObjectsFilteringArgs arguments)
        {
            foreach (IObjectsFilterableListener item in listener)
                await item.ApplyFilter((arguments as ObjectsFilteringArgs));
        }
    }
}