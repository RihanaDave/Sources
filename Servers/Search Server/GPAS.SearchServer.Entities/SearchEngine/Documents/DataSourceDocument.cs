using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Entities.SearchEngine.Documents
{
    public class DataSourceDocument
    {
        public ACL Acl { set; get; }
        
        public string Description { get; set; }
        
        public string Id { get; set; }
        
        public string Name { get; set; }
        
        public long Type { get; set; }

        public string CreatedBy { get; set; }

        public string CreatedTime { get; set; }

    }
}
