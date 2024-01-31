using GPAS.Dispatch.Entities.Concepts;
//using GPAS.Horizon.Access.RepositoryService;
using GPAS.Horizon.Entities;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.Horizon.Access.DataClient
{
    public class RetrieveDataClient
    {
        public static Queue<long> DataSourceIdOfCachedACLs = new Queue<long>();
        public static Dictionary<long, AccessControl.ACL> CachedDataSourceACLsPerDataSourceId = new Dictionary<long, AccessControl.ACL>();
        private static object ACLCacheLockObject = new object();
        private const int MaxCacheSize = 1000;

        #region private functions
        private void CacheRetrievedACLs(List<SearchService.SearchDataSourceACL> dataSourceACLs)
        {
            foreach (var dataSourceACL in dataSourceACLs)
            {
                if (DataSourceIdOfCachedACLs.Count >= MaxCacheSize)
                {
                    lock (ACLCacheLockObject)
                    {
                        RemoveACLFromCache();
                        AddACLToCache(dataSourceACL);
                    }
                }
                else
                {
                    lock (ACLCacheLockObject)
                    {
                        AddACLToCache(dataSourceACL);
                    }
                }
            }
        }

        private void RemoveACLFromCache()
        {
            var dequeueACL = DataSourceIdOfCachedACLs.Dequeue();
            CachedDataSourceACLsPerDataSourceId.Remove(dequeueACL);
        }

        private void AddACLToCache(SearchService.SearchDataSourceACL dbDataSourceACL)
        {
            DataSourceIdOfCachedACLs.Enqueue(dbDataSourceACL.Id);
            CachedDataSourceACLsPerDataSourceId.Add(dbDataSourceACL.Id, dbDataSourceACL.Acl);
        }

        private List<SearchService.SearchDataSourceACL> RetrieveACLs(List<long> aclsToRetrieve)
        {
            //ServiceClient sc = null;
            GPAS.Horizon.Access.SearchService.ServiceClient serviceClient = null;
            try
            {
                //sc = new ServiceClient();
                serviceClient = new GPAS.Horizon.Access.SearchService.ServiceClient();

                //List<DBDataSourceACL> dataSourceACLs = sc.RetrieveDataSourceACLs(aclsToRetrieve.ToArray()).ToList();
                //return dataSourceACLs;

                SearchService.SearchDataSourceACL[] dataSourceACLs = serviceClient.RetrieveDataSourceACLs(aclsToRetrieve.ToArray());

                //List<DBDataSourceACL> dBDataSourceACLs = new List<DBDataSourceACL>();
                List<SearchService.SearchDataSourceACL> dBDataSourceACLs = new List<SearchService.SearchDataSourceACL>();


                foreach (var item in dataSourceACLs)
                {
                    SearchService.SearchDataSourceACL dB = new SearchService.SearchDataSourceACL()
                    {
                        Id = item.Id,
                        Acl = item.Acl
                    };
                    dBDataSourceACLs.Add(dB);
                }

                return dBDataSourceACLs;

                //ServiceClient sc = null;
                //try
                //{
                //    sc = new ServiceClient();
                //    List<DBDataSourceACL> dataSourceACLs = sc.RetrieveDataSourceACLs(aclsToRetrieve.ToArray()).ToList();
                //    return dataSourceACLs;
                //}
                //finally
                //{
                //    if (sc != null)
                //        sc.Close();
                //}
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (serviceClient != null)
                    serviceClient.Close();
            }
        }

        private List<SearchService.SearchDataSourceACL> RetrieveTopNACLs(long topN)
        {
            //ServiceClient sc = null;
            SearchService.ServiceClient serviceClient = null;
            try
            {
                //sc = new ServiceClient();
                serviceClient = new SearchService.ServiceClient();

                //List<DBDataSourceACL> dataSourceACLs = sc.RetrieveTopNDataSourceACLs(topN).ToList();
                //return dataSourceACLs;


               return serviceClient.RetrieveTopNDataSourceACLs(topN).ToList();
            }
            finally
            {
                if (serviceClient != null)
                    serviceClient.Close();
            }
        }

        private void InitCache(List<SearchService.SearchDataSourceACL> dataSourceACLs)
        {
            foreach (var dataSourceACL in dataSourceACLs)
            {
                lock (ACLCacheLockObject)
                {
                    AddACLToCache(dataSourceACL);
                }
            }
        }

        public async Task<List<KObject>> RetrieveObjectsByIDsAsync(long[] ids)
        {
            //ServiceClient sc = null;
            //DBObject[] serverResonse;
            SearchService.ServiceClient serviceClient = null;
            List<KObject> kObjects = new List<KObject>();

            try
            {
                //sc = new ServiceClient();
                //serverResonse = await sc.GetObjectsAsync(ids);

                serviceClient = new SearchService.ServiceClient();
                SearchService.SearchObject[] searchObjects =  serviceClient.GetObjectByIDs(ids);

                foreach (var item in searchObjects)
                {
                    KObject kObject = new KObject()
                    {
                        Id = item.Id,
                        LabelPropertyID = item.LabelPropertyID,
                        TypeUri = item.TypeUri
                    };

                    kObjects.Add(kObject);
                }
                return kObjects;

            }
            finally
            {
                //if (sc != null)
                //sc.Close();

                if (serviceClient != null)
                    serviceClient.Close();
            }
            //EntityConvertor convertor = new EntityConvertor();
            //return convertor.ConvertDBObjectsArrayToKObjectsArray(serverResonse).ToList();
        }

        private List<AccessControled<RelationshipBaseKlink>> ConvertDBRelationshipsToAccessControled(SearchService.SearchRelationship[] dbRelationships)
        {
            if (!dbRelationships.Any())
                return new List<AccessControled<RelationshipBaseKlink>>();

            Dictionary<long, AccessControl.ACL> aclDic= GetDataSourceACLs(dbRelationships.Select(o => o.DataSourceID).Distinct().ToList());
            EntityConvertor convertor = new EntityConvertor();
            List<AccessControled<RelationshipBaseKlink>> result = new List<AccessControled<RelationshipBaseKlink>>();
            for (int i = 0; i < dbRelationships.Length; i++)
            {
                result.Add(
                    new AccessControled<RelationshipBaseKlink>
                    {
                        Acl = aclDic[dbRelationships[i].DataSourceID],
                        ConceptInstance = convertor.ConvertDBRelationshipToRelationshipBaseKlink(dbRelationships[i])
                    }
                    );
            }
            return result;
        }

        //private List<AccessControled<KProperty>> ConvertDBPoepertiesToAccessControled(DBProperty[] dbProperties)
        //{
        //    if (!dbProperties.Any())
        //        return new List<AccessControled<KProperty>>();

        //    GetDataSourceACLs(dbProperties.Select(o => o.Id).Distinct().ToList());

        //    EntityConvertor convertor = new EntityConvertor();
        //    List<AccessControled<KProperty>> result = new List<AccessControled<KProperty>>();
        //    for (int i = 0; i < dbProperties.Length; i++)
        //    {
        //        result.Add(
        //            new AccessControled<KProperty>()
        //            {
        //                ConceptInstance = convertor.ConvertDBPropertyToKProperty(dbProperties[i]),
        //                Acl = CachedDataSourceACLsPerDataSourceId[dbProperties[i].Id]
        //            }
        //            );//convertor.ConvertDBPropertyToKProperty(dbProperties[i]));
        //    }
        //    return result;
        //}
        #endregion

        #region public functions
        public void PrepairDataSourceCachedForReset()
        {
            //clear cache
            DataSourceIdOfCachedACLs.Clear();
            CachedDataSourceACLsPerDataSourceId.Clear();
            //get top MaxCacheSize from repository and add to cache
            //List<DBDataSourceACL> dataSourceACLs = RetrieveTopNACLs(MaxCacheSize);
            List<SearchService.SearchDataSourceACL> dataSourceACLs = RetrieveTopNACLs(MaxCacheSize);
            InitCache(dataSourceACLs);
        }

        public Dictionary<long, AccessControl.ACL> GetDataSourceACLs(List<long> dataSourceIds)
        {
            if (!dataSourceIds.Any())
                return new Dictionary<long, AccessControl.ACL>();
            Dictionary<long, AccessControl.ACL> aclsDic = new Dictionary<long, AccessControl.ACL>();

            List<long> aclsToRetrieve = new List<long>();
            foreach (var aclId in dataSourceIds)
            {
                if (CachedDataSourceACLsPerDataSourceId.Keys.Contains(aclId))
                {
                    aclsDic.Add(aclId, CachedDataSourceACLsPerDataSourceId[aclId]);
                }
                else
                {
                    aclsToRetrieve.Add(aclId);
                }
            }
            if (aclsToRetrieve.Any())
            {
                List<SearchService.SearchDataSourceACL> dataSourceACLs = RetrieveACLs(aclsToRetrieve);
                CacheRetrievedACLs(dataSourceACLs);
                foreach (var item in dataSourceACLs)
                {
                    aclsDic.Add(item.Id, item.Acl);
                }
            }
            return aclsDic;
        }

        public List<KProperty> RetrievePropertiesOfObjects(List<long> objectIDs)
        {
            //ServiceClient sc = null;
            //DBProperty[] serverResonse;
            SearchService.ServiceClient serviceClient = null;
            List<KProperty> kProperties = new List<KProperty>(); 

            try
            {
                //sc = new ServiceClient();
                //serverResonse = sc.GetPropertiesOfObjectsWithoutAuthorization(objectIDs.ToArray());

                serviceClient = new SearchService.ServiceClient();
                SearchService.SearchProperty[] searchProperties =  serviceClient.GetDBPropertyByObjectIdsWithoutAuthorization(objectIDs.ToArray());
                foreach (var item in searchProperties)
                {
                    KProperty kProperty = new KProperty()
                    {
                        Id = item.Id,
                        DataSourceID = item.DataSourceID,
                        TypeUri = item.TypeUri,
                        Value = item.Value,
                        Owner = new KObject()
                        {
                            Id = item.OwnerObject.Id,
                            TypeUri = item.OwnerObject.TypeUri,
                            LabelPropertyID = item.OwnerObject.LabelPropertyID
                        }
                    };
                    kProperties.Add(kProperty);
                }

                return kProperties;
            }
            finally
            {
                if (serviceClient != null)
                    serviceClient.Close();
            }
            //EntityConvertor convertor = new EntityConvertor();
            //return convertor.ConvertDBPoepertiesArrayToKPropertiesList(serverResonse);
        }
        
        public KObject[] GetObjectsByIDs(List<long> ids)
        {
            //ServiceClient sc = null;
            GPAS.Horizon.Access.SearchService.ServiceClient serviceClient = null;
            //DBObject[] serverResonse;
            try
            {
                
                //sc = new ServiceClient();
                //serverResonse = sc.GetObjects(ids.ToArray());

                serviceClient = new SearchService.ServiceClient();
                SearchService.SearchObject[] searchObjects = serviceClient.GetObjectByIDs(ids.ToArray());

                List<KObject> kObjects = new List<KObject>();

                foreach (var item in searchObjects)
                {
                    KObject kObject = new KObject()
                    {
                        Id = item.Id,
                        TypeUri = item.TypeUri,
                        LabelPropertyID = item.LabelPropertyID
                    };

                    kObjects.Add(kObject);
                   
                }

                return kObjects.ToArray();
            }
            finally
            {
                if (serviceClient != null)
                    serviceClient.Close();
            }
            
            //EntityConvertor convertor = new EntityConvertor();
            //return convertor.ConvertDBObjectsArrayToKObjectsArray(serverResonse);
        }

        public async Task<List<AccessControled<RelationshipBaseKlink>>> GetRelationshipsByIDsAsync(List<long> relationshipIDs)
        {
            //ServiceClient sc = null;
            //DBRelationship[] serverResonse;
            SearchService.ServiceClient serviceClient = null;
            //List<DBRelationship> dBRelationships = new List<DBRelationship>();

            List<SearchService.SearchRelationship> dBRelationships = new List<SearchService.SearchRelationship>();

            try
            {
                //sc = new ServiceClient();
                //serverResonse = await sc.RetrieveRelationshipsAsync(relationshipIDs.ToArray());

                serviceClient = new SearchService.ServiceClient();
                SearchService.SearchRelationship[] searchRelationships = serviceClient.RetrieveRelationships(relationshipIDs.ToArray());

                foreach (var item in searchRelationships)
                {
                    //DBRelationship dBRelationship = new DBRelationship()
                    //{
                    //    Id = item.Id,
                    //    DataSourceID = item.DataSourceID,
                    //    Description = "",
                    //     Direction = (RepositoryLinkDirection)item.Direction,
                    //    TypeURI = item.TypeUri,
                    //    TimeBegin = System.DateTime.MinValue,
                    //    TimeEnd = System.DateTime.MaxValue,
                    //    Source = new DBObject()
                    //    {
                    //        Id = item.SourceObjectId,
                    //        TypeUri = item.SourceObjectTypeUri,
                    //        IsGroup = false
                    //    },
                    //    Target = new DBObject()
                    //    {
                    //        Id = item.TargetObjectId,
                    //        TypeUri = item.TargetObjectTypeUri,
                    //        IsGroup = false
                    //    },
                    //};

                    //dBRelationships.Add(dBRelationship);
                    //------------------------------------------

                    SearchService.SearchRelationship dBRelationship = new SearchService.SearchRelationship()
                    {
                        Id = item.Id,
                        DataSourceID = item.DataSourceID,
                        Direction = item.Direction,
                        TypeUri = item.TypeUri,
                        SourceObjectId = item.SourceObjectId,
                        SourceObjectTypeUri = item.SourceObjectTypeUri,
                        TargetObjectId = item.TargetObjectId,
                        TargetObjectTypeUri = item.TargetObjectTypeUri
                    };

                    dBRelationships.Add(dBRelationship);

                }

            }
            finally
            {
                //if (sc != null)
                //sc.Close();

                if (serviceClient != null)
                    serviceClient.Close();
            }
            //return ConvertDBRelationshipsToAccessControled(serverResonse.);
            return ConvertDBRelationshipsToAccessControled(dBRelationships.ToArray());
        }
        #endregion
    }
}