using GPAS.Graph.GraphViewer.Foundations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Graph.GraphViewer
{
    public class AnimatingExpandGroupCompletedEventArgs
    {
        public AnimatingExpandGroupCompletedEventArgs(GroupMasterVertex expandRootVertex)
        {
            if (expandRootVertex == null)
                throw new ArgumentNullException("expandRootVertex");

            ExpandRootVertex = expandRootVertex;
        }

        public GroupMasterVertex ExpandRootVertex
        {
            private set;
            get;
        }
    }
}
