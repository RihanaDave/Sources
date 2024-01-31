using GPAS.Workspace.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Controls.Graph
{
    public class AnimatingCollapseGroupCompletedEventArgs
    {
        public AnimatingCollapseGroupCompletedEventArgs(GroupMasterKWObject collapseRootObject)
        {
            if (collapseRootObject == null)
                throw new ArgumentNullException("collapseRootObject");

            CollapseRootObject = collapseRootObject;
        }

        public GroupMasterKWObject CollapseRootObject
        {
            private set;
            get;
        }
    }
}
