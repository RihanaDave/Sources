using System.Runtime.Serialization;

namespace GPAS.AccessControl
{
    [DataContract]
    public enum DataSourceType
    {
        [EnumMember]
        ManuallyEntered = 1,
        [EnumMember]
        Document = 2,
        [EnumMember]
        Graph = 3,
        [EnumMember]
        CsvFile = 4,
        [EnumMember]
        AttachedDatabaseTable = 5,
        [EnumMember]
        DataLakeSearchResult = 6,
        [EnumMember]
        ExcelSheet = 7,
        [EnumMember]
        AccessTable = 8
    }
}
