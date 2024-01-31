using GPAS.Workspace.Presentation.Observers.Base;
using System.Collections.Generic;

namespace GPAS.Workspace.Presentation.Observers.Properties
{
    public class PropertiesChangeableObserver : ObserverBase<IPropertiesChangeableListener, PropertiesChangedArgs>
    {
        protected override void WakeupListeners(IEnumerable<IPropertiesChangeableListener> listener, PropertiesChangedArgs arguments)
        {
            foreach (var item in listener)
            {
                item.ChangeProperties(arguments);
            }
        }
    }
}
