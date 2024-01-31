using System.Runtime.Serialization;

namespace GPAS.Dispatch.Entities.Concepts
{
    [DataContract]
    public class KProperty
    {
        [DataMember]
        public string TypeUri { set; get; }

        [DataMember]
        public string Value { set; get; }

        [DataMember]
        public long Id { set; get; }

        [DataMember]
        public KObject Owner { set; get; }
        [DataMember]
        public long DataSourceID { set; get; }
    }
}
