using GPAS.Dispatch.Entities.Concepts;
using GPAS.SearchAround;
using DAM = GPAS.Workspace.DataAccessManager;
using GPAS.Workspace.DataAccessManager.EntityConvertors;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Entities.SearchAroundResult;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.Workspace.Logic.Search
{
    public class SearchAround
    {
        public static readonly int LoadingDefaultBatchSize = int.Parse(ConfigurationManager.AppSettings["SearchAroundResultsLoadingDefaultBatchSize"]);
        public static readonly int TotalResultsThreshould = int.Parse(ConfigurationManager.AppSettings["SearchAroundTotalResultsThreshould"]);

        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <remarks>این سازنده برای جلوگیری از ایجاد نمونه از این کلاس، محلی شده است</remarks>
        private SearchAround()
        { }

        public static async Task<RelationshipBasedResult> GetRelatedEntities(IEnumerable<KWObject> objectsToSearchAround)
        {
            if (objectsToSearchAround == null)
                throw new ArgumentNullException(nameof(objectsToSearchAround));

            RelationshipBasedResult ralationshipBasedResult = await DAM.Search.SearchAroundManager.GetRelatedEntities(objectsToSearchAround.Distinct());
            return ralationshipBasedResult;
        }

        public static async Task<RelationshipBasedResult> GetRelatedEvents(IEnumerable<KWObject> objectsToSearchAround)
        {
            if (objectsToSearchAround == null)
                throw new ArgumentNullException(nameof(objectsToSearchAround));

            IEnumerable<KWObject> distinctSearchedObjects = objectsToSearchAround.Distinct();

            RelationshipBasedResult relationshipBasedResult = await DAM.Search.SearchAroundManager.GetRelatedEvents(distinctSearchedObjects);
            return relationshipBasedResult;
        }

        public static async Task<PropertyBasedResult> GetObjectsWithSameProperty(List<KWProperty> searchBaseProperties)
        {
            if (searchBaseProperties == null)
                throw new ArgumentNullException("searchBaseProperties");

            PropertyBasedResultMeatdata metadata = await DAM.Search.SearchAroundManager.GetObjectsWithSamePropertyAsync(searchBaseProperties, LoadingDefaultBatchSize, TotalResultsThreshould);
            if (metadata == null)
                throw new NullReferenceException("Invalid search response");
            PropertyBasedResult result = new PropertyBasedResult()
            {
                IsResultsCountMoreThanThreshold = metadata.IsResultsCountMoreThanThreshold,
                Results = new List<PropertyBasedResultsPerSearchedProperty>(metadata.ResultsPerSearchedPropertyID.Count)
            };
            foreach (PropertyBasedResultMetadatasPerSearchedProperty metadataPerBaseProp in metadata.ResultsPerSearchedPropertyID.Values)
            {
                result.Results.Add(new PropertyBasedResultsPerSearchedProperty()
                {
                    SearchedProperty = metadataPerBaseProp.SearchedProperty,
                    LoadedResults = metadataPerBaseProp.LoadedResults
                        .Where(p => !p.Owner.Equals(metadataPerBaseProp.SearchedProperty.Owner))
                        .Select(p => LinkManager.GetPropertyBasedKWLinkFromLinkInnerProperties(metadataPerBaseProp.SearchedProperty, p))
                        .ToList(),
                    NotLoadedResultPropertyIDs = metadataPerBaseProp.NotLoadedResultPropertyIDs
                });
            }
            return result;
        }

        public static async Task<EventBasedResult> GetRelatedObjectsByMediatorEvents(IEnumerable<KWObject> objectsToSearchAround)
        {
            if (objectsToSearchAround == null)
                throw new ArgumentNullException(nameof(objectsToSearchAround));

            IEnumerable<KWObject> distinctSearchedObjects = objectsToSearchAround.Distinct();

            EventBasedResult eventBasedResult = await DAM.Search.SearchAroundManager.GetRelatedEntitiesByMediatorEvents(distinctSearchedObjects, LoadingDefaultBatchSize, TotalResultsThreshould);
            return eventBasedResult;
        }

        public static async Task<RelationshipBasedResult> GetRelatedDocuments(IEnumerable<KWObject> objectsToSearchAround)
        {
            if (objectsToSearchAround == null)
                throw new ArgumentNullException(nameof(objectsToSearchAround));

            IEnumerable<KWObject> distinctSearchedObjects = objectsToSearchAround.Distinct();

            RelationshipBasedResult ralationshipBasedResult = await DAM.Search.SearchAroundManager.GetRelatedDocuments(distinctSearchedObjects);
            return ralationshipBasedResult;
        }

        public static async Task<KWCustomSearchAroundResult> PerformCustomSearchAround(KWObject[] objects,
            CustomSearchAroundCriteria customSearchAroundCriteria)
        {
            if (objects == null)
                throw new ArgumentNullException("objects");

            if (customSearchAroundCriteria == null)
                throw new ArgumentNullException("customSearchAroundCriteria");
            KWCustomSearchAroundResult kWCustomSearchAroundResult = null;
            Dictionary<string, long[]> searchedObjects = new Dictionary<string, long[]>();
            foreach (var item in objects.GroupBy(o => o.TypeURI))
            {
                searchedObjects.Add(item.Key, item.Select(i => i.ID).ToArray());
            }
            CustomSearchAroundResult customSearchAroundResult = 
                await DAM.Search.SearchAroundManager.PerformCustomSearchAround(searchedObjects, customSearchAroundCriteria);
            kWCustomSearchAroundResult = new KWCustomSearchAroundResult();
            SearchAroundEntitiesConvertor searchAroundEntitiesConvertor = new SearchAroundEntitiesConvertor();
            kWCustomSearchAroundResult = await searchAroundEntitiesConvertor.KWCustomSearchAroundResultToKWCustomSearchAroundResult(customSearchAroundResult);

            return await LoadCSATargets(kWCustomSearchAroundResult, LoadingDefaultBatchSize);
        }

        private async static Task<KWCustomSearchAroundResult> LoadCSATargets(KWCustomSearchAroundResult kWCustomSearchAroundResult, int count)
        {
            List<RelationshipBasedResultsPerSearchedObjects> relations = new List<RelationshipBasedResultsPerSearchedObjects>();
            List<EventBasedResultsPerSearchedObjects> events = new List<EventBasedResultsPerSearchedObjects>();
            foreach (var item in kWCustomSearchAroundResult.RalationshipBasedResult)
            {
                var searchedObject = item.SearchedObject;
                List<RelationshipBasedLoadedTargetResult> relationshipBasedLoadedResults = new List<RelationshipBasedLoadedTargetResult>();
                List<RelationshipBasedNotLoadedResult> relationshipBasedNotLoadedResults = new List<RelationshipBasedNotLoadedResult>();
                List<RelationshipBasedNotLoadedResult> relationshipBasedNotLoadedResultsToLoad = new List<RelationshipBasedNotLoadedResult>();
                int counter = 1;
                foreach (var notLoadedResult in item.NotLoadedResults)
                {
                    if (counter <= count)
                    {
                        relationshipBasedNotLoadedResultsToLoad.Add(notLoadedResult);
                    }
                    else
                    {
                        relationshipBasedNotLoadedResults.Add(notLoadedResult);
                    }
                    counter++;
                }
                relationshipBasedLoadedResults = await FillRelatonshipBasedTargetObjects(relationshipBasedNotLoadedResultsToLoad);

                relations.Add(
                    new RelationshipBasedResultsPerSearchedObjects()
                    {
                        SearchedObject = searchedObject,
                        NotLoadedResults = relationshipBasedNotLoadedResults,
                        LoadedResults = relationshipBasedLoadedResults
                    }
                    );
            }

            foreach (var item in kWCustomSearchAroundResult.EventBasedResult)
            {
                var searchedObject = item.SearchedObject;
                List<EventBasedLoadedTargetResult> eventBasedLoadedTargetResults = new List<EventBasedLoadedTargetResult>();
                List<EventBasedNotLoadedResult> eventBasedNotLoadedResults = new List<EventBasedNotLoadedResult>();
                List<EventBasedNotLoadedResult> eventBasedNotLoadedResultsToLoad = new List<EventBasedNotLoadedResult>();
                int counter = 1;
                foreach (var notLoadedResult in item.NotLoadedResults)
                {
                    if (counter <= count)
                    {
                        eventBasedNotLoadedResultsToLoad.Add(notLoadedResult);
                    }
                    else
                    {
                        eventBasedNotLoadedResults.Add(notLoadedResult);
                    }
                    counter++;
                }
                eventBasedLoadedTargetResults = await FillEventBasedTargetObject(eventBasedNotLoadedResultsToLoad);

                events.Add(
                   new EventBasedResultsPerSearchedObjects()
                   {
                       SearchedObject = searchedObject,
                       NotLoadedResults = eventBasedNotLoadedResults,
                       LoadedResults = eventBasedLoadedTargetResults
                   }
                   );
            }
            return new KWCustomSearchAroundResult()
            {
                EventBasedResult = events,
                RalationshipBasedResult = relations,
                IsResultsCountMoreThanThreshold = kWCustomSearchAroundResult.IsResultsCountMoreThanThreshold
            };
        }

        private async static Task<List<EventBasedLoadedTargetResult>> FillEventBasedTargetObject(List<EventBasedNotLoadedResult> notLoadedResultsToLoad)
        {
            List<EventBasedLoadedTargetResult> results = new List<EventBasedLoadedTargetResult>();
            //var objs = ObjectManager.GetObjectsById(notLoadedResultsToLoad.Select(o => o.TargetObjectID));

            foreach (var item in notLoadedResultsToLoad)
            {
                results.Add(
                    new EventBasedLoadedTargetResult()
                    {
                        TargetObject = await ObjectManager.GetObjectById(item.TargetObjectID),
                        InnerRelationshipIDs = item.InnerRelationships
                    }
                    );
            }
            return results;
        }

        private async static Task<List<RelationshipBasedLoadedTargetResult>> FillRelatonshipBasedTargetObjects(List<RelationshipBasedNotLoadedResult> notLoadedResults1)
        {
            List<RelationshipBasedLoadedTargetResult> results = new List<RelationshipBasedLoadedTargetResult>();
            var objs = ObjectManager.GetObjectsById(notLoadedResults1.Select(o => o.TargetObjectID));

            foreach (var item in notLoadedResults1)
            {
                results.Add(
                    new RelationshipBasedLoadedTargetResult()
                    {
                        TargetObject = await ObjectManager.GetObjectById(item.TargetObjectID),
                        RelationshipIDs = item.RelationshipIDs
                    }
                    );
            }
            return results;
        }

        private static async Task<Dictionary<KWObject, List<RelationshipBasedKWLink>>> ConvertRelationshipBaseKlinkToRelationshipBaseKWlink(KWObject[] objectIds, RelationshipBaseKlink[] ralationshps)
        {
            if (ralationshps == null)
                throw new ArgumentNullException("ralationshps");

            List<RelationshipBasedKWLink> retrievedLinks = (await DAM.LinkManager.GetRelationshipBaseLinksFromRetrievedDataAsync(ralationshps)).ToList();
            Dictionary<KWObject, List<RelationshipBasedKWLink>> retrievedLinksPerObjects = new Dictionary<KWObject, List<RelationshipBasedKWLink>>(retrievedLinks.Count);
            foreach (KWObject obj in objectIds)
            {
                retrievedLinksPerObjects.Add(obj, new List<RelationshipBasedKWLink>());
            }

            foreach (RelationshipBasedKWLink link in retrievedLinks)
            {
                if (retrievedLinksPerObjects.ContainsKey(link.Source))
                {
                    (retrievedLinksPerObjects[link.Source]).Add(link);
                }
                if (retrievedLinksPerObjects.ContainsKey(link.Target))
                {
                    (retrievedLinksPerObjects[link.Target]).Add(link);
                }
            }
            return retrievedLinksPerObjects;
        }

        private static async Task<Dictionary<KWObject, List<EventBasedKWLink>>> ConvertEventBaseKWLinkToEventBaseKLink(KWObject[] objects, EventBaseKlink[] eventBaseKLink)
        {
            List<EventBasedKWLink> retrievedLinks = new List<EventBasedKWLink>();
            //foreach (EventBaseKlink kLink in eventBaseKLink)
            //{
            //    EventBasedKWLink link = await DataAccessManager.LinkManager.ConvertEventBaseKlinkToEventBasedKWLink(kLink);
            //    retrievedLinks.Add(link);
            //}

            retrievedLinks = await DAM.LinkManager.ConvertEventBaseKlinksToEventBasedKWLinks(eventBaseKLink.ToList());

            Dictionary<KWObject, List<EventBasedKWLink>> retrievedLinksPerObjects = new Dictionary<KWObject, List<EventBasedKWLink>>(retrievedLinks.Count);
            foreach (KWObject obj in objects)
            {
                retrievedLinksPerObjects.Add(obj, new List<EventBasedKWLink>());
            }

            foreach (EventBasedKWLink link in retrievedLinks)
            {
                if (retrievedLinksPerObjects.ContainsKey(link.Source))
                {
                    (retrievedLinksPerObjects[link.Source]).Add(link);
                }
                if (retrievedLinksPerObjects.ContainsKey(link.Target))
                {
                    (retrievedLinksPerObjects[link.Target]).Add(link);
                }
            }
            return retrievedLinksPerObjects;
        }
    }
}