using GPAS.DataImport.Material.SemiStructured;
using GPAS.DataImport.Publish;
using GPAS.JobServer.Data;
using GPAS.JobServer.Data.Models;
using GPAS.JobServer.Data.Context;
using GPAS.JobServer.Logic.Entities;
using GPAS.JobsManagement;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Transactions;
using JobsDBEntities = GPAS.JobServer.Data.Context.JobsDBEntities;

namespace GPAS.JobServer.Logic
{
    public class JobsStoreAndRetrieveProvider
    {
        /// <summary>
        /// این تابع اعتبارسنجی شناسه کار و وضعیت آن را تعیین می‌کند
        /// </summary>
        /// <param name="jobId"></param>
        /// <returns></returns>
        public static bool IsBusyJob(int jobId)
        {
            Data.Context.JobsDBEntities db = new JobsDBEntities();

            var getRequestQuery = (from temp in db.JobsTables
                                   where
                                      temp.id == jobId
                                   select temp).FirstOrDefault();

            if (getRequestQuery.state.Equals(JobRequestStatus.Busy.ToString()))
            {
                return true;
            }
            else
            {
                return false;
            }

        }

        public static JobRequestStatus GetJobStatus(int jobId)
        {
            JobsDBEntities db = new JobsDBEntities();

            var getRequestQuery = (from temp in db.JobsTables
                                   where
                                      temp.id == jobId
                                   select temp).FirstOrDefault();

            if (getRequestQuery == null)
            {
                throw new InvalidOperationException("The jobId is not exist in database");
            }

            return (JobRequestStatus)Enum.Parse(typeof(JobRequestStatus), getRequestQuery.state, true);
        }

        public static Request GetJobById(int jobId)
        {
            byte[] tempRequest = { };
            JobsDBEntities db = new JobsDBEntities();

            var getRequestQuery = (from temp in db.JobsTables
                                   where
                                      temp.id == jobId
                                   select temp).FirstOrDefault();
            if (getRequestQuery == null)
            {
                throw new InvalidOperationException("The jobId is not exist in database");
            }
            RequestSerializer serializer = new RequestSerializer();
            return serializer.Deserialize(new MemoryStream(getRequestQuery.request));
        }

        public static Guid GetJobGuidById(int jobId)
        {
            JobsDBEntities db = new JobsDBEntities();

            var getRequestQuery = (from temp in db.JobsTables
                                   where
                                      temp.id == jobId
                                   select temp).FirstOrDefault();
            if (getRequestQuery == null)
            {
                throw new InvalidOperationException("Job uniqe_Id belongs to the job_Id is not exist in database.");
            }
            Guid jobGuid = Guid.Empty;
            string stingGuid = getRequestQuery.uniqeID;
            jobGuid = new Guid(stingGuid);
            return jobGuid;
        }

        public int GetAssignedProcessID(int jobId)
        {
            JobsDBEntities db = new JobsDBEntities();
            var getRequestQuery = (from temp in db.JobsTables
                                   where
                                      temp.id == jobId
                                   select temp).FirstOrDefault();
            int processId = -1;
            if (int.TryParse(getRequestQuery.processID, out processId))
                return processId;
            else
                return -1;
        }

        public List<JobRequest> GetJobsRequest(JobRequestStatus jobsStatus, JobRequestType jobsType)
        {
            List<JobRequest> result = new List<JobRequest>();
            JobsDBEntities db = new JobsDBEntities();
            var query = from temp in db.JobsTables
                        where
                              temp.type == jobsType.ToString()
                              && temp.state == jobsStatus.ToString()
                        select temp;
            foreach (var item in query)
                result.Add(new JobRequest()
                {
                    ID = item.id,
                    RegisterTime = item.registerDate,
                    BeginTime = item.startDate,
                    EndTime = item.finishDate,
                    State = (JobRequestStatus)Enum.Parse(typeof(JobRequestStatus), item.state, true),
                    StatusMeesage = item.message,
                    Type = (JobRequestType)Enum.Parse(typeof(JobRequestType), item.type, true)
                });
            return result;
        }

