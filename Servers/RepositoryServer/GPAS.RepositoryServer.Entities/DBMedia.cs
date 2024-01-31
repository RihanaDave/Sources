using System.Runtime.Serialization;

namespace GPAS.RepositoryServer.Entities
{
    [DataContract]
    public class DBMedia
    {
        [DataMember]
        public string URI { set; get; }

        [DataMember]
        public string Description { set; get; }

        [DataMember]
        public long Id { set; get; }

        [DataMember]
        public long ObjectId { set; get; }
        [DataMember]
        public long DataSourceID { set; get; }
    }
}
