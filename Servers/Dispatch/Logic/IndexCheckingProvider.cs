using GPAS.AccessControl;
using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.Entities.IndexChecking;
using System.Collections.Generic;
using FileRepository = GPAS.Dispatch.ServiceAccess.FileRepositoryService;
using Horizon = GPAS.Dispatch.ServiceAccess.HorizonService;
using Search = GPAS.Dispatch.ServiceAccess.SearchService;

namespace GPAS.Dispatch.Logic
{
    public class IndexCheckingProvider 
    {
        public IndexCheckingProvider(string callerUserName)
        {
            authorizationParameter = UserAccountControlProvider.GetUserAuthorizationParametters(callerUserName);
        }

        private readonly AuthorizationParametters authorizationParameter;

        public SearchIndexCheckingResult SearchIndexChecking(SearchIndexCheckingInput input)
        {
            if (OntologyLoader.OntologyLoader.GetOntology().IsImageDocument(input.ObjectType))
            {
                FileRepository.ServiceClient fileServiceClient = new FileRepository.ServiceClient();
                input.DocumentContent = fileServiceClient.DownloadDocumentFile(input.ObjectId);
            }

            Search.ServiceClient serviceClient = new Search.ServiceClient();
            var result = serviceClient.IndexChecking(ConverterSearchIndexCheckingInput(input), authorizationParameter);

            return ConverterSearchIndexCheckingResult(result);
        }

        private Search.SearchIndexCheckingInput ConverterSearchIndexCheckingInput(SearchIndexCheckingInput input)
        {
            var convertedInput = new Search.SearchIndexCheckingInput
            {
                ObjectId = input.ObjectId,
                DocumentContent = input.DocumentContent,
                RelationsIds = input.RelationsIds.ToArray()
            };

            var convertedProperties = new List<KProperty>();

            foreach (var property in input.Properties)
            {
                convertedProperties.Add(new KProperty
                {
                    Id = property.Id,
                    Value = property.Value
                });
            }

            convertedInput.Properties = convertedProperties.ToArray();

            return convertedInput;
        }

        private SearchIndexCheckingResult ConverterSearchIndexCheckingResult(Search.SearchIndexCheckingResult result)
        {
            var convertedResult = new SearchIndexCheckingResult
            {
                ObjectIndexStatus = result.ObjectIndexStatus,
                DocumentIndexStatus = result.DocumentIndexStatus,
                ImageIndexStatus = result.ImageIndexStatus
            };

            var convertedProperties = new List<IndexingStatus>();

            foreach (var property in result.PropertiesIndexStatus)
            {
                convertedProperties.Add(new IndexingStatus
                {
                    Id = property.Id,
                    IndexStatus = property.IndexStatus
                });
            }

            convertedResult.PropertiesIndexStatus = convertedProperties;

            var convertedRelations = new List<IndexingStatus>();

            foreach (var relation in result.RelationsIndexStatus)
            {
                convertedRelations.Add(new IndexingStatus
                {
                    Id = relation.Id,
                    IndexStatus = relation.IndexStatus
                });
            }

            convertedResult.RelationsIndexStatus = convertedRelations;

            return convertedResult;
        }

        public HorizonIndexCheckingResult HorizonIndexChecking(HorizonIndexCheckingInput input)
        {
            Horizon.ServiceClient serviceClient = new Horizon.ServiceClient();
            var result = serviceClient.HorizonIndexChecking(ConverterHorizonIndexCheckingInput(input), authorizationParameter);

            return ConverterHorizonIndexCheckingResult(result);
        }

        private Horizon.HorizonIndexCheckingInput ConverterHorizonIndexCheckingInput(HorizonIndexCheckingInput input)
        {
            var convertedInput = new Horizon.HorizonIndexCheckingInput
            {
                ObjectId = input.ObjectId,
                ObjectTypeUri = input.ObjectTypeUri,
                ResultLimit = input.ResultLimit,
                RelationsIds = input.RelationsIds.ToArray()
            };

            var convertedProperties = new List<KProperty>();

            foreach (var property in input.Properties)
            {
                convertedProperties.Add(new KProperty
                {
                    Id = property.Id,
                    Value = property.Value,
                    TypeUri = property.TypeUri
                });
            }

            convertedInput.Properties = convertedProperties.ToArray();

            return convertedInput;
        }

        private HorizonIndexCheckingResult ConverterHorizonIndexCheckingResult(Horizon.HorizonIndexCheckingResult result)
        {
            var convertedResult = new HorizonIndexCheckingResult
            {
                ObjectIndexStatus = result.ObjectIndexStatus
            };

            var convertedProperties = new List<IndexingStatus>();

            foreach (var property in result.PropertiesIndexStatus)
            {
                convertedProperties.Add(new IndexingStatus
                {
                    Id = property.Id,
                    IndexStatus = property.IndexStatus
                });
            }
            convertedResult.PropertiesIndexStatus = convertedProperties;

            var convertedRelations = new List<IndexingStatus>();

            foreach (var relation in result.RelationsIndexStatus)
            {
                convertedRelations.Add(new IndexingStatus
                {
                    Id = relation.Id,
                    IndexStatus = relation.IndexStatus
                });
            }
            convertedResult.RelationsIndexStatus = convertedRelations;

            return convertedResult;
        }
    }
}
