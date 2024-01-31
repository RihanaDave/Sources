using GPAS.Graph.GraphViewer.Foundations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Graph.GraphViewer
{
    public class AnimatingCollapseGroupCompletedEventArgs
    {
        public AnimatingCollapseGroupCompletedEventArgs(GroupMasterVertex collapseRootVertex)
        {
            if (collapseRootVertex == null)
                throw new ArgumentNullException("collapseRootVertex");

            CollapseRootVertex = collapseRootVertex;
        }

        public GroupMasterVertex CollapseRootVertex
        {
            private set;
            get;
        }
    }
}
