using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.StatisticalQuery.Formula.DrillDown.TypeBased
{
    [DataContract]
    public class LinkBasedDrillDown : FormulaStep
    {
        public LinkBasedDrillDown() 
        {
            Portions = new List<LinkBasedDrillDownPortionBase>();
        }

        [DataMember]
        public List<LinkBasedDrillDownPortionBase> Portions { get; set; }
    }
}
