using System.ComponentModel;
using System.Xml.Serialization;

namespace GPAS.FilterSearch
{
    public class FloatPropertyCriteriaOperatorValuePair : PropertyCriteriaOperatorValuePair
    {
        private float criteriaValue;
        public float CriteriaValue
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

        private int equalityPrecision;
        public int EqualityPrecision
        {
            get { return equalityPrecision; }
            set { SetValue(ref equalityPrecision, value); }
        }

        [XmlType("GPAS.FilterSearch.FloatPropertyCriteriaOperatorValuePair.RelationalOperator")]
        public enum RelationalOperator
        {
            [Description("=")]
            Equals = 0,
            [Description(">")]
            GreaterThan = 1,
            [Description("<")]
            LessThan = 2,
            [Description(">=")]
            GreaterThanOrEquals = 3,
            [Description("<=")]
            LessThanOrEquals = 4,
            [Description("!=")]
            NotEquals = 5
        }
    }
}