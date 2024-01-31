using GPAS.AccessControl;
//using GPAS.SearchServer.Access.RepositoryService;
using GPAS.SearchServer.Entities;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Access.DataClient
{
    public class RetrieveDataClient
    {
        private static Queue<long> DataSourceIdOfCachedACLs = new Queue<long>();
        private static Dictionary<long, ACL> CachedDataSourceACLsPerDataSourceId = new Dictionary<long, ACL>();
        private static object ACLCacheLockObject = new object();
        private const int MaxCacheSize = 1000;

        #region public functions
        public void PrepairDataSourceCachedForReset()
        {
            //clear cache
            DataSourceIdOfCachedACLs.Clear();
            CachedDataSourceACLsPerDataSourceId.Clear();
            //get top MaxCacheSize from repository and add to cache
            List<SearchDataSourceACL> dataSourceACLs = RetrieveTopNACLs(MaxCacheSize);
            InitCache(dataSourceACLs);
        }

        public Dictionary<long, ACL> GetDataSourceACLsMapping(List<long> dataSourceIds)
        {
            Dictionary<long, ACL> aclIdToAclMapping = new Dictionary<long, ACL>();
            if (!dataSourceIds.Any())
                return aclIdToAclMapping;

            List<long> aclsToRetrieve = new List<long>();
            foreach (var aclId in dataSourceIds)
            {
                if (CachedDataSourceACLsPerDataSourceId.Keys.Contains(aclId))
                {
                    if (!aclIdToAclMapping.Keys.Contains(aclId))
                    {
                        aclIdToAclMapping.Add(aclId, CachedDataSourceACLsPerDataSourceId[aclId]);
                    }
                }
                else
                {
                    aclsToRetrieve.Add(aclId);
                }
            }
            if (aclsToRetrieve.Any())
            {
                List<SearchDataSourceACL> dataSourceACLs = RetrieveACLs(aclsToRetrieve);
                List<ACL> newCachedACL = CacheRetrievedACLs(dataSourceACLs);
                foreach (SearchDataSourceACL dataSourceAcl in dataSourceACLs)
                {
                    if (!aclIdToAclMapping.Keys.Contains(dataSourceAcl.Id))
                    {
                        aclIdToAclMapping.Add(dataSourceAcl.Id, dataSourceAcl.Acl);
                    }
                }
            }
            return aclIdToAclMapping;
        }
        public List<AccessControled<SearchMedia>> RetrieveMediasOfObjects(long[] searchingObjectIDs)
        {
            //ServiceClient sc = null;
            //DBMedia[] serverResonse;
            //try
            //{
            //    sc = new ServiceClient();
            //    serverResonse = sc.GetMediasForObjectsWithoutAuthorization(searchingObjectIDs);
            //}
            //finally
            //{
            //    if (sc != null)
            //        sc.Close();
            //}
            //Dictionary<long, ACL> dataSourceIdToAclMapping = GetDataSourceACLsMapping(serverResonse.Select(id => id.DataSourceID).ToList());
            //EntityConvertor convertor = new EntityConvertor();
            //List<SearchMedia> convertedResponse = convertor.ConvertDBMediasArrayToSearchMediasList(serverResonse);
            //List<AccessControled<SearchMedia>> accessControledSearchMediaList = new List<AccessControled<SearchMedia>>();
            //foreach (SearchMedia searchMedia in convertedResponse)
            //{
            //    ACL acl = ConvertDBDataSourceAclToAccessControlAcl(dataSourceIdToAclMapping[searchMedia.DataSourceID]);

            //    accessControledSearchMediaList.Add(new AccessControled<SearchMedia>
            //    {
            //        ConceptInstance = searchMedia,
            //        Acl = acl
            //    });
            //}

            //return accessControledSearchMediaList;

            return new List<AccessControled<SearchMedia>>();
        }

        public List<AccessControled<SearchRelationship>> RetrieveRelationshipsSourcedByObjects(List<long> searchObjectsPerID)
        {
            //ServiceClient sc = null;
            //DBRelationship[] serverResonse;
            //try
            //{
            //    sc = new ServiceClient();
            //    serverResonse = sc.GetRelationshipsBySourceObjectWithoutAuthParams(searchObjectsPerID.ToArray());
            //}
            //finally
            //{
            //    if (sc != null)
            //        sc.Close();
            //}

            var serverResonse = new Access.SearchEngine.ApacheSolr.AccessClient().GetRelationshipsBySourceObjectWithoutAuthParams(searchObjectsPerID);

            Dictionary<long, ACL> dataSourceIdToAclMapping = GetDataSourceACLsMapping(serverResonse.Select(id => id.DataSourceID).ToList());
            EntityConvertor convertor = new EntityConvertor();
            List<SearchRelationship> convertedResponse = convertor.ConvertDBRelationshipsArrayToSearchRelationshipsList(serverResonse.ToArray());
            List<AccessControled<SearchRelationship>> accessControledSearchPropertyList = new List<AccessControled<SearchRelationship>>();
            foreach (SearchRelationship searchRel in convertedResponse)
            {
                ACL acl = ConvertDBDataSourceAclToAccessControlAcl(dataSourceIdToAclMapping[searchRel.DataSourceID]);
                accessControledSearchPropertyList.Add(new AccessControled<SearchRelationship>
                {
                    ConceptInstance = searchRel,
                    Acl = acl
                });
            }
            return accessControledSearchPropertyList;
        }

        public List<AccessControled<SearchProperty>> RetrievePropertiesOfObjects(List<long> searchObjectsPerID)
        {
            //ServiceClient sc = null;
            //DBProperty[] serverResonse;
            //try
            //{
            //    sc = new ServiceClient();
            //    serverResonse = sc.GetPropertiesOfObjectsWithoutAuthorization(searchObjectsPerID.ToArray());
            //}
            //finally
            //{
            //    if (sc != null)
            //        sc.Close();
            //}


            var serverResonse = new Access.SearchEngine.ApacheSolr.AccessClient().GetDBPropertyByObjectIdsWithoutAuthorization(searchObjectsPerID.ToArray());

            Dictionary<long, ACL> dataSourceIdToAclMapping = GetDataSourceACLsMapping(serverResonse.Select(id => id.DataSourceID).ToList());
            EntityConvertor convertor = new EntityConvertor();
            List<SearchProperty> convertedResponse = convertor.ConvertDBPoepertiesArrayToSearchPropertiesList(serverResonse.ToArray()).ToList();
            List<AccessControled<SearchProperty>> accessControledSearchPropertyList = new List<AccessControled<SearchProperty>>();
            foreach (SearchProperty searchProperty in convertedResponse)
            {
                ACL acl = ConvertDBDataSourceAclToAccessControlAcl(dataSourceIdToAclMapping[searchProperty.DataSourceID]);
                accessControledSearchPropertyList.Add(new AccessControled<SearchProperty>
                {
                    ConceptInstance = searchProperty,
                    Acl = acl
                });
            }
            return accessControledSearchPropertyList;


        }

        public async Task<List<SearchObject>> RetrieveObjectsSequentialByIDRangeAsync(long firstID, long lastID)
        {
            //ServiceClient sc = null;
            //DBObject[] serverResonse;
            //try
            //{
            //    sc = new ServiceClient();
            //    serverResonse = await sc.RetrieveObjectsSequentialByIDRangeAsync(firstID, lastID);
            //}
            //finally
            //{
            //    if (sc != null)
            //        sc.Close();
            //}
            //EntityConvertor convertor = new EntityConvertor();
            //return convertor.ConvertDBObjectsArrayToSearchObjectsList(serverResonse);

            return new Access.SearchEngine.ApacheSolr.AccessClient().RetrieveObjectsSequentialByIDRange(firstID, lastID);
        }

        public List<SearchObject> GetObjectsByIDsAsync(long[] ids)
        {
            //ServiceClient sc = null;
            //DBObject[] serverResonse;
            //try
            //{
            //    sc = new ServiceClient();
            //    serverResonse = await sc.GetObjectsAsync(ids);
            //}
            //finally
            //{
            //    if (sc != null)
            //        sc.Close();
            //}
            //EntityConvertor convertor = new EntityConvertor();
            //return convertor.ConvertDBObjectsArrayToSearchObjectsArray(serverResonse).ToList();
            //-------------------------------------------------

            List<SearchObject> serverResonse = new List<SearchObject>();
            try
            {
                serverResonse =  new Access.SearchEngine.ApacheSolr.AccessClient().GetObjectByIDs(ids);
            }
            finally
            {
                //if (sc != null)
                    //sc.Close();
            }
            EntityConvertor convertor = new EntityConvertor();
            return convertor.ConvertDBObjectsArrayToSearchObjectsArray(serverResonse).ToList();
        }


        //public SearchObject[] GetObjectsByIDs(List<long> ids)
        //{
        //    ServiceClient sc = null;
        //    DBObject[] serverResonse;
        //    try
        //    {
        //        sc = new ServiceClient();
        //        serverResonse = sc.GetObjects(ids.ToArray());
        //    }
        //    finally
        //    {
        //        if (sc != null)
        //            sc.Close();
        //    }
        //    EntityConvertor convertor = new EntityConvertor();
        //    return convertor.ConvertDBObjectsArrayToSearchObjectsArray(serverResonse);
        //}
        #endregion

        #region private functions

        private ACL ConvertDBDataSourceAclToAccessControlAcl(ACL repositoryAcl)
        {
            List<ACI> permissions = new List<ACI>();
            foreach (var permissoin in repositoryAcl.Permissions)
            {
                permissions.Add(
                    new ACI()
                    {
                        AccessLevel = (AccessControl.Permission)permissoin.AccessLevel,
                        GroupName = permissoin.GroupName
                    });
            }
            return new ACL()
            {
                Classification = repositoryAcl.Classification,
                Permissions = permissions
            };
        }

        private List<ACL> CacheRetrievedACLs(List<SearchDataSourceACL> dataSourceACLs)
        {
            List<ACL> newCachedACL = new List<ACL>();
            foreach (var dataSourceACL in dataSourceACLs)
            {
                if (DataSourceIdOfCachedACLs.Count >= MaxCacheSize)
                {
                    lock (ACLCacheLockObject)
                    {
                        RemoveACLFromCache();
                        AddACLToCache(dataSourceACL);
                        newCachedACL.Add(dataSourceACL.Acl);
                    }
                }
                else
                {
                    lock (ACLCacheLockObject)
                    {
                        AddACLToCache(dataSourceACL);
                        newCachedACL.Add(dataSourceACL.Acl);
                    }
                }
            }
            return newCachedACL;
        }

        private void RemoveACLFromCache()
        {
            var dequeueACL = DataSourceIdOfCachedACLs.Dequeue();
            CachedDataSourceACLsPerDataSourceId.Remove(dequeueACL);
        }

        private List<SearchDataSourceACL> RetrieveTopNACLs(long topN)
        {
            //ServiceClient sc = null;
            //try
            //{
            //    sc = new ServiceClient();
            //    List<DBDataSourceACL> dataSourceACLs = sc.RetrieveTopNDataSourceACLs(topN).ToList();
            //    return dataSourceACLs;
            //}
            //finally
            //{
            //    if (sc != null)
            //        sc.Close();
            //}

            return new Access.SearchEngine.ApacheSolr.AccessClient().RetrieveTopNDataSourceACLs(topN);
        }

        private void InitCache(List<SearchDataSourceACL> dataSourceACLs)
        {
            foreach (var dataSourceACL in dataSourceACLs)
            {
                lock (ACLCacheLockObject)
                {
                    AddACLToCache(dataSourceACL);
                }
            }
        }

        private void AddACLToCache(SearchDataSourceACL dbDataSourceACL)
        {
            DataSourceIdOfCachedACLs.Enqueue(dbDataSourceACL.Id);
            CachedDataSourceACLsPerDataSourceId.Add(dbDataSourceACL.Id, dbDataSourceACL.Acl);
        }

        public async Task<DataSourceInfo[]> GetDataSourcesByIDsAsync(List<long> dataSourceIDs)
        {
            //ServiceClient sc = null;
            //try
            //{
            //    sc = new ServiceClient();
            //    return await sc.GetDataSourcesByIDsAsync(dataSourceIDs.ToArray());
            //}
            //finally
            //{
            //    if (sc != null)
            //        sc.Close();
            //}

            DataSourceInfo[] result = null;

            await Task.Run(() =>
            {
                SearchEngine.ApacheSolr.AccessClient accessClient = new SearchEngine.ApacheSolr.AccessClient();
                result = accessClient.GetDataSourcesByIDs(dataSourceIDs).ToArray();
            });

            return result;
        }

        private List<SearchDataSourceACL> RetrieveACLs(List<long> aclsToRetrieve)
        {
            //ServiceClient sc = null;
            GPAS.SearchServer.Access.SearchEngine.ApacheSolr.AccessClient accessClient = null;
            try
            {
                //sc = new ServiceClient();
                accessClient = new SearchEngine.ApacheSolr.AccessClient();

                //List<DBDataSourceACL> dataSourceACLs = sc.RetrieveDataSourceACLs(aclsToRetrieve.ToArray()).ToList();
                //return dataSourceACLs;

                List<SearchDataSourceACL> dataSourceACLs = accessClient.RetrieveDataSourceACLs(aclsToRetrieve.ToArray()).ToList();

                List<SearchDataSourceACL> dBDataSourceACLs = new List<SearchDataSourceACL>();
                foreach (var item in dataSourceACLs)
                {
                    SearchDataSourceACL dB = new SearchDataSourceACL()
                    {
                         Id = item.Id, 
                          Acl = item.Acl
                    };
                    dBDataSourceACLs.Add(dB);
                }

                return dBDataSourceACLs;
            }
            finally
            {
                //if (sc != null)
                    //sc.Close();
            }
        }

        #endregion
    }
}