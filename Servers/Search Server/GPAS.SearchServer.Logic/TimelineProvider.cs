using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPAS.AccessControl;
using GPAS.SearchServer.Access.SearchEngine;

namespace GPAS.SearchServer.Logic
{
    public class TimelineProvider
    {
        public long GetTimeLineMaxFrequecyCount(List<string> propertiesTypeUri, string binLevel, AuthorizationParametters authorizationParametters)
        {
            IAccessClient accessClient = SearchEngineProvider.GetNewDefaultSearchEngineClient();
            return accessClient.GetTimelineMaxFrequecyCount(propertiesTypeUri, binLevel, OntologyProvider.GetOntology(), authorizationParametters);
        }

        public DateTime GetTimeLineMaxDate(List<string> propertiesTypeUri, string binLevel, AuthorizationParametters authorizationParametters)
        {
            IAccessClient accessClient = SearchEngineProvider.GetNewDefaultSearchEngineClient();
            return accessClient.GetTimelineMaxDate(propertiesTypeUri, binLevel, OntologyProvider.GetOntology(), authorizationParametters);
        }

        public DateTime GetTimeLineMinDate(List<string> propertiesTypeUri, string binLevel, AuthorizationParametters authorizationParametters)
        {
            IAccessClient accessClient = SearchEngineProvider.GetNewDefaultSearchEngineClient();
            return accessClient.GetTimelineMinDate(propertiesTypeUri, binLevel, OntologyProvider.GetOntology(), authorizationParametters);
        }
    }
}
