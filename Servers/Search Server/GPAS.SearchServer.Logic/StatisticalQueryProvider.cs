using GPAS.AccessControl;
using GPAS.SearchServer.Access.SearchEngine;
using GPAS.StatisticalQuery;
using GPAS.StatisticalQuery.ResultNode;
using GPAS.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace GPAS.SearchServer.Logic
{
    public class StatisticalQueryProvider
    {
        public QueryResult RunQuery(byte[] queryByteArray, AuthorizationParametters authorizationParametters)
        {
            Query query = DeserializeQuery(queryByteArray);
            IAccessClient accessClient = SearchEngineProvider.GetNewDefaultSearchEngineClient();
            QueryResult queryResult = accessClient.RunStatisticalQuery(query, OntologyProvider.GetOntology(), authorizationParametters);
            return CorrectQueryResult(queryResult, OntologyProvider.GetOntology());            
        }

        private QueryResult CorrectQueryResult(QueryResult queryResult, Ontology.Ontology ontology)
        {
            QueryResult correctedQueryResult = new QueryResult()
            {
                ObjectTypePreview = new List<TypeBasedStatistic>(),
                PropertyTypePreview = queryResult.PropertyTypePreview
            };
            Dictionary<string, long> ontologyConceptStatistics = new Dictionary<string, long>();

            foreach (var objectType in queryResult.ObjectTypePreview)
            {
                ArrayList parents = ontology.GetAllParents(objectType.TypeUri);

                if (!ontologyConceptStatistics.ContainsKey(objectType.TypeUri))
                {
                    ontologyConceptStatistics.Add(objectType.TypeUri, objectType.Frequency);
                    foreach (var parent in parents)
                    {
                        if (!ontologyConceptStatistics.ContainsKey((string) parent))
                        {
                            ontologyConceptStatistics.Add((string) parent, objectType.Frequency);
                        }
                        else if((string)parent != objectType.TypeUri)
                        {
                            ontologyConceptStatistics[(string) parent] += objectType.Frequency;
                        }
                    }
                }
                else
                {
                    ontologyConceptStatistics[objectType.TypeUri] += objectType.Frequency;
                    foreach (var parent in parents)
                    {
                        if (!ontologyConceptStatistics.ContainsKey((string) parent))
                        {
                            TypeBasedStatistic temp = new TypeBasedStatistic
                            {
                                TypeUri = objectType.TypeUri,
                                Frequency = objectType.Frequency
                            };
                            ontologyConceptStatistics.Add(objectType.TypeUri, objectType.Frequency);
                        }
                        else if((string)parent != objectType.TypeUri)
                        {
                            ontologyConceptStatistics[(string)parent] += objectType.Frequency;
                        }
                    }
                }
            }

            foreach (var item in ontologyConceptStatistics)
            {
                TypeBasedStatistic temp = new TypeBasedStatistic
                {
                    TypeUri = item.Key,
                    Frequency = item.Value
                };
                correctedQueryResult.ObjectTypePreview.Add(temp);
            }            

            return correctedQueryResult;
        }

        public long[] RetrieveObjectIDsByQuery(byte[] queryByteArray,int PassObjectsCountLimit, AuthorizationParametters authParams)
        {
            Query query = DeserializeQuery(queryByteArray);
            IAccessClient accessClient = SearchEngineProvider.GetNewDefaultSearchEngineClient();
            return accessClient.RetrieveObjectIDsByStatisticalQuery(query, PassObjectsCountLimit, OntologyProvider.GetOntology(), authParams);
        }

        private static Query DeserializeQuery(byte[] queryByteArray)
        {
            StreamUtility streamUtil = new StreamUtility();
            QuerySerializer querySerializer = new QuerySerializer();
            string serializedPrimaryQueryString = streamUtil.ByteArrayToStringUtf8(queryByteArray);
            MemoryStream memoryStream = streamUtil.ConvertStreamToMemoryStream(streamUtil.GenerateStreamFromString(serializedPrimaryQueryString));
            Query query = querySerializer.Deserialize(memoryStream);
            return query;
        }

        public PropertyValueStatistics RetrievePropertyValueStatistics
            (byte[] queryByteArray, string exploredPropertyTypeUri, int startOffset, int resultsLimit
            , long minimumCount, AuthorizationParametters authParams)
        {
            Query query = DeserializeQuery(queryByteArray);
            IAccessClient accessClient = SearchEngineProvider.GetNewDefaultSearchEngineClient();
            return accessClient.RetrievePropertyValueStatistics 
                (query, exploredPropertyTypeUri, startOffset, resultsLimit, minimumCount, OntologyProvider.GetOntology(), authParams);
        }

        public LinkTypeStatistics RetrieveLinkTypeStatistics(byte[] queryByteArray, AuthorizationParametters authorizationParametters)
        {
            Query query = DeserializeQuery(queryByteArray);
            IAccessClient accessClient = SearchEngineProvider.GetNewDefaultSearchEngineClient();
            return accessClient.RetrieveLinkTypeStatistics(query, OntologyProvider.GetOntology(), authorizationParametters);            
        }

        public long[] RetrieveLinkedObjectIDsByStatisticalQuery(byte[] queryByteArray, int PassObjectsCountLimit, AuthorizationParametters authorizationParametters)
        {
            Query query = DeserializeQuery(queryByteArray);
            IAccessClient accessClient = SearchEngineProvider.GetNewDefaultSearchEngineClient();
            return accessClient.RetrieveLinkedObjectIDsByStatisticalQuery(query, PassObjectsCountLimit, OntologyProvider.GetOntology(), authorizationParametters);
        }

        public PropertyBarValues RetrievePropertyBarValuesStatistics(byte[] queryByteArray, string numericPropertyTypeUri, long bucketCount, double minValue, double maxValue, AuthorizationParametters authorizationParametters)
        {
            Query query = DeserializeQuery(queryByteArray);
            IAccessClient accessClient = SearchEngineProvider.GetNewDefaultSearchEngineClient();
            return accessClient.RetrievePropertyBarValuesStatistics(query, numericPropertyTypeUri, bucketCount, minValue, maxValue, OntologyProvider.GetOntology(), authorizationParametters);

        }

    }
}
