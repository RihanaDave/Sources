using System.Collections.Generic;
using System.Runtime.Serialization;
using GPAS.Dispatch.Entities.Concepts;

namespace GPAS.Horizon.Entities.IndexChecking
{
    [DataContract]
    public class HorizonIndexCheckingInput
    {
        [DataMember]
        public long ObjectId { set; get; }

        [DataMember]
        public string ObjectTypeUri { set; get; }

        [DataMember]
        public long ResultLimit { set; get; }

        [DataMember]
        public List<KProperty> Properties { set; get; }

        [DataMember]
        public List<long> RelationsIds { set; get; }
    }
}
