using GPAS.JobServer.Logic;
using GPAS.JobServer.Logic.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.ServiceProcess;
using System.Threading.Tasks;

namespace GPAS.JobServer.JobMonitoringAgent
{
    public partial class JobMonitoringAgentService : ServiceBase
    {
        private int CheckingIntervalMiliseconds = 0; //default value of wait for running pending items
        private string WorkerProcessPath = "";
        MonitoringLogger logger = new MonitoringLogger();
        public JobMonitoringAgentService(string[] args)
        {
            InitializeComponent();
            CanPauseAndContinue = true;
        }

        protected async override void OnStart(string[] args)
        {
            await StartMonitoring();
        }

        private bool isMonitoringRunning = false;
        private async Task StartMonitoring()
        {
            InitLogger();
            WriteLog("Starting Agent service...");
            InitWorkerProcessPath();
            InitMonitoringIntervalTime();
            isMonitoringRunning = true;
            WriteLog("Agent service Started.");

            await MonitorJobs();
        }

        private void InitMonitoringIntervalTime()
        {
            if (!int.TryParse(ConfigurationManager.AppSettings["CheckingIntervalMiliseconds"], out CheckingIntervalMiliseconds))
            {
                CheckingIntervalMiliseconds = 5000; // Default value
            }
        }

        private void InitLogger()
        {
            try
            {
                logger.Initialization();
            }
            catch (Exception ex)
            {
                WriteLog($"Unable to init Process-Logger, no log will store. {ex.ToString()}");
            }
        }

        private void InitWorkerProcessPath()
        {
            WorkerProcessPath = AppDomain.CurrentDomain.BaseDirectory + ConfigurationManager.AppSettings["WorkerProcessPath"]; //jobworker.exe directory
            WriteLog($"Worker path is: {WorkerProcessPath}");
        }

        private async Task MonitorJobs()
        {
            Logger.ExceptionDetailGenerator detGen = new Logger.ExceptionDetailGenerator();
            while (isMonitoringRunning)
            {
                try
                {
                    CheckoutJobs();
                    WriteLog("Ready");
                }
                catch (Exception ex)
                {
                    WriteLog(string.Format("Monitoing iteration to checking out Jobs failed; {0}", detGen.GetDetails(ex)));
                }
                await Task.Delay(CheckingIntervalMiliseconds);
            }
            try
            {
                WriteLog("Job monitoring stoped");
                logger.Finalization();
            }
            catch
            { }
        }

        protected override void OnStop()
        {
            StopMonitoring();
        }

        private void StopMonitoring()
        {
            WriteLog("Stoping Agent service...");
            JobsStoreAndRetrieveProvider jobDatabaseProvider = new JobsStoreAndRetrieveProvider();
            foreach (var checkingType in Enum.GetValues(typeof(JobRequestType)))
            {
                var busyRequests = jobDatabaseProvider.GetJobsRequest(JobRequestStatus.Busy, (JobRequestType)checkingType);
                foreach (var item in busyRequests)
                {
                    int processId = jobDatabaseProvider.GetAssignedProcessID(item.ID);
                    if (!IsProcessAlive(processId))
                        SetJobAsTerminated(item.ID, "Job process is terminated because of an unknown reason");
                    // else if(...
                    // TODO: Check Job time-out duration
                }
            }
            isMonitoringRunning = false;
            WriteLog("Agent service Stoped.");
        }

