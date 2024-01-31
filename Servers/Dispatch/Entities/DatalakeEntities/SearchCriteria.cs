using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch.Entities.DatalakeEntities
{
    public class SearchCriteria
    {
        public string Type { get; set; }

        public string Value { get; set; }

        public ComparatorType Comparator { get; set; }

        public BaseDataType CriteriaDataType { get; set; }

    }
}
