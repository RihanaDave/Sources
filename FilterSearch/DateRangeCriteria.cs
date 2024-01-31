using System;

namespace GPAS.FilterSearch
{
    public class DateRangeCriteria : CriteriaBase
    {
        private string startTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).ToString();
        public string StartTime
        {
            get { return startTime; }
            set { SetValue(ref startTime, value); }
        }

        private string endTime = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).ToString();
        public string EndTime
        {
            get { return endTime; }
            set { SetValue(ref endTime, value); }
        }
    }
}
