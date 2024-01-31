using GPAS.AccessControl;
using GPAS.RepositoryServer.Entities;
using GPAS.RepositoryServer.Entities.Publish;
using System.Collections.Generic;
using System.ServiceModel;

namespace GPAS.RepositoryServer
{
    [ServiceContract]
    public interface IService
    {
        #region Objects Retrievation
        [OperationContract]
        List<DBObject> GetObjects(List<long> dbObjectIDs);
        [OperationContract]
        List<DBObject> RetrieveObjectsSequentialByIDRange(long firstID, long lastID);
        #endregion

        #region Properties Retrievation
        [OperationContract]
        List<DBProperty> GetPropertiesOfObject(DBObject dbObject, AuthorizationParametters authParams);
        [OperationContract]
        List<DBProperty> GetPropertiesOfObjectsWithoutAuthorization(long[] objectIDs);
        [OperationContract]
        List<DBProperty> GetPropertiesOfObjects(long[] objectIDs, AuthorizationParametters authParams);
        [OperationContract]
        List<DBProperty> GetSpecifiedPropertiesOfObjectsByTypes(List<long> objectsId, List<string> propertiesType, AuthorizationParametters authParams);
        [OperationContract]
        List<DBProperty> GetSpecifiedPropertiesOfObjectsByTypeAndValue(List<long> objectIDs, string propertyTypeUri, string propertyValue, AuthorizationParametters authParams);
        [OperationContract]
        List<DBProperty> GetPropertiesByID(List<long> dbPropertyIDs, AuthorizationParametters authParams);
        #endregion

        #region Relationships Retrievation
        [OperationContract]
        List<DBRelationship> GetRelationships(List<long> dbRelationshipIDs, AuthorizationParametters authParams);
        [OperationContract]
        List<DBRelationship> RetrieveRelationships(List<long> dbRelationshipIDs);
        [OperationContract]
        List<DBRelationship> RetrieveRelationshipsSequentialByIDRange(long firstID, long lastID);
        [OperationContract]
        List<DBRelationship> GetSourceLink(DBObject dbObject, string typeURI, AuthorizationParametters authParams);
        [OperationContract]
        List<DBRelationship> GetRelationshipsBySourceObject(long objectID, string typeURI, AuthorizationParametters authParams);
        [OperationContract]
        List<DBRelationship> GetRelationshipsBySourceOrTargetObject(List<long> objectIDs, AuthorizationParametters authParams);
        [OperationContract]
        List<DBRelationship> GetRelationshipsBySourceObjectWithoutAuthParams(List<long> objectIDs);
        [OperationContract]
        List<DBRelationship> GetRelationshipsBySourceOrTargetObjectWithoutAuthParams(List<long> objectIDs);
        [OperationContract]
        DBRelationship GetExistingRelationship(string typeURI, long source, long target, RepositoryLinkDirection direction, AuthorizationParametters authParams);
        #endregion

        #region Medias Retrievation
        [OperationContract]
        List<DBMedia> GetMediaForObject(long objectID, AuthorizationParametters authParams);
        [OperationContract]
        List<DBMedia> GetMediasForObjectsWithoutAuthorization(long[] objectIDs);
        [OperationContract]
        List<DBMedia> GetMediasForObjects(long[] objectIDs, AuthorizationParametters authParams);
        #endregion

        #region Publish
        [OperationContract]
        void Publish(DBAddedConcepts addedConcept, DBModifiedConcepts modifiedConcept, DBResolvedObject[] resolvedObjects, long dataSourceID);
        #endregion

        #region Graph Store and Retrieve
        [OperationContract]
        DBGraphArrangement CreateNewGraphArrangment(DBGraphArrangement dbGraphArrangement);
        [OperationContract]
        List<DBGraphArrangement> GetGraphArrangements(AuthorizationParametters authParams);
        [OperationContract]
        byte[] GetGraphImage(int dbGraphArrangementID, AuthorizationParametters authParams);
        [OperationContract]
        byte[] GetGraphArrangementXML(int dbGraphArrangementID, AuthorizationParametters authParams);
        [OperationContract]
        bool DeleteGraph(int id);
        #endregion

        #region ID Assignment
        [OperationContract]
        long GetLastAsignedObjectId();
        [OperationContract]
        long GetLastAsignedPropertyId();
        [OperationContract]
        long GetLastAsignedRelationshipId();
        [OperationContract]
        long GetLastAsignedMediaId();
        [OperationContract]
        long GetLastAsignedGraphId();
        [OperationContract]
        long GetLastAsignedDataSourceId();
        #endregion

        #region Repository Server Management
        [OperationContract]
        void Optimize();
        [OperationContract]
        void TruncateDatabase();
        #endregion

        #region ACL
        [OperationContract]
        void RegisterNewDataSource(long dsId, string name, DataSourceType type, ACL acl, string description, string createBy, string createdTime);
        [OperationContract]
        long[] GetSubsetOfConceptsByPermission(ConceptType conceptType, long[] IDs, string[] groupNames, Permission minimumPermission);
        [OperationContract]
        List<DBDataSourceACL> RetrieveDataSourceACLs(long[] DataSourceIDs);
        [OperationContract]
        List<DBDataSourceACL> RetrieveTopNDataSourceACLs(long topN);
        [OperationContract]
        List<DataSourceInfo> RetriveDataSourcesSequentialIDByIDRange(long firstID, long lastID);
        [OperationContract]
        List<DataSourceInfo> GetDataSourcesByIDs(List<long> ids);

        #endregion

        [OperationContract]
        void IsAvailable();
    }
}