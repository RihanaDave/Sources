namespace GPAS.JobServer.Logic.Entities
{
    public enum JobRequestType
    {
        Unknown,
        ImportFromCsvFile,
        ImportFromExcelSheet,
        ImportFromAccessTable,
        ImportFromAttachedDatabaseTableOrView,
        ImportFromEmlDirectory       
    }
}