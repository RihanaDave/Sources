using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPAS.RepositoryServer.Entities
{
    [DataContract]
    public class DBModifiedConcepts
    {
        [DataMember]
        public List<DBModifiedProperty> ModifiedPropertyList { set; get; }

        [DataMember]
        public List<long> DeletedMediaIDList { set; get; }
    }
}