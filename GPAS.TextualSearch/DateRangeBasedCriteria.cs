using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.TextualSearch
{
   public class DateRangeBasedCriteria: BaseSearchCriteria
    {
        public string CriteriaName
        {
            get;
            set;
        }

        public DateTime? CriteriaStartValue
        {
            get;
            set;
        }

        public DateTime? CriteriaEndValue
        {
            get;
            set;
        }
    }
}
