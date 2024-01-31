using GPAS.Graph.GraphViewer.Foundations;
using System;

namespace GPAS.Graph.GraphViewer
{
    public class VertexDoubleClickEventArgs
    {
        public VertexDoubleClickEventArgs(VertexControl doubleClickedVertexControl)
        {
            if (doubleClickedVertexControl == null)
                throw new ArgumentNullException("doubleClickedVertexControl");

            DoubleClickedVertexControl = doubleClickedVertexControl;
        }

        public VertexControl DoubleClickedVertexControl
        {
            get;
            private set;
        }
    }
}
