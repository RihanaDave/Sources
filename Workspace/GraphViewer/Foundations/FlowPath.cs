using System.Collections.Generic;

namespace GPAS.Graph.GraphViewer.Foundations
{
    public class FlowPath
    {
        public double Weight { get; set; }
        public List<EdgeControl> PathOrderedEdges { get; set; }
        public bool IsShown { get; set; }
        public PathType Type { get; set; }
    }
}
