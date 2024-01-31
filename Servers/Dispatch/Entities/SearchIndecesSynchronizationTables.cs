using System.Runtime.Serialization;

namespace GPAS.Dispatch.Entities
{
    public enum SearchIndecesSynchronizationTables
    {
        [EnumMember]
        HorizonServerUnsyncObjects = 0,
        [EnumMember]
        HorizonServerUnsyncRelationships = 1,
        [EnumMember]
        SearchServerUnsyncObjects = 2,
        [EnumMember]
        SearchServerUnsyncDataSources = 3
    }
}
