using System.Runtime.Serialization;

namespace GPAS.StatisticalQuery.Formula.DrillDown.TypeBased
{
    [DataContract]
    public class HasPropertyWithType : TypeBasedDrillDownPortionBase
    {
        public HasPropertyWithType()
        {

        }
        [DataMember]
        public string PropertyTypeUri { get; set; }
        public override string ToString()
        {
            return $"Has property type '{PropertyTypeUri}'";
        }
    }
}
