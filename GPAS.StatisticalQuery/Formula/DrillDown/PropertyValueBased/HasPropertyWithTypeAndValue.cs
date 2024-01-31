using System.Runtime.Serialization;

namespace GPAS.StatisticalQuery.Formula.DrillDown.PropertyValueBased
{
    [DataContract]
    public class HasPropertyWithTypeAndValue
    {
        public HasPropertyWithTypeAndValue()
        {

        }

        [DataMember]
        public string PropertyTypeUri { get; set; }

        [DataMember]
        public string PropertyValue { get; set; }

        public override string ToString()
        {
            return $"Has property type '{PropertyTypeUri}' and value '{PropertyValue}'";
        }
    }
}
