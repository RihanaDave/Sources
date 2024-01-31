using FileRepository = GPAS.Dispatch.ServiceAccess.FileRepositoryService;
using Horizon = GPAS.Dispatch.ServiceAccess.HorizonService;
using Job = GPAS.Dispatch.ServiceAccess.JobService;
//using Repository = GPAS.Dispatch.ServiceAccess.RepositoryService;
using Search = GPAS.Dispatch.ServiceAccess.SearchService;

namespace GPAS.Dispatch.Logic
{
    public class CheckingServersStatusProvider
    {

        public void CheckRepositoryStatus()
        {
            //Repository.ServiceClient serviceClient = new Repository.ServiceClient();
            //serviceClient.IsAvailable();
        }

        public void CheckFileRepositoryStatus()
        {
            FileRepository.ServiceClient serviceClient = new FileRepository.ServiceClient();
            serviceClient.IsAvailable();
        }

        public void CheckHorizonStatus()
        {
            Horizon.ServiceClient serviceClient = new Horizon.ServiceClient();
            serviceClient.IsAvailable();
        }

        public void CheckJobStatus()
        {
            Job.ServiceClient serviceClient = new Job.ServiceClient();
            serviceClient.IsAvailable();
        }

        public void CheckSearchStatus()
        {
            Search.ServiceClient serviceClient = new Search.ServiceClient();
            serviceClient.IsAvailable();
        }
    }
}
