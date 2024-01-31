namespace GPAS.FilterSearch
{
    public class ContainerCriteria : CriteriaBase
    {
        private CriteriaSet criteriaSet = new CriteriaSet();
        public CriteriaSet CriteriaSet
        {
            get { return criteriaSet; }
            set { SetValue(ref criteriaSet, value); }
        }
    }
}
