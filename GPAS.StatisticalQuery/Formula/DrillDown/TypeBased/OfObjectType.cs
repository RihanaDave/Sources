using System.Runtime.Serialization;

namespace GPAS.StatisticalQuery.Formula.DrillDown.TypeBased
{
    [DataContract]
    public class OfObjectType : TypeBasedDrillDownPortionBase
    {
        public OfObjectType()
        {

        }
        [DataMember]
        public string ObjectTypeUri { get; set; }
        public override string ToString()
        {
            return $"Of type '{ObjectTypeUri}'";
        }
    }
}
