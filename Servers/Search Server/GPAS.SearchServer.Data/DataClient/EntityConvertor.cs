//using GPAS.SearchServer.Access.RepositoryService;
using GPAS.SearchServer.Entities;
using System.Collections.Generic;

namespace GPAS.SearchServer.Access.DataClient
{
    internal class EntityConvertor
    {
        internal List<SearchProperty> ConvertDBPoepertiesArrayToSearchPropertiesList(SearchProperty[] dbProperties)
        {
            List<SearchProperty> result = new List<SearchProperty>(dbProperties.Length);
            for (int i = 0; i < dbProperties.Length; i++)
            {
                result.Add(ConvertDBPropertyToSearchProperty(dbProperties[i]));
            }
            return result;
        }

        //internal SearchProperty[] ConvertDBPoepertiesArrayToSearchPropertiesArray(DBProperty[] dbProperties)
        //{
        //    SearchProperty[] result = new SearchProperty[dbProperties.Length];
        //    for (int i = 0; i < dbProperties.Length; i++)
        //    {
        //        result[i] = ConvertDBPropertyToSearchProperty(dbProperties[i]);
        //    }
        //    return result;
        //}

        internal SearchProperty ConvertDBPropertyToSearchProperty(SearchProperty dBProperty)
        {
            return new SearchProperty()
            {
                Id = dBProperty.Id,
                TypeUri = dBProperty.TypeUri,
                Value = dBProperty.Value,
                OwnerObject = ConvertDBObjectToSearchObject(dBProperty.OwnerObject),
                DataSourceID = dBProperty.DataSourceID
            };
        }

        internal SearchObject ConvertDBObjectToSearchObject(SearchObject dbObj)
        {
            return new SearchObject()
            {
                Id = dbObj.Id,
                TypeUri = dbObj.TypeUri,
                LabelPropertyID = dbObj.LabelPropertyID,
                IsMaster = dbObj.IsMaster,
                SearchObjectMaster = ConvertDBObjectMasterToSearchObjectMaster(dbObj.SearchObjectMaster)
            };
        }

        private SearchObjectMaster ConvertDBObjectMasterToSearchObjectMaster(SearchObjectMaster dbMaster)
        {
            return new SearchObjectMaster()
            {
                Id = dbMaster.Id,
                MasterId = dbMaster.MasterId,
                ResolveTo = new List<long>(dbMaster.ResolveTo).ToArray()
            };
        }

        internal SearchObject[] ConvertDBObjectsArrayToSearchObjectsArray(List<SearchObject> dbObjects)
        {
            SearchObject[] result = new SearchObject[dbObjects.Count];
            for (int i = 0; i < dbObjects.Count; i++)
            {
                result[i] = ConvertDBObjectToSearchObject(dbObjects[i]);
            }
            return result;
        }

        internal List<SearchObject> ConvertDBObjectsArrayToSearchObjectsList(SearchObject[] dbObjects)
        {
            List<SearchObject> result = new List<SearchObject>(dbObjects.Length);
            for (int i = 0; i < dbObjects.Length; i++)
            {
                result.Add(ConvertDBObjectToSearchObject(dbObjects[i]));
            }
            return result;
        }

        //internal List<SearchMedia> ConvertDBMediasArrayToSearchMediasList(DBMedia[] dbMedias)
        //{
        //    List<SearchMedia> result = new List<SearchMedia>(dbMedias.Length);
        //    for (int i = 0; i < dbMedias.Length; i++)
        //    {
        //        result.Add(ConvertDBMediaToSearchMedia(dbMedias[i]));
        //    }
        //    return result;
        //}

        //internal SearchMedia ConvertDBMediaToSearchMedia(DBMedia dbMedia)
        //{
        //    SearchMedia newMedia = new SearchMedia()
        //    {
        //        Id = dbMedia.Id,
        //        URI = dbMedia.URI,
        //        Description = dbMedia.Description,
        //        OwnerObjectId = dbMedia.ObjectId,
        //        DataSourceID = dbMedia.DataSourceID
        //    };
        //    return newMedia;
        //}

        internal List<SearchRelationship> ConvertDBRelationshipsArrayToSearchRelationshipsList(SearchRelationship[] dbRelationships)
        {
            List<SearchRelationship> result = new List<SearchRelationship>(dbRelationships.Length);
            for (int i = 0; i < dbRelationships.Length; i++)
            {
                result.Add(ConvertDBrelationshipToSearchrelationship(dbRelationships[i]));
            }
            return result;
        }

        private SearchRelationship ConvertDBrelationshipToSearchrelationship(SearchRelationship dBRelationship)
        {
            return new SearchRelationship()
            {
                Id = dBRelationship.Id,
                TypeUri = dBRelationship.TypeUri,
                SourceObjectId = dBRelationship.SourceObjectId,
                SourceObjectTypeUri = dBRelationship.SourceObjectTypeUri,
                TargetObjectId = dBRelationship.TargetObjectId,
                TargetObjectTypeUri = dBRelationship.TargetObjectTypeUri,
                DataSourceID = dBRelationship.DataSourceID
            };
        }
    }
}
