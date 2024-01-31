using System;
using GPAS.AccessControl;
using GPAS.Dispatch.ServiceAccess.SearchService;
using GPAS.StatisticalQuery;
using GPAS.StatisticalQuery.ResultNode;
using QueryResult = GPAS.StatisticalQuery.QueryResult;

namespace GPAS.Dispatch.Logic
{
    public class StatisticalQueryProvider
    {
        string CallerUserName = string.Empty;
        public StatisticalQueryProvider(string callerUserName)
        {
            CallerUserName = callerUserName;
        }

        public QueryResult RunQuery(byte[] queryByteArray)
        {

            AuthorizationParametters authorizationParametter
                = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);
            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                return proxy.RunStatisticalQuery(queryByteArray, authorizationParametter);
            }
            finally
            {
                if(proxy != null)
                {
                    proxy.Close();
                }
            }
        }
        public long[] RetrieveObjectIDsByQuery(byte[] queryByteArray,int PassObjectsCountLimit)
        {
            AuthorizationParametters authorizationParametter
                = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);
            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                return proxy.RetrieveObjectIDsByStatisticalQuery(queryByteArray, PassObjectsCountLimit, authorizationParametter);
            }
            finally
            {
                if (proxy != null)
                {
                    proxy.Close();
                }
            }
        }
        public PropertyValueStatistics RetrievePropertyValueStatistics
            (byte[] queryByteArray, string exploredPropertyTypeUri, int startOffset, int resultsLimit, long minimumCount)
        {
            AuthorizationParametters authorizationParametter
                = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);
            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                return proxy.RetrievePropertyValueStatistics
                    (queryByteArray, exploredPropertyTypeUri, startOffset
                    , resultsLimit, minimumCount, authorizationParametter);
            }
            finally
            {
                if (proxy != null)
                {
                    proxy.Close();
                }
            }
        }

        public PropertyBarValues RetrievePropertyBarValuesStatistics
    (byte[] queryByteArray, string exploredPropertyTypeUri, long bucketCount, double minValue, double maxValue)
        {
            AuthorizationParametters authorizationParametter
                = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);
            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                return proxy.RetrievePropertyBarValuesStatistics
                    (queryByteArray, exploredPropertyTypeUri, bucketCount, minValue, maxValue, authorizationParametter);
            }
            finally
            {
                if (proxy != null)
                {
                    proxy.Close();
                }
            }
        }


        public LinkTypeStatistics RetrieveLinkTypeStatistics(byte[] queryByteArray)
        {
            AuthorizationParametters authorizationParametter
               = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);
            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                return proxy.RetrieveLinkTypeStatistics
                    (queryByteArray, authorizationParametter);
            }
            finally
            {
                if (proxy != null)
                {
                    proxy.Close();
                }
            }
        }

        public long[] RetrieveLinkedObjectIDsByStatisticalQuery(byte[] queryByteArray, int PassObjectsCountLimit)
        {
            AuthorizationParametters authorizationParametter
               = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);
            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                return proxy.RetrieveLinkedObjectIDsByStatisticalQuery
                    (queryByteArray, PassObjectsCountLimit, authorizationParametter);
            }
            finally
            {
                if (proxy != null)
                {
                    proxy.Close();
                }
            }
        }
    }
}
