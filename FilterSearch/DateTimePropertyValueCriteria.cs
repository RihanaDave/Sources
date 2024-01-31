using System;
using System.ComponentModel;
using System.Xml.Serialization;

namespace GPAS.FilterSearch
{
    public class DateTimePropertyCriteriaOperatorValuePair : PropertyCriteriaOperatorValuePair
    {
        private DateTime criteriaValue;
        public DateTime CriteriaValue
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

        [XmlType("GPAS.FilterSearch.DateTimePropertyCriteriaOperatorValuePair.RelationalOperator")]
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