using GPAS.TagCloudViewer.Foundations;
using GraphX.PCL.Common.Interfaces;

namespace GPAS.TagCloudViewer.LayoutAlgorithms
{
    /// <summary>
    /// اینترفیس الگوریتم های چینش گره ها
    /// </summary>
    public interface ILayoutAlgorithm : IExternalLayout<Vertex, IGraphXEdge<Vertex>>
    {
    }
}
