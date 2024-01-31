using GPAS.AccessControl;
using GPAS.Dispatch.Entities.Concepts.SearchAroundResult;
using GPAS.Horizon.Entities.IndexChecking;
using GPAS.Horizon.GraphRepository;
using GPAS.Ontology;
using GPAS.Utility;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace GPAS.Horizon.Logic
{
    public class IndexCheckingProvider
    {
        public IndexCheckingProvider()
        {
            result = new HorizonIndexCheckingResult();
        }

        private readonly HorizonIndexCheckingResult result;

        public HorizonIndexCheckingResult StartHorizonIndexChecking(HorizonIndexCheckingInput input, AuthorizationParametters authorizationParameters)
        {
            GraphRepositoryProvider graphRepositoryProvider = new GraphRepositoryProvider();
            graphRepositoryProvider.Init();
            IAccessClient accessClient = graphRepositoryProvider.GetNewSearchEngineClient();
            accessClient.OpenConnetion();

            var searchedObject = (JObject)accessClient.RetrieveObjectWithId(input.ObjectId);

            if (searchedObject == null)
                return NotIndexedObject(input);

            result.ObjectIndexStatus = true;

            PropertiesIndexChecking(input, searchedObject);

            RelationsIndexChecking(input, authorizationParameters);

            return result;
        }

        private void PropertiesIndexChecking(HorizonIndexCheckingInput input, JObject searchedObject)
        {
            var resultArray = searchedObject["result"];

            foreach (var property in input.Properties)
            {
                bool indexed = false;
                string propertyValue = Regex.Replace(resultArray.First["P" + EncodingConverter.GetMd5HashCode(property.TypeUri)]?.ToString(),
                    @"\s", string.Empty);

                switch (OntologyProvider.GetOntology().GetBaseDataTypeOfProperty(property.TypeUri))
                {
                    case BaseDataTypes.Double:
                        propertyValue = propertyValue.Replace("[", string.Empty);
                        propertyValue = propertyValue.Replace("]", string.Empty);

                        indexed = double.Parse(property.Value) == double.Parse(propertyValue);
                        break;
                    case BaseDataTypes.Int:
                    case BaseDataTypes.Long:
                    case BaseDataTypes.Boolean:
                        indexed = $"[{property.Value}]" == propertyValue;
                        break;
                    case BaseDataTypes.GeoTime:
                        indexed = $"[\"{property.Value}\"]" == propertyValue;
                        break;
                    case BaseDataTypes.String:
                    case BaseDataTypes.HdfsURI:
                        indexed = $"[\"{EncodingConverter.GetBase64Encode(property.Value)}\"]" == propertyValue;
                        break;
                }

                result.PropertiesIndexStatus.Add(new IndexingStatus
                {
                    Id = property.Id,
                    IndexStatus = indexed
                });
            }
        }

        private void RelationsIndexChecking(HorizonIndexCheckingInput input, AuthorizationParametters authorizationParameters)
        {
            var arg = new Dictionary<string, long[]>();
            arg.Add(input.ObjectTypeUri, new[] { input.ObjectId });

            SearchAroundProvider provider = new SearchAroundProvider();
            var relatedEntities = provider.FindRelatedEntities(arg, input.ResultLimit, authorizationParameters);
            var relatedDocuments = provider.FindRelatedDocuments(arg, input.ResultLimit, authorizationParameters);
            var relatedEvents = provider.FindRelatedEvents(arg, input.ResultLimit, authorizationParameters);

            var allRelations = new List<RelationshipBasedNotLoadedResult>();

            foreach (var entity in relatedEntities)
            {
                allRelations.AddRange(entity.NotLoadedResults);
            }

            foreach (var document in relatedDocuments)
            {
                allRelations.AddRange(document.NotLoadedResults);
            }

            foreach (var myEvent in relatedEvents)
            {
                allRelations.AddRange(myEvent.NotLoadedResults);
            }

            foreach (var relationId in input.RelationsIds)
            {
                bool indexed = false;

                foreach (var relation in allRelations)
                {
                    if (relationId == relation.RelationshipID)
                    {
                        indexed = true;
                        break;
                    }
                }

                result.RelationsIndexStatus.Add(new IndexingStatus
                {
                    Id = relationId,
                    IndexStatus = indexed
                });
            }
        }

        private HorizonIndexCheckingResult NotIndexedObject(HorizonIndexCheckingInput input)
        {
            var notIndexedResult = new HorizonIndexCheckingResult();

            foreach (var property in input.Properties)
            {
                notIndexedResult.PropertiesIndexStatus.Add(new IndexingStatus
                {
                    Id = property.Id,
                    IndexStatus = false
                });
            }

            foreach (var relationId in input.RelationsIds)
            {
                notIndexedResult.RelationsIndexStatus.Add(new IndexingStatus
                {
                    Id = relationId,
                    IndexStatus = false
                });
            }

            return notIndexedResult;
        }
    }
}
