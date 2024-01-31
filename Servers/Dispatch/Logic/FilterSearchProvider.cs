using GPAS.AccessControl;
using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.ServiceAccess.SearchService;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.Dispatch.Logic
{
    public class FilterSearchProvider
    {
        private string CallerUserName = "";
        public FilterSearchProvider(string callerUserName)
        {
            CallerUserName = callerUserName;
        }

        public List<KObject> PerformFilterSearch(byte[] stream, int? count)
        {
            AuthorizationParametters authorizationParametter
                = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

            if (authorizationParametter.permittedGroupNames.Count == 0 || authorizationParametter.readableClassifications.Count == 0)
            {
                return new List<KObject>();
            }
            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                List<SearchObject> searchObjects = proxy.PerformFilterSearch(stream, count, authorizationParametter).ToList();

                List<KObject> kObjects = new List<KObject>();

                foreach (SearchObject searchObject in searchObjects)
                {
                    KObject kObject = new KObject()
                    {
                        Id = searchObject.Id,
                        LabelPropertyID = searchObject.LabelPropertyID,
                        TypeUri = searchObject.TypeUri,
                        IsMaster = searchObject.IsMaster,
                    };

                    if (searchObject.SearchObjectMaster != null)
                        kObject.KObjectMaster = new KObjectMaster()
                        {
                            Id = searchObject.SearchObjectMaster.Id,
                            MasterId = searchObject.SearchObjectMaster.MasterId,
                            ResolveTo = searchObject.SearchObjectMaster.ResolveTo
                        };

                    kObjects.Add(kObject);
                }

                return kObjects;
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }



        public List<long> PerformSelectMatching(byte[] stream, long[] ObjectIDs)
        {
            AuthorizationParametters authorizationParametter
                = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                List<long> objectIds = proxy.PerformSelectMatching(stream, ObjectIDs.ToArray(), authorizationParametter).ToList();
                return objectIds;
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }
    }
}
