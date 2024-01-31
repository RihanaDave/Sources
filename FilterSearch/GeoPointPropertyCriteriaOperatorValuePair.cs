using GPAS.Dispatch.Entities.Concepts.Geo;
using System.ComponentModel;
using System.Xml.Serialization;

namespace GPAS.FilterSearch
{
    public class GeoPointPropertyCriteriaOperatorValuePair : PropertyCriteriaOperatorValuePair
    {
        private GeoCircleEntityRawData criteriaValue;
        public GeoCircleEntityRawData CriteriaValue
        {
            get { return criteriaValue; }
            set { SetValue(ref criteriaValue, value); }
        }

        private RelationalOperator criteriaOperator;
        public RelationalOperator CriteriaOperator
        {
            get { return criteriaOperator; }
            set { SetValue(ref criteriaOperator, value); }
        }

        [XmlType("GPAS.FilterSearch.GeoPointPropertyCriteriaOperatorValuePair.RelationalOperator")]
        public enum RelationalOperator
        {
            [Description("=")]
            Equals = 0,
            [Description("!=")]
            NotEquals = 5
        }
    }
}
