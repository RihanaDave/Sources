using System;
using System.Collections.Generic;

namespace GPAS.SearchServer.Entities.SearchEngine.Documents
{
    public class File
    {
        public string Id { get; set; }
        public List<string> OwnerObjectIds { get; set; }
        public string Description { get; set; }
        public byte[] Content { get; set; }
        public ACL Acl { get; set; }
        public DateTime? CreatedTime { get; set; }
        public DateTime? ImportDate { get; set; }
        public long FileSize { get; set; }
        public string RelatedWord { get; set; }
        public string FileType { get;set; }
    }
}
