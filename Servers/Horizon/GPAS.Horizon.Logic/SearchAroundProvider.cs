using GPAS.AccessControl;
using GPAS.Dispatch.Entities.Concepts.SearchAroundResult;
using GPAS.Horizon.Entities;
using GPAS.Horizon.GraphRepository;
using GPAS.SearchAround;
using GPAS.Utility;
using System;
using System.Collections.Generic;
using System.IO;

namespace GPAS.Horizon.Logic
{
    public class SearchAroundProvider
    {
        public SearchAroundProvider()
        {
        }
        
        private IAccessClient GetNewAccessClient()
        {
            GraphRepositoryProvider graphRepositoryProvider = new GraphRepositoryProvider();
            graphRepositoryProvider.Init();
            IAccessClient accessClient = graphRepositoryProvider.GetNewSearchEngineClient();
            accessClient.OpenConnetion();
            return accessClient;
        }

        private Ontology.Ontology currentOntology = OntologyProvider.GetOntology();

        public List<RelationshipBasedResultsPerSearchedObjects> FindRelatedEntities(Dictionary<string, long[]> searchedObjects, long resultLimit,
            AuthorizationParametters authorizationParametters)
        {
            string[] entityTypeURIs = currentOntology.GetAllChilds(currentOntology.GetEntityTypeURI()).ToArray();
            IAccessClient accessClient = GetNewAccessClient();
            return accessClient.RetrieveRelatedVertices(searchedObjects, entityTypeURIs, resultLimit, authorizationParametters);
        }

        public List<RelationshipBasedResultsPerSearchedObjects> FindRelatedDocuments(Dictionary<string, long[]> searchedObjects, long resultLimit,
            AuthorizationParametters authorizationParametters)
        {
            string[] documentTypeURIs = currentOntology.GetAllChilds(currentOntology.GetDocumentTypeURI()).ToArray();
            IAccessClient accessClient = GetNewAccessClient();
            return accessClient.RetrieveRelatedVertices(searchedObjects, documentTypeURIs, resultLimit, authorizationParametters);
        }

        public List<RelationshipBasedResultsPerSearchedObjects> FindRelatedEvents(Dictionary<string, long[]> searchedObjects, long resultLimit,
            AuthorizationParametters authorizationParametters)
        {
            string[] eventTypeURIs = currentOntology.GetAllChilds(currentOntology.GetEventTypeURI()).ToArray();
            IAccessClient accessClient = GetNewAccessClient();
            return accessClient.RetrieveRelatedVertices(searchedObjects, eventTypeURIs, resultLimit, authorizationParametters);
        }

        public List<EventBasedResultsPerSearchedObjects> FindRelatedEntitiesAppearedInEvents(Dictionary<string, long[]> searchedObjects,
            long resultLimit, AuthorizationParametters authorizationParametters)
        {
            string[] eventTypeURIs = currentOntology.GetAllChilds(currentOntology.GetEventTypeURI()).ToArray();
            IAccessClient accessClient = GetNewAccessClient();
            List<EventBasedResultsPerSearchedObjects> eventBaseNotLoadedLinksID = 
                accessClient.RetrieveTransitiveRelatedVertices(searchedObjects, eventTypeURIs, resultLimit, authorizationParametters);
            return eventBaseNotLoadedLinksID;
        }

        public CustomSearchAroundResultIDs[] PerformCustomSearchAround(Dictionary<string, long[]> searchedObjects,
            byte[] serializedCustomSearchAroundCriteria, long resultLimit, AuthorizationParametters authorizationParametters)
        {
            if (searchedObjects == null)
                throw new ArgumentNullException(nameof(searchedObjects));

            if (searchedObjects.Count == 0)
                throw new ArgumentException("No source object defined", nameof(searchedObjects));

            if (serializedCustomSearchAroundCriteria == null)
                throw new ArgumentNullException(nameof(serializedCustomSearchAroundCriteria));

            if (serializedCustomSearchAroundCriteria.Length == 0)
                throw new ArgumentException("Empty criterial stream", nameof(serializedCustomSearchAroundCriteria));

            if (authorizationParametters == null)
                throw new ArgumentNullException(nameof(authorizationParametters));

            CustomSearchAroundCriteriaSerializer serializer = new CustomSearchAroundCriteriaSerializer();
            StreamUtility streamUtil = new StreamUtility();
            string serializedCriteriaString = streamUtil.ByteArrayToStringUtf8(serializedCustomSearchAroundCriteria);
            Stream xmlStream = streamUtil.GenerateStreamFromString(serializedCriteriaString);
            CustomSearchAroundCriteria criteria = serializer.Deserialize(xmlStream);
            if (criteria == null || !(criteria is CustomSearchAroundCriteria))
            {
                throw new ArgumentException("Unable to deserialize the criteria");
            }
            if (!criteria.IsValid())
            {
                throw new ArgumentException("Criteria is not valid");
            }

            IAccessClient accessClient = GetNewAccessClient();
            return accessClient.RetrieveRelatedVerticesWithCustomCriteria(searchedObjects, criteria, resultLimit, authorizationParametters);
        }
    }
}