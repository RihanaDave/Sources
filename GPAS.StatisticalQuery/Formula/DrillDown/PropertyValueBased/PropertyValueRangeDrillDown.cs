using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.StatisticalQuery.Formula.DrillDown.PropertyValueBased
{
    [DataContract]
    public class PropertyValueRangeDrillDown : FormulaStep
    {
        public PropertyValueRangeDrillDown()
        {

        }

        [DataMember]
        public PropertyValueRangeStatistics DrillDownDetails { get; set; }
    }
}
