using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPAS.StatisticalQuery.Formula.DrillDown.TypeBased
{
    [DataContract]
    public class TypeBasedDrillDown : FormulaStep
    {
        public TypeBasedDrillDown()
        {
            Portions = new List<TypeBasedDrillDownPortionBase>();
        }

        [DataMember]
        public List<TypeBasedDrillDownPortionBase> Portions { get; set; }
    }
}
