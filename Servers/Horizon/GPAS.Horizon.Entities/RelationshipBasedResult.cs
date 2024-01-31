using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Horizon.Entities
{
    [DataContract]
    public class RelationshipBasedResult
    {
        [DataMember]
        public long RelationshipID { get; set; }
        [DataMember]
        public long TargetObjectID { get; set; }
    }
}
