using GPAS.JobServer.Logic;
using GPAS.JobServer.Logic.Entities;
using GPAS.JobServer.Logic.SemiStructuredDataImport;
using GPAS.Logger;
using System;
using System.Collections.Generic;
using System.Data;

namespace GPAS.JobServer.Service
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in both code and config file together.
    public class Service : IService
    {
        private void WriteErrorLog(Exception ex)
        {
            ExceptionHandler exceptionHandler = new ExceptionHandler();
            exceptionHandler.WriteErrorLog(ex);
        }

#if DEBUG
        public string test()
        {
            return "Server Is Running";
        }
#endif

        public List<JobRequest> GetJobRequests()
        {
            try
            {
                JobsStoreAndRetrieveProvider provider = new JobsStoreAndRetrieveProvider();
                return provider.GetJobRequests();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void RegisterNewImportRequests(SemiStructuredDataImportRequestMetadata[] requestsData)
        {
            try
            {
                RequestManager reqManager = new RequestManager();
                reqManager.RegisterNewImportRequests(requestsData);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public string[] GetUriOfDatabasesForImport()
        {
            try
            {
                DatabaseImporter importer = new DatabaseImporter();
                return importer.GetUriOfAliveDatabasesForImport();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public DataSet GetTablesAndViewsPreviewOfDatabaseForImport(string dbUri)
        {
            try
            {
                DatabaseImporter importer = new DatabaseImporter();
                return importer.GetTablesAndViewsPreviewForDatabase(dbUri);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
        public void SetPauseStateForJob(int id)
        {
            try
            {
                JobsStoreAndRetrieveProvider.SetPausedStateForJob(id, "Job is paused by user");
                JobsStoreAndRetrieveProvider.RestartJobMonitoringAgent();
                JobsStoreAndRetrieveProvider.KillRelatedJobWorker(id);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void SetResumeStateForJob(int id)
        {
            try
            {
                JobsStoreAndRetrieveProvider.SetResumeStateForJob(id, "Job is resumed by user");
                JobsStoreAndRetrieveProvider.RestartJobMonitoringAgent();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void RestartJobMonitoringAgentService()
        {
            try
            {
                JobsStoreAndRetrieveProvider.RestartJobMonitoringAgent();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public double GetImprotingPercent(int jobId)
        {
            double result = 0;
            try
            {
                int lastObject = JobsStoreAndRetrieveProvider.GetLastImportingObjectIndex(jobId);
                int lastRelation = JobsStoreAndRetrieveProvider.GetLastImportingRelationIndex(jobId) * 1000;
                int totalObject = JobsStoreAndRetrieveProvider.GetTotalImportingObjects(jobId);
                int totalRelation = JobsStoreAndRetrieveProvider.GetTotalImportingRelations(jobId);
                //if ((totalObject + totalRelation)!=0)
                //{
                //    if(totalRelation - lastRelation < 1000)
                //        result = Math.Floor( (double)(((double)lastObject + (double)lastRelation) /( (double)totalObject + (double)totalRelation))*100);
                //    else
                //        result = Math.Floor((double)(((double)lastObject + (double)totalRelation) / ((double)totalObject + (double)totalRelation)) * 100);
                //}
                //else
                //{
                //    result =0;
                //}

                JobRequestStatus jobStatus = JobsStoreAndRetrieveProvider.GetJobStatus(jobId);
                if (jobStatus == JobRequestStatus.Success)
                {
                    return 100;
                }
                else
                {
                    int current = lastObject + lastRelation;
                    if (totalObject == 0)
                        return 0;

                    result = Math.Round((double)(100 * current) / totalObject);
                }

            }
            catch(Exception ex)
            {
                WriteErrorLog(ex);
            }
            
            return result;
        }
        public void IsAvailable()
        {
        }
    }
}
