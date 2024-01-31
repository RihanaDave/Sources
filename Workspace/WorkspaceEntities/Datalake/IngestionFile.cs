using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Entities.Datalake
{
    public class IngestionFile
    {
        public Guid id { get; set; }
        public string Category { get; set; }
        public DateTime DataFlowDateTime { get; set; }
        public string FilePath { get; set; }        
        public FileSeparator FileSeparator { get; set; }
        public string Headers { get; set; }
        public DateTime TimeBegin { get; set; }
        public IngestionFile()
        {            
            Category = string.Empty;
            FilePath = string.Empty;
            DataFlowDateTime = DateTime.Now;
            FileSeparator = FileSeparator.Comma;
            Headers = string.Empty;
            TimeBegin = DateTime.Now;
        }
    }
}
