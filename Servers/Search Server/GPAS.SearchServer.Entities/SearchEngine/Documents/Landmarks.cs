using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPAS.SearchServer.Entities.SearchEngine.Documents
{
    [DataContract]
    public class Landmarks
    {
        [DataMember]
        public List<double> marks { get; set; }
    }
}