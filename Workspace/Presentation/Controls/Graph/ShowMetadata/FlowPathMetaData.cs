using GPAS.Graph.GraphViewer.Foundations;
using System.Collections.Generic;

namespace GPAS.Workspace.Presentation.Controls.Graph.ShowMetadata
{
    public class FlowPathMetaData
    {
        public double Weight { get; set; }
        public List<EdgeMetadata> PathOrderedEdges { get; set; }
        public bool IsShown { get; set; }
        public PathType Type { get; set; }
    }
}
