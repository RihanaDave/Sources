using System.Threading.Tasks;
//using Repo = GPAS.Dispatch.ServiceAccess.RepositoryService;
using Horizon = GPAS.Dispatch.ServiceAccess.HorizonService;
using Search = GPAS.Dispatch.ServiceAccess.SearchService;

namespace GPAS.Dispatch.Logic
{
    public class OptimizationProvider
    {
        public void PerformOptimization()
        {
            //Repo.ServiceClient repoService = null;
            Horizon.ServiceClient hrznService = null;
            Search.ServiceClient srchService = null;

            try
            {
                //repoService = new Repo.ServiceClient();
                //hrznService = new Horizon.ServiceClient();
                srchService = new Search.ServiceClient();

                //repoService.Open();
                //hrznService.Open();
                srchService.Open();

                //Task repoOpimizeTask = repoService.OptimizeAsync();
                Task srchOpimizeTask = srchService.OptimizeAsync();

                //Task.WaitAll(new Task[] { repoOpimizeTask, srchOpimizeTask });
                Task.WaitAll(new Task[] { srchOpimizeTask });
            }
            finally
            {
                //if (repoService != null)
                    //repoService.Close();
                if (hrznService != null)
                    hrznService.Close();
                if (srchService != null)
                    srchService.Close();
            }
        }
    }
}
