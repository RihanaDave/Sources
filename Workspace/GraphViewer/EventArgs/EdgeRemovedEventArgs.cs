using GPAS.Graph.GraphViewer.Foundations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Graph.GraphViewer
{
    /// <summary>
    /// آرگومان صدور رخداد «حذف یال از گراف»
    /// </summary>
    public class EdgeRemovedEventArgs
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        public EdgeRemovedEventArgs(Edge removedEdge)
        {
            if (removedEdge == null)
                throw new ArgumentNullException("removedEdge");

            RemovedEdge = removedEdge;
        }
        /// <summary>
        /// یالی که از گراف حذف شده
        /// </summary>
        public Edge RemovedEdge
        {
            get;
            private set;
        }
    }
}
