using GraphX.Controls;
using GraphX.PCL.Common.Interfaces;
using GraphX.PCL.Logic.Models;
using QuickGraph;

namespace GPAS.TagCloudViewer.Foundations
{
    // Layout visual class
    public class GraphArea : GraphArea<Vertex, IGraphXEdge<Vertex>, BidirectionalGraph<Vertex, IGraphXEdge<Vertex>>> { }
    // Graph data class
    public class GraphData : BidirectionalGraph<Vertex, IGraphXEdge<Vertex>> { }
    // Logics core class
    public class GraphLogic : GXLogicCore<Vertex, IGraphXEdge<Vertex>, BidirectionalGraph<Vertex, IGraphXEdge<Vertex>>> { }
}