using System.ComponentModel;
using System.Xml.Serialization;

namespace GPAS.FilterSearch
{
    public class BooleanPropertyCriteriaOperatorValuePair : PropertyCriteriaOperatorValuePair
    {
        private bool criteriaValue;
        public bool CriteriaValue
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

        [XmlType("GPAS.FilterSearch.BooleanPropertyCriteriaOperatorValuePair.RelationalOperator")]
        public enum RelationalOperator
        {
            [Description("=")]
            Equals = 0,
            [Description("!=")]
            NotEquals = 5
        }
    }
}