using GPAS.AccessControl;
using GPAS.Dispatch.ServiceAccess.SearchService;
using GPAS.TextualSearch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GPAS.Dispatch.Logic
{
    public class TextualSearchProvider
    {
        private string CallerUserName = "";
        public TextualSearchProvider(string callerUserName)
        {
            CallerUserName = callerUserName;
        }

        public List<TextualSearch.BaseSearchResult> PerformTextualSearch(byte[] stream)
        {
            List<TextualSearch.BaseSearchResult> list = new List<TextualSearch.BaseSearchResult>();

            AuthorizationParametters authorizationParametter
                = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

            ServiceClient proxy = null;
            try
            {
                proxy = new ServiceClient();
                var result = proxy.PerformTextualSearch(stream, authorizationParametter).ToList();
                if (result.Count > 0)
                {
                    //var typeUris = GetTypeUriFromDataRepository(result.Select(x => x.ObjectId).ToList());
                    //var filenames = GetFilenameFromDataRepository(result.Select(x => x.ObjectId).ToList(), authorizationParametter);

                    foreach (var item in result)
                    {
                        if (item is ServiceAccess.SearchService.DocumentBasedSearchResult)
                        {
                            var documentBasedSearchResult = (ServiceAccess.SearchService.DocumentBasedSearchResult)item;

                            TextualSearch.DocumentBasedSearchResult searchResult = new TextualSearch.DocumentBasedSearchResult();
                            searchResult.ObjectId = documentBasedSearchResult.ObjectId;
                            searchResult.TotalRow = documentBasedSearchResult.TotalRow;
                            searchResult.FoundNumber = documentBasedSearchResult.FoundNumber;
                            searchResult.FileName = ""; //filenames.FirstOrDefault(x => x.Item1 == documentBasedSearchResult.ObjectId).Item2;
                            searchResult.TypeURI = ""; //typeUris.FirstOrDefault(x => x.Key == documentBasedSearchResult.ObjectId).Value;
                            searchResult.FileSize = GetFileSizeFromFileRepository(/*searchResult.FileName*/ item.ObjectId.ToString());
                            searchResult.TextResult = new TextualSearch.TextResult() { PartOfText = documentBasedSearchResult.TextResult.PartOfText.ToList() };

                            list.Add(searchResult);
                        }
                        else
                        {
                            var objectBasedSearchResult = (ServiceAccess.SearchService.ObjectBasedSearchResult)item;

                            TextualSearch.ObjectBasedSearchResult searchResult = new TextualSearch.ObjectBasedSearchResult();
                            searchResult.ObjectId = objectBasedSearchResult.ObjectId;
                            searchResult.TotalRow = objectBasedSearchResult.TotalRow;
                            searchResult.FoundNumber = objectBasedSearchResult.FoundNumber;
                            searchResult.TypeURI = ""; //typeUris.FirstOrDefault(x => x.Key == objectBasedSearchResult.ObjectId).Value;
                            searchResult.TextResult = new TextualSearch.TextResult() { PartOfText = objectBasedSearchResult.TextResult.PartOfText.ToList() };

                            list.Add(searchResult);
                        }
                    }
                }
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }

            return list;
        }

        //private Dictionary<long,string> GetTypeUriFromDataRepository(List<long> ids)
        //{
        //    Dictionary<long, string> dic = new Dictionary<long, string>();

        //   ServiceAccess.RepositoryService.ServiceClient proxy = null;
        //    try
        //    {
        //        proxy = new ServiceAccess.RepositoryService.ServiceClient();
        //        var result = proxy.GetObjects(ids.ToArray());

        //        foreach (var item in result)
        //        {
        //            dic.Add(item.Id, item.TypeUri);
        //        }

        //        return dic;
        //    }
        //    finally
        //    {
        //        if (proxy != null)
        //            proxy.Close();
        //    }
        //}

        //private List<Tuple<long, string>> GetFilenameFromDataRepository(List<long> ids, AuthorizationParametters authParams)
        //{
        //    List<Tuple<long, string>> list = new List<Tuple<long, string>>();

        //    ServiceAccess.RepositoryService.ServiceClient proxy = null;
        //    try
        //    {
        //        proxy = new ServiceAccess.RepositoryService.ServiceClient();
        //        var result = proxy.GetPropertiesOfObjects(ids.ToArray(),authParams);

        //        foreach (var item in result)
        //        {
        //            list.Add(new Tuple<long, string>(item.Owner.Id, item.Value));
        //        }

        //        return list;
        //    }
        //    finally
        //    {
        //        if (proxy != null)
        //            proxy.Close();
        //    }
        //}

        private long GetFileSizeFromFileRepository(string filename)
        {
            ServiceAccess.FileRepositoryService.ServiceClient proxy = null;
            try
            {
                proxy = new ServiceAccess.FileRepositoryService.ServiceClient();
                return proxy.GetDataSourceAndDocumentFileSizeInBytes(filename);
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                return 0;
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }
    }
}

