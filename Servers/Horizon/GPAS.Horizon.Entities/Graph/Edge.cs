using GPAS.AccessControl;
using GPAS.Dispatch.Entities.Concepts;

namespace GPAS.Horizon.Entities.Graph
{
    public class Edge
    {
        public long ID { set; get; }
        public string TypeUri { set; get; }
        public LinkDirection Direction { set; get; }
        public long SourceVertexID { set; get; }
        public long TargetVertexID { set; get; }
        public string SourceVertexTypeUri { set; get; }
        public string TargetVertexTypeUri { set; get; }
        public ACL Acl { set; get; }
    }
}
