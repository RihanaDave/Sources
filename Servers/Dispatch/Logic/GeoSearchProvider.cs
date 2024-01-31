using GPAS.AccessControl;
using GPAS.FilterSearch;
using GPAS.GeoSearch;
using System;
using GPAS.Dispatch.ServiceAccess.SearchService;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPAS.Dispatch.Entities.Concepts;

namespace GPAS.Dispatch.Logic
{
   public class GeoSearchProvider
    {

        private string CallerUserName = "";
        public GeoSearchProvider(string callerUserName)
        {
            CallerUserName = callerUserName;
        }

        public List<KObject> PerformGeoCircleSearch(CircleSearchCriteria circleSearchCriteria, int maxResult)
        {
            AuthorizationParametters authorizationParametter
               = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                SearchObject[] searchObjects = proxy.PerformGeoCircleSearch(circleSearchCriteria, maxResult, authorizationParametter);

                List<KObject> kObjects = new List<KObject>();

                foreach (var searchObject in searchObjects)
                {
                    kObjects.Add(new KObject()
                    {
                        Id = searchObject.Id,
                        LabelPropertyID = searchObject.LabelPropertyID,
                        TypeUri = searchObject.TypeUri
                    });
                }

                return kObjects;

                //return objectIds;
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }

        public List<KObject> PerformGeoPolygonSearch(PolygonSearchCriteria polygonSearchCriteria, int maxResult)
        {
            AuthorizationParametters authorizationParametter
               = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                SearchObject[] searchObjects =  proxy.PerformGeoPolygonSearch(polygonSearchCriteria, maxResult, authorizationParametter);

                List<KObject> kObjects = new List<KObject>();

                foreach (var searchObject in searchObjects)
                {
                    kObjects.Add(new KObject()
                    {
                        Id = searchObject.Id,
                        LabelPropertyID = searchObject.LabelPropertyID,
                        TypeUri = searchObject.TypeUri
                    });
                }

                return kObjects;

                //return objectIds;
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }


        public List<KObject> PerformGeoPolygonFilterSearch(PolygonSearchCriteria polygonSearchCriteria, FilterSearch.CriteriaSet filterSearchCriteria, int maxResult)
        {
            AuthorizationParametters authorizationParametter
             = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                SearchObject[] searchObjects = proxy.PerformGeoPolygonFilterSearch(polygonSearchCriteria, filterSearchCriteria, maxResult, authorizationParametter);

                List<KObject> kObjects = new List<KObject>();

                foreach (var searchObject in searchObjects)
                {
                    kObjects.Add(new KObject()
                    {
                        Id = searchObject.Id,
                        LabelPropertyID = searchObject.LabelPropertyID,
                        TypeUri = searchObject.TypeUri
                    });
                }

                return kObjects;

                //return objectIds;
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }

        public List<KObject> PerformGeoCircleFilterSearch(CircleSearchCriteria circleSearchCriteria, FilterSearch.CriteriaSet filterSearchCriteria, int maxResult)
        {
            AuthorizationParametters authorizationParametter
             = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                SearchObject[] searchObjects = proxy.PerformGeoCircleFilterSearch(circleSearchCriteria, filterSearchCriteria, maxResult, authorizationParametter);


                List<KObject> kObjects = new List<KObject>();

                foreach (var searchObject in searchObjects)
                {
                    kObjects.Add(new KObject()
                    {
                        Id = searchObject.Id,
                        LabelPropertyID = searchObject.LabelPropertyID,
                        TypeUri = searchObject.TypeUri
                    });
                }

                return kObjects;

                //return objectIds;
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }
    }
}