        public static void SetFailStateForJob(int jobId, string failReasonMessage)
        {
            JobsDBEntities db = new JobsDBEntities();
            var getRequestQuery = (from temp in db.JobsTables
                                   where
                                      temp.id == jobId
                                   select temp).FirstOrDefault();
            getRequestQuery.finishDate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            getRequestQuery.state = JobRequestStatus.Failed.ToString();
            getRequestQuery.message = failReasonMessage;
            db.SaveChanges();
        }

        public static void SetTerminatedStateForJob(int jobId, string terminatedReasonMessage)
        {
            JobsDBEntities db = new JobsDBEntities();
            var getRequestQuery = (from temp in db.JobsTables
                                   where
                                      temp.id == jobId
                                   select temp).FirstOrDefault();
            getRequestQuery.finishDate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            getRequestQuery.state = JobRequestStatus.Terminated.ToString();
            getRequestQuery.message = terminatedReasonMessage;
            db.SaveChanges();
        }
        public static void SetPausedStateForJob(int jobId, string pauseReasonMessage)
        {
            JobsDBEntities db = new JobsDBEntities();
            var getRequestQuery = (from temp in db.JobsTables
                                   where
                                      temp.id == jobId
                                   select temp).FirstOrDefault();
            getRequestQuery.state = JobRequestStatus.Pause.ToString();
            getRequestQuery.message = pauseReasonMessage;
            db.SaveChanges();
        }

        public static void SetResumeStateForJob(int jobId, string resumeReasonMessage)
        {
            JobsDBEntities db = new JobsDBEntities();
            var getRequestQuery = (from temp in db.JobsTables
                                   where
                                      temp.id == jobId
                                   select temp).FirstOrDefault();
            getRequestQuery.state = JobRequestStatus.Resume.ToString();
            getRequestQuery.message = resumeReasonMessage;
            db.SaveChanges();
        }
        public static void SetLastImportingObjectIndex(int jobId, int lastImportingObjectIndex, int totalImportingObjectIndex)
        {
            JobsDBEntities db = new JobsDBEntities();
            var getRequestQuery = (from temp in db.JobsTables
                                   where
                                      temp.id == jobId
                                   select temp).FirstOrDefault();
            getRequestQuery.lastPublishedObjectIndex = lastImportingObjectIndex.ToString() + "-" + totalImportingObjectIndex.ToString();
            db.SaveChanges();
        }

        public static void SetLastImportingRelationIndex(int jobId, int lastRelationshipIndex, int totalRelationshipsIndex)
        {
            JobsDBEntities db = new JobsDBEntities();
            var getRequestQuery = (from temp in db.JobsTables
                                   where
                                      temp.id == jobId
                                   select temp).FirstOrDefault();

            getRequestQuery.lastPublishedRelationIndex = lastRelationshipIndex.ToString() + "-" + totalRelationshipsIndex.ToString();
            db.SaveChanges();
        }

        public static int GetLastImportingObjectIndex(int jobId)
        {
            JobsDBEntities db = new JobsDBEntities();
            var getRequestQuery = (from temp in db.JobsTables
                                   where
                                      temp.id == jobId
                                   select temp).FirstOrDefault();
            int lastImportingObjectIndex = -1;

            if (int.TryParse(getRequestQuery.lastPublishedObjectIndex?.Split('-').First(), out lastImportingObjectIndex))
                return lastImportingObjectIndex;
            else
                return 0;
        }

        public static int GetTotalImportingObjects(int jobId)
        {
            JobsDBEntities db = new JobsDBEntities();
            var getRequestQuery = (from temp in db.JobsTables
                                   where
                                      temp.id == jobId
                                   select temp).FirstOrDefault();
            int totalImportingObjects = -1;

            if (int.TryParse(getRequestQuery.lastPublishedObjectIndex?.Split('-')[1], out totalImportingObjects))
                return totalImportingObjects;
            else
                return 0;
        }
        public static int GetLastImportingRelationIndex(int jobId)
        {
            JobsDBEntities db = new JobsDBEntities();
            var getRequestQuery = (from temp in db.JobsTables
                                   where
                                      temp.id == jobId
                                   select temp).FirstOrDefault();

            int lastImportingRelationIndex = -1;

            if (int.TryParse(getRequestQuery.lastPublishedRelationIndex?.Split('-').First(), out lastImportingRelationIndex))
                return lastImportingRelationIndex;
            else
                return 0;
        }
        public static int GetTotalImportingRelations(int jobId)
        {
            JobsDBEntities db = new JobsDBEntities();
            var getRequestQuery = (from temp in db.JobsTables
                                   where
                                      temp.id == jobId
                                   select temp).FirstOrDefault();

            int totalImportingRelations = -1;
            if (int.TryParse(getRequestQuery.lastPublishedRelationIndex?.Split('-')[1], out totalImportingRelations))
                return totalImportingRelations;
            else
                return 0;
        }

        public static void SetBusyStateForJob(int jobId)
        {
            JobsDBEntities db = new JobsDBEntities();
            var getRequestQuery = (from temp in db.JobsTables
                                   where
                                      temp.id == jobId
                                   select temp).FirstOrDefault();
            getRequestQuery.startDate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            getRequestQuery.state = JobRequestStatus.Busy.ToString();
            db.SaveChanges();
        }

        public void SetPendingStateForJob(int jobId)
        {
            JobsDBEntities db = new JobsDBEntities();
            var getRequestQuery = (from temp in db.JobsTables
                                   where
                                      temp.id == jobId
                                   select temp).FirstOrDefault();
            getRequestQuery.startDate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            getRequestQuery.state = JobRequestStatus.Pending.ToString();
            db.SaveChanges();
        }

        public static void SetProcessIdForJob(int jobId, int processId)
        {
            JobsDBEntities db = new JobsDBEntities();
            var getRequestQuery = (from temp in db.JobsTables
                                   where
                                      temp.id == jobId
                                   select temp).FirstOrDefault();
            getRequestQuery.startDate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            getRequestQuery.processID = processId.ToString();
            db.SaveChanges();
        }

        public static void SetSuccessStateForJob(int jobId, string successReasonMessage)
        {
            JobsDBEntities db = new JobsDBEntities();
            var getRequestQuery = (from temp in db.JobsTables
                                   where
                                      temp.id == jobId
                                   select temp).FirstOrDefault();
            getRequestQuery.finishDate = DateTime.Now.ToString(CultureInfo.InvariantCulture);
            getRequestQuery.state = JobRequestStatus.Success.ToString();
            getRequestQuery.message = successReasonMessage;
            db.SaveChanges();
        }

        public List<JobRequest> GetJobRequests()
        {
            List<JobRequest> result = new List<JobRequest>();

            JobsDBEntities db = new JobsDBEntities();
            var query = from tempRequest in db.JobsTables
                        select tempRequest;
            foreach (var item in query)
            {
                result.Add
                    (new JobRequest()
                    {
                        ID = item.id
                        ,
                        RegisterTime = item.registerDate
                        ,
                        BeginTime = item.startDate
                        ,
                        EndTime = item.finishDate
                        ,
                        State = (JobRequestStatus)Enum.Parse(typeof(JobRequestStatus), item.state, true)
                        ,
                        StatusMeesage = item.message
                        ,
                        Type = (JobRequestType)Enum.Parse(typeof(JobRequestType), item.type, true)
                        ,
                        LastPublishedObjectIndex = item.lastPublishedObjectIndex
                        ,
                        LastPublishedRelationIndex = item.lastPublishedRelationIndex
                    });
            }
            return result;
        }

        public static string GetProcessIdByJobId(int jobId)
        {
            string processId = string.Empty;
            JobsDBEntities db = new JobsDBEntities();
            var query = (from temp in db.JobsTables
                         where
                               temp.id == jobId
                         select temp).FirstOrDefault();

            if (query != null)
            {
                processId = query.processID;
            }


            return processId;
        }

        private static JobsTable GetJobTableEntityForRequest(Request requestToSave)
        {
            JobRequestType requestType = JobRequestType.Unknown;
            if (requestToSave is SemiStructuredDataImportRequest)
            {
                SemiStructuredDataImportRequest ssImportRequest = requestToSave as SemiStructuredDataImportRequest;
                if (ssImportRequest.ImportMaterial is CsvFileMaterial)
                {
                    requestType = JobRequestType.ImportFromCsvFile;
                }
                else if (ssImportRequest.ImportMaterial is ExcelSheet)
                {
                    requestType = JobRequestType.ImportFromExcelSheet;
                }
                else if (ssImportRequest.ImportMaterial is AccessTable)
                {
                    requestType = JobRequestType.ImportFromAccessTable;
                }
                else if (ssImportRequest.ImportMaterial is AttachedDatabaseTableMaterial)
                {
                    requestType = JobRequestType.ImportFromAttachedDatabaseTableOrView;
                }
                else if (ssImportRequest.ImportMaterial is EmlDirectory)
                {
                    requestType = JobRequestType.ImportFromEmlDirectory;
                }
                else
                    throw new NotSupportedException("This semi-structured request type is unknown");
            }
            else
                throw new NotSupportedException("This request type is unknown");

            RequestSerializer serializer = new RequestSerializer();
            MemoryStream requestStream = new MemoryStream();
            serializer.Serialize(requestStream, requestToSave as Request);

            JobsTable jobsTable = new JobsTable()
            {
                request = requestStream.ToArray(),
                uniqeID = Guid.NewGuid().ToString(),
                type = requestType.ToString(),
                state = JobRequestStatus.Pending.ToString(),
                priority = JobRequestPriority.Normal.ToString(),
                registerDate = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                timeOutDuration = -1
            };
            return jobsTable;
        }

        internal static void SaveNewRequests(List<SemiStructuredDataImportRequest> newRequests)
        {
            using (TransactionScope transaction = new TransactionScope())
            {
                using (JobsDBEntities databaseContext = new JobsDBEntities())
                {
                    foreach (SemiStructuredDataImportRequest req in newRequests)
                    {
                        JobsTable newJob = GetJobTableEntityForRequest(req);
                        databaseContext.JobsTables.Add(newJob);
                        databaseContext.SaveChanges();
                    }
                    transaction.Complete();
                }
            }
        }

        //public void PauseJobMonitoringAgentService()
        //{
        //    ServiceController service = new ServiceController("JobMonitoringAgentService");
        //    service.Pause();
        //}

        public static void RestartJobMonitoringAgent()
        {
            ServiceController service = new ServiceController("JobMonitoringAgent");

            int timeoutMilliseconds = 4000;
            TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);


            if (service.Status == ServiceControllerStatus.Running)
            {
                service.Stop();
                service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);
            }

            if (service.Status == ServiceControllerStatus.Stopped)
            {
                service.Start();
                service.WaitForStatus(ServiceControllerStatus.Running, timeout);
            }
        }

        public static void KillRelatedJobWorker(int jobId)
        {
            int processId = 0;
            int.TryParse(JobsStoreAndRetrieveProvider.GetProcessIdByJobId(jobId), out processId);
            if (processId == 0)
            {
                return;
            }

            //Process proc = Process.GetProcessById(processId);
            //if (proc != null && proc.Responding)
            //{
            //    proc.Kill();
            //    proc.WaitForExit();
            //}

            Process[] processes = Process.GetProcesses();
            foreach (var item in processes)
            {
                if (item.Id == processId)
                {
                    item.Kill();
                    item.WaitForExit();
                }
            }
        }
    }
}
