using System.Runtime.Serialization;

namespace GPAS.Dispatch.Entities.Concepts
{
    [DataContract]
    public class KMedia
    {
        [DataMember]
        public string URI { set; get; }

        [DataMember]
        public string Description { set; get; }

        [DataMember]
        public long Id { set; get; }

        [DataMember]
        public long OwnerObjectId { set; get; }
        [DataMember]
        public long DataSourceID { set; get; }
    }
}
