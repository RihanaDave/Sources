using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.AccessControl.Groups
{
    [DataContract]
   public enum NativeGroup
    {
        [EnumMember]
        Administrators,

        [EnumMember]
        EveryOne
    }
}
