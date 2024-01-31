using System;
using System.Runtime.Serialization;

namespace GPAS.SearchServer.Entities.SearchEngine
{
    [Serializable]
    [DataContract]
    public enum DataType
    {
        [EnumMember]
        Int = 0,
        [EnumMember]
        Bool = 1,
        [EnumMember]
        String = 2,
        [EnumMember]
        Double = 3,
        [EnumMember]
        Long = 4,
        [EnumMember]
        DateTime = 5,
        [EnumMember]
        GeoLocation = 6,
        [EnumMember]
        DateRange = 7,
        [EnumMember]
        PInts = 8,
        [EnumMember]
        KeywordTokenizedString = 9
    }
}
