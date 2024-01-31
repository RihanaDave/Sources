namespace GPAS.FilterSearch
{
    public class PropertyValueCriteria : CriteriaBase
    {
        public PropertyValueCriteria()
        {

        }

        private string propertyTypeUri = string.Empty;
        public string PropertyTypeUri
        {
            get { return propertyTypeUri; }
            set { SetValue(ref propertyTypeUri, value); }
        }

        private PropertyCriteriaOperatorValuePair operatorValuePair = new EmptyPropertyCriteriaOperatorValuePair();
        public PropertyCriteriaOperatorValuePair OperatorValuePair
        {
            get { return operatorValuePair; }
            set { SetValue(ref operatorValuePair, value); }
        }
    }
}
