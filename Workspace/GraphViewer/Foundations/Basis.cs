using GraphX.Controls;
using GraphX.PCL.Logic.Models;
using QuickGraph;

namespace GPAS.Graph.GraphViewer.Foundations
{
    // Layout visual class
    public class GraphArea : GraphArea<Vertex, Edge, BidirectionalGraph<Vertex, Edge>> { }
    // Graph data class
    public class GraphData : BidirectionalGraph<Vertex, Edge> { }
    // Logics core class
    public class GraphLogic : GXLogicCore<Vertex, Edge, BidirectionalGraph<Vertex, Edge>> { }
}