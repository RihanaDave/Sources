using GPAS.AccessControl;
using GPAS.SearchServer.Access.SearchEngine.ApacheSolr;
using GPAS.TextualSearch;
using GPAS.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Logic.Search
{
    public class TextualSearchProvider
    {
        public List<TextualSearch.BaseSearchResult> PerformFilterSearch(byte[] serializedTextualSearchQuery, AuthorizationParametters authorizationParametters)
        {
            StreamUtility streamUtil = new StreamUtility();
            TextualQuerySerializer serializer = new TextualQuerySerializer();
            string serializedPrimaryQueryString = streamUtil.ByteArrayToStringUtf8(serializedTextualSearchQuery);
            Stream xmlStream = streamUtil.GenerateStreamFromString(serializedPrimaryQueryString);
            TextualSearchQuery deserialize = serializer.Deserialize(xmlStream);

            AccessClient accessClient = new AccessClient();
            return accessClient.GetResultsForTextualSearch(deserialize, authorizationParametters);
        }
    }
}
