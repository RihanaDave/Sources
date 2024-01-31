using GPAS.Dispatch.Entities;
using GPAS.Dispatch.Entities.Concepts;
//using GPAS.Dispatch.ServiceAccess.RepositoryService;
using GPAS.Dispatch.ServiceAccess.SearchService;
using System.Linq;

namespace GPAS.Dispatch.Logic
{
    public class EntityConvertor
    {
        //public KObject ConvertDBObjectToKObject(DBObject dbObject, string callerUserName)
        //{
        //    return new KObject()
        //    {
        //        LabelPropertyID = dbObject.LabelPropertyID,
        //        Id = dbObject.Id,
        //        TypeUri = dbObject.TypeUri,
        //        IsGroup = dbObject.IsGroup,
        //        ResolvedTo = null//(new RepositoryProvider(callerUserName)).GetResolveMasterKObjectForDBObject(dbObject)
        //    };
        //}

        //internal KRelationship ConvertDBRelationshipToKRelationship(DBRelationship dbRelationshipResullt)
        //{
        //    return new KRelationship()
        //    {
        //        Description = dbRelationshipResullt.Description,
        //        Direction = (LinkDirection)dbRelationshipResullt.Direction,
        //        Id = dbRelationshipResullt.Id,
        //         DataSourceID = dbRelationshipResullt.DataSourceID,
        //        TimeBegin = dbRelationshipResullt.TimeBegin,
        //        TimeEnd = dbRelationshipResullt.TimeEnd
        //    };
        //}

        //internal DBObject ConvertKObjectToDBObject(KObject kObject)
        //{
        //    return new DBObject()
        //    {
        //        Id = kObject.Id,
        //        IsGroup = kObject.IsGroup,
        //        TypeUri = kObject.TypeUri,
        //        LabelPropertyID = kObject.LabelPropertyID,
        //        ResolvedTo = RepositoryProvider.GetDBObjectResolvedToFieldForKObject(kObject)
        //    };
        //}

        internal KObject ConvertSearchObjectToKObject(SearchObject searchObject, string callerUserName)
        {
            KObject kObject = new KObject
            {
                LabelPropertyID = searchObject.LabelPropertyID,
                Id = searchObject.Id,
                TypeUri = searchObject.TypeUri,
                IsMaster = searchObject.IsMaster
            };

            if (searchObject.Slaves != null)
                kObject.Slaves = searchObject.Slaves.Select(so => ConvertSearchObjectToKObject(so, callerUserName)).ToList();

            if (searchObject.SearchObjectMaster != null)
                kObject.KObjectMaster = new KObjectMaster()
                {
                    Id = searchObject.SearchObjectMaster.Id,
                    MasterId = searchObject.SearchObjectMaster.MasterId,
                    ResolveTo = searchObject.SearchObjectMaster.ResolveTo
                };

            return kObject;
        }

        //internal KGraphArrangement ConvertDBGraphArrangementToKGraphArrangement(DBGraphArrangement dbGraphArrangement)
        //{
        //    return new KGraphArrangement()
        //    {
        //        Id = dbGraphArrangement.Id,
        //        Title = dbGraphArrangement.Title,
        //        Description = dbGraphArrangement.Description,
        //        GraphArrangement = dbGraphArrangement.GraphArrangementXML,
        //        GraphImage = dbGraphArrangement.GraphImage,
        //        TimeCreated = dbGraphArrangement.TimeCreated,
        //        NodesCount = dbGraphArrangement.NodesCount ,
        //        DataSourceID = dbGraphArrangement.DataSourceID
        //    };
        //}

        internal KGraphArrangement ConvertDBGraphArrangementToKGraphArrangement(SearchGraphArrangement dbGraphArrangement)
        {
            return new KGraphArrangement()
            {
                Id = dbGraphArrangement.Id,
                Title = dbGraphArrangement.Title,
                Description = dbGraphArrangement.Description,
                GraphArrangement = dbGraphArrangement.GraphArrangementXML,
                GraphImage = dbGraphArrangement.GraphImage,
                TimeCreated = dbGraphArrangement.TimeCreated,
                NodesCount = dbGraphArrangement.NodesCount,
                DataSourceID = dbGraphArrangement.DataSourceID
            };
        }

        //internal KProperty ConvertDBPropertyToKProperty(DBProperty dbProperty, string callerUserName)
        //{
        //    return new KProperty()
        //    {
        //        Id = dbProperty.Id,
        //        Owner = ConvertDBObjectToKObject(dbProperty.Owner, callerUserName),
        //        TypeUri = dbProperty.TypeUri,
        //        Value = dbProperty.Value,
        //        DataSourceID =dbProperty.DataSourceID
        //    };
        //}

        //internal DBProperty ConvertKPropertyToDBProperty(KProperty kProperty)
        //{
        //    return new DBProperty()
        //    {
        //        Owner = ConvertKObjectToDBObject(kProperty.Owner),
        //        Id = kProperty.Id,
        //        TypeUri = kProperty.TypeUri,
        //        Value = kProperty.Value
        //    };
        //}

        //internal RelationshipBaseKlink ConvertDBRelationshipToRelationshipBaseKlink(DBRelationship dbRelationship, string callerUserName)
        //{
        //    return new RelationshipBaseKlink()
        //    {
        //        Relationship = ConvertDBRelationshipToKRelationship(dbRelationship),
        //        TypeURI = dbRelationship.TypeURI,
        //        Source = ConvertDBObjectToKObject(dbRelationship.Source, callerUserName),
        //        Target = ConvertDBObjectToKObject(dbRelationship.Target, callerUserName)
        //    };
        //}

        internal RelationshipBaseKlink ConvertDBRelationshipToRelationshipBaseKlink(Dispatch.ServiceAccess.SearchService.SearchRelationship dbRelationship, string callerUserName)
        {
            Dispatch.ServiceAccess.SearchService.ServiceClient serviceClient = new ServiceAccess.SearchService.ServiceClient();

            Dispatch.ServiceAccess.SearchService.SearchObject sourceSearchObject = serviceClient.GetObject(dbRelationship.SourceObjectId);
            KObject sourcekObject = new KObject()
            {
                Id = sourceSearchObject.Id,
                LabelPropertyID = sourceSearchObject.LabelPropertyID,
                TypeUri = sourceSearchObject.TypeUri
            };

            Dispatch.ServiceAccess.SearchService.SearchObject targetSearchObject = serviceClient.GetObject(dbRelationship.TargetObjectId);
            KObject targetObject = new KObject()
            {
                Id = targetSearchObject.Id,
                LabelPropertyID = targetSearchObject.LabelPropertyID,
                TypeUri = targetSearchObject.TypeUri
            };

            KRelationship dBRelationship = new KRelationship()
            {
                Id = dbRelationship.Id,
                DataSourceID = dbRelationship.DataSourceID,
                Description = dbRelationship.TypeUri.Replace("_", " "),
                Direction = (LinkDirection)dbRelationship.Direction,
                TimeBegin = System.DateTime.MinValue,
                TimeEnd = System.DateTime.MaxValue,
                //Source = sourcekObject,// new DBObject() { Id = dbRelationship.SourceObjectId, TypeUri = dbRelationship.SourceObjectTypeUri, IsGroup=false },
                //Target = targetObject, // new DBObject() { Id = dbRelationship.TargetObjectId, TypeUri = dbRelationship.TargetObjectTypeUri,IsGroup=false },
                //TypeUri = dbRelationship.TypeUri,
                //Direction = (RepositoryLinkDirection)dbRelationship.Direction
            };

            return new RelationshipBaseKlink()
            {
                Relationship = dBRelationship, //ConvertDBRelationshipToKRelationship(dBRelationship),
                TypeURI = dbRelationship.TypeUri,
                Source = sourcekObject, //ConvertDBObjectToKObject(sourcekObject, callerUserName),
                Target = targetObject //ConvertDBObjectToKObject(targetObject, callerUserName)
            };
        }

        //internal KMedia ConvertDBMediaToKMedia(DBMedia dbMediaResult)
        //{
        //    return new KMedia()
        //    {
        //        OwnerObjectId = dbMediaResult.ObjectId,
        //        Description = dbMediaResult.Description,
        //        URI = dbMediaResult.URI,
        //        Id = dbMediaResult.Id
        //    };
        //}

        //internal KProperty ConvertSearchPropertyToKProperty(SearchProperty searchProperty, SearchObject searchObject, string callerUserName)
        //{
        //    return new KProperty()
        //    {
        //        Id = searchProperty.Id,
        //        Value = searchProperty.Value,
        //        TypeUri = searchProperty.TypeUri,
        //        Owner = ConvertSearchObjectToKObject(searchObject, callerUserName)
        //    };
        //}
    }
}
