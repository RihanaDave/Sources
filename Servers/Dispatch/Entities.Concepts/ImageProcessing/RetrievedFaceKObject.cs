using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch.Entities.Concepts.ImageProcessing
{
    [DataContract]
    public class RetrievedFaceKObject
    {
        [DataMember]
        public KObject kObject { get; set; }

        [DataMember]
        public double distance { get; set; }

        [DataMember]
        public BoundingBox boundingBox { get; set; }
    }
}
