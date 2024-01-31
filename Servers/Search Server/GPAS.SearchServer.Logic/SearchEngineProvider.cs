using GPAS.AccessControl.Groups;
using GPAS.SearchServer.Access.SearchEngine;
using System.Linq;

namespace GPAS.SearchServer.Logic
{
    public class SearchEngineProvider
    {
        public static void Init()
        {
            string[] existGroupNames = RetrieveActiveGroupNames();
            IAccessClient initializerEngineClient = GetNewDefaultSearchEngineClient();
            initializerEngineClient.Init(existGroupNames);
        }

        public static IAccessClient GetNewDefaultSearchEngineClient()
        {
            return new Access.SearchEngine.ApacheSolr.AccessClient();
        }

        private static string[] RetrieveActiveGroupNames()
        {
            GroupInfo[] groups;
            Access.DispatchService.InfrastructureServiceClient proxy = null;
            try
            {
                proxy = new Access.DispatchService.InfrastructureServiceClient();
                groups = proxy.GetGroups();
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
            return groups.Select(g => g.GroupName).ToArray();
        }
    }
}
