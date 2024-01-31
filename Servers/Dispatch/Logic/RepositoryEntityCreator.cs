//using GPAS.Dispatch.ServiceAccess.RepositoryService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPAS.Dispatch.Entities;
using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.ServiceAccess.SearchService;

namespace GPAS.Dispatch.Logic
{
    public class RepositoryEntityCreator
    {
        //internal DBObject CreateDBObject
        //    (string typeURI, long labelPropertyID, bool isGroup, long? resolvedTo = null)
        //{
        //    return new DBObject()
        //    {
        //        LabelPropertyID = labelPropertyID,
        //        TypeUri = typeURI,
        //        IsGroup = isGroup,
        //        ResolvedTo = resolvedTo
        //    };
        //}

        //internal DBRelationship CreateDBRelationship(DBObject dbSourceObject, DBObject dbTargetObject, string relationshipTypeURI, string displayText, LinkDirection linkDiraction, DateTime timeBegin, DateTime timeEnd)
        //{
        //    return new DBRelationship()
        //    {
        //        Source = dbSourceObject,
        //        Target = dbTargetObject,
        //        Description = displayText,
        //        TypeURI = relationshipTypeURI,
        //        TimeBegin = timeBegin != null ? (DateTime?)timeBegin : null,
        //        TimeEnd = timeEnd != null ? (DateTime?)timeEnd : null,
        //        Direction = (RepositoryLinkDirection)linkDiraction
        //    };
        //}

        //internal DBProperty CreateDBProperty(string typeURI, string value, DBObject dbObject)
        //{
        //    return new DBProperty()
        //    {
        //        Owner = dbObject,
        //        TypeUri = typeURI,
        //        Value = value
        //    };
        //}

        internal  SearchGraphArrangement CreateSearchGraphArrangement(long id, string title, string description, byte[] graphImage, byte[] graphArrangement, int nodesCount, string timeCreated, long dataSourceID)
        {
            return new SearchGraphArrangement()
            {
                Id = id,
                Title = title,
                Description = description,
                GraphImage = graphImage,
                GraphArrangementXML = graphArrangement,
                TimeCreated = timeCreated,
                NodesCount = nodesCount,
                DataSourceID = dataSourceID
            };
        }

        //internal DBGraphArrangement CreateDBGraphArrangement(long id, string title, string description, byte[] graphImage, byte[] graphArrangement, int nodesCount, string timeCreated , long dataSourceID)
        //{
        //    return new DBGraphArrangement()
        //    {
        //        Id = id,
        //        Title = title,
        //        Description = description,
        //        GraphImage = graphImage,
        //        GraphArrangementXML = graphArrangement,
        //        TimeCreated = timeCreated,
        //        NodesCount = nodesCount,
        //        DataSourceID = dataSourceID
        //    };
        //}

        //internal DBMedia CreateDBMedia(long objectId, string description, string URI)
        //{
        //    return new DBMedia()
        //    {
        //        ObjectId = objectId,
        //        Description = description,
        //        URI = URI,
        //    };
        //}
    }
}
