using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch.ServiceAccess
{
    public class SearchSynchronizationServiceClient
    {
        public async Task<bool> IsHorizonDataIndicesStable()
        {
            ServiceAccess.HorizonService.ServiceClient proxy = null;
            bool result = false;
            try
            {
                proxy = new ServiceAccess.HorizonService.ServiceClient();
                result = await proxy.IsDataIndicesStableAsync();
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }

            return result;
        }
        public async Task<bool> IsSearchDataIndicesStable()
        {
            ServiceAccess.SearchService.ServiceClient proxy = null;
            bool result = false;
            try
            {
                proxy = new ServiceAccess.SearchService.ServiceClient();
                result =  await proxy.IsDataIndicesStableAsync();
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }

            return result;
        }
    }
}
