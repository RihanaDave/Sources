using GPAS.Workspace.Entities;
using System;
using System.Collections.Generic;

namespace GPAS.Workspace.Presentation.Controls.Timeline
{
    public class ObjectsSelectionRequestEventArgs : EventArgs
    {
        public ObjectsSelectionRequestEventArgs(IEnumerable<KWObject> objects)
        {
            Objects = objects;
        }

        public IEnumerable<KWObject> Objects { get; protected set; }
    }
}
