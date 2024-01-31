using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.TextualSearch
{
  public  class DoubleRangeBasedCriteria: BaseSearchCriteria
    {
        public string CriteriaName
        {
            get;
            set;
        }

        public double CriteriaStartValue
        {
            get;
            set;
        }

        public double CriteriaEndValue
        {
            get;
            set;
        }
    }
}
