using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPAS.AccessControl;
using GPAS.Dispatch.ServiceAccess.SearchService;

namespace GPAS.Dispatch.Logic
{
    public class TimelineProvider
    {
        private string CallerUserName = "";
        public TimelineProvider(string callerUserName)
        {
            CallerUserName = callerUserName;
        }

        public long GetTimeLineMaxFrequecyCount(string[] propertiesTypeUri, string binLevel)
        {
            AuthorizationParametters authorizationParametter
            = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                return proxy.GetTimeLineMaxFrequecyCount(propertiesTypeUri, binLevel, authorizationParametter);
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }

        public DateTime GetTimeLineMaxDate(string[] propertiesTypeUri, string binLevel)
        {
            AuthorizationParametters authorizationParametter
                        = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                return proxy.GetTimeLineMaxDate(propertiesTypeUri, binLevel, authorizationParametter);
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }

        public DateTime GetTimeLineMinDate(string[] propertiesTypeUri, string binLevel)
        {
            AuthorizationParametters authorizationParametter
                        = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                return proxy.GetTimeLineMinDate(propertiesTypeUri, binLevel, authorizationParametter);
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }
    }
}
