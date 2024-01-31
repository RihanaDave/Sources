using System.Runtime.Serialization;

namespace GPAS.SearchServer.Entities
{
    [DataContract]
    public class SearchMedia
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
