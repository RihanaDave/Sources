using GPAS.SearchServer.Entities.SearchEngine.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Entities
{
    [DataContract]
    public class RetrievedFace
    {
        [DataMember]
        public string imageId { get; set; }

        [DataMember]
        public double distance { get; set; }

        [DataMember]
        public BoundingBox boundingBox { get; set; }

    }
}
