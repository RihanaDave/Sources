using GPAS.Graph.GraphViewer.Foundations;
using GraphX.PCL.Common.Interfaces;

namespace GPAS.Graph.GraphViewer.LayoutAlgorithms
{
    /// <summary>
    /// اینترفیس الگوریتم های چینش گره ها
    /// </summary>
    public interface ILayoutAlgorithm : IExternalLayout<Vertex, Edge>
    {
    }
}
