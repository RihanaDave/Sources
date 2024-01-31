namespace GPAS.Graph.GraphViewer.Foundations
{
    public class MasterVertex : ResolvedVertex
    {
        protected internal MasterVertex(string vertexTitle, VertexControl relatedVertexControl = null)
            : base(vertexTitle, relatedVertexControl)
        {
            RelatedVertexControl.IsMaster = true;
            Master = this;
        }
    }
}
