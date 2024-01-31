using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch.Entities.Concepts
{
    [DataContract]
    public enum LinkDirection
    {
        [EnumMember]
        SourceToTarget = 1,
        [EnumMember]
        TargetToSource = 2,
        [EnumMember]
        Bidirectional = 3
    }
}
