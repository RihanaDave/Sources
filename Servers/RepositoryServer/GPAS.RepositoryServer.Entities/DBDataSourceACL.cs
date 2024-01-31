using GPAS.AccessControl;
using MongoDB.Bson;
using System.Runtime.Serialization;

namespace GPAS.RepositoryServer.Entities
{
    [DataContract]
    public class DBDataSourceACL
    {

        [DataMember]
        public long Id;

        [DataMember]
        public ACL Acl;
    }
}
