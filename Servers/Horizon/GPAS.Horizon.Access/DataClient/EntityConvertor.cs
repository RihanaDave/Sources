using GPAS.Dispatch.Entities.Concepts;
//using GPAS.Horizon.Access.RepositoryService;
using System.Collections.Generic;
using System.Linq;
using System;

namespace GPAS.Horizon.Access.DataClient
{
    public class EntityConvertor
    {
        //public KObject[] ConvertDBObjectsArrayToKObjectsArray(DBObject[] dbObjects)
        //{
        //    KObject[] result = new KObject[dbObjects.Length];
        //    for (int i = 0; i < dbObjects.Length; i++)
        //    {
        //        result[i] = ConvertDBObjectToKObject(dbObjects[i]);
        //    }
        //    return result;
        //}

        //public List<KObject> ConvertDBObjectsArrayToKObjectsList(DBObject[] dbObjects)
        //{
        //    List<KObject> result = new List<KObject>(dbObjects.Length);
        //    for (int i = 0; i < dbObjects.Length; i++)
        //    {
        //        result.Add(ConvertDBObjectToKObject(dbObjects[i]));
        //    }
        //    return result;
        //}

        //public KObject ConvertDBObjectToKObject(DBObject dbObject)
        //{
        //    return new KObject()
        //    {
        //        LabelPropertyID = dbObject.LabelPropertyID,
        //        Id = dbObject.Id,
        //        TypeUri = dbObject.TypeUri,
        //        IsGroup = dbObject.IsGroup,
        //        ResolvedTo = GetResolveMasterKObjectForDBObject(dbObject)
        //    };
        //}

        private static object retrieveResolveMasterForDBObjLockObject = new object();

        //internal static KObject GetResolveMasterKObjectForDBObject(DBObject dbObjToGetItsResolveMaster)
        //{
        //    if (dbObjToGetItsResolveMaster.ResolvedTo == null)
        //        return null;
        //    else
        //        lock (retrieveResolveMasterForDBObjLockObject)
        //        {
        //            RetrieveDataClient dc = new RetrieveDataClient();
        //            return dc.GetObjectsByIDs(new List<long> { dbObjToGetItsResolveMaster.ResolvedTo.Value }).FirstOrDefault();
        //        }
        //}

        internal KRelationship ConvertDBRelationshipToKRelationship(SearchService.SearchRelationship dbRelationshipResullt)
        {
            return new KRelationship()
            {
                Description = "",
                Direction = (LinkDirection)dbRelationshipResullt.Direction,
                Id = dbRelationshipResullt.Id,
                TimeBegin = DateTime.MinValue, // dbRelationshipResullt.TimeBegin,
                TimeEnd = DateTime.MinValue, // dbRelationshipResullt.TimeEnd,
                DataSourceID = dbRelationshipResullt.DataSourceID
            };
        }

        //internal List<KProperty> ConvertDBPoepertiesArrayToKPropertiesList(DBProperty[] dbProperties)
        //{
        //    List<KProperty> result = new List<KProperty>(dbProperties.Length);
        //    for (int i = 0; i < dbProperties.Length; i++)
        //    {
        //        result.Add(ConvertDBPropertyToKProperty(dbProperties[i]));
        //    }
        //    return result;
        //}

        //internal KProperty ConvertDBPropertyToKProperty(DBProperty dbProperty)
        //{
        //    return new KProperty()
        //    {
        //        Id = dbProperty.Id,
        //        Owner = ConvertDBObjectToKObject(dbProperty.Owner),
        //        TypeUri = dbProperty.TypeUri,
        //        Value = dbProperty.Value,
        //        DataSourceID = dbProperty.DataSourceID
        //    };
        //}

        internal List<RelationshipBaseKlink> ConvertDBRelationshipsArrayToRelationshipBasedKLinkList(SearchService.SearchRelationship[] dbRelationships)
        {
            List<RelationshipBaseKlink> result = new List<RelationshipBaseKlink>(dbRelationships.Length);
            for (int i = 0; i < dbRelationships.Length; i++)
            {
                result.Add(ConvertDBRelationshipToRelationshipBaseKlink(dbRelationships[i]));
            }
            return result;
        }

        internal RelationshipBaseKlink ConvertDBRelationshipToRelationshipBaseKlink(SearchService.SearchRelationship dbRelationship)
        {
            return new RelationshipBaseKlink()
            {
                Relationship = ConvertDBRelationshipToKRelationship(dbRelationship),
                TypeURI = dbRelationship.TypeUri,
                Source = new KObject() { Id = dbRelationship.SourceObjectId,  TypeUri = dbRelationship.SourceObjectTypeUri }, //ConvertDBObjectToKObject(dbRelationship.Source),
                Target = new KObject() { Id = dbRelationship.TargetObjectId, TypeUri = dbRelationship.TargetObjectTypeUri}, //ConvertDBObjectToKObject(dbRelationship.Target)
            };
        }

        //internal KMedia ConvertDBMediaToKMedia(DBMedia dbMediaResult)
        //{
        //    return new KMedia()
        //    {
        //        OwnerObjectId = dbMediaResult.ObjectId,
        //        Description = dbMediaResult.Description,
        //        URI = dbMediaResult.URI,
        //        Id = dbMediaResult.Id,
        //        DataSourceID = dbMediaResult.DataSourceID
        //    };
        //}
    }
}