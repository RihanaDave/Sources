using GPAS.Graph.GraphViewer.Foundations;
using System;

namespace GPAS.Graph.GraphViewer
{
    /// <summary>
    /// آرگومان صدور رخداد «حذف گره از گراف»
    /// </summary>
    public class VertexRemovedEventArgs
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        public VertexRemovedEventArgs(Vertex removedVertex)
        {
            if (removedVertex == null)
                throw new ArgumentNullException("removedVertex");

            RemovedVertex = removedVertex;
        }
        /// <summary>
        /// گرهی که قرار است از گراف حذف شود
        /// </summary>
        public Vertex RemovedVertex
        {
            get;
            private set;
        }
    }
}
