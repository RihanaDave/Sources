using System;
using System.Runtime.Serialization;

namespace GPAS.AccessControl
{
    [Serializable]
    [DataContract]
    public enum Permission
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Discovery = 1,
        [EnumMember]
        Read = 2,
        [EnumMember]
        Write = 3,
        [EnumMember]
        Owner = 4
    }
}
