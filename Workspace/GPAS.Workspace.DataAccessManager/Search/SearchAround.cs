using GPAS.Dispatch.Entities.Concepts;
using DispatchSAResult = GPAS.Dispatch.Entities.Concepts.SearchAroundResult;
using GPAS.SearchAround;
using GPAS.Utility;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Entities.SearchAroundResult;
using GPAS.Workspace.ServiceAccess;
using GPAS.Workspace.ServiceAccess.RemoteService;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.Workspace.DataAccessManager.Search
{
    public class SearchAroundManager
    {
        // TODO: تامین این مقادیر برعهده‌ی منطق است و باید به توابع این کلاس پاس داده شود
        static readonly int LoadingDefaultBatchSize = int.Parse(ConfigurationManager.AppSettings["SearchAroundResultsLoadingDefaultBatchSize"]);
        static readonly int TotalResultsThreshould = int.Parse(ConfigurationManager.AppSettings["SearchAroundTotalResultsThreshould"]);

        private SearchAroundManager() { }

        #region تعامل با سرویس‌دهنده راه‌دور
        private static async Task<DispatchSAResult.RelationshipBasedResult> FindRelatedPublishedEntities(IEnumerable<KWObject> objectsToSearchAround)
        {
            if (objectsToSearchAround == null)
                throw new ArgumentNullException(nameof(objectsToSearchAround));

            DispatchSAResult.RelationshipBasedResult relationshipBasedResult = null;
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            Dictionary<string, long[]> searchedObjects = new Dictionary<string, long[]>();
            foreach (var item in objectsToSearchAround.GroupBy(o => o.TypeURI))
            {
                searchedObjects.Add(item.Key, item.Select(i => i.ID).ToArray());
            }

            try
            {
                relationshipBasedResult = await sc.FindRelatedEntitiesAsync(searchedObjects, TotalResultsThreshould);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }

            return relationshipBasedResult;
        }
        private static async Task<DispatchSAResult.RelationshipBasedResult> FindRelatedPublishedEventsAsync(IEnumerable<KWObject> objectsToSearchAround)
        {
            if (objectsToSearchAround == null)
                throw new ArgumentNullException(nameof(objectsToSearchAround));

            DispatchSAResult.RelationshipBasedResult relationshipBasedResult = null;

            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            Dictionary<string, long[]> searchedObjects = new Dictionary<string, long[]>();
            foreach (var item in objectsToSearchAround.GroupBy(o => o.TypeURI))
            {
                searchedObjects.Add(item.Key, item.Select(i => i.ID).ToArray());
            }
            
            try
            {
                relationshipBasedResult = await sc.FindRelatedEventsAsync(searchedObjects, TotalResultsThreshould);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
            return relationshipBasedResult;
        }

        private static async Task<DispatchSAResult.RelationshipBasedResult> FindRelatedPublishedDocumentsAsync(IEnumerable<KWObject> objectsToSearchAround)
        {
            if (objectsToSearchAround == null)
                throw new ArgumentNullException(nameof(objectsToSearchAround));
            DispatchSAResult.RelationshipBasedResult relationshipBasedResult = null;

            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            Dictionary<string, long[]> searchedObjects = new Dictionary<string, long[]>();
            foreach (var item in objectsToSearchAround.GroupBy(o => o.TypeURI))
            {
                searchedObjects.Add(item.Key, item.Select(i => i.ID).ToArray());
            }
            try
            {
                relationshipBasedResult = await sc.FindRelatedDocumentsAsync(searchedObjects, TotalResultsThreshould);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
            return relationshipBasedResult;
        }
        private static async Task<DispatchSAResult.PropertyBasedResult> FindPropertiesSameWithAsync(KProperty[] properties, int loadNResult, int totalResultThreshold)
        {
            if (properties == null)
                throw new ArgumentNullException(nameof(properties));
            if (loadNResult < 0)
            {    //throw new ArgumentException(nameof(loadNResult));
                loadNResult = 0;
            }
            if (totalResultThreshold < 0)
            {
                //throw new ArgumentException(nameof(totalResultThreshold));
                totalResultThreshold = 0;
            }
            DispatchSAResult.PropertyBasedResult propertyBasedResult = null;
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            try
            {
                propertyBasedResult = await sc.FindPropertiesSameWithAsync(properties, loadNResult, totalResultThreshold);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
            return propertyBasedResult;
        }

        private static async Task<DispatchSAResult.EventBasedResult> FindRelatedPublishedEntitiesAppearedInEvents(IEnumerable<KWObject> objectsToSearchAround)
        {
            if (objectsToSearchAround == null)
                throw new ArgumentNullException(nameof(objectsToSearchAround));
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            DispatchSAResult.EventBasedResult eventBasedResult = null;
            Dictionary<string, long[]> searchedObjects = new Dictionary<string, long[]>();
            foreach (var item in objectsToSearchAround.GroupBy(o => o.TypeURI))
            {
                searchedObjects.Add(item.Key, item.Select(i => i.ID).ToArray());
            }
            try
            {
                eventBasedResult = await sc.FindRelatedEntitiesAppearedInEventsAsync(searchedObjects, TotalResultsThreshould);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
            return eventBasedResult;
        }
        #endregion

        #region توابع جستجو

        /////////////////// Relations ////////////////////////////////////////////////////////
        public static async Task<RelationshipBasedResult> GetRelatedEntities(IEnumerable<KWObject> searchedObjects)
        {
            if (searchedObjects == null)
                throw new ArgumentNullException(nameof(searchedObjects));

            RelationshipBasedResult result = new RelationshipBasedResult();

            IEnumerable<KWObject> publishedSearchedObjects = searchedObjects.Where(o => !ObjectManager.IsUnpublishedObject(o));

            RelationshipBasedResult unpublishRalationshipBasedResult = await GetUnpublishRalationshipBasedResult(searchedObjects);

            RelationshipBasedResult wsPublishedRelationshipBasedResult = null;
            if (publishedSearchedObjects.Any())
            {
                wsPublishedRelationshipBasedResult = await GetPublishRalationshipBasedResult(publishedSearchedObjects);
            }

            result = MergePublishedAndUnpublishedRelationshipResult(wsPublishedRelationshipBasedResult, unpublishRalationshipBasedResult);

            //// TODO: یکی کردن درخواست جستجوی پیرامونی به جای تودرتو بودن آن برای اشیاء ادغام شده
            //IEnumerable<KWObject> objectsWhereLocallyResolvedToSearchedObjects
            //    = searchedObjects.SelectMany(o => ObjectManager.GetObjectWhereLocallyResolvedToObject(o.ID));
            //if (objectsWhereLocallyResolvedToSearchedObjects.Any())
            //{
            //    RelationshipBasedResult relationshipBasedResultForLocallyResolvedObjs =
            //        await GetRelatedEntities(objectsWhereLocallyResolvedToSearchedObjects, loadNResults, totalResultsThreshold);
            //}

            RelationshipBasedResult relationshipBasedResultWithLoadedResults = await LoadRelationshipBasedSearchAroundTargets(result, LoadingDefaultBatchSize);

            return relationshipBasedResultWithLoadedResults;
        }

        private async static Task<RelationshipBasedResult> GetPublishRalationshipBasedResult(IEnumerable<KWObject> publishedSearchedObjects)
        {
            DispatchSAResult.RelationshipBasedResult remotePublishedRelationshipBasedResult = await FindRelatedPublishedEntities(publishedSearchedObjects);

            EntityConvertors.SearchAroundEntitiesConvertor searchAroundEntitiesConvertor = new EntityConvertors.SearchAroundEntitiesConvertor();
            return await searchAroundEntitiesConvertor.ConvertRemoteRalationResultToWSRalationResult(remotePublishedRelationshipBasedResult);
        }

        private async static Task<RelationshipBasedResult> LoadRelationshipBasedSearchAroundTargets(RelationshipBasedResult relationshipBasedResult,
            int loadNResults)
        {
            RelationshipBasedResult result = new RelationshipBasedResult();

            result.IsResultsCountMoreThanThreshold = relationshipBasedResult.IsResultsCountMoreThanThreshold;

            foreach (var currentResult in relationshipBasedResult.Results)
            {
                if (currentResult.LoadedResults.Count < loadNResults)
                {
                    Tuple<List<RelationshipBasedLoadedTargetResult>,
                        List<RelationshipBasedNotLoadedResult>> separateResults = await SeparateLoadedResultFromNotLoadedResult(currentResult, loadNResults);
                    RelationshipBasedResultsPerSearchedObjects relationPerObject = new RelationshipBasedResultsPerSearchedObjects()
                    {
                        SearchedObject = currentResult.SearchedObject,
                        LoadedResults = separateResults.Item1,
                        NotLoadedResults = separateResults.Item2
                    };
                    result.Results.Add(relationPerObject);
                }
                else
                {
                    result.Results.Add(currentResult);
                }
            }

            return result;
        }

        private async static Task<Tuple<List<RelationshipBasedLoadedTargetResult>, List<RelationshipBasedNotLoadedResult>>> SeparateLoadedResultFromNotLoadedResult(RelationshipBasedResultsPerSearchedObjects relationPerObject, int loadNResults)
        {
            List<RelationshipBasedLoadedTargetResult> loaded = new List<RelationshipBasedLoadedTargetResult>();
            List<RelationshipBasedNotLoadedResult> notLoaded = new List<RelationshipBasedNotLoadedResult>();
            if (relationPerObject.LoadedResults.Count < loadNResults)
            {
                for (int i = 0; i < relationPerObject.LoadedResults.Count; i++)
                {
                    loaded.Add(relationPerObject.LoadedResults.ElementAt(i));
                }
                
                if (loaded.Count < loadNResults)
                {
                    foreach (var item in relationPerObject.NotLoadedResults)
                    {
                        if (loaded.Count < loadNResults)
                        {
                            loaded.Add(new RelationshipBasedLoadedTargetResult()
                            {
                                RelationshipIDs = item.RelationshipIDs,
                                TargetObject = await ObjectManager.GetObjectByIdAsync(item.TargetObjectID)
                            });
                        }
                        else
                        {
                            notLoaded.Add(item);
                        }
                    }
                }
            }

            return new Tuple<List<RelationshipBasedLoadedTargetResult>, List<RelationshipBasedNotLoadedResult>>(loaded, notLoaded);
        }

        private static RelationshipBasedResult MergePublishedAndUnpublishedRelationshipResult(RelationshipBasedResult publishedRelationResult,
            RelationshipBasedResult unpublishRalationResult)
        {
            RelationshipBasedResult result = new RelationshipBasedResult();

            if (publishedRelationResult == null && unpublishRalationResult != null)
            {
                result = unpublishRalationResult;
                return result;
            }

            if (unpublishRalationResult == null && publishedRelationResult != null)
            {
                result = publishedRelationResult;
                return result;
            }

            result.Results = unpublishRalationResult.Results;

            foreach (var currentPublishedResult in publishedRelationResult.Results)
            {
                if (!result.Results.Where(r => r.SearchedObject.ID == currentPublishedResult.SearchedObject.ID).Any())
                {
                    result.Results.Add(currentPublishedResult);
                }
                else
                {
                    RelationshipBasedResultsPerSearchedObjects existanceRelationPerObj = result.Results.Where(r => r.SearchedObject.ID == currentPublishedResult.SearchedObject.ID).First();
                    foreach (var currentLoadedResult in currentPublishedResult.LoadedResults)
                    {
                        if (existanceRelationPerObj.LoadedResults.Count < LoadingDefaultBatchSize)
                        {
                            existanceRelationPerObj.LoadedResults.Add(currentLoadedResult);
                        }
                        else
                        {
                            existanceRelationPerObj.NotLoadedResults.Add(new RelationshipBasedNotLoadedResult()
                            {
                                RelationshipIDs = currentLoadedResult.RelationshipIDs,
                                TargetObjectID = currentLoadedResult.TargetObject.ID
                            });
                        }
                    }

                    foreach (var currentNotLoadedResult in currentPublishedResult.NotLoadedResults)
                    {
                        existanceRelationPerObj.NotLoadedResults.Add(currentNotLoadedResult);
                    }
                }
            }
            
            if ((unpublishRalationResult.IsResultsCountMoreThanThreshold || publishedRelationResult.IsResultsCountMoreThanThreshold))
            {
                result.IsResultsCountMoreThanThreshold = true;
            }
            else
            {
                int relationTargetCounts = 0;
                foreach (var currentResult in result.Results)
                {
                    relationTargetCounts += currentResult.LoadedResults.Count;
                    relationTargetCounts += currentResult.NotLoadedResults.Count;
                }

                if (relationTargetCounts > TotalResultsThreshould + 1)
                {
                    result.IsResultsCountMoreThanThreshold = true;
                }
            }

            return result;
        }

        private static async Task<RelationshipBasedResult> GetUnpublishRalationshipBasedResult(IEnumerable<KWObject> objectsToSearchAround)
        {
            Dictionary<long, List<RelationshipBasedKWLink>> objectToRelationMapping = new Dictionary<long, List<RelationshipBasedKWLink>>();
            IEnumerable<LinkManager.CachedRelationshipMetadata> unpublishRelationships = LinkManager.GetUnpublishedRelationships();
            HashSet<long> searchedObjectIDsHashSet = new HashSet<long>(objectsToSearchAround.Select(o => o.ID));

            foreach (var currentUnpublishRelation in unpublishRelationships)
            {
                if (searchedObjectIDsHashSet.Contains(currentUnpublishRelation.RelationshipSourceId) &&
                    System.GetOntology().IsEntity((await ObjectManager.GetObjectByIdAsync(currentUnpublishRelation.RelationshipTargetId)).TypeURI))
                {
                    if (objectToRelationMapping.ContainsKey(currentUnpublishRelation.RelationshipSourceId))
                    {
                        objectToRelationMapping[currentUnpublishRelation.RelationshipSourceId].Add(await LinkManager.GenerateRelationshipBasedLinkAsync(currentUnpublishRelation));
                    }
                    else
                    {
                        objectToRelationMapping.Add(currentUnpublishRelation.RelationshipSourceId, new List<RelationshipBasedKWLink>());
                        objectToRelationMapping[currentUnpublishRelation.RelationshipSourceId].Add(await LinkManager.GenerateRelationshipBasedLinkAsync(currentUnpublishRelation));
                    }
                }
                if (searchedObjectIDsHashSet.Contains(currentUnpublishRelation.RelationshipTargetId) &&
                   System.GetOntology().IsEntity(((await ObjectManager.GetObjectByIdAsync(currentUnpublishRelation.RelationshipSourceId)).TypeURI)))
                {
                    if (objectToRelationMapping.ContainsKey(currentUnpublishRelation.RelationshipTargetId))
                    {
                        objectToRelationMapping[currentUnpublishRelation.RelationshipTargetId].Add(await LinkManager.GenerateRelationshipBasedLinkAsync(currentUnpublishRelation));
                    }
                    else
                    {
                        objectToRelationMapping.Add(currentUnpublishRelation.RelationshipTargetId, new List<RelationshipBasedKWLink>());
                        objectToRelationMapping[currentUnpublishRelation.RelationshipTargetId].Add(await LinkManager.GenerateRelationshipBasedLinkAsync(currentUnpublishRelation));
                    }
                }
            }


            RelationshipBasedResult ralationshipBasedResult = new RelationshipBasedResult();
            List<RelationshipBasedResultsPerSearchedObjects> relationshipResultPerObjList = new List<RelationshipBasedResultsPerSearchedObjects>();
            HashSet<long> targetObjectIDsHashSet = new HashSet<long>();

            foreach (var currentSearchedObj in objectToRelationMapping.Keys)
            {
                RelationshipBasedResultsPerSearchedObjects relationshipResultPerObj = new RelationshipBasedResultsPerSearchedObjects();

                relationshipResultPerObj.SearchedObject = await ObjectManager.GetObjectByIdAsync(currentSearchedObj);
                foreach (var currentRelation in objectToRelationMapping[currentSearchedObj])
                {
                    if (searchedObjectIDsHashSet.Contains(currentRelation.Source.ID))
                    {
                        if (!targetObjectIDsHashSet.Contains(currentRelation.Target.ID))
                        {
                            if (relationshipResultPerObj.LoadedResults.Count < LoadingDefaultBatchSize)
                            {
                                RelationshipBasedLoadedTargetResult relationBasedLoadedTargetResult = new RelationshipBasedLoadedTargetResult()
                                {
                                    RelationshipIDs = new List<long>() { currentRelation.Relationship.ID },
                                    TargetObject = currentRelation.Target
                                };
                                relationshipResultPerObj.LoadedResults.Add(relationBasedLoadedTargetResult);
                                targetObjectIDsHashSet.Add(currentRelation.Target.ID);
                            }
                            else
                            {
                                RelationshipBasedNotLoadedResult relationshipBasedNotLoadedResult = new RelationshipBasedNotLoadedResult()
                                {
                                    RelationshipIDs = new List<long>() { currentRelation.Relationship.ID },
                                    TargetObjectID = currentRelation.Target.ID
                                };
                                relationshipResultPerObj.NotLoadedResults.Add(relationshipBasedNotLoadedResult);
                                targetObjectIDsHashSet.Add(currentRelation.Target.ID);
                            }

                        }
                        else
                        {
                            if (relationshipResultPerObj.LoadedResults.Count < LoadingDefaultBatchSize)
                            {
                                RelationshipBasedLoadedTargetResult relationBasedLoadedTargetResult = relationshipResultPerObj.LoadedResults.Where(r => r.TargetObject.ID == currentRelation.Target.ID).FirstOrDefault();
                                relationBasedLoadedTargetResult.RelationshipIDs.Add(currentRelation.Relationship.ID);
                            }
                            else
                            {
                                RelationshipBasedNotLoadedResult relationshipBasedNotLoadedResult = relationshipResultPerObj.NotLoadedResults.Where(r => r.TargetObjectID == currentRelation.Target.ID).FirstOrDefault();
                                relationshipBasedNotLoadedResult.RelationshipIDs.Add(currentRelation.Relationship.ID);
                            }
                        }
                    }
                    else if(searchedObjectIDsHashSet.Contains(currentRelation.Target.ID))
                    {
                        if (!targetObjectIDsHashSet.Contains(currentRelation.Source.ID))
                        {
                            if (relationshipResultPerObj.LoadedResults.Count < LoadingDefaultBatchSize)
                            {
                                RelationshipBasedLoadedTargetResult relationBasedLoadedTargetResult = new RelationshipBasedLoadedTargetResult()
                                {
                                    RelationshipIDs = new List<long>() { currentRelation.Relationship.ID },
                                    TargetObject = currentRelation.Source
                                };
                                relationshipResultPerObj.LoadedResults.Add(relationBasedLoadedTargetResult);
                                targetObjectIDsHashSet.Add(currentRelation.Source.ID);
                            }
                            else
                            {
                                RelationshipBasedNotLoadedResult relationshipBasedNotLoadedResult = new RelationshipBasedNotLoadedResult()
                                {
                                    RelationshipIDs = new List<long>() { currentRelation.Relationship.ID },
                                    TargetObjectID = currentRelation.Source.ID
                                };
                                relationshipResultPerObj.NotLoadedResults.Add(relationshipBasedNotLoadedResult);
                                targetObjectIDsHashSet.Add(currentRelation.Source.ID);
                            }

                        }
                        else
                        {
                            if (relationshipResultPerObj.LoadedResults.Count < LoadingDefaultBatchSize)
                            {
                                RelationshipBasedLoadedTargetResult relationBasedLoadedTargetResult = relationshipResultPerObj.LoadedResults.Where(r => r.TargetObject.ID == currentRelation.Source.ID).FirstOrDefault();
                                relationBasedLoadedTargetResult.RelationshipIDs.Add(currentRelation.Relationship.ID);
                            }
                            else
                            {
                                RelationshipBasedNotLoadedResult relationshipBasedNotLoadedResult = relationshipResultPerObj.NotLoadedResults.Where(r => r.TargetObjectID == currentRelation.Source.ID).FirstOrDefault();
                                relationshipBasedNotLoadedResult.RelationshipIDs.Add(currentRelation.Relationship.ID);
                            }
                        }
                    }                    
                }
                relationshipResultPerObjList.Add(relationshipResultPerObj);
            }
            ralationshipBasedResult.Results = relationshipResultPerObjList;

            int relationTargetCounts = 0;
            foreach (var currentResult in ralationshipBasedResult.Results)
            {
                relationTargetCounts += currentResult.LoadedResults.Count;
                relationTargetCounts += currentResult.NotLoadedResults.Count;
            }

            if (relationTargetCounts > TotalResultsThreshould + 1)
            {
                ralationshipBasedResult.IsResultsCountMoreThanThreshold = true;
            }

            return ralationshipBasedResult;
        }

        ///////////////// Events //////////////////////////////////////////////////////////////
        public static async Task<EventBasedResult> GetRelatedEntitiesByMediatorEvents(IEnumerable<KWObject> searchedObjects, int loadNResults, int totalResultsThreshold)
        {
            if (searchedObjects == null)
                throw new ArgumentNullException(nameof(searchedObjects));

            EventBasedResult result = new EventBasedResult();

            IEnumerable<KWObject> publishedSearchedObjects = searchedObjects.Where(o => !ObjectManager.IsUnpublishedObject(o));

            EventBasedResult unpublishEventBasedResult = await GetUnpublishEventBasedResult(searchedObjects);
                        
            EventBasedResult wsPublishedEventBasedResult = null;

            if (publishedSearchedObjects.Any())
            {
                wsPublishedEventBasedResult = await GetPublishEventBasedResult(publishedSearchedObjects);
            }

            result = MergePublishedAndUnpublishedEventResult(wsPublishedEventBasedResult, unpublishEventBasedResult);

            //// TODO: یکی کردن درخواست جستجوی پیرامونی به جای تودرتو بودن آن برای اشیاء ادغام شده
            //IEnumerable<KWObject> objectsWhereLocallyResolvedToSearchedObjects
            //    = searchedObjects.SelectMany(o => ObjectManager.GetObjectWhereLocallyResolvedToObject(o.ID));
            //if (objectsWhereLocallyResolvedToSearchedObjects.Any())
            //{
            //    RelationshipBasedResult relationshipBasedResultForLocallyResolvedObjs =
            //        await GetRelatedEntities(objectsWhereLocallyResolvedToSearchedObjects, loadNResults, totalResultsThreshold);
            //}

            EventBasedResult eventBasedResultWithLoadedResults = await LoadEventBasedSearchAroundTargets(result, loadNResults);

            return eventBasedResultWithLoadedResults;
        }

        private async static Task<EventBasedResult> GetPublishEventBasedResult(IEnumerable<KWObject> publishedSearchedObjects)
        {
            DispatchSAResult.EventBasedResult remotePublishedEventResult = await FindRelatedPublishedEntitiesAppearedInEvents(publishedSearchedObjects);

            EntityConvertors.SearchAroundEntitiesConvertor searchAroundEntitiesConvertor = new EntityConvertors.SearchAroundEntitiesConvertor();
            return await searchAroundEntitiesConvertor.ConvertRemoteEventResultToWSRalationResult(remotePublishedEventResult);
        }

        private static EventBasedResult MergePublishedAndUnpublishedEventResult(EventBasedResult publishedEventBasedResult, EventBasedResult unpublishEventBasedResult)
        {
            EventBasedResult result = new EventBasedResult();

            if (publishedEventBasedResult == null && unpublishEventBasedResult != null)
            {
                result = unpublishEventBasedResult;
                return result;
            }

            if (unpublishEventBasedResult == null && publishedEventBasedResult != null)
            {
                result = publishedEventBasedResult;
                return result;
            }

            result.Results = unpublishEventBasedResult.Results;

            foreach (var currentPublishedEvent in publishedEventBasedResult.Results)
            {
                if (!result.Results.Where(r => r.SearchedObject.ID == currentPublishedEvent.SearchedObject.ID).Any())
                {
                    result.Results.Add(currentPublishedEvent);
                }
                else
                {
                    EventBasedResultsPerSearchedObjects existanceEventPerObj = result.Results.Where(r => r.SearchedObject.ID == currentPublishedEvent.SearchedObject.ID).First();
                    foreach (var currentLoadedResult in currentPublishedEvent.LoadedResults)
                    {
                        if (existanceEventPerObj.LoadedResults.Count < LoadingDefaultBatchSize)
                        {
                            existanceEventPerObj.LoadedResults.Add(currentLoadedResult);
                        }
                        else
                        {
                            existanceEventPerObj.NotLoadedResults.Add(new EventBasedNotLoadedResult()
                            {
                                InnerRelationships = currentLoadedResult.InnerRelationshipIDs,
                                TargetObjectID = currentLoadedResult.TargetObject.ID
                            });
                        }
                    }

                    foreach (var currentNotLoadedResult in currentPublishedEvent.NotLoadedResults)
                    {
                        existanceEventPerObj.NotLoadedResults.Add(currentNotLoadedResult);
                    }
                }
            }

            if ((unpublishEventBasedResult.IsResultsCountMoreThanThreshold || publishedEventBasedResult.IsResultsCountMoreThanThreshold))
            {
                result.IsResultsCountMoreThanThreshold = true;
            }
            else
            {
                int relationTargetCounts = 0;
                foreach (var currentResult in result.Results)
                {
                    relationTargetCounts += currentResult.LoadedResults.Count;
                    relationTargetCounts += currentResult.NotLoadedResults.Count;
                }

                if (relationTargetCounts > TotalResultsThreshould + 1)
                {
                    result.IsResultsCountMoreThanThreshold = true;
                }
            }

            return result;
        }

        private async static Task<EventBasedResult> LoadEventBasedSearchAroundTargets(EventBasedResult eventBasedResult, int loadNResults)
        {
            EventBasedResult result = new EventBasedResult();

            List<EventBasedResultsPerSearchedObjects> tempResults = new List<EventBasedResultsPerSearchedObjects>();

            result.IsResultsCountMoreThanThreshold = eventBasedResult.IsResultsCountMoreThanThreshold;

            foreach (var currentResult in eventBasedResult.Results)
            {
                if (currentResult.LoadedResults.Count < loadNResults)
                {                    
                    Tuple<List<EventBasedLoadedTargetResult>,
                        List<EventBasedNotLoadedResult>> separateResults = await SeparateEventBasedLoadedResultFromNotLoadedResult(currentResult, loadNResults);
                    EventBasedResultsPerSearchedObjects relationPerObject = new EventBasedResultsPerSearchedObjects()
                    {
                        SearchedObject = currentResult.SearchedObject,
                        LoadedResults = separateResults.Item1,
                        NotLoadedResults = separateResults.Item2
                    };
                    tempResults.Add(relationPerObject);
                }
                else
                {
                    tempResults.Add(currentResult);
                }
            }
            result.Results = tempResults;

            return result;
        }

        private async static Task<Tuple<List<EventBasedLoadedTargetResult>, List<EventBasedNotLoadedResult>>> SeparateEventBasedLoadedResultFromNotLoadedResult(EventBasedResultsPerSearchedObjects eventPerObject, int loadNResults)
        {
            List<EventBasedLoadedTargetResult> loaded = new List<EventBasedLoadedTargetResult>();
            List<EventBasedNotLoadedResult> notLoaded = new List<EventBasedNotLoadedResult>();
            if (eventPerObject.LoadedResults.Count < loadNResults)
            {
                for (int i = 0; i < eventPerObject.LoadedResults.Count; i++)
                {
                    loaded.Add(eventPerObject.LoadedResults.ElementAt(i));
                }

                if (loaded.Count < loadNResults)
                {
                    foreach (var item in eventPerObject.NotLoadedResults)
                    {
                        if (loaded.Count < loadNResults)
                        {
                            loaded.Add(new EventBasedLoadedTargetResult()
                            {
                                InnerRelationshipIDs = item.InnerRelationships,
                                TargetObject = await ObjectManager.GetObjectByIdAsync(item.TargetObjectID)
                            });                            
                        }
                        else
                        {
                            notLoaded.Add(item);
                        }
                    }
                }
            }

            return new Tuple<List<EventBasedLoadedTargetResult>, List<EventBasedNotLoadedResult>>(loaded, notLoaded);
        }

        private async static Task<EventBasedResult> GetUnpublishEventBasedResult(IEnumerable<KWObject> objectsToSearchAround)
        {
            EventBasedResult unpublishEventBasedResult = new EventBasedResult();

            List<RelationshipBasedKWLink> relationsWithPublishedOrUnpublishEvents = await GetUnpublishRelationsWithEvents(objectsToSearchAround);

            List<KWObject> intermediateEvents = GetEventObjectFromRelationshipBasedKWLink(relationsWithPublishedOrUnpublishEvents, objectsToSearchAround);

            List<RelationshipBasedKWLink> relationsWithIntermediateEvents = await GetUnpublishRelationsWithEvents(intermediateEvents);

            unpublishEventBasedResult = await GenerateEventBasedKWLink(objectsToSearchAround, intermediateEvents, relationsWithPublishedOrUnpublishEvents, relationsWithIntermediateEvents);

            return unpublishEventBasedResult;
        }

        public static async Task<List<RelationshipBasedKWLink>> GetUnpublishRelationsWithEvents(IEnumerable<KWObject> objectsToSearchAround)
        {
            if (objectsToSearchAround == null)
                throw new ArgumentNullException(nameof(objectsToSearchAround));
            HashSet<long> searchedObjectIDsHashSet = new HashSet<long>(objectsToSearchAround.Select(o => o.ID));
            List<RelationshipBasedKWLink> relatedEvents = new List<RelationshipBasedKWLink>();
            IEnumerable<LinkManager.CachedRelationshipMetadata> cachedRelationships = LinkManager.GetCachedRelationships();
            IEnumerable<LinkManager.CachedRelationshipMetadata> unpublishedRelationships = cachedRelationships.Where(r=>!r.IsPublished);
            if (unpublishedRelationships.Any())
            {
                foreach (var item in unpublishedRelationships)
                {
                    if (searchedObjectIDsHashSet.Contains(item.RelationshipSourceId) &&
                        ((System.GetOntology().IsEvent((await ObjectManager.GetObjectByIdAsync(item.RelationshipTargetId)).TypeURI)) ||
                        (System.GetOntology().IsEvent((await ObjectManager.GetObjectByIdAsync(item.RelationshipSourceId)).TypeURI))))
                    {
                        relatedEvents.Add(await LinkManager.GenerateRelationshipBasedLinkAsync(item));
                    }
                    if (searchedObjectIDsHashSet.Contains(item.RelationshipTargetId) &&
                        ((System.GetOntology().IsEvent((await ObjectManager.GetObjectByIdAsync(item.RelationshipSourceId)).TypeURI)) ||
                        (System.GetOntology().IsEvent((await ObjectManager.GetObjectByIdAsync(item.RelationshipTargetId)).TypeURI))))
                    {
                        relatedEvents.Add(await LinkManager.GenerateRelationshipBasedLinkAsync(item));
                    }
                }
            }
            return relatedEvents;
        }

        private async static Task<EventBasedResult> GenerateEventBasedKWLink(IEnumerable<KWObject> objectsToSearchAround,
            IEnumerable<KWObject> intermediateEvents,
            IEnumerable<RelationshipBasedKWLink> relationsWithPublishedOrUnpublishEvents,
            List<RelationshipBasedKWLink> relationsWithIntermediateEvents)
        {
            List<RelationshipBasedKWLink> distinctRelations = new List<RelationshipBasedKWLink>();
            HashSet<long> relationIDs = new HashSet<long>();
            HashSet<long> objectsToSearchAroundIDs = new HashSet<long>(objectsToSearchAround.Select(o => o.ID));
            HashSet<long> intermediateEventIDs = new HashSet<long>(intermediateEvents.Select(o => o.ID));
            HashSet<long> targetIDs = new HashSet<long>();

            foreach (var currentRelation in relationsWithPublishedOrUnpublishEvents)
            {
                if (!relationIDs.Contains(currentRelation.Relationship.ID))
                {
                    distinctRelations.Add(currentRelation);
                    relationIDs.Add(currentRelation.Relationship.ID);
                }
            }

            foreach (var currentRelation in relationsWithIntermediateEvents)
            {
                if (!relationIDs.Contains(currentRelation.Relationship.ID))
                {
                    distinctRelations.Add(currentRelation);
                    relationIDs.Add(currentRelation.Relationship.ID);
                }
            }

            Dictionary<long, EventBasedResultsPerSearchedObjects> objectToEventPerObjMapping = new Dictionary<long, EventBasedResultsPerSearchedObjects>();

            foreach (var currentRelatioin in distinctRelations)
            {
                if (objectsToSearchAroundIDs.Contains(currentRelatioin.Source.ID))
                {
                    if (intermediateEventIDs.Contains(currentRelatioin.Target.ID))
                    {
                        Entities.SearchAroundResult.EventBasedResultsPerSearchedObjects eventPerObject = null;
                        if (objectToEventPerObjMapping.ContainsKey(currentRelatioin.Source.ID))
                        {
                            eventPerObject = objectToEventPerObjMapping[currentRelatioin.Source.ID];
                            RelationshipBasedKWLink secondRelation = GetAnotherSideOfEvent(currentRelatioin, currentRelatioin.Target.ID, distinctRelations);
                            long targetId = (secondRelation.Source.ID == currentRelatioin.Target.ID) ? secondRelation.Target.ID : secondRelation.Source.ID;

                            if (targetIDs.Contains(targetId))
                            {
                                EventBasedLoadedTargetResult eventBasedLoadedTargetResult = eventPerObject.LoadedResults.Where(t => t.TargetObject.ID == targetId).First();
                                eventBasedLoadedTargetResult.InnerRelationshipIDs.Add(new EventBasedResultInnerRelationships()
                                {
                                    FirstRelationshipID = currentRelatioin.Relationship.ID,
                                    SecondRelationshipID = secondRelation.Relationship.ID
                                });
                            }
                            else
                            {
                                if (eventPerObject.LoadedResults.Count < LoadingDefaultBatchSize)
                                {
                                    EventBasedLoadedTargetResult eventBasedLoadedTargetResult = new EventBasedLoadedTargetResult()
                                    {
                                        InnerRelationshipIDs = new List<EventBasedResultInnerRelationships>()
                                                {
                                                    new EventBasedResultInnerRelationships()
                                                    {
                                                        FirstRelationshipID = currentRelatioin.Relationship.ID,
                                                        SecondRelationshipID = secondRelation.Relationship.ID
                                                    }
                                                },
                                        TargetObject = await DataAccessManager.ObjectManager.GetObjectByIdAsync(targetId)
                                    };
                                    eventPerObject.LoadedResults.Add(eventBasedLoadedTargetResult);
                                    targetIDs.Add(targetId);
                                }
                                else
                                {
                                    EventBasedNotLoadedResult eventBasedNotLoadedResult = new EventBasedNotLoadedResult()
                                    {
                                        InnerRelationships = new List<EventBasedResultInnerRelationships>()
                                                {
                                                    new EventBasedResultInnerRelationships()
                                                    {
                                                        FirstRelationshipID = currentRelatioin.Relationship.ID,
                                                        SecondRelationshipID = secondRelation.Relationship.ID
                                                    }
                                                },
                                        TargetObjectID = targetId
                                    };
                                    eventPerObject.NotLoadedResults.Add(eventBasedNotLoadedResult);
                                    targetIDs.Add(targetId);
                                }

                            }
                        }
                        else
                        {
                            eventPerObject = new Entities.SearchAroundResult.EventBasedResultsPerSearchedObjects();
                            eventPerObject.SearchedObject = await DataAccessManager.ObjectManager.GetObjectByIdAsync(currentRelatioin.Source.ID);

                            RelationshipBasedKWLink secondRelation = GetAnotherSideOfEvent(currentRelatioin, currentRelatioin.Target.ID, distinctRelations);
                            long targetId = (secondRelation.Source.ID == currentRelatioin.Target.ID) ? secondRelation.Target.ID : secondRelation.Source.ID;

                            EventBasedLoadedTargetResult eventBasedLoadedTargetResult = new EventBasedLoadedTargetResult()
                            {
                                InnerRelationshipIDs = new List<EventBasedResultInnerRelationships>()
                                                {
                                                    new EventBasedResultInnerRelationships()
                                                    {
                                                        FirstRelationshipID = currentRelatioin.Relationship.ID,
                                                        SecondRelationshipID = secondRelation.Relationship.ID
                                                    }
                                                },
                                TargetObject = await DataAccessManager.ObjectManager.GetObjectByIdAsync(targetId)
                            };

                            eventPerObject.LoadedResults.Add(eventBasedLoadedTargetResult);
                            targetIDs.Add(targetId);
                            objectToEventPerObjMapping.Add(eventPerObject.SearchedObject.ID, eventPerObject);
                        }
                    }
                }
                else if (objectsToSearchAroundIDs.Contains(currentRelatioin.Target.ID))
                {
                    if (intermediateEventIDs.Contains(currentRelatioin.Source.ID))
                    {
                        Entities.SearchAroundResult.EventBasedResultsPerSearchedObjects eventPerObject = null;
                        if (objectToEventPerObjMapping.ContainsKey(currentRelatioin.Target.ID))
                        {
                            eventPerObject = objectToEventPerObjMapping[currentRelatioin.Target.ID];
                            RelationshipBasedKWLink secondRelation = GetAnotherSideOfEvent(currentRelatioin, currentRelatioin.Source.ID, distinctRelations);
                            long targetId = (secondRelation.Source.ID == currentRelatioin.Source.ID) ? secondRelation.Target.ID : secondRelation.Source.ID;

                            if (targetIDs.Contains(targetId))
                            {
                                EventBasedLoadedTargetResult eventBasedLoadedTargetResult = eventPerObject.LoadedResults.Where(t => t.TargetObject.ID == targetId).First();
                                eventBasedLoadedTargetResult.InnerRelationshipIDs.Add(new EventBasedResultInnerRelationships()
                                {
                                    FirstRelationshipID = currentRelatioin.Relationship.ID,
                                    SecondRelationshipID = secondRelation.Relationship.ID
                                });
                            }
                            else
                            {
                                EventBasedLoadedTargetResult eventBasedLoadedTargetResult = new EventBasedLoadedTargetResult()
                                {
                                    InnerRelationshipIDs = new List<EventBasedResultInnerRelationships>()
                                                {
                                                    new EventBasedResultInnerRelationships()
                                                    {
                                                        FirstRelationshipID = currentRelatioin.Relationship.ID,
                                                        SecondRelationshipID = secondRelation.Relationship.ID
                                                    }
                                                },
                                    TargetObject = await DataAccessManager.ObjectManager.GetObjectByIdAsync(targetId)
                                };
                                eventPerObject.LoadedResults.Add(eventBasedLoadedTargetResult);
                                targetIDs.Add(targetId);
                            }
                        }
                        else
                        {
                            eventPerObject = new Entities.SearchAroundResult.EventBasedResultsPerSearchedObjects();
                            eventPerObject.LoadedResults = new List<EventBasedLoadedTargetResult>();
                            eventPerObject.SearchedObject = await DataAccessManager.ObjectManager.GetObjectByIdAsync(currentRelatioin.Target.ID);

                            RelationshipBasedKWLink secondRelation = GetAnotherSideOfEvent(currentRelatioin, currentRelatioin.Source.ID, distinctRelations);
                            long targetId = (secondRelation.Source.ID == currentRelatioin.Source.ID) ? secondRelation.Target.ID : secondRelation.Source.ID;                            

                            if (eventPerObject.LoadedResults.Count < LoadingDefaultBatchSize)
                            {
                                EventBasedLoadedTargetResult eventBasedLoadedTargetResult = new EventBasedLoadedTargetResult()
                                {
                                    InnerRelationshipIDs = new List<EventBasedResultInnerRelationships>()
                                                {
                                                    new EventBasedResultInnerRelationships()
                                                    {
                                                        FirstRelationshipID = currentRelatioin.Relationship.ID,
                                                        SecondRelationshipID = secondRelation.Relationship.ID
                                                    }
                                                },
                                    TargetObject = await DataAccessManager.ObjectManager.GetObjectByIdAsync(targetId)
                                };
                                eventPerObject.LoadedResults.Add(eventBasedLoadedTargetResult);
                            }
                            else
                            {
                                EventBasedNotLoadedResult eventBasedNotLoadedResult = new EventBasedNotLoadedResult()
                                {
                                    InnerRelationships = new List<EventBasedResultInnerRelationships>()
                                                {
                                                    new EventBasedResultInnerRelationships()
                                                    {
                                                        FirstRelationshipID = currentRelatioin.Relationship.ID,
                                                        SecondRelationshipID = secondRelation.Relationship.ID
                                                    }
                                                },
                                    TargetObjectID = targetId
                                };
                                eventPerObject.NotLoadedResults.Add(eventBasedNotLoadedResult);
                            }
                            
                            targetIDs.Add(targetId);
                            objectToEventPerObjMapping.Add(eventPerObject.SearchedObject.ID, eventPerObject);
                        }
                    }
                }                
            }

            EventBasedResult result = new EventBasedResult();
            result.Results = new List<EventBasedResultsPerSearchedObjects>();

            foreach (var currentObject in objectToEventPerObjMapping.Keys)
            {
                EventBasedResultsPerSearchedObjects resultPerObj = new EventBasedResultsPerSearchedObjects()
                {
                    SearchedObject = objectToEventPerObjMapping[currentObject].SearchedObject,
                    LoadedResults = objectToEventPerObjMapping[currentObject].LoadedResults,
                    NotLoadedResults = objectToEventPerObjMapping[currentObject].NotLoadedResults
                };
                result.Results.Add(resultPerObj);
            }

            int relationTargetCounts = 0;
            foreach (var currentResult in result.Results)
            {
                relationTargetCounts += currentResult.LoadedResults.Count;
                relationTargetCounts += currentResult.NotLoadedResults.Count;
            }

            if (relationTargetCounts > TotalResultsThreshould + 1)
            {
                result.IsResultsCountMoreThanThreshold = true;
            }

            return result;
        }

        private static RelationshipBasedKWLink GetAnotherSideOfEvent(RelationshipBasedKWLink firestRelation, long eventId, List<RelationshipBasedKWLink> distinctRelations)
        {
            long searchedObjectId = (firestRelation.Source.ID == eventId) ? firestRelation.Target.ID : firestRelation.Source.ID;
            RelationshipBasedKWLink secondRelation = null;

            foreach (var item in distinctRelations)
            {
                if ((item.Source.ID == eventId || item.Target.ID == eventId) &&
                    !((item.Source.ID == searchedObjectId) || (item.Target.ID == searchedObjectId)))
                {
                    secondRelation = item;
                    break;
                }
            }
            return secondRelation;
        }

        private static List<KWObject> GetEventObjectFromRelationshipBasedKWLink(
            IEnumerable<RelationshipBasedKWLink> relatedEventForObjectToSearchAround, IEnumerable<KWObject> searchedObjects)
        {
            List<KWObject> result = new List<KWObject>();
            foreach (var currentRelation in relatedEventForObjectToSearchAround)
            {
                if (searchedObjects.Contains(currentRelation.Source))
                {
                    if (!result.Contains(currentRelation.Target))
                    {
                        result.Add(currentRelation.Target);
                    }
                }
                else if (searchedObjects.Contains(currentRelation.Target))
                {
                    if (!result.Contains(currentRelation.Source))
                    {
                        result.Add(currentRelation.Source);
                    }                    
                }
            }
            return result;
        }


        //////////////////////// Browser Events ////////////////////////////////////////////////////
        public async static Task<RelationshipBasedResult> GetRelatedEvents(IEnumerable<KWObject> objectsToSearchAround)
        {
            if (objectsToSearchAround == null)
                throw new ArgumentNullException(nameof(objectsToSearchAround));

            RelationshipBasedResult result = new RelationshipBasedResult();

            IEnumerable<KWObject> publishedSearchedObjects = objectsToSearchAround.Where(o => !ObjectManager.IsUnpublishedObject(o));

            RelationshipBasedResult unpublishFirstRalationOfEventResult = await GetUnpublishFirstRalationOfEventResult(objectsToSearchAround);

            RelationshipBasedResult wsPublishedRelationshipBasedResult = null;

            if (publishedSearchedObjects.Any())
            {
                wsPublishedRelationshipBasedResult = await GetPublishFirstRalationOfEventResult(publishedSearchedObjects);
            }

            result = MergePublishedAndUnpublishedRelationshipResult(wsPublishedRelationshipBasedResult, unpublishFirstRalationOfEventResult);

            //// TODO: یکی کردن درخواست جستجوی پیرامونی به جای تودرتو بودن آن برای اشیاء ادغام شده
            //IEnumerable<KWObject> objectsWhereLocallyResolvedToSearchedObjects
            //    = searchedObjects.SelectMany(o => ObjectManager.GetObjectWhereLocallyResolvedToObject(o.ID));
            //if (objectsWhereLocallyResolvedToSearchedObjects.Any())
            //{
            //    RelationshipBasedResult relationshipBasedResultForLocallyResolvedObjs =
            //        await GetRelatedEntities(objectsWhereLocallyResolvedToSearchedObjects, loadNResults, totalResultsThreshold);
            //}

            RelationshipBasedResult relationshipBasedResultWithLoadedResults = await LoadRelationshipBasedSearchAroundTargets(result, LoadingDefaultBatchSize);

            return relationshipBasedResultWithLoadedResults;
        }

        private async static Task<RelationshipBasedResult> GetPublishFirstRalationOfEventResult(IEnumerable<KWObject> publishedSearchedObjects)
        {
            DispatchSAResult.RelationshipBasedResult remotePublishedRelationshipBasedResult = await FindRelatedPublishedEventsAsync(publishedSearchedObjects);

            EntityConvertors.SearchAroundEntitiesConvertor searchAroundEntitiesConvertor = new EntityConvertors.SearchAroundEntitiesConvertor();
            return await searchAroundEntitiesConvertor.ConvertRemoteRalationResultToWSRalationResult(remotePublishedRelationshipBasedResult);
        }

        private async static Task<RelationshipBasedResult> GetUnpublishFirstRalationOfEventResult(IEnumerable<KWObject> objectsToSearchAround)
        {
            Dictionary<long, List<RelationshipBasedKWLink>> objectToRelationMapping = new Dictionary<long, List<RelationshipBasedKWLink>>();
            IEnumerable<LinkManager.CachedRelationshipMetadata> unpublishRelationships = LinkManager.GetUnpublishedRelationships();
            HashSet<long> searchedObjectIDsHashSet = new HashSet<long>(objectsToSearchAround.Select(o => o.ID));

            foreach (var currentUnpublishRelation in unpublishRelationships)
            {
                if (searchedObjectIDsHashSet.Contains(currentUnpublishRelation.RelationshipSourceId) &&
                    System.GetOntology().IsEvent((await ObjectManager.GetObjectByIdAsync(currentUnpublishRelation.RelationshipTargetId)).TypeURI))
                {
                    if (objectToRelationMapping.ContainsKey(currentUnpublishRelation.RelationshipSourceId))
                    {
                        objectToRelationMapping[currentUnpublishRelation.RelationshipSourceId].Add(await LinkManager.GenerateRelationshipBasedLinkAsync(currentUnpublishRelation));
                    }
                    else
                    {
                        objectToRelationMapping.Add(currentUnpublishRelation.RelationshipSourceId, new List<RelationshipBasedKWLink>());
                        objectToRelationMapping[currentUnpublishRelation.RelationshipSourceId].Add(await LinkManager.GenerateRelationshipBasedLinkAsync(currentUnpublishRelation));
                    }
                }
                if (searchedObjectIDsHashSet.Contains(currentUnpublishRelation.RelationshipTargetId) &&
                   System.GetOntology().IsEvent(((await ObjectManager.GetObjectByIdAsync(currentUnpublishRelation.RelationshipSourceId)).TypeURI)))
                {
                    if (objectToRelationMapping.ContainsKey(currentUnpublishRelation.RelationshipTargetId))
                    {
                        objectToRelationMapping[currentUnpublishRelation.RelationshipTargetId].Add(await LinkManager.GenerateRelationshipBasedLinkAsync(currentUnpublishRelation));
                    }
                    else
                    {
                        objectToRelationMapping.Add(currentUnpublishRelation.RelationshipTargetId, new List<RelationshipBasedKWLink>());
                        objectToRelationMapping[currentUnpublishRelation.RelationshipTargetId].Add(await LinkManager.GenerateRelationshipBasedLinkAsync(currentUnpublishRelation));
                    }
                }
            }


            RelationshipBasedResult ralationshipBasedResult = new RelationshipBasedResult();
            List<RelationshipBasedResultsPerSearchedObjects> relationshipResultPerObjList = new List<RelationshipBasedResultsPerSearchedObjects>();
            HashSet<long> targetObjectIDsHashSet = new HashSet<long>();

            foreach (var currentSearchedObj in objectToRelationMapping.Keys)
            {
                RelationshipBasedResultsPerSearchedObjects relationshipResultPerObj = new RelationshipBasedResultsPerSearchedObjects();
                relationshipResultPerObj.LoadedResults = new List<RelationshipBasedLoadedTargetResult>();
                relationshipResultPerObj.NotLoadedResults = new List<RelationshipBasedNotLoadedResult>();

                relationshipResultPerObj.SearchedObject = await ObjectManager.GetObjectByIdAsync(currentSearchedObj);
                foreach (var currentRelation in objectToRelationMapping[currentSearchedObj])
                {
                    if (!targetObjectIDsHashSet.Contains(currentRelation.Target.ID))
                    {
                        if (relationshipResultPerObj.LoadedResults.Count < LoadingDefaultBatchSize)
                        {
                            RelationshipBasedLoadedTargetResult relationBasedLoadedTargetResult = new RelationshipBasedLoadedTargetResult()
                            {
                                RelationshipIDs = new List<long>() { currentRelation.Relationship.ID },
                                TargetObject = currentRelation.Target
                            };
                            relationshipResultPerObj.LoadedResults.Add(relationBasedLoadedTargetResult);
                            targetObjectIDsHashSet.Add(currentRelation.Target.ID);
                        }
                        else
                        {
                            RelationshipBasedNotLoadedResult relationshipBasedNotLoadedResult = new RelationshipBasedNotLoadedResult()
                            {
                                RelationshipIDs = new List<long>() { currentRelation.Relationship.ID },
                                TargetObjectID = currentRelation.Target.ID
                            };
                            relationshipResultPerObj.NotLoadedResults.Add(relationshipBasedNotLoadedResult);
                            targetObjectIDsHashSet.Add(currentRelation.Target.ID);
                        }

                    }
                    else
                    {
                        if (relationshipResultPerObj.LoadedResults.Count < LoadingDefaultBatchSize)
                        {
                            RelationshipBasedLoadedTargetResult relationBasedLoadedTargetResult = relationshipResultPerObj.LoadedResults.Where(r => r.TargetObject.ID == currentRelation.Target.ID).FirstOrDefault();
                            relationBasedLoadedTargetResult.RelationshipIDs.Add(currentRelation.Relationship.ID);
                        }
                        else
                        {
                            RelationshipBasedNotLoadedResult relationshipBasedNotLoadedResult = relationshipResultPerObj.NotLoadedResults.Where(r => r.TargetObjectID == currentRelation.Target.ID).FirstOrDefault();
                            relationshipBasedNotLoadedResult.RelationshipIDs.Add(currentRelation.Relationship.ID);
                        }
                    }
                }
                relationshipResultPerObjList.Add(relationshipResultPerObj);
            }
            ralationshipBasedResult.Results = relationshipResultPerObjList;

            int relationTargetCounts = 0;
            foreach (var currentResult in ralationshipBasedResult.Results)
            {
                relationTargetCounts += currentResult.LoadedResults.Count;
                relationTargetCounts += currentResult.NotLoadedResults.Count;
            }

            if (relationTargetCounts > TotalResultsThreshould + 1)
            {
                ralationshipBasedResult.IsResultsCountMoreThanThreshold = true;
            }

            return ralationshipBasedResult;
        }

        /////////////////////// Ducuments //////////////////////////////////////////////////////
        public static async Task<RelationshipBasedResult> GetRelatedDocuments(IEnumerable<KWObject> searchedObjects)
        {
            if (searchedObjects == null)
                throw new ArgumentNullException(nameof(searchedObjects));

            RelationshipBasedResult result = new RelationshipBasedResult();

            IEnumerable<KWObject> publishedSearchedObjects = searchedObjects.Where(o => !ObjectManager.IsUnpublishedObject(o));

            RelationshipBasedResult unpublishDocumentResult = await GetUnpublishDocumentResult(searchedObjects);

            RelationshipBasedResult wsPublishedDocumentResult = null;

            if (publishedSearchedObjects.Any())
            {
                wsPublishedDocumentResult = await GetPublishDocumentResult(publishedSearchedObjects);
            }

            result = MergePublishedAndUnpublishedRelationshipResult(wsPublishedDocumentResult, unpublishDocumentResult);

            //// TODO: یکی کردن درخواست جستجوی پیرامونی به جای تودرتو بودن آن برای اشیاء ادغام شده
            //IEnumerable<KWObject> objectsWhereLocallyResolvedToSearchedObjects
            //    = searchedObjects.SelectMany(o => ObjectManager.GetObjectWhereLocallyResolvedToObject(o.ID));
            //if (objectsWhereLocallyResolvedToSearchedObjects.Any())
            //{
            //    RelationshipBasedResult relationshipBasedResultForLocallyResolvedObjs =
            //        await GetRelatedEntities(objectsWhereLocallyResolvedToSearchedObjects, loadNResults, totalResultsThreshold);
            //}

            RelationshipBasedResult relationshipBasedResultWithLoadedResults = await LoadRelationshipBasedSearchAroundTargets(result, LoadingDefaultBatchSize);

            return relationshipBasedResultWithLoadedResults;
        }

        private async static Task<RelationshipBasedResult> GetPublishDocumentResult(IEnumerable<KWObject> publishedSearchedObjects)
        {
            DispatchSAResult.RelationshipBasedResult remotePublishedRelationshipBasedResult = await FindRelatedPublishedDocumentsAsync(publishedSearchedObjects);

            EntityConvertors.SearchAroundEntitiesConvertor searchAroundEntitiesConvertor = new EntityConvertors.SearchAroundEntitiesConvertor();
            return await searchAroundEntitiesConvertor.ConvertRemoteRalationResultToWSRalationResult(remotePublishedRelationshipBasedResult);
        }

        private async static Task<RelationshipBasedResult> GetUnpublishDocumentResult(IEnumerable<KWObject> objectsToSearchAround)
        {
            Dictionary<long, List<RelationshipBasedKWLink>> objectToRelationMapping = new Dictionary<long, List<RelationshipBasedKWLink>>();
            IEnumerable<LinkManager.CachedRelationshipMetadata> unpublishRelationships = LinkManager.GetUnpublishedRelationships();
            HashSet<long> searchedObjectIDsHashSet = new HashSet<long>(objectsToSearchAround.Select(o => o.ID));

            foreach (var currentUnpublishRelation in unpublishRelationships)
            {
                if (searchedObjectIDsHashSet.Contains(currentUnpublishRelation.RelationshipSourceId) &&
                    System.GetOntology().IsDocument(((await ObjectManager.GetObjectByIdAsync(currentUnpublishRelation.RelationshipTargetId)).TypeURI)))
                {
                    if (objectToRelationMapping.ContainsKey(currentUnpublishRelation.RelationshipSourceId))
                    {
                        objectToRelationMapping[currentUnpublishRelation.RelationshipSourceId].Add(await LinkManager.GenerateRelationshipBasedLinkAsync(currentUnpublishRelation));
                    }
                    else
                    {
                        objectToRelationMapping.Add(currentUnpublishRelation.RelationshipSourceId, new List<RelationshipBasedKWLink>());
                        objectToRelationMapping[currentUnpublishRelation.RelationshipSourceId].Add(await LinkManager.GenerateRelationshipBasedLinkAsync(currentUnpublishRelation));
                    }
                }
                if (searchedObjectIDsHashSet.Contains(currentUnpublishRelation.RelationshipTargetId) &&
                   System.GetOntology().IsDocument(((await ObjectManager.GetObjectByIdAsync(currentUnpublishRelation.RelationshipSourceId)).TypeURI)))
                {
                    if (objectToRelationMapping.ContainsKey(currentUnpublishRelation.RelationshipTargetId))
                    {
                        objectToRelationMapping[currentUnpublishRelation.RelationshipTargetId].Add(await LinkManager.GenerateRelationshipBasedLinkAsync(currentUnpublishRelation));
                    }
                    else
                    {
                        objectToRelationMapping.Add(currentUnpublishRelation.RelationshipTargetId, new List<RelationshipBasedKWLink>());
                        objectToRelationMapping[currentUnpublishRelation.RelationshipTargetId].Add(await LinkManager.GenerateRelationshipBasedLinkAsync(currentUnpublishRelation));
                    }
                }
            }


            RelationshipBasedResult ralationshipBasedResult = new RelationshipBasedResult();
            List<RelationshipBasedResultsPerSearchedObjects> relationshipResultPerObjList = new List<RelationshipBasedResultsPerSearchedObjects>();
            HashSet<long> targetObjectIDsHashSet = new HashSet<long>();

            foreach (var currentSearchedObj in objectToRelationMapping.Keys)
            {
                RelationshipBasedResultsPerSearchedObjects relationshipResultPerObj = new RelationshipBasedResultsPerSearchedObjects();
                relationshipResultPerObj.LoadedResults = new List<RelationshipBasedLoadedTargetResult>();
                relationshipResultPerObj.NotLoadedResults = new List<RelationshipBasedNotLoadedResult>();

                List<RelationshipBasedLoadedTargetResult> tempLoadedResults = new List<RelationshipBasedLoadedTargetResult>();

                relationshipResultPerObj.SearchedObject = await ObjectManager.GetObjectByIdAsync(currentSearchedObj);
                foreach (var currentRelation in objectToRelationMapping[currentSearchedObj])
                {
                    if (searchedObjectIDsHashSet.Contains(currentRelation.Source.ID))
                    {
                        if (!targetObjectIDsHashSet.Contains(currentRelation.Target.ID))
                        {
                            RelationshipBasedLoadedTargetResult relationBasedLoadedTargetResult = new RelationshipBasedLoadedTargetResult()
                            {
                                RelationshipIDs = new List<long>() { currentRelation.Relationship.ID },
                                TargetObject = currentRelation.Target
                            };
                            tempLoadedResults.Add(relationBasedLoadedTargetResult);
                            targetObjectIDsHashSet.Add(currentRelation.Target.ID);
                        }
                        else
                        {
                            RelationshipBasedLoadedTargetResult relationBasedLoadedTargetResult = tempLoadedResults.Where(r => r.TargetObject.ID == currentRelation.Target.ID).FirstOrDefault();
                            relationBasedLoadedTargetResult.RelationshipIDs.Add(currentRelation.Relationship.ID);
                        }
                    }
                    else if (searchedObjectIDsHashSet.Contains(currentRelation.Target.ID))
                    {
                        if (!targetObjectIDsHashSet.Contains(currentRelation.Source.ID))
                        {
                            RelationshipBasedLoadedTargetResult relationBasedLoadedTargetResult = new RelationshipBasedLoadedTargetResult()
                            {
                                RelationshipIDs = new List<long>() { currentRelation.Relationship.ID },
                                TargetObject = currentRelation.Source
                            };
                            tempLoadedResults.Add(relationBasedLoadedTargetResult);
                            targetObjectIDsHashSet.Add(currentRelation.Source.ID);
                        }
                        else
                        {
                            RelationshipBasedLoadedTargetResult relationBasedLoadedTargetResult = tempLoadedResults.Where(r => r.TargetObject.ID == currentRelation.Source.ID).FirstOrDefault();
                            relationBasedLoadedTargetResult.RelationshipIDs.Add(currentRelation.Relationship.ID);
                        }
                    }

                }
                relationshipResultPerObj.LoadedResults = tempLoadedResults;
                relationshipResultPerObjList.Add(relationshipResultPerObj);
            }
            ralationshipBasedResult.Results = relationshipResultPerObjList;

            int relationTargetCounts = 0;
            foreach (var currentResult in ralationshipBasedResult.Results)
            {
                if (currentResult.LoadedResults != null)
                {
                    relationTargetCounts += currentResult.LoadedResults.Count();
                }
            }

            if (relationTargetCounts > TotalResultsThreshould + 1)
            {
                ralationshipBasedResult.IsResultsCountMoreThanThreshold = true;
            }

            return ralationshipBasedResult;
        }

        private static async Task<IEnumerable<RelationshipBasedKWLink>> GetUnpublishRelationshipOfDocument(IEnumerable<KWObject> objectsToSearchAround)
        {
            IEnumerable<LinkManager.CachedRelationshipMetadata> unpublishedRelationships = LinkManager.GetUnpublishedRelationships();
            HashSet<long> searchedObjectIDsHashSet = new HashSet<long>(objectsToSearchAround.Select(o => o.ID));
            List<RelationshipBasedKWLink> relatedDocuments = new List<RelationshipBasedKWLink>();
            foreach (var item in unpublishedRelationships)
            {
                if (searchedObjectIDsHashSet.Contains(item.RelationshipSourceId) &&
                    System.GetOntology().IsDocument((await ObjectManager.GetObjectByIdAsync(item.RelationshipTargetId)).TypeURI))
                {
                    relatedDocuments.Add(await LinkManager.GenerateRelationshipBasedLinkAsync(item));
                }
                if (searchedObjectIDsHashSet.Contains(item.RelationshipTargetId) &&
                   System.GetOntology().IsDocument(((await ObjectManager.GetObjectByIdAsync(item.RelationshipSourceId)).TypeURI)))
                {
                    relatedDocuments.Add(await LinkManager.GenerateRelationshipBasedLinkAsync(item));
                }
            }
            return relatedDocuments;
        }

        ////////////////////// Properties //////////////////////////////////////////////////////        
        public static async Task<PropertyBasedResultMeatdata> GetObjectsWithSamePropertyAsync(List<KWProperty> searchBaseProperties, int loadNResult, int totalResultThreshold)
        {
            if (searchBaseProperties == null)
                throw new ArgumentNullException("searchBaseProperties");

            PropertyBasedResultMeatdata totalResult = new PropertyBasedResultMeatdata()
            {
                IsResultsCountMoreThanThreshold = false,
                ResultsPerSearchedPropertyID = new Dictionary<long, PropertyBasedResultMetadatasPerSearchedProperty>(searchBaseProperties.Count)

            };
            foreach (KWProperty baseProperty in searchBaseProperties)
            {
                IEnumerable<KWProperty> matchedCachedProperties =
                    PropertyManager.GetCachedPropertiesWithSpecifiedTypeAndValue(baseProperty.TypeURI, baseProperty.Value);
                totalResult.ResultsPerSearchedPropertyID.Add(baseProperty.ID, new PropertyBasedResultMetadatasPerSearchedProperty()
                {
                    SearchedProperty = baseProperty,
                    LoadedResults = new List<KWProperty>(matchedCachedProperties)
                });
            }

            int minLoadedPropertiesCountPerSearchedProperties = totalResult.ResultsPerSearchedPropertyID.Min(kv => kv.Value.LoadedResults.Count);
            if (minLoadedPropertiesCountPerSearchedProperties >= TotalResultsThreshould)
            {
                totalResult.IsResultsCountMoreThanThreshold = true;
                return totalResult;
            }

            KProperty[] baseKProperties = (await PropertyManager.GetKPropertiesFromKWProperties(searchBaseProperties)).ToArray();
            DispatchSAResult.PropertyBasedResult remoteResults = await FindPropertiesSameWithAsync
                (baseKProperties, loadNResult - minLoadedPropertiesCountPerSearchedProperties, TotalResultsThreshould - minLoadedPropertiesCountPerSearchedProperties);
            totalResult.IsResultsCountMoreThanThreshold = remoteResults.IsResultsCountMoreThanThreshold;
            foreach (DispatchSAResult.PropertyBasedResultsPerSearchedProperty remoteResultPerSearchProp in remoteResults.Results)
            {
                KWProperty[] matchedNotCachedProperties
                    = await PropertyManager.GetPropertyFromRetrievedDataArrayAsync
                        (remoteResultPerSearchProp.LoadedResults
                            .Where(kProp => !PropertyManager.IsPropertyCached(kProp.Id))
                            .ToArray());
                totalResult.ResultsPerSearchedPropertyID[remoteResultPerSearchProp.SearchedProperty.Id]
                    .LoadedResults.AddRange(matchedNotCachedProperties);
                totalResult.ResultsPerSearchedPropertyID[remoteResultPerSearchProp.SearchedProperty.Id]
                    .NotLoadedResultPropertyIDs = new List<long>(remoteResultPerSearchProp.NotLoadedResultPropertyIDs);

            }
            return totalResult;
        }

        ////////////////////// Custom Search Around //////////////////////////////////////////////////////
        public static async Task<CustomSearchAroundResult> PerformCustomSearchAround(Dictionary<string, long[]> searchedObjects,
            CustomSearchAroundCriteria criteria)
        {
            if (searchedObjects == null)
                throw new ArgumentNullException(nameof(searchedObjects));

            if (criteria == null)
                throw new ArgumentNullException(nameof(criteria));

            CustomSearchAroundCriteriaSerializer serializer = new CustomSearchAroundCriteriaSerializer();
            MemoryStream streamWriter = new MemoryStream();
            serializer.Serialize(streamWriter, criteria);
            StreamUtility streamUtil = new StreamUtility();
            byte[] criteriaByteArray = streamUtil.ReadStreamAsBytesArray(streamWriter);

            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            CustomSearchAroundResult customSearchAroundResult = null;
            try
            {
                customSearchAroundResult = await sc.PerformCustomSearchAroundAsync(searchedObjects, criteriaByteArray, TotalResultsThreshould);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
            return customSearchAroundResult;
        }

        private static IEnumerable<RelationshipBasedKWLink> GetRelatedRelationships(KWObject currentEvent, IEnumerable<RelationshipBasedKWLink> relatedEventsForSearchedObjects)
        {
            return relatedEventsForSearchedObjects.Where(r => (r.Source == currentEvent || r.Target == currentEvent));
        }

        private static IEnumerable<KWObject> GetEntityObjectFromRelationshipBasedKWLink(
            IEnumerable<RelationshipBasedKWLink> relatedEventForObjectToSearchAround, IEnumerable<KWObject> searchedObjects)
        {
            List<KWObject> result = new List<KWObject>();
            foreach (var currentRelation in relatedEventForObjectToSearchAround)
            {
                if (searchedObjects.Contains(currentRelation.Source))
                {
                    result.Add(currentRelation.Source);
                }
                else if (searchedObjects.Contains(currentRelation.Target))
                {
                    result.Add(currentRelation.Target);
                }
            }
            return result;
        }

        #endregion
    }
}
