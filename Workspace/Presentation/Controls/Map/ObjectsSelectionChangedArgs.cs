using GPAS.Workspace.Entities;
using System.Collections.Generic;

namespace GPAS.Workspace.Presentation.Controls.Map
{
    public class ObjectsSelectionChangedArgs
    {
        public ObjectsSelectionChangedArgs(IEnumerable<KWObject> currentlySelectedObjects)
        {
            CurrentlySelectedObjects = currentlySelectedObjects;
        }

        public IEnumerable<KWObject> CurrentlySelectedObjects
        {
            get;
            private set;
        }
    }
}
