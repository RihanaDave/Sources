namespace GPAS.FilterSearch
{
    public class Query
    {
        public Query()
        { }

        public CriteriaSet CriteriasSet = new CriteriaSet();
        /// <summary>
        /// مقداری را برای نشان دادن فیلتر خالی (بدون شرط) ارائه می‌دهد
        /// </summary>
        public bool IsEmpty()
        {
            return CriteriasSet == null || CriteriasSet.IsEmpty();
        }
    }
}