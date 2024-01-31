using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.TextualSearch
{
    public class DoubleBasedCriteria : BaseSearchCriteria
    {
        public string CriteriaName
        {
            get;
            set;
        }

        public double CriteriaValue
        {
            get;
            set;
        }
    }
}
