using System.Runtime.Serialization;

namespace GPAS.SearchServer.Entities.Sync
{
    [DataContract]
    public class ModifiedProperty
    {
        [DataMember]
        public long ID { set; get; }

        [DataMember]
        public string newValue { set; get; }

        [DataMember]
        public string TypeUri { get; set; }

        [DataMember]
        public long OwnerObjectID { get; set; }
    }
}