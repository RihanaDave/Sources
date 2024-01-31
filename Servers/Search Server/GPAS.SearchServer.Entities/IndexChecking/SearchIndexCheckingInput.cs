using System.Collections.Generic;
using System.Runtime.Serialization;
using GPAS.Dispatch.Entities.Concepts;

namespace GPAS.SearchServer.Entities.IndexChecking
{
    [DataContract]
    public class SearchIndexCheckingInput
    {
        [DataMember]
        public long ObjectId { set; get; }

        [DataMember]
        public byte[] DocumentContent { set; get; }

        [DataMember]
        public List<KProperty> Properties { set; get; }

        [DataMember]
        public List<long> RelationsIds { set; get; }
    }
}
