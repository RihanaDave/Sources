using System.Collections.Generic;
using System.Data;

namespace GPAS.LoadTest.Core
{
    public class LoadTestResult
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DataTable Statistics { get; set; }
        public string TotalPublishTimeString { get; set; }
        public string AveragePublishTimeString { get; set; }
        public string ClearAllDataTimeString { get; set; }
    }
}
