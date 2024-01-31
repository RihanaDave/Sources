using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPAS.TextualSearch;
using GPAS.Utility;
using GPAS.Workspace.Entities;
using GPAS.Workspace.ServiceAccess;
using GPAS.Workspace.ServiceAccess.RemoteService;

namespace GPAS.Workspace.DataAccessManager.Search
{
    public class TextualSearchProvider
    {
        private TextualSearchProvider()
        {
        }

        public static List<TextualSearch.BaseSearchResult> GetTextualSearchResult(TextualSearchQuery textualSearchQuery)
        {
            if (textualSearchQuery == null)
                throw new ArgumentNullException("textualSearchQuery");

            List<TextualSearch.BaseSearchResult> list = new List<TextualSearch.BaseSearchResult>();

            TextualQuerySerializer serializer = new TextualQuerySerializer();
            MemoryStream streamWriter = new MemoryStream();
            serializer.Serialize(streamWriter, textualSearchQuery);
            StreamUtility streamUtil = new StreamUtility();
            byte[] filterSearchQueryByteArray = streamUtil.ReadStreamAsBytesArray(streamWriter);
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            try
            {
                var result = sc.PerformTextualSearch(filterSearchQueryByteArray);

                foreach (var item in result)
                {
                    if (item is ServiceAccess.RemoteService.DocumentBasedSearchResult)
                    {
                        var documentBasedSearchResult = (ServiceAccess.RemoteService.DocumentBasedSearchResult)item;

                        TextualSearch.DocumentBasedSearchResult searchResult = new TextualSearch.DocumentBasedSearchResult();

                        List<TextualSearch.TextResult> textResults = new List<TextualSearch.TextResult>();
                        searchResult.FoundNumber = documentBasedSearchResult.FoundNumber;
                        searchResult.TotalRow = documentBasedSearchResult.TotalRow;
                        searchResult.FileSize = documentBasedSearchResult.FileSize;
                        searchResult.TypeURI = documentBasedSearchResult.TypeURI;
                        searchResult.FileName = documentBasedSearchResult.FileName;
                        searchResult.ObjectId = documentBasedSearchResult.ObjectId;
                        searchResult.TextResult = new TextualSearch.TextResult() { PartOfText = documentBasedSearchResult.TextResult.PartOfText.ToList() };

                        list.Add(searchResult);
                    }
                    else
                    {
                        var objectBasedSearchResult = (ServiceAccess.RemoteService.ObjectBasedSearchResult)item;

                        TextualSearch.ObjectBasedSearchResult searchResult = new TextualSearch.ObjectBasedSearchResult();

                        List<TextualSearch.TextResult> textResults = new List<TextualSearch.TextResult>();
                        searchResult.FoundNumber = objectBasedSearchResult.FoundNumber;
                        searchResult.TotalRow = objectBasedSearchResult.TotalRow;
                        searchResult.ObjectId = objectBasedSearchResult.ObjectId;
                        searchResult.TypeURI  = objectBasedSearchResult.TypeURI;
                        searchResult.TextResult = new TextualSearch.TextResult() { PartOfText = objectBasedSearchResult.TextResult.PartOfText.ToList() };

                        list.Add(searchResult);
                    }
                }
            }
            finally
            {
                if (sc != null)
                {
                    sc.Close();
                }
            }

            return list;
        }
    }
}
