using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch.Entities.DatalakeEntities
{
    public class StreamingIngestion
    {
        public Guid id { get; set; }
        public string Category { get; set; }
        public string InputPort { get; set; }
        public string Headers { get; set; }
        public DateTime? RelatedDateTime { get; set; }
        public DateTime startTime { get; set; }
        public FileSeparator StreamingDataSeparator { get; set; }

        public StreamingIngestion()
        {
            Category = string.Empty;
            InputPort = string.Empty;
            RelatedDateTime = null;
            StreamingDataSeparator = FileSeparator.Comma;
            startTime = DateTime.Now;
        }
    }
}
