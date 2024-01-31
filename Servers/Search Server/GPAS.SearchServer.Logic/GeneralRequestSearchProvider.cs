using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPAS.AccessControl;
using GPAS.SearchServer.Entities;

namespace GPAS.SearchServer.Logic
{
    public class GeneralRequestSearchProvider
    {
        public List<SearchProperty> GetDBPropertyByObjectId(long objectIDs, AccessControl.AuthorizationParametters authorizationParametters)
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            return accessClient.GetDBPropertyByObjectId(objectIDs, authorizationParametters);
        }

        public SearchObject GetObject(long objectId)
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            return accessClient.GetObject(objectId);
        }

        public List<SearchProperty> GetDBPropertyByObjectIds(long[] propertyIds, AuthorizationParametters authorizationParametters)
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            return accessClient.GetDBPropertyByObjectIds(propertyIds, authorizationParametters);
        }

        public List<SearchProperty> GetSpecifiedPropertiesOfObjectsByTypes(List<long> objectIDs, List<string> specifiedPropertyTypeUris, AuthorizationParametters authorizationParametters)
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            return accessClient.GetSpecifiedPropertiesOfObjectsByTypes(objectIDs, specifiedPropertyTypeUris, authorizationParametters);
        }

        public void RegisterNewDataSource(long dsId, string name, DataSourceType type, AccessControl.ACL acl, string description, string createBy, string createdTime)
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            accessClient.RegisterNewDataSource(dsId, name, type, acl, description, createBy, createdTime);
        }

        public List<SearchDataSourceACL> RetrieveDataSourceACLs(long[] dataSourceIDs)
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            return accessClient.RetrieveDataSourceACLs(dataSourceIDs);
        }

        public List<SearchObject> GetObjectByIDs(long[] objectIDs)
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            return accessClient.GetObjectByIDs(objectIDs);

        }

        public long GetLastAsignedDataSourceId()
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            return accessClient.GetLastAsignedDataSourceId();
        }

        public long GetLastAsignedObjectId()
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            return accessClient.GetLastAsignedObjectId();
        }

        public long GetLastAsignedPropertyId()
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            return accessClient.GetLastAsignedPropertyId();
        }

        public long GetLastAsignedRelationId()
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            return accessClient.GetLastAsignedRelationId();
        }

        public long GetLastAssignedGraphaId()
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            return accessClient.GetLastAssignedGraphaId();
        }

        public List<SearchProperty> GetDBPropertyByObjectIdsWithoutAuthorization(long[] objectIDs)
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            return accessClient.GetDBPropertyByObjectIdsWithoutAuthorization(objectIDs);
        }

        public List<SearchRelationship> RetrieveRelationships(long[] relationshipIDs)
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            return accessClient.RetrieveRelationships(relationshipIDs);
        }

        public List<SearchRelationship> GetRelationships(List<long> relationshipIDs, AuthorizationParametters authorizationParametters)
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            return accessClient.GetRelationships(relationshipIDs,authorizationParametters);
        }

        public List<SearchRelationship> GetRelationshipsBySourceOrTargetObjectWithoutAuthParams(List<long> objectIDs)
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            return accessClient.GetRelationshipsBySourceOrTargetObjectWithoutAuthParams(objectIDs);
        }

        public long[] GetSubsetOfConceptsByPermission(ConceptType conceptType, long[] IDs, string[] groupNames, Permission minimumPermission)
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            return accessClient.GetSubsetOfConceptsByPermission(conceptType,IDs,groupNames,minimumPermission);
        }

        public List<SearchDataSourceACL> RetrieveTopNDataSourceACLs(long topN)
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            return accessClient.RetrieveTopNDataSourceACLs(topN);
        }

        public List<SearchRelationship> GetRelationshipsBySourceObject(long objectID, string typeUri, AuthorizationParametters authorizationParametters)
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            return accessClient.GetRelationshipsBySourceObject(objectID,typeUri,authorizationParametters);
        }

        public List<SearchRelationship> GetRelationshipsBySourceOrTargetObject(List<long> objectIDs, AuthorizationParametters authorizationParametters)
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            return accessClient.GetRelationshipsBySourceOrTargetObject(objectIDs, authorizationParametters);
        }

        public SearchRelationship GetExistingRelationship(string typeUri, long source, long target, int direction, AuthorizationParametters authorizationParametters)
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            return accessClient.GetExistingRelationship(typeUri,source,target,direction,authorizationParametters);
        }

        public List<SearchRelationship> GetSourceLink(long objectId, string typeUri, AuthorizationParametters authorizationParametters)
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            return accessClient.GetSourceLink(objectId, typeUri, authorizationParametters);
        }

        public List<SearchProperty> GetSpecifiedPropertiesOfObjectsByTypeAndValue(List<long> objectIDs, string propertyTypeUri, string propertyValue, AuthorizationParametters authorizationParametters)
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            return accessClient.GetSpecifiedPropertiesOfObjectsByTypeAndValue(objectIDs, propertyTypeUri, propertyValue, authorizationParametters);
        }
    }
}