        public void CheckoutJobs()
        {
            JobsStoreAndRetrieveProvider jobDatabaseProvider = new JobsStoreAndRetrieveProvider();

            foreach (var checkingType in Enum.GetValues(typeof(JobRequestType)))
            {
                if ((JobRequestType)checkingType == JobRequestType.Unknown)
                    continue;

                var resumeRequests = jobDatabaseProvider.GetJobsRequest(JobRequestStatus.Resume, (JobRequestType)checkingType);
                var pendingRequests = jobDatabaseProvider.GetJobsRequest(JobRequestStatus.Pending, (JobRequestType)checkingType);

                if (resumeRequests.Count > 0)
                {
                    DateTime latestResumedRequest = resumeRequests.Min(request => DateTime.Parse(request.RegisterTime, CultureInfo.InvariantCulture));
                    StartRunningJob(resumeRequests);
                }
                else
                {
                    var busyRequests = jobDatabaseProvider.GetJobsRequest(JobRequestStatus.Busy, (JobRequestType)checkingType);
                    if (pendingRequests.Count > 0)
                    {
                        DateTime latestPenndingRequest = pendingRequests.Min(request => DateTime.Parse(request.RegisterTime, CultureInfo.InvariantCulture));
                        StartRunningJob(pendingRequests
                            .Where(request => DateTime.Parse(request.RegisterTime, CultureInfo.InvariantCulture).Equals(latestPenndingRequest)).Take(ParallelBusyJobsCount - busyRequests.Count));
                    }
                }

                //var pauseRequests = jobDatabaseProvider.GetJobsRequest(JobRequestStatus.Pause, (JobRequestType)checkingType);
                //if (pauseRequests.Count() > 0)
                //{
                //    WriteLog(string.Format("Pause Request is {0}", pauseRequests.ElementAt(0).ID));
                //    PauseRunningJob(pauseRequests);
                //}

                //var busyRequests = jobDatabaseProvider.GetJobsRequest(JobRequestStatus.Busy, (JobRequestType)checkingType);
                //if (busyRequests.Count < ParallelBusyJobsCount)
                //{
                //    var pendingRequests = jobDatabaseProvider.GetJobsRequest(JobRequestStatus.Pending, (JobRequestType)checkingType);
                //    if (pendingRequests.Count >= 1)
                //    {
                //        DateTime latestPenndingRequest = pendingRequests.Min(request => DateTime.Parse(request.RegisterTime, CultureInfo.InvariantCulture));
                //        StartRunningJob(pendingRequests
                //            .Where(request => DateTime.Parse(request.RegisterTime, CultureInfo.InvariantCulture).Equals(latestPenndingRequest)).Take(ParallelBusyJobsCount - busyRequests.Count));
                //    }
                //}
                //else
                //    foreach (var item in busyRequests)
                //    {
                //        int processId = jobDatabaseProvider.GetAssignedProcessID(item.ID);
                //        if (!IsProcessAlive(processId))
                //            SetJobAsTerminated(item.ID, "Job process is terminated because of an unknown reason");
                //        // else if(...
                //        // TODO: Check Job time-out duration
                //    }
            }
        }

        private bool IsProcessAlive(int processId)
        {
            return Process.GetProcesses().Where(p => p.Id == processId).Count() > 0;
        }
        private void StartRunningJob(IEnumerable<JobRequest> requestsToStart)
        {
            JobsStoreAndRetrieveProvider provider = new JobsStoreAndRetrieveProvider();
            List<JobRequest> jobRequests = provider.GetJobRequests();
            bool existBusyJob = false;
            if (jobRequests.Where(j => j.State == JobRequestStatus.Busy).Count() > 0)
            {
                existBusyJob = true;
            }

            foreach (var request in requestsToStart)
            {
                try
                {
                    if (!JobsStoreAndRetrieveProvider.IsBusyJob(request.ID) && !existBusyJob)
                    {
                        ProcessStartInfo startInfo = new ProcessStartInfo();
                        startInfo.FileName = WorkerProcessPath;
                        startInfo.Arguments = request.ID.ToString(CultureInfo.InvariantCulture);
                        // فراخوانی پروسه اجرای کار
                        Process process = Process.Start(startInfo);

                        WriteLog(string.Format("Start running Job with ID: {0}", request.ID));
                        // ثبت شناسه پروسه در بانک اطلاعاتی
                        JobsStoreAndRetrieveProvider.SetProcessIdForJob(request.ID, process.Id);
                        WriteLog(string.Format("Set process ID for busy Job; Job ID: {0}; Process ID: {1}", request.ID, process.Id));
                    }
                }
                catch (Exception ex)
                {
                    WriteLog(ex.ToString());
                }
            }
        }
        private void PauseRunningJob(IEnumerable<JobRequest> requestsToStart)
        {
            JobsStoreAndRetrieveProvider jobDatabaseProvider = new JobsStoreAndRetrieveProvider();

            foreach (var request in requestsToStart)
            {
                try
                {
                    // فراخوانی پروسه اجرای کار
                    if (IsProcessAlive(jobDatabaseProvider.GetAssignedProcessID(request.ID)))
                    {
                        Process process = Process.GetProcesses().Where(p => p.Id == jobDatabaseProvider.GetAssignedProcessID(request.ID)).ElementAt(0);
                        WriteLog(string.Format("Pause running Job with ID: {0}", request.ID));
                        process.Kill();
                    }
                    WriteLog(string.Format("Pause running Job is done with ID: {0}", request.ID));
                }
                catch (Exception ex)
                {
                    WriteLog(ex.ToString());
                }
            }

        }
        private void SetJobAsTerminated(int jobId, string message)
        {
            // ثبت وضعیت در بانک اطلاعاتی
            JobsStoreAndRetrieveProvider.SetTerminatedStateForJob(jobId, message);
            WriteLog(string.Format("Set Job as terminated; Job ID: {0}; Reason: {1}", jobId, message));
        }

        private void WriteLog(string logMessage)
        {
            if (!logMessage.Equals(lastLogMessage))
            {
                logger.WriteLog(logMessage);
                lastLogMessage = logMessage;
            }
        }

        string lastLogMessage = string.Empty;

        private static readonly int ParallelBusyJobsCount = int.Parse(ConfigurationManager.AppSettings["ParallelBusyJobsCount"]);

    }
}