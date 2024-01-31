using System.ComponentModel;
using System.Xml.Serialization;

namespace GPAS.FilterSearch
{
    public class StringPropertyCriteriaOperatorValuePair : PropertyCriteriaOperatorValuePair
    {
        private string criteriaValue = string.Empty;
        public string CriteriaValue
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

        public string GetStringOfLIKERelationalOperator()
        {
            return Properties.Resources.Like_Operator;
        }

        [XmlType("GPAS.FilterSearch.StringPropertyCriteriaOperatorValuePair.RelationalOperator")]
        public enum RelationalOperator
        {
            [Description("=")]
            Equals = 0,
            [Description("!=")]
            NotEquals = 5,
            Like = 6
        }
    }
}