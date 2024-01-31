using GPAS.Workspace.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Observers
{
    public class ObjectsSelectionChangedArgs
    {
        public ObjectsSelectionChangedArgs(IEnumerable<KWObject> selectedObjects)
        {
            SelectedObjects = selectedObjects;
        }

        public IEnumerable<KWObject> SelectedObjects
        {
            get;
            set;
        }
    }
}
