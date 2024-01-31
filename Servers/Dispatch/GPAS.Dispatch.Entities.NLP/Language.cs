

using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GPAS.Dispatch.Entities.NLP
{
    [Serializable]
    [DataContract]
    public enum Language
    {
        [EnumMember]
        [Description("PERSIAN")]
        fa = 0,
        [EnumMember]
        [Description("ENGLISH")]
        en = 1,
        [EnumMember]
        [Description("ARABIC")]
        ar = 2

    }
}
