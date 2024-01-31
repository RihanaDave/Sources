using System.Collections.Generic;

namespace GPAS.SearchServer.Entities.SearchEngine.Documents
{
    public class FaceSpecification
    {
        public string FaceId { get; set; }

        public BoundingBox BoundingBox { get; set; }

        public List<double> VectorFeatue { get; set; }
    }
}