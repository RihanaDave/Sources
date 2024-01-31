using GPAS.AccessControl;
using GPAS.FilterSearch;
using GPAS.GeoSearch;
using GPAS.Ontology;
using GPAS.SearchServer.Access.SearchEngine.ApacheSolr;
using GPAS.SearchServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Logic
{
    public class GeoSearch
    {

        public List<SearchObject> PerformGeoCircleSearch(CircleSearchCriteria circleSearchCriteria, int maxResult, AuthorizationParametters authorizationParametters)
        {
            AccessClient accessClient = new AccessClient();
            return accessClient.GetObjectDocumentIDByGeoCircleSearch(circleSearchCriteria, maxResult, authorizationParametters);
            //return resultObjectIDList;
        }

        public List<SearchObject> PerformGeoPolygonSearch(PolygonSearchCriteria polygonSearchCriteria, int maxResult, AuthorizationParametters authorizationParametters)
        {
            AccessClient accessClient = new AccessClient();
            return accessClient.GetObjectDocumentIDByGeoPolygonSearch(polygonSearchCriteria, maxResult, authorizationParametters);
            //return resultObjectIDList;
        }

        public List<SearchObject> PerformGeoCircleFilterSearch(CircleSearchCriteria circleSearchCriteria, CriteriaSet filterSearchCriteria, int maxResult, AuthorizationParametters authorizationParametters)
        {
            AccessClient accessClient = new AccessClient();
            return accessClient.GetObjectDocumentIDByGeoCircleFilterSearch(circleSearchCriteria, filterSearchCriteria, maxResult, authorizationParametters, OntologyProvider.GetOntology());
            //return resultObjectIDList;
        }
        public List<SearchObject> PerformGeoPolygonFilterSearch(PolygonSearchCriteria polygonSearchCriteria, CriteriaSet filterSearchCriteria, int maxResult, AuthorizationParametters authorizationParametters)
        {
            AccessClient accessClient = new AccessClient();
            return accessClient.GetObjectDocumentIDByGeoPolygonFilterSearch(polygonSearchCriteria, filterSearchCriteria, maxResult, authorizationParametters, OntologyProvider.GetOntology());
            //return resultObjectIDList;
        }
    }
}
