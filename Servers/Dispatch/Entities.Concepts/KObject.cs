using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPAS.Dispatch.Entities.Concepts
{
    [DataContract]
    public class KObjectMaster
    {
        [DataMember]
        public long Id { get; set; }

        [DataMember]
        public long MasterId { get; set; }

        [DataMember]
        public long[] ResolveTo { get; set; }
    }

    [DataContract]
    public class KObject
    {
        [DataMember]
        public string TypeUri { set; get; }
        [DataMember]
        public long? LabelPropertyID { set; get; }

        [DataMember]
        public long Id { set; get; }

        [DataMember]
        public int IsMaster { get; set; }

        [DataMember]
        public KObjectMaster KObjectMaster { get; set; }

        [DataMember]
        public List<KObject> Slaves
        {
            get;
            set;
        }
    }
}
