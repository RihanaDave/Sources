using GPAS.AccessControl;
using GPAS.RepositoryServer.Entities.Publish;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.RepositoryServer.Entities
{
    public interface IAccessClient
    {
        #region Objects Retrievation

        List<DBObject> GetObjects(IEnumerable<long> dbObjectIDs);

        List<DBObject> RetrieveObjectsSequentialByIDRange(long firstID, long lastID);

        #endregion

        #region Properties Retrievation

        List<DBProperty> GetPropertiesOfObject(DBObject dbObject, AuthorizationParametters authParams);

        List<DBProperty> GetPropertiesOfObjectsWithoutAuthorization(long[] objectIDs);

        List<DBProperty> GetPropertiesOfObjects(long[] objectIDs, AuthorizationParametters authParams);

        List<DBProperty> GetSpecifiedPropertiesOfObjectsByTypes(List<long> objectIDs, List<string> specifiedPropertyTypeUris, AuthorizationParametters authParams);

        List<DBProperty> GetSpecifiedPropertiesOfObjectsByTypeAndValue(List<long> objectIDs, string propertyTypeUri, string propertyValue, AuthorizationParametters authParams);
        
        List<DBProperty> GetPropertiesByID(List<long> dbPropertyIDs, AuthorizationParametters authParams);

        #endregion

        #region Relationships Retrievation

        List<DBRelationship> GetRelationships(List<long> dbRelationshipIDs, AuthorizationParametters authParams);

        List<DBRelationship> RetrieveRelationships(List<long> dbRelationshipIDs);

        List<DBRelationship> GetRelationshipsBySourceObject(long objectID, string typeURI, AuthorizationParametters authParams);

        List<DBRelationship> GetRelationshipsBySourceOrTargetObject(List<long> objectIDs, AuthorizationParametters authParams);

        List<DBRelationship> GetRelationshipsBySourceObjectWithoutAuthParams(List<long> objectIDs);

        List<DBRelationship> GetRelationshipsBySourceOrTargetObjectWithoutAuthParams(List<long> objectIDs);

        List<DBRelationship> GetSourceLink(DBObject dbObject, string typeURI, AuthorizationParametters authParams);

        DBRelationship GetExistingRelationship(string typeURI, long source, long target, RepositoryLinkDirection direction, AuthorizationParametters authParams);

        List<DBRelationship> RetrieveRelationshipsSequentialByIDRange(long firstID, long lastID);

        #endregion

        #region Publish

        void Publish(DBAddedConcepts addedConcept, DBModifiedConcepts modifiedConcept, DBResolvedObject[] resolvedObjects, long dataSourceID);

        #endregion

        #region ID Assignment

        long GetLastAsignedObjectId();

        long GetLastAsignedGraphId();
        
        long GetLastAsignedPropertyId();
        
        long GetLastAsignedRelationshipId();
        
        long GetLastAsignedMediaId();
        
        long GetLastAsignedDataSourceId();

        #endregion

        #region Repository Server Management

        void Optimize();

        void TruncateDatabase();

        #endregion

        #region ACL

        void RegisterNewDataSource(long dsId, string name, DataSourceType type, ACL acl, string description, string createBy, string createdTime);

        long[] GetSubsetOfConceptsByPermission(ConceptType conceptType, long[] IDs, string[] groupNames, Permission minimumPermission);

        List<DBDataSourceACL> RetrieveDataSourceACLs(long[] DataSourceIDs);

        List<DBDataSourceACL> RetrieveTopNDataSourceACLs(long topN);

        List<DataSourceInfo> RetriveDataSourcesSequentialIDByIDRange(long firstID, long lastID);

        List<DataSourceInfo> GetDataSourcesByIDs(List<long> ids);

        #endregion

        #region Init

        void Init();

        #endregion

        void IsAvailable();
    }
}
