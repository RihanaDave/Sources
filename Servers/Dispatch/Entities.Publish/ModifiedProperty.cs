using System.Runtime.Serialization;

namespace GPAS.Dispatch.Entities.Publish
{
    [DataContract]
    public class ModifiedProperty
    {
        [DataMember]
        public string NewValue { set; get; }

        [DataMember]
        public long Id { set; get; }

        [DataMember]
        public string TypeUri { set; get; }

        [DataMember]
        public long OwnerObjectID { get; set; }
    }
}
