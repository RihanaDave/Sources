using GPAS.Dispatch.ServiceAccess.JobService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.Dispatch.Logic
{
    public class JobsProvider
    {
        public Entities.Jobs.JobRequest[] GetJobRequests()
        {
            ServiceClient proxy = null;
            JobRequest[] result = null;
            try
            {
                proxy = new ServiceClient();
                result = proxy.GetJobRequests();
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }

            if (result == null)
            {
                throw new NullReferenceException("Invalid server responce");
            }
            else
            {
                return GetDispatchJobRequestFromJobSideInstance(result).ToArray();
            }
        }

        public static double GetImprotingPercent(int jobId)
        {
            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
              return  proxy.GetImprotingPercent(jobId);
            }
            catch (Exception)
            {
                return 0;
            }
        }

        public async static Task<List<Entities.Jobs.JobRequest>> GetJobsRequestAsync()
        {
            ServiceClient proxy = null;
            JobRequest[] result = null;
            try
            {
                proxy = new ServiceClient();
                result = await proxy.GetJobRequestsAsync();
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }

            if (result == null)
            {
                throw new NullReferenceException("Invalid server responce");
            }
            else
            {
                return GetDispatchJobRequestFromJobSideInstance(result);
            }
        }

        public static void SetPendingStateForJob(int jobId)
        {
            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                proxy.SetPauseStateForJob(jobId);
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }

        public static void SetPauseStateForJob(int jobId)
        {
            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                proxy.SetPauseStateForJob(jobId);
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }

        public static void SetResumedStateForJob(int jobId)
        {
            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                proxy.SetResumeStateForJob(jobId);
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }

        private static List<Entities.Jobs.JobRequest> GetDispatchJobRequestFromJobSideInstance(JobRequest[] serverSideJobRequest)
        {
            List<Entities.Jobs.JobRequest> workspaceSideJobRequest = new List<Entities.Jobs.JobRequest>();
            foreach (var item in serverSideJobRequest)
            {
                Entities.Jobs.JobRequest temp = new Entities.Jobs.JobRequest();
                temp.ID = item.ID;
                temp.RegisterTime = item.RegisterTime;
                temp.BeginTime = item.BeginTime;
                temp.EndTime = item.EndTime;
                temp.State = (Entities.Jobs.JobRequestStatus)item.State;
                temp.StatusMeesage = item.StatusMeesage;
                temp.Type = (Entities.Jobs.JobRequestType)item.Type;
                temp.LastPublishedObjectIndex = item.LastPublishedObjectIndex;
                temp.LastPublishedRelationIndex = item.LastPublishedRelationIndex;
                workspaceSideJobRequest.Add(temp);
            }
            return workspaceSideJobRequest;
        }
    }
}
