using GPAS.Workspace.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Controls.Graph
{
    public class DoubleClickedVertexEventArgs
    {
        public DoubleClickedVertexEventArgs(KWObject doubleClickedObject)
        {
            if (doubleClickedObject == null)
                throw new ArgumentNullException("doubleClickedObject");

            DoubleClickedObject = doubleClickedObject;
        }

        public KWObject DoubleClickedObject
        {
            get;
            private set;
        }
    }
}
