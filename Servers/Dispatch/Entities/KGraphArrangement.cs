using System.Runtime.Serialization;

namespace GPAS.Dispatch.Entities
{
    [DataContract]
    public class KGraphArrangement
    {

        [DataMember]
        public string Title { set; get; }

        [DataMember]
        public string Description { set; get; }

        [DataMember]
        public long Id { set; get; }

        [DataMember]
        public string TimeCreated { set; get; }

        [DataMember]
        public byte[] GraphImage { set; get; }

        [DataMember]
        public byte[] GraphArrangement { set; get; }

        [DataMember]
        public int NodesCount { set; get; }
        [DataMember]
        public long DataSourceID { set; get; }

    }
}
