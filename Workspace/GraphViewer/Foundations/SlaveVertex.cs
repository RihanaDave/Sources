namespace GPAS.Graph.GraphViewer.Foundations
{
    public class SlaveVertex : ResolvedVertex
    {
        protected internal SlaveVertex(string vertexTitle, VertexControl relatedVertexControl = null) 
            : base(vertexTitle, relatedVertexControl)
        {
            RelatedVertexControl.IsSlave = true;
            Slaves.Add(this);
        }
    }
}
