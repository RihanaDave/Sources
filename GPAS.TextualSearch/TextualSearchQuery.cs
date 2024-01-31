using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.TextualSearch
{
    public class TextualSearchQuery
    {
        public List<BaseSearchCriteria> Criterias
        {
            get;
            set;
        }

        public long StartIndex
        {
            get;
            set;
        }

        public long ToIndex
        {
            get;
            set;
        }

        public string QueryParam
        {
            get;
            set;
        }

        public SearchTargetSet SearchTarget
        {
            get;
            set;
        }

        public int Snippets
        {
            get;
            set;
        }

        public int Fragsize
        {
            get;
            set;
        }

        public HighlightMode HighlightMode
        {
            get;
            set;
        }
    }
}
