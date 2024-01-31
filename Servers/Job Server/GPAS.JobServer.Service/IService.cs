using GPAS.JobServer.Logic.Entities;
using GPAS.JobServer.Logic.SemiStructuredDataImport;
using System;
using System.Collections.Generic;
using System.Data;
using System.ServiceModel;

namespace GPAS.JobServer.Service
{
    [ServiceContract]
    public interface IService
    {
#if DEBUG
        [OperationContract]
        string test();
#endif
        [OperationContract]
        List<JobRequest> GetJobRequests();

        [OperationContract]
        void RegisterNewImportRequests(SemiStructuredDataImportRequestMetadata[] requestsData);

        [OperationContract]
        string[] GetUriOfDatabasesForImport();
        [OperationContract]
        DataSet GetTablesAndViewsPreviewOfDatabaseForImport(string dbForImportURI);

        [OperationContract]
        void SetPauseStateForJob(int id);

        [OperationContract]
        void SetResumeStateForJob(int id);

        [OperationContract]
        void IsAvailable();

        //[OperationContract]
        //void PauseJobMonitoringAgentService();
        [OperationContract]
        void RestartJobMonitoringAgentService();

        [OperationContract]
        double GetImprotingPercent(int jobId);
    }
}