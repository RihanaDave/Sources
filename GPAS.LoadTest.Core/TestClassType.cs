namespace GPAS.LoadTest.Core
{
    public enum TestClassType
    {
        #region DataRepository

        Publish,
        ObjectsRetrieve,
        PropertiesRetrieve,
        RelationsRetrieve,

        #endregion

        #region SearchServer
        SyncPublishChanges,
        RunStatisticalQuery,
        RetrievePropertyValueStatistics,
        RetrievePropertyBarValuesStatistics,
        RetrieveObjectIDsByStatistical,
        RetrieveLinkTypeStatistics,
        RetrieveLinkedObjectIDsByStatistical,
        PerformGeoPolygonSearch,
        PerformGeoPolygonFilterSearch,
        PerformFilterSearch,
        GetTypeBasedResolutionCandidates,
        #endregion

        #region HorizonServer
        SingleConnectedEntitisRetrival,
        MultipleConnectedEntitisRetrival,
        EventRetrival,
        DocumentRetrival,
        PublishLinks,
        IntermediateEventRetrival,
        #endregion

        #region FileRepository

        UploadAndDownload,

        #endregion
    }
}
