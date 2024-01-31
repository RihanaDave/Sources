using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPAS.Dispatch.Entities.Concepts.ImageProcessing
{
    [DataContract]
    public class Landmarks
    {
        [DataMember]
        public List<double> marks { get; set; }
    }
}