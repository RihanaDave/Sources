using GPAS.AccessControl;
using GPAS.SearchServer.Access.SearchEngine;
using GPAS.SearchServer.Entities.SearchEngine.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Logic
{
    public class DataSourceProvider
    {
        public List<DataSourceInfo> GetDataSources(long dataSourceType, int star, int count, string filter, AuthorizationParametters authorizationParametters)
        {
            SearchEngineDocumentConvertor searchEngineDocumentConvertor = new SearchEngineDocumentConvertor();
            IAccessClient engineAccess = SearchEngineProvider.GetNewDefaultSearchEngineClient();
            return searchEngineDocumentConvertor.ConvertDataSourceInfoToDataSource(engineAccess.GetDataSources(dataSourceType, star,count, filter, authorizationParametters));
        }

        public List<DataSourceInfo> GetAllDataSources(int count, string filter, AuthorizationParametters authorizationParametters)
        {
            SearchEngineDocumentConvertor searchEngineDocumentConvertor = new SearchEngineDocumentConvertor();
            IAccessClient engineAccess = SearchEngineProvider.GetNewDefaultSearchEngineClient();
            return searchEngineDocumentConvertor.ConvertDataSourceInfoToDataSource(engineAccess.GetAllDataSources(count, filter, authorizationParametters));
        }
    }
}
