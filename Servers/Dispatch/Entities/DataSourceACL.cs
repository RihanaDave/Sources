using GPAS.AccessControl;
using System.Runtime.Serialization;

namespace GPAS.Dispatch.Entities
{
    [DataContract]
    public class DataSourceACL
    {
        [DataMember]
        public long Id;
        [DataMember]
        public ACL Acl;
    }
}
