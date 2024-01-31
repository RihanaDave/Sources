using System.Collections.Generic;

namespace GPAS.Horizon.GraphRepositoryPlugins.Neo4j
{
    public class ResolveModel
    {
        public List<long> VerticesID { get; set; }
        public long MasterVertexID { get; set; }
        public string TypeUri { get; set; }
    }
}
