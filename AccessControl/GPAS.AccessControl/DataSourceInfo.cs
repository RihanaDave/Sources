using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.AccessControl
{
    [DataContract]
    public class DataSourceInfo
    {
        [DataMember]
        public ACL Acl { set; get; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public long Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public long Type { get; set; }

        [DataMember]
        public string CreatedBy { get; set; }

        [DataMember]
        public string CreatedTime { get; set; }
    }
}
