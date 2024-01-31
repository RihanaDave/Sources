using GPAS.AccessControl;
using GPAS.Ontology;
using GPAS.SearchServer.Access.SearchEngine;
using GPAS.SearchServer.Entities.IndexChecking;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace GPAS.SearchServer.Logic
{
    public class IndexCheckingProvider
    {
        public IndexCheckingProvider()
        {
            properties = new JArray();
            result = new SearchIndexCheckingResult();
            engineAccess = SearchEngineProvider.GetNewDefaultSearchEngineClient();
        }

        private JToken properties;
        private readonly SearchIndexCheckingResult result;
        private readonly IAccessClient engineAccess;

        public SearchIndexCheckingResult StartSearchIndexChecking(SearchIndexCheckingInput input, AuthorizationParametters authorizationParameters)
        {
            properties = engineAccess.RetrieveEntireDocumentObjectFromSolr(input.ObjectId.ToString());

            if (!properties.Any())
                return NotIndexedObject(input);

            result.ObjectIndexStatus = true;

            SpecifyDocumentType(input, authorizationParameters);

            PropertiesIndexChecking(input);

            RelationsIndexChecking(input);

            return result;
        }

        private void SpecifyDocumentType(SearchIndexCheckingInput input, AuthorizationParametters authorizationParameters)
        {
            string objectTypeUri = properties.First[nameof(Entities.SearchEngine.Documents.Property.OwnerObjectTypeUri)]
                .ToString();

            if (OntologyProvider.GetOntology().IsDocument(objectTypeUri))
            {
                result.DocumentIndexStatus = engineAccess.IsFileDocumentExistWithID(input.ObjectId.ToString());

                ImageProcessingProvider imageProcessingProvider = new ImageProcessingProvider();

                if (imageProcessingProvider.IsMachneVisonServiceInstalled() &&
                     OntologyProvider.GetOntology().IsImageDocument(objectTypeUri))
                {
                    var faceDetectionResult = imageProcessingProvider.FaceDetection(input.DocumentContent, objectTypeUri,
                        authorizationParameters);

                    var imageDocuments = engineAccess.RetrieveImageDocumentFromSolr(input.ObjectId.ToString());

                    result.ImageIndexStatus = faceDetectionResult.Count == imageDocuments.Count();
                    result.ImageIndexCount = faceDetectionResult.Count + " of " + imageDocuments.Count();
                }
                else
                {
                    result.ImageIndexStatus = null;
                }
            }
        }

        private void PropertiesIndexChecking(SearchIndexCheckingInput input)
        {
            foreach (var inputProperty in input.Properties)
            {
                bool propertyIndexed = false;

                foreach (var property in properties)
                {
                    if (property["id"].ToString() == input.ObjectId + "!" + inputProperty.Id)
                    {
                        string propertyValue = string.Empty;

                        switch (OntologyProvider.GetOntology().GetBaseDataTypeOfProperty(property[nameof(Entities.SearchEngine.Documents.Property.TypeUri)].ToString()))
                        {
                            case BaseDataTypes.Boolean:
                                propertyValue = property[nameof(Entities.SearchEngine.Documents.Property.BooleanValue)].ToString();
                                break;
                            case BaseDataTypes.DateTime:
                                propertyValue = property[nameof(Entities.SearchEngine.Documents.Property.DateTimeValue)].ToString();
                                break;
                            case BaseDataTypes.Double:
                                propertyValue = property[nameof(Entities.SearchEngine.Documents.Property.DoubleValue)].ToString();
                                break;
                            case BaseDataTypes.GeoTime:
                                propertyValue = property[nameof(Entities.SearchEngine.Documents.Property.GeoTime)]?.ToString();
                                break;
                            case BaseDataTypes.String:
                                propertyValue = property[nameof(Entities.SearchEngine.Documents.Property.StringValue)].ToString();
                                break;
                            case BaseDataTypes.Long:
                            case BaseDataTypes.Int:
                                propertyValue = property[nameof(Entities.SearchEngine.Documents.Property.LongValue)].ToString();
                                break;
                        }

                        if (inputProperty.Value == propertyValue)
                            propertyIndexed = true;

                        break;
                    }
                }

                result.PropertiesIndexStatus.Add(new IndexingStatus
                {
                    Id = inputProperty.Id,
                    IndexStatus = propertyIndexed
                });
            }
        }

        private void RelationsIndexChecking(SearchIndexCheckingInput input)
        {
            foreach (var inputRelationId in input.RelationsIds)
            {
                bool relationIndexed = false;

                foreach (var property in properties)
                {
                    if (property["id"].ToString() == input.ObjectId + "!R" + inputRelationId)
                    {
                        relationIndexed = true;
                        break;
                    }
                }

                result.RelationsIndexStatus.Add(new IndexingStatus
                {
                    Id = inputRelationId,
                    IndexStatus = relationIndexed
                });
            }
        }

        private SearchIndexCheckingResult NotIndexedObject(SearchIndexCheckingInput input)
        {
            var notIndexedResult = new SearchIndexCheckingResult();

            foreach (var property in input.Properties)
            {
                notIndexedResult.PropertiesIndexStatus.Add(new IndexingStatus
                {
                    Id = property.Id,
                    IndexStatus = false
                });
            }

            foreach (var relation in input.RelationsIds)
            {
                notIndexedResult.RelationsIndexStatus.Add(new IndexingStatus
                {
                    Id = relation,
                    IndexStatus = false
                });
            }

            return notIndexedResult;
        }
    }
}
