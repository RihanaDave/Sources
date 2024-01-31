using GPAS.TextualSearch;
using GPAS.Workspace.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Logic.Search
{
    public class TextualSearchProvider
    {
        public TextualSearchProvider()
        {
        }

        public List<TextualSearch.BaseSearchResult> PerformTextualSearchAsync(TextualSearchQuery textualSearchQuery)
        {
            return DataAccessManager.Search.TextualSearchProvider.GetTextualSearchResult(textualSearchQuery);
        }
    }
}
