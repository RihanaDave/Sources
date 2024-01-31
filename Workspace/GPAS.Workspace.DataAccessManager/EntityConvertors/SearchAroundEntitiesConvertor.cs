using GPAS.SearchAround;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Entities.SearchAroundResult;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.Workspace.DataAccessManager.EntityConvertors
{
    public class SearchAroundEntitiesConvertor
    {
        private List<Entities.SearchAroundResult.EventBasedNotLoadedResult> ConvertRemoteEventBasedNotLoadedResultToWEventBasedNotLoadedResult(List<Dispatch.Entities.Concepts.SearchAroundResult.EventBasedNotLoadedResult> eventBasedNotLoadedResults)
        {
            List<Entities.SearchAroundResult.EventBasedNotLoadedResult> eventBasedNotLoadedResultsList = new List<Entities.SearchAroundResult.EventBasedNotLoadedResult>();

            Dictionary<long, List<EventBasedResultInnerRelationships>> eventBasedResultInnerRelationshipsMapping = new Dictionary<long, List<EventBasedResultInnerRelationships>>();

            foreach (var eventBasedNotLoaded in eventBasedNotLoadedResults)
            {
                if (eventBasedResultInnerRelationshipsMapping.ContainsKey(eventBasedNotLoaded.TargetObjectID))
                {
                    eventBasedResultInnerRelationshipsMapping[eventBasedNotLoaded.TargetObjectID].Add(
                        new EventBasedResultInnerRelationships()
                        {
                            FirstRelationshipID = eventBasedNotLoaded.FirstRealationshipID,
                            SecondRelationshipID = eventBasedNotLoaded.SecondRealationshipID
                        }
                        );
                }
                else
                {
                    eventBasedResultInnerRelationshipsMapping[eventBasedNotLoaded.TargetObjectID] = new List<EventBasedResultInnerRelationships>();
                    eventBasedResultInnerRelationshipsMapping[eventBasedNotLoaded.TargetObjectID].Add(
                        new EventBasedResultInnerRelationships()
                        {
                            FirstRelationshipID = eventBasedNotLoaded.FirstRealationshipID,
                            SecondRelationshipID = eventBasedNotLoaded.SecondRealationshipID
                        }
                        );
                }
            }
            foreach (var eventBasedNotLoaded in eventBasedResultInnerRelationshipsMapping.Keys)
            {
                eventBasedNotLoadedResultsList.Add(
                    new Entities.SearchAroundResult.EventBasedNotLoadedResult()
                    {
                        TargetObjectID = eventBasedNotLoaded,
                        InnerRelationships = eventBasedResultInnerRelationshipsMapping[eventBasedNotLoaded]
                    }
                    );
            }
            return eventBasedNotLoadedResultsList;
        }

        private List<Entities.SearchAroundResult.RelationshipBasedNotLoadedResult> ConvertRemoteRelationshipBasedNotLoadedResultToWRelationshipBasedNotLoadedResult(List<Dispatch.Entities.Concepts.SearchAroundResult.RelationshipBasedNotLoadedResult> relationshipBasedNotLoadedResults)
        {
            List<Entities.SearchAroundResult.RelationshipBasedNotLoadedResult> relationshipBasedNotLoadedResultsList = new List<Entities.SearchAroundResult.RelationshipBasedNotLoadedResult>();
            Dictionary<long, List<long>> relationshipBasedNotLoadedMapping = new Dictionary<long, List<long>>();

            foreach (var relationshipBasedNotLoaded in relationshipBasedNotLoadedResults)
            {
                if (relationshipBasedNotLoadedMapping.ContainsKey(relationshipBasedNotLoaded.TargetObjectID))
                {
                    relationshipBasedNotLoadedMapping[relationshipBasedNotLoaded.TargetObjectID].Add(relationshipBasedNotLoaded.RelationshipID);
                }
                else
                {
                    relationshipBasedNotLoadedMapping[relationshipBasedNotLoaded.TargetObjectID] = new List<long>();
                    relationshipBasedNotLoadedMapping[relationshipBasedNotLoaded.TargetObjectID].Add(relationshipBasedNotLoaded.RelationshipID);
                }
            }

            foreach (var relationshipBasedNotLoaded in relationshipBasedNotLoadedResults)
            {
                relationshipBasedNotLoadedResultsList.Add(
                    new RelationshipBasedNotLoadedResult()
                    {
                        TargetObjectID = relationshipBasedNotLoaded.TargetObjectID,
                        RelationshipIDs = relationshipBasedNotLoadedMapping[relationshipBasedNotLoaded.TargetObjectID]
                    }
                    );
            }
            return relationshipBasedNotLoadedResultsList;
        }

        public async Task<KWCustomSearchAroundResult> KWCustomSearchAroundResultToKWCustomSearchAroundResult(CustomSearchAroundResult customSearchAroundResult)
        {
            return new KWCustomSearchAroundResult()
            {
                IsResultsCountMoreThanThreshold = customSearchAroundResult.IsResultsCountMoreThanThreshold,
                EventBasedResult = (await ConvertRemoteEventBasedResultsPerSearchedObjectsToWEventBasedResultsPerSearchedObjects(customSearchAroundResult.EventBaseKLink)),
                RalationshipBasedResult = (await ConvertRemoteRelationshipBasedResultsPerSearchedObjectsToWRelationshipBasedResultsPerSearchedObjects(customSearchAroundResult.Ralationships))
            };
        }

        private async Task<List<Entities.SearchAroundResult.EventBasedResultsPerSearchedObjects>> ConvertRemoteEventBasedResultsPerSearchedObjectsToWEventBasedResultsPerSearchedObjects(Dispatch.Entities.Concepts.SearchAroundResult.EventBasedResultsPerSearchedObjects[] eventBaseKLinks)
        {
            List<Entities.SearchAroundResult.EventBasedResultsPerSearchedObjects> eventBasedResultsPerSearchedObjects = new List<Entities.SearchAroundResult.EventBasedResultsPerSearchedObjects>();
            foreach (var eventBaseKLink in eventBaseKLinks)
            {
                eventBasedResultsPerSearchedObjects.Add(
                    new EventBasedResultsPerSearchedObjects()
                    {
                        LoadedResults = new List<EventBasedLoadedTargetResult>(),
                        NotLoadedResults = ConvertRemoteEventBasedNotLoadedResultToWEventBasedNotLoadedResult(eventBaseKLink.NotLoadedResults.ToList()),
                        SearchedObject = await ObjectManager.GetObjectByIdAsync(eventBaseKLink.SearchedObjectID)
                    }
                    );
            }

            return eventBasedResultsPerSearchedObjects;
        }

        private async Task<List<RelationshipBasedResultsPerSearchedObjects>> ConvertRemoteRelationshipBasedResultsPerSearchedObjectsToWRelationshipBasedResultsPerSearchedObjects(Dispatch.Entities.Concepts.SearchAroundResult.RelationshipBasedResultsPerSearchedObjects[] ralationships)
        {
            List<Entities.SearchAroundResult.RelationshipBasedResultsPerSearchedObjects> relationshipBasedResultsPerSearchedObjects = new List<Entities.SearchAroundResult.RelationshipBasedResultsPerSearchedObjects>();

            foreach (var ralationship in ralationships)
            {
                relationshipBasedResultsPerSearchedObjects.Add(
                    new RelationshipBasedResultsPerSearchedObjects()
                    {
                        LoadedResults = new List<RelationshipBasedLoadedTargetResult>(),
                        NotLoadedResults = ConvertRemoteRelationshipBasedNotLoadedResultToWRelationshipBasedNotLoadedResult(ralationship.NotLoadedResults.ToList()),
                        SearchedObject = await ObjectManager.GetObjectByIdAsync(ralationship.SearchedObjectID)
                    }
                    );

            }
            return relationshipBasedResultsPerSearchedObjects;
        }

        public async Task<RelationshipBasedResult> ConvertRemoteRalationResultToWSRalationResult(Dispatch.Entities.Concepts.SearchAroundResult.RelationshipBasedResult remoteRelation)
        {
            RelationshipBasedResult ralationshipBasedResult = new RelationshipBasedResult()
            {
                IsResultsCountMoreThanThreshold = remoteRelation.IsResultsCountMoreThanThreshold,
                Results = await ConvertRelationResultsPerObjects(remoteRelation.Results)
            };
            return ralationshipBasedResult;
        }

        internal async Task<EventBasedResult> ConvertRemoteEventResultToWSRalationResult(Dispatch.Entities.Concepts.SearchAroundResult.EventBasedResult remotePublishedEventResult)
        {
            EventBasedResult eventBasedResult = null;
            if (remotePublishedEventResult.Results == null)
            {
                eventBasedResult = new EventBasedResult()
                {
                    IsResultsCountMoreThanThreshold = remotePublishedEventResult.IsResultsCountMoreThanThreshold,
                    Results = new List<EventBasedResultsPerSearchedObjects>()
                };
            }
            else
            {
                eventBasedResult = new EventBasedResult()
                {
                    IsResultsCountMoreThanThreshold = remotePublishedEventResult.IsResultsCountMoreThanThreshold,
                    Results = await ConvertRemoteEventBasedResultsPerSearchedObjectsToWEventBasedResultsPerSearchedObjects(remotePublishedEventResult.Results)

                };
            }

            return eventBasedResult;
        }

        private async Task<List<RelationshipBasedResultsPerSearchedObjects>> ConvertRelationResultsPerObjects(Dispatch.Entities.Concepts.SearchAroundResult.RelationshipBasedResultsPerSearchedObjects[] resultsPerObjects)
        {
            List<RelationshipBasedResultsPerSearchedObjects> result = new List<RelationshipBasedResultsPerSearchedObjects>();
            await ObjectManager.GetObjectsListByIdAsync(resultsPerObjects.Select(o=>o.SearchedObjectID).ToArray());

                foreach (var currentResult in resultsPerObjects)
                {
                    result.Add(new RelationshipBasedResultsPerSearchedObjects()
                    {
                        LoadedResults = new List<RelationshipBasedLoadedTargetResult>(),
                        NotLoadedResults = ConvertNotLoadedResults(currentResult.NotLoadedResults),
                        SearchedObject = await ObjectManager.GetObjectByIdAsync(currentResult.SearchedObjectID)
                    });
                }
            return result;
        }

        private List<RelationshipBasedNotLoadedResult> ConvertNotLoadedResults(Dispatch.Entities.Concepts.SearchAroundResult.RelationshipBasedNotLoadedResult[] notLoadedResults)
        {
            List<RelationshipBasedNotLoadedResult> results = new List<RelationshipBasedNotLoadedResult>();
            HashSet<long> targetIDs = new HashSet<long>();

            foreach (var currentNotLoadedResult in notLoadedResults)
            {
                if (targetIDs.Contains(currentNotLoadedResult.TargetObjectID))
                {
                    RelationshipBasedNotLoadedResult savedRelation = results.Where(r => r.TargetObjectID == currentNotLoadedResult.TargetObjectID).FirstOrDefault();
                    savedRelation.RelationshipIDs.Add(currentNotLoadedResult.RelationshipID);
                }
                else
                {
                    results.Add(new RelationshipBasedNotLoadedResult()
                    {
                        RelationshipIDs = new List<long>() { currentNotLoadedResult.RelationshipID },
                        TargetObjectID = currentNotLoadedResult.TargetObjectID
                    });
                    targetIDs.Add(currentNotLoadedResult.TargetObjectID);
                }
            }
            return results;
        }
    }
}
