namespace GPAS.FilterSearch
{
    public class KeywordCriteria : CriteriaBase
    {
        private string keyword = string.Empty;
        public string Keyword
        {
            get { return keyword; }
            set { SetValue(ref keyword, value); }
        }
    }
}
