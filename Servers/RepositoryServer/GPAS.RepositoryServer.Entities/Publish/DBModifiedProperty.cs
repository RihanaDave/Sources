using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.RepositoryServer.Entities
{
    [DataContract]
    public class DBModifiedProperty
    {
        [DataMember]
        public string NewValue { set; get; }

        [DataMember]
        public long Id { set; get; }
    }
}
