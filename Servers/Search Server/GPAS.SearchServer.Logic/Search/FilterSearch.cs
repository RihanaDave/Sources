using GPAS.AccessControl;
using GPAS.FilterSearch;
using GPAS.SearchServer.Access.SearchEngine.ApacheSolr;
using GPAS.SearchServer.Entities;
using GPAS.Utility;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GPAS.SearchServer.Logic
{
    public class FilterSearch
    {
        private static FilterSearch _Instance = new FilterSearch();
        private readonly string FilterSearchResultDefaultCountAppSettingValue
        = System.Configuration.ConfigurationManager.AppSettings["FilterSearchResultDefaultCount"];

        public static FilterSearch Instance
        {
            get { return _Instance; }
        }

        public List<long> PerformSelectMatching(byte[] stream, List<long> objectIDs, AuthorizationParametters authorizationParametters)
        {
            long topValue = long.Parse(FilterSearchResultDefaultCountAppSettingValue);
            StreamUtility streamUtil = new StreamUtility();
            QuerySerializer serializer = new QuerySerializer();
            string serializedPrimaryQueryString = streamUtil.ByteArrayToStringUtf8(stream);
            Stream xmlStream = streamUtil.GenerateStreamFromString(serializedPrimaryQueryString);
            Query deserialize = serializer.Deserialize(xmlStream);
            CriteriaSet queryCriteriaSet = null;
            if (deserialize is Query)
            {
                queryCriteriaSet = deserialize.CriteriasSet;
            }
            AccessClient accessClient = new AccessClient();
            List<long> resultObjectIDList = accessClient.GetObjectDocumentIDByFilterCriteriaSet(
                 objectIDs
                , queryCriteriaSet, authorizationParametters
                , OntologyProvider.GetOntology(), topValue
                );
            return resultObjectIDList;
        }
        
        public List<SearchObject> PerformFilterSearch(byte[] serializedFilterSearchQuery, int? count, AuthorizationParametters authorizationParametters)
        {
            long topValue = 0;
            if (count.HasValue)
                topValue = (long)count;
            else
                topValue = long.Parse(FilterSearchResultDefaultCountAppSettingValue);

            StreamUtility streamUtil = new StreamUtility();
            QuerySerializer serializer = new QuerySerializer();
            string serializedPrimaryQueryString = streamUtil.ByteArrayToStringUtf8(serializedFilterSearchQuery);
            Stream xmlStream = streamUtil.GenerateStreamFromString(serializedPrimaryQueryString);
            Query deserialize = serializer.Deserialize(xmlStream);
            CriteriaSet queryCriteriaSet = null;
            if (deserialize is Query)
            {
                queryCriteriaSet = deserialize.CriteriasSet;
            }
            AccessClient accessClient = new AccessClient();
            List<SearchObject> resultObjectIDList = accessClient.GetObjectDocumentIDByFilterCriteriaSet(
                queryCriteriaSet, authorizationParametters
                , OntologyProvider.GetOntology(), topValue
                );
            return resultObjectIDList;
        }
    }
}
