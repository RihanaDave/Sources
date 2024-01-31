using System.Runtime.Serialization;

namespace GPAS.RepositoryServer.Entities.Publish
{
    [DataContract]
    public class DBResolvedObject
    {
        [DataMember]
        public long ResolutionMasterObjectID { get; set; }
        [DataMember]
        public long[] ResolvedObjectIDs { get; set; }
        [DataMember]
        public DBMatchedProperty[] MatchedProperties { get; set; }
    }
}
