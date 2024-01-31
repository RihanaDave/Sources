using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.AccessControl.Groups
{
    [DataContract]
    public class GroupInfo
    {
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string GroupName { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string CreatedBy { get; set; }

        [DataMember]
        public string CreatedTime { get; set; }
    }
}
