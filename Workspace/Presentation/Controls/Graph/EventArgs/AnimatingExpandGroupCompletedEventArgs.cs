using GPAS.Workspace.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Controls.Graph
{
    public class AnimatingExpandGroupCompletedEventArgs
    {
        public AnimatingExpandGroupCompletedEventArgs(GroupMasterKWObject expandRootObject)
        {
            if (expandRootObject == null)
                throw new ArgumentNullException("expandRootObject");

            ExpandRootObject = expandRootObject;
        }

        public GroupMasterKWObject ExpandRootObject
        {
            private set;
            get;
        }
    }
}
