using GPAS.AccessControl;
using GPAS.SearchServer.Access.DataClient;
using GPAS.SearchServer.Access.SearchEngine;
using GPAS.SearchServer.Entities;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Logic
{
    /// <summary>
    /// این کلاس جست و جوی سریع را مدیریت می کند.
    /// </summary>
    public class QuickSearch
    {
        private static readonly int QuickSearchResultsTreshould = int.Parse(ConfigurationManager.AppSettings["QuickSearchResultsTreshould"]);
        public static readonly long QuickFileContentSearchResultsTreshould = long.Parse(ConfigurationManager.AppSettings["QuickFileContentSearchResultsTreshould"]);

        /// <summary>
        /// جست و جوی سریع را در پایگاه داده براساس کلیدواژه دریافتی انجام می کند.
        /// </summary>
        /// <param name="keyword">   کلیدواژه مورد نظر که براساس آن جست و جو صورت می گیرد.   </param>
        /// <returns>    لیستی از اشیا که در نتایج جست و جو آمده است را برمی گرداند.    </returns>
        public SearchObject[] Search(string keyword, AuthorizationParametters authorizationParametters)
        {
            IAccessClient accessClient = SearchEngineProvider.GetNewDefaultSearchEngineClient();
            List<SearchObject> result = new List<SearchObject>();

            List<SearchObject> entityDocumentIDs = new List<SearchObject>(), eventDocumentIDs = new List<SearchObject>(), documentDocumentIDs = new List<SearchObject>(), fileDocumentOwnerObjectIDs = new List<SearchObject>();
            //Parallel.Invoke(() => entityDocumentIDs = accessClient.GetEntityDocumentIDsForMatchedKeyword(keyword, authorizationParametters, QuickSearchResultsTreshould, OntologyProvider.GetOntology()),
            //() => eventDocumentIDs = accessClient.GetEventDocumentIDsForMatchedKeyword(keyword, authorizationParametters, QuickSearchResultsTreshould, OntologyProvider.GetOntology()),
            //() => documentDocumentIDs = accessClient.GetDocumentDocumentIDsForMatchedKeyword(keyword, authorizationParametters, QuickSearchResultsTreshould, OntologyProvider.GetOntology())
            ////() => fileDocumentOwnerObjectIDs = accessClient.GetFileDocumentOwnerObjectIDs(keyword, authorizationParametters, QuickFileContentSearchResultsTreshould)
            //);

            entityDocumentIDs = accessClient.GetEntityDocumentIDsForMatchedKeyword(keyword, authorizationParametters, QuickSearchResultsTreshould, OntologyProvider.GetOntology());
            eventDocumentIDs = accessClient.GetEventDocumentIDsForMatchedKeyword(keyword, authorizationParametters, QuickSearchResultsTreshould, OntologyProvider.GetOntology());
            documentDocumentIDs = accessClient.GetDocumentDocumentIDsForMatchedKeyword(keyword, authorizationParametters, QuickSearchResultsTreshould, OntologyProvider.GetOntology());

            result.AddRange(entityDocumentIDs);
            result.AddRange(eventDocumentIDs);
            result.AddRange(documentDocumentIDs);
            //result.AddRange(fileDocumentOwnerObjectIDs);


            return result.ToArray();

            //RetrieveDataClient retrieveClient = new RetrieveDataClient();
            //return retrieveClient.GetObjectsByIDs(result.Distinct().ToList());
        }

        //public List<SearchResultModel> Search(SearchModel model, AuthorizationParametters authorizationParametters)
        //{
        //    IAccessClient accessClient = SearchEngineProvider.GetNewDefaultSearchEngineClient();
        //    var searchObject = accessClient.GetResultsForDocText(model, authorizationParametters, OntologyProvider.GetOntology());
        //    return ConvertSearchObjectToSearchResultModel(searchObject);

        //}

        //public long GetTotalTextDocResults(SearchModel model, AuthorizationParametters authorizationParametters)
        //{
        //    IAccessClient accessClient = SearchEngineProvider.GetNewDefaultSearchEngineClient();
        //    var resultscount = accessClient.GetTotalTextDocResults(model, authorizationParametters);
        //    return resultscount;

        //}

        //private List<SearchResultModel> ConvertSearchObjectToSearchResultModel(List<SearchObject> searchObjects)
        //{
        //    List<SearchResultModel> searchResultModels = new List<SearchResultModel>();
        //    foreach (var item in searchObjects)
        //    {
        //        SearchResultModel searchResultModel = new SearchResultModel()
        //        {
        //            Id = item.Id,
        //            Image = item.TypeUri,
        //            Type = item.TypeUri,
        //            PartOfText = item.PartOfText,
        //            FileName = item.FileName,
        //        };
        //        searchResultModels.Add(searchResultModel);
        //    }
        //    return searchResultModels;
        //}
    }
}