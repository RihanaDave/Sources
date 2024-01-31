using GPAS.AccessControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Entities
{
    [DataContract]
   public class SearchDataSourceACL
    {
        [DataMember]
        public long Id;

        [DataMember]
        public ACL Acl;
    }
}
