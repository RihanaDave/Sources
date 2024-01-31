using GPAS.Workspace.Entities;
using GPAS.Workspace.Presentation.Observers.Base;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.Workspace.Presentation.Observers
{
    public class ObjectsSelectionObserver
        : ObserverBase<IObjectsSelectableListener, ObjectsSelectionChangedArgs>
    {
        protected override void WakeupListeners(IEnumerable<IObjectsSelectableListener> listener, ObjectsSelectionChangedArgs arguments)
        {
            foreach (IObjectsSelectableListener item in listener)
                item.SelectObjects((arguments as ObjectsSelectionChangedArgs).SelectedObjects);
        }
    }
}