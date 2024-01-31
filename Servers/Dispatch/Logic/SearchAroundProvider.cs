using GPAS.AccessControl;
using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.Entities.Concepts.SearchAroundResult;
using GPAS.FilterSearch;
using GPAS.SearchAround;
using GPAS.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Horizon = GPAS.Dispatch.ServiceAccess.HorizonService;
//using Repository = GPAS.Dispatch.ServiceAccess.RepositoryService;
using Search = GPAS.Dispatch.ServiceAccess.SearchService;

namespace GPAS.Dispatch.Logic
{
    /// <summary>
    /// این کلاس جست و جوی پیرامونی را مدیریت می کند.
    /// </summary>
    public class SearchAroundProvider
    {
        private string CallerUserName = "";
        private static readonly long CustomSearchAroundStepResultLimit = long.Parse(ConfigurationManager.AppSettings["CustomSearchAroundStepResultLimit"]);
        public SearchAroundProvider(string callerUserName)
        {
            CallerUserName = callerUserName;
        }

        /// <summary>
        /// این تابع یک شناسه ی شیء دریافت می کند و موجودیتهایی که با آن رابطه دارند را پیدا می کند و لیستی از روابط بدست آمده را می دهد.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RelationshipBasedResult FindRelatedEntities(Dictionary<string, long[]> searchedVertices, int totalResultsThreshold)
        {
            AuthorizationParametters authorizationParametter
                = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

            if (authorizationParametter.permittedGroupNames.Count == 0 || authorizationParametter.readableClassifications.Count == 0)
            {
                return new RelationshipBasedResult();
            }
            RelationshipBasedResultsPerSearchedObjects[] relationships;
            Horizon.ServiceClient horizonProxy = null;
            try
            {
                horizonProxy = new Horizon.ServiceClient();
                relationships = horizonProxy.FindRelatedEntities(searchedVertices, totalResultsThreshold + 1, authorizationParametter);
            }
            finally
            {
                if (horizonProxy != null)
                    horizonProxy.Close();
            }

            bool isMoreThanThereshold = false;
            foreach (var relation in relationships)
            {
                if (relation.NotLoadedResults.Length > totalResultsThreshold)
                {
                    isMoreThanThereshold = true;
                }
            }
            RelationshipBasedResult resultRelationships = new RelationshipBasedResult()
            {
                IsResultsCountMoreThanThreshold = isMoreThanThereshold,
                Results = relationships
            };

            return resultRelationships;
        }
        /// <summary>
        /// این تابع یک شناسه ی شیء دریافت می کند و رخدادهایی که با آن رابطه دارند را پیدا می کند
        /// سپس تمام اشیایی که با این رخدادها در ارتباط هستند را پیدا می کند
        /// سپس لیستی از رابطه های بدست آمده از طریق این رخدادها را می دهد.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public EventBasedResult FindRelatedEntitiesApearedInEvents(Dictionary<string, long[]> searchedVertices, int totalResultsThreshold)
        {
            EventBasedResult result = new EventBasedResult();

            AuthorizationParametters authorizationParametter
                = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);
            if (authorizationParametter.permittedGroupNames.Count == 0 || authorizationParametter.readableClassifications.Count == 0)
            {
                return new EventBasedResult();
            }
            EventBasedResultsPerSearchedObjects[] eventBaseLinkComponents;
            Horizon.ServiceClient horizonServiceProxy = null;
            try
            {
                horizonServiceProxy = new Horizon.ServiceClient();
                eventBaseLinkComponents = horizonServiceProxy.FindRelatedEntitiesAppearedInEvents(searchedVertices, totalResultsThreshold + 1,
                    authorizationParametter);
            }
            finally
            {
                if (horizonServiceProxy != null)
                    horizonServiceProxy.Close();
            }
            bool isMoreThanThereshold = false;
            foreach (var eventBaseLink in eventBaseLinkComponents)
            {
                if (eventBaseLink.NotLoadedResults.Length > totalResultsThreshold)
                {
                    isMoreThanThereshold = true;
                }
            }
            return new EventBasedResult()
            {
                IsResultsCountMoreThanThreshold = isMoreThanThereshold,
                Results = eventBaseLinkComponents
            };


        }
        /// <summary>
        /// این تابع یک شناسه ی شیء دریافت می کند و مستنداتی که با آن رابطه دارند را پیدا می کند و لیستی از روابط بدست آمده را می دهد.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RelationshipBasedResult FindRelatedDocuments(Dictionary<string, long[]> searchedVertices, int totalResultsThreshold)
        {
            AuthorizationParametters authorizationParametter
                = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);
            if (authorizationParametter.permittedGroupNames.Count == 0 || authorizationParametter.readableClassifications.Count == 0)
            {
                return new RelationshipBasedResult();
            }
            RelationshipBasedResultsPerSearchedObjects[] relationships;
            Horizon.ServiceClient horizonProxy = null;
            try
            {
                horizonProxy = new Horizon.ServiceClient();
                relationships = horizonProxy.FindRelatedDocuments(searchedVertices, totalResultsThreshold + 1, authorizationParametter);
            }
            finally
            {
                if (horizonProxy != null)
                    horizonProxy.Close();
            }

            bool isMoreThanThereshold = false;
            foreach (var relation in relationships)
            {
                if (relation.NotLoadedResults.Length > totalResultsThreshold)
                {
                    isMoreThanThereshold = true;
                }
            }
            return new RelationshipBasedResult()
            {
                IsResultsCountMoreThanThreshold = isMoreThanThereshold,
                Results = relationships
            };
        }
        /// <summary>
        /// این تابع یک شناسه ی شیء دریافت می کند و موجودیتهایی که با آن رابطه دارند را پیدا می کند و لیستی از روابط بدست آمده را می دهد.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public RelationshipBasedResult FindRelatedEvents(Dictionary<string, long[]> searchedVertices, int totalResultsThreshold)
        {
            AuthorizationParametters authorizationParametter
                = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);
            if (authorizationParametter.permittedGroupNames.Count == 0 || authorizationParametter.readableClassifications.Count == 0)
            {
                return new RelationshipBasedResult();
            }
            RelationshipBasedResultsPerSearchedObjects[] relationships;
            Horizon.ServiceClient horizonProxy = null;
            try
            {
                horizonProxy = new Horizon.ServiceClient();
                relationships = horizonProxy.FindRelatedEvents(searchedVertices, totalResultsThreshold + 1, authorizationParametter);
            }
            finally
            {
                if (horizonProxy != null)
                    horizonProxy.Close();
            }

            bool isMoreThanThereshold = false;
            foreach (var relation in relationships)
            {
                if (relation.NotLoadedResults.Length > totalResultsThreshold)
                {
                    isMoreThanThereshold = true;
                }
            }
            return new RelationshipBasedResult()
            {
                IsResultsCountMoreThanThreshold = isMoreThanThereshold,
                Results = relationships
            };
        }

        public PropertyBasedResult FindPropertiesSameWith(KProperty[] searchedProperties, int loadNResults, int totalResultsThreshold)
        {
            AuthorizationParametters authorizationParametter
                = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);
            if (authorizationParametter.permittedGroupNames.Count == 0 || authorizationParametter.readableClassifications.Count == 0)
            {
                return new PropertyBasedResult();
            }

            List<Search.PropertiesMatchingResults> matchedProperties
                = GetMatchedPropertiesFromSearchServer(searchedProperties, totalResultsThreshold, authorizationParametter);

            Dictionary<long, KProperty> searchedPropertiesMapping = searchedProperties.ToDictionary(p => p.Id);

            bool resultMoreThanThershold = false;
            List<ResultPerSearchedProperty> resultsPerSearchProperties = new List<ResultPerSearchedProperty>();
            List<long> retriveFromRepo = new List<long>();
            foreach (var propertyMatched in matchedProperties)
            {
                if (propertyMatched.ResultPropertiesID.Length > totalResultsThreshold)
                {
                    resultMoreThanThershold = true;
                }
                List<long> mustBeLoad = new List<long>();
                List<long> notMustBeLoad = new List<long>();
                for (int i = 0; i < propertyMatched.ResultPropertiesID.Length; i++)
                {
                    if (i < loadNResults)
                    {
                        mustBeLoad.Add(propertyMatched.ResultPropertiesID[i]);
                    }
                    else
                    {
                        notMustBeLoad.Add(propertyMatched.ResultPropertiesID[i]);
                    }
                }
                retriveFromRepo.AddRange(mustBeLoad);
                resultsPerSearchProperties.Add(
                    new ResultPerSearchedProperty()
                    {
                        SearchedProperty = propertyMatched.SearchedPropertyID,
                        MustBeLoadProperties = mustBeLoad,
                        NotMustBeLoadProperties = notMustBeLoad
                    });
            }
            RepositoryProvider repoProvider = new RepositoryProvider(CallerUserName);
            List<KProperty> retrivedProperties = repoProvider.GetPropertiesById(retriveFromRepo.ToArray());
            Dictionary<long, KProperty> retrivedPropertiesMappping = new Dictionary<long, KProperty>();
            foreach (var property in retrivedProperties)
            {
                retrivedPropertiesMappping.Add(property.Id, property);
            }

            List<PropertyBasedResultsPerSearchedProperty> propertyBasedResultsPerSearchedProperty = new List<PropertyBasedResultsPerSearchedProperty>();
            foreach (var resultPerSearch in resultsPerSearchProperties)
            {
                List<KProperty> loadResult = new List<KProperty>();
                foreach (var propertyId in resultPerSearch.MustBeLoadProperties)
                {
                    loadResult.Add(retrivedPropertiesMappping[propertyId]);
                }
                propertyBasedResultsPerSearchedProperty.Add(new PropertyBasedResultsPerSearchedProperty()
                {
                    SearchedProperty = searchedPropertiesMapping[resultPerSearch.SearchedProperty],
                    LoadedResults = loadResult.ToArray(),
                    NotLoadedResultPropertyIDs = resultPerSearch.NotMustBeLoadProperties.ToArray()
                });
            }
            return (new PropertyBasedResult()
            {
                IsResultsCountMoreThanThreshold = resultMoreThanThershold,
                Results = propertyBasedResultsPerSearchedProperty.ToArray()
            });
        }

        private static List<Search.PropertiesMatchingResults> GetMatchedPropertiesFromSearchServer(KProperty[] searchedProperties, 
            int totalResultsThreshold, AuthorizationParametters authorizationParametter)
        {
            List<Search.PropertiesMatchingResults> matchedProperties;
            Search.ServiceClient proxy = null;
            try
            {
                proxy = new Search.ServiceClient();
                matchedProperties = (proxy.FindPropertiesSameWith(searchedProperties, totalResultsThreshold + 1, authorizationParametter)).ToList();
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }

            return matchedProperties;
        }

        public CustomSearchAroundResult PerformCustomSearchAround(Dictionary<string, long[]> searchedVertices,
            byte[] serializedCustomSearchAroundCriteria, int totalResultsThreshold)
        {
            AuthorizationParametters authorizationParametter
               = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);
            if (authorizationParametter.permittedGroupNames.Count == 0 || authorizationParametter.readableClassifications.Count == 0)
            {
                return new CustomSearchAroundResult();
            }

            // 1. Perform CSA by Horizon
            //    Result: matched links and targets, without authorization checking on target object properties
            Horizon.CustomSearchAroundResultIDs[] horizonResultPerSteps = PerformHorizonCustomSearchAround
                (searchedVertices, serializedCustomSearchAroundCriteria, authorizationParametter);
            foreach (var stepResult in horizonResultPerSteps)
            {
                ValidateHorizonCSAStepResults(stepResult);
            }

            // 2. Get accessable target object subset by Search server
            List<EventBasedResultsPerSearchedObjects> eventBasedResultsPerSearchedObjects = new List<EventBasedResultsPerSearchedObjects>();
            List<RelationshipBasedResultsPerSearchedObjects> relationshipBasedResultsPerSearchedObjects = 
                new List<RelationshipBasedResultsPerSearchedObjects>();

            CustomSearchAroundCriteria criteria = GetCriteriaFromSerializedData(serializedCustomSearchAroundCriteria);
            Parallel.ForEach(horizonResultPerSteps, (stepResult) =>
            {
                CustomSearchAroundStepResultType stepResultType = GetHorizonValidCSAStepResultType(stepResult);
                if (stepResultType == CustomSearchAroundStepResultType.NoResult)
                {// No result to "Select Matching"
                    return;
                }

                SearchAroundStep step = CustomSearchAroundCriteria.GetStepByGuid(criteria, stepResult.SearchAroundStepGuid);
                if (step.TargetObjectPropertyCriterias == null || !step.TargetObjectPropertyCriterias.Any())
                {
                    AppendStepResultsToTotalResults(stepResult, stepResultType, ref relationshipBasedResultsPerSearchedObjects,
                        ref eventBasedResultsPerSearchedObjects);
                }
                else
                {
                    byte[] serializedSelectMatchingCriteria = GetSerializedSelectMatchingCriteriaForStep(step);
                    long[] uncheckedStepTargetObjectIDs = GetStepTargetObjectIDs(stepResult, stepResultType);
                    long[] matchedObjectIDs = GetPropertyFilterMatchedSubset(serializedSelectMatchingCriteria, 
                        uncheckedStepTargetObjectIDs, authorizationParametter);
                    AppendStepMatchedResultsToTotalResults(stepResult, stepResultType, matchedObjectIDs, 
                        ref relationshipBasedResultsPerSearchedObjects, ref eventBasedResultsPerSearchedObjects);
                }
            });

            // 3. Retrieve final result links from Repository
            bool isResultsCountMoreThanThreshold = false;
            if ((eventBasedResultsPerSearchedObjects.Count + relationshipBasedResultsPerSearchedObjects.Count) > totalResultsThreshold)
            {
                isResultsCountMoreThanThreshold = true;
            }
            return new CustomSearchAroundResult()
            {
                EventBaseKLink = eventBasedResultsPerSearchedObjects.ToArray(),
                Ralationships = relationshipBasedResultsPerSearchedObjects.ToArray(),
                IsResultsCountMoreThanThreshold = isResultsCountMoreThanThreshold
            };
        }

        private void AppendStepMatchedResultsToTotalResults(Horizon.CustomSearchAroundResultIDs stepResult,
            CustomSearchAroundStepResultType stepResultType, long[] matchedObjectIDs,
            ref List<RelationshipBasedResultsPerSearchedObjects> relationshipBasedResultsPerSearchedObjects,
            ref List<EventBasedResultsPerSearchedObjects> eventBasedResultsPerSearchedObjects)
        {
            switch (stepResultType)
            {
                case CustomSearchAroundStepResultType.RelationshipBased:
                    HashSet<long> matchedObjectIDsHashSet = new HashSet<long>(matchedObjectIDs);
                    foreach (var relationshipNotLoadedResultID in stepResult.RelationshipNotLoadedResultIDs)
                    {
                        List<RelationshipBasedNotLoadedResult> matchedResults = new List<RelationshipBasedNotLoadedResult>();
                        foreach (var relationshipBasedNotLoadedResult in relationshipNotLoadedResultID.NotLoadedResults)
                        {
                            if (matchedObjectIDsHashSet.Contains(relationshipBasedNotLoadedResult.TargetObjectID))
                            {
                                matchedResults.Add(relationshipBasedNotLoadedResult);
                            }
                        }
                        if (matchedResults.Count > 0)
                        {
                            relationshipNotLoadedResultID.NotLoadedResults = matchedResults.ToArray();
                            AddRelationshipBasedResultToList(relationshipNotLoadedResultID, ref relationshipBasedResultsPerSearchedObjects);
                        }
                    }
                    break;
                case CustomSearchAroundStepResultType.EventBased:
                    matchedObjectIDsHashSet = new HashSet<long>(matchedObjectIDs);
                    foreach (var eventBasedNotLoadedResultID in stepResult.EventBasedNotLoadedResults)
                    {
                        List<EventBasedNotLoadedResult> matchedResults = new List<EventBasedNotLoadedResult>();
                        foreach (var eventBasedNotLoadedResult in eventBasedNotLoadedResultID.NotLoadedResults)
                        {
                            if (matchedObjectIDsHashSet.Contains(eventBasedNotLoadedResult.TargetObjectID))
                            {
                                matchedResults.Add(eventBasedNotLoadedResult);
                            }
                        }
                        if (matchedResults.Count > 0)
                        {
                            eventBasedNotLoadedResultID.NotLoadedResults = matchedResults.ToArray();
                            AddEventBasedResultToList(eventBasedNotLoadedResultID, ref eventBasedResultsPerSearchedObjects);
                        }
                    }
                    break;
                case CustomSearchAroundStepResultType.NoResult:
                    if (matchedObjectIDs.Any())
                    {
                        throw new InvalidOperationException("No subset can given for search around with 'No Result' type");
                    }
                    break;
                default:
                    throw new NotSupportedException("Unknown CSA result");
            }
        }

        private void AppendStepResultsToTotalResults(Horizon.CustomSearchAroundResultIDs stepResult, CustomSearchAroundStepResultType stepResultType, 
            ref List<RelationshipBasedResultsPerSearchedObjects> relationshipBasedResultsPerSearchedObjects,
            ref List<EventBasedResultsPerSearchedObjects> eventBasedResultsPerSearchedObjects)
        {

            switch (stepResultType)
            {
                case CustomSearchAroundStepResultType.RelationshipBased:
                    foreach (var relationshipNotLoadedResultID in stepResult.RelationshipNotLoadedResultIDs)
                    {
                        AddRelationshipBasedResultToList(relationshipNotLoadedResultID, ref relationshipBasedResultsPerSearchedObjects);
                    }
                    break;
                case CustomSearchAroundStepResultType.EventBased:
                    foreach (var eventBasedNotLoadedResult in stepResult.EventBasedNotLoadedResults)
                    {
                        AddEventBasedResultToList(eventBasedNotLoadedResult, ref eventBasedResultsPerSearchedObjects);
                    }
                    break;
                case CustomSearchAroundStepResultType.NoResult:
                    break;
                default:
                    throw new NotSupportedException("Unknown CSA result");
            }
        }

        private object addToEventBasedResultListLockObject = new object();
        private void AddEventBasedResultToList(EventBasedResultsPerSearchedObjects result, 
            ref List<EventBasedResultsPerSearchedObjects> eventBasedResultsPerSearchedObjects)
        {
            lock (addToEventBasedResultListLockObject)
            {
                eventBasedResultsPerSearchedObjects.Add(result);
            }
        }

        private object addToRelationshipBasedResultListLockObject = new object();
        private void AddRelationshipBasedResultToList(RelationshipBasedResultsPerSearchedObjects result, 
            ref List<RelationshipBasedResultsPerSearchedObjects> relationshipBasedResultsPerSearchedObjects)
        {
            lock (addToRelationshipBasedResultListLockObject)
            {
                relationshipBasedResultsPerSearchedObjects.Add(result);
            }
        }

        private long[] GetPropertyFilterMatchedSubset(byte[] serializedSelectMatchingCriteria, long[] targetObjectIDs,
            AuthorizationParametters authorizationParametter)
        {
            Search.ServiceClient sc = null;
            long[] matchedObjectIDs;
            try
            {
                sc = new Search.ServiceClient();
                matchedObjectIDs = sc.PerformSelectMatching(serializedSelectMatchingCriteria, targetObjectIDs, authorizationParametter);
            }
            finally
            {
                if (sc != null)
                    sc.Close();
            }
            return matchedObjectIDs;
        }

        private long[] GetStepTargetObjectIDs(Horizon.CustomSearchAroundResultIDs stepResult, CustomSearchAroundStepResultType stepResultType)
        {
            List<long> results = new List<long>();
            switch (stepResultType)
            {
                case CustomSearchAroundStepResultType.RelationshipBased:
                    var relations = (stepResult.RelationshipNotLoadedResultIDs.Select(r => r.NotLoadedResults));
                    foreach (var item in relations)
                    {
                        results.AddRange(item.Select(v => v.TargetObjectID));
                    }
                    return results.ToArray();
                case CustomSearchAroundStepResultType.EventBased:
                    var events = (stepResult.EventBasedNotLoadedResults.Select(r => r.NotLoadedResults));
                    foreach (var item in events)
                    {
                        results.AddRange(item.Select(v => v.TargetObjectID));
                    }
                    return results.ToArray();
                case CustomSearchAroundStepResultType.NoResult:
                    return new long[] { };
                default:
                    throw new NotSupportedException("Unknown CSA result");
            }
        }

        private CustomSearchAroundStepResultType GetHorizonValidCSAStepResultType(Horizon.CustomSearchAroundResultIDs stepResult)
        {
            CustomSearchAroundStepResultType stepResultType = CustomSearchAroundStepResultType.Unknown;
            if (stepResult.RelationshipNotLoadedResultIDs.Any())
            {
                stepResultType = CustomSearchAroundStepResultType.RelationshipBased;
            }
            else if (stepResult.EventBasedNotLoadedResults.Any())
            {
                stepResultType = CustomSearchAroundStepResultType.EventBased;
            }
            else
            {
                stepResultType = CustomSearchAroundStepResultType.NoResult;
            }

            return stepResultType;
        }

        private void ValidateHorizonCSAStepResults(Horizon.CustomSearchAroundResultIDs stepResult)
        {
            if (stepResult.RelationshipNotLoadedResultIDs == null || stepResult.EventBasedNotLoadedResults == null)
            {
                throw new NullReferenceException("Invalid search-around step result; Result array(ies) are null");
            }
            if (stepResult.RelationshipNotLoadedResultIDs.Any() && stepResult.EventBasedNotLoadedResults.Any())
            {
                throw new NullReferenceException("Invalid search-around step result; Both result arraies contains value");
            }
        }

        private byte[] GetSerializedSelectMatchingCriteriaForStep(SearchAroundStep step)
        {
            Query selectMatchingCriteria = new Query();
            QuerySerializer serializer = new QuerySerializer();
            selectMatchingCriteria.CriteriasSet = new CriteriaSet();
            selectMatchingCriteria.CriteriasSet.SetOperator = BooleanOperator.All;
            foreach (PropertyValueCriteria propertyFilter in step.TargetObjectPropertyCriterias)
            {
                selectMatchingCriteria.CriteriasSet.Criterias.Add(propertyFilter);
            }
            MemoryStream memStreamToSerailize = new MemoryStream();
            serializer.Serialize(memStreamToSerailize, selectMatchingCriteria);
            StreamUtility strmUtil = new StreamUtility();
            byte[] serializedSelectMatchingCriteria = strmUtil.ReadStreamAsBytesArray(memStreamToSerailize);
            return serializedSelectMatchingCriteria;
        }

        private CustomSearchAroundCriteria GetCriteriaFromSerializedData(byte[] serializedCustomSearchAroundCriteria)
        {
            StreamUtility streamUtil = new StreamUtility();
            CustomSearchAroundCriteriaSerializer serializer = new CustomSearchAroundCriteriaSerializer();
            MemoryStream serializedDataMemoryStream = new MemoryStream(serializedCustomSearchAroundCriteria);
            string serializedDataString = streamUtil.ByteArrayToStringUtf8(serializedDataMemoryStream.ToArray());
            Stream serializedDataStream = streamUtil.GenerateStreamFromString(serializedDataString);
            // بازیابی درخواست سری‌سازی شده
            CustomSearchAroundCriteria deserializedCriteria = serializer.Deserialize(serializedDataStream);
            if (!deserializedCriteria.IsValid())
            {
                throw new InvalidDataException("Invalid Custom Search Around Criteria");
            }
            return deserializedCriteria;
        }

        private Horizon.CustomSearchAroundResultIDs[] PerformHorizonCustomSearchAround(Dictionary<string, long[]> searchedVertices,
            byte[] serializedCustomSearchAroundCriteria, AuthorizationParametters authorizationParametter)
        {
            Horizon.ServiceClient horizonProxy = null;
            Horizon.CustomSearchAroundResultIDs[] horizonResult = null;
            try
            {
                horizonProxy = new Horizon.ServiceClient();
                horizonResult = horizonProxy.PerformCustomSearchAround(searchedVertices, serializedCustomSearchAroundCriteria,
                    CustomSearchAroundStepResultLimit, authorizationParametter);
            }
            finally
            {
                if (horizonProxy != null)
                    horizonProxy.Close();
            }

            return horizonResult;
        }

        //private Repository.DBRelationship[] GetRelationshipsByID(long[] relationshipIDs, AuthorizationParametters authorizationParametter)
        //{
        //    Repository.ServiceClient repositoryProxy = null;
        //    Repository.DBRelationship[] dBRelationships = null;
        //    try
        //    {
        //        repositoryProxy = new Repository.ServiceClient();
        //        dBRelationships = repositoryProxy.GetRelationships(relationshipIDs, authorizationParametter);
        //    }
        //    finally
        //    {
        //        if (repositoryProxy != null)
        //        {
        //            repositoryProxy.Close();
        //        }
        //    }
        //    return dBRelationships;
        //}
        private class ResultPerSearchedProperty
        {
            public long SearchedProperty { get; set; }
            public List<long> MustBeLoadProperties { get; set; }
            public List<long> NotMustBeLoadProperties { get; set; }
        }
    }
}
