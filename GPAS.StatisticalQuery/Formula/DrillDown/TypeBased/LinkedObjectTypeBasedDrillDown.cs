using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.StatisticalQuery.Formula.DrillDown.TypeBased
{
    [DataContract]
    public class LinkedObjectTypeBasedDrillDown : LinkBasedDrillDownPortionBase
    {
        public LinkedObjectTypeBasedDrillDown(){}

        [DataMember]
        public string LinkedObjectTypeUri { get; set; }
    }
}
