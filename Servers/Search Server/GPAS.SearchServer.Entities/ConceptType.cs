using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Entities
{
    [Serializable]
    [DataContract]
    public enum ConceptType
    {
        [EnumMember]
        Property = 1,
        [EnumMember]
        Relationship = 2,
        [EnumMember]
        Media = 3,
        [EnumMember]
        Object = 4
    }
}
