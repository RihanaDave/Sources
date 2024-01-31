using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Entities.SearchEngine.Documents
{
    public class Relationship
    {
        public string Id { get; set; }
        public string LinkTypeUri { get; set; }
        public string SourceObjectId { get; set; }
        public string SourceObjectTypeUri { get; set; }
        public string TargetObjectId { get; set; }
        public string TargetObjectTypeUri { get; set; }
        public long DataSourceId { get; set; }
        public int Direction { get; set; }
        public ACL Acl { get; set; }
    }
}
