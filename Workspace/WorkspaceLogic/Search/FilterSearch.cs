using GPAS.FilterSearch;
using GPAS.Workspace.Entities;
using GPAS.Workspace.ServiceAccess.RemoteService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Logic.Search
{
    public class FilterSearch
    {
        private FilterSearch()
        { }
        public async static Task<IEnumerable<KWObject>> PerformFilterSearchAsync(Query filterSearchQuery, string count)
        {
            IEnumerable<KWObject> result = new List<KWObject>();
            result = await DataAccessManager.Search.FilterSearch.GetFilterSearchResult(filterSearchQuery,int.Parse(count));             
            return result;                  
        }
    }
}
