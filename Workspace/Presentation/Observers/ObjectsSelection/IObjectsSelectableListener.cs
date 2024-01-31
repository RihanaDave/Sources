using GPAS.Workspace.Entities;
using GPAS.Workspace.Presentation.Observers.Base;
using System;
using System.Collections.Generic;

namespace GPAS.Workspace.Presentation.Observers
{
    public interface IObjectsSelectableListener
    {
        void SelectObjects(IEnumerable<KWObject> objectsToSelect);
    }
}