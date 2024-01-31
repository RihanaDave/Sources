using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPAS.StatisticalQuery.Formula.DrillDown.PropertyValueBased
{
    [DataContract]
    public class PropertyValueBasedDrillDown : FormulaStep
    {
        public PropertyValueBasedDrillDown()
        {

        }

        [DataMember]
        public List<HasPropertyWithTypeAndValue> Portions { get; set; }
    }
}
