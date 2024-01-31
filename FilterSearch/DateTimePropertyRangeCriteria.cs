using System;

namespace GPAS.FilterSearch
{
    public class DateTimePropertyRangeCriteria : CriteriaBase
    {
        private DateTime startTime = DateTime.MinValue;
        public DateTime StartTime
        {
            get { return startTime; }
            set { SetValue(ref startTime, value); }
        }

        private DateTime endTime = DateTime.MaxValue;
        public DateTime EndTime
        {
            get { return endTime; }
            set { SetValue(ref endTime, value); }
        }

        private string propertyTypeUri = string.Empty;
        public string PropertyTypeUri
        {
            get { return propertyTypeUri; }
            set { SetValue(ref propertyTypeUri, value); }
        }
    }
}
