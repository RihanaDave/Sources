using System.Collections.Generic;

namespace GPAS.SearchServer.Entities.SearchEngine.Documents
{
    public class ACL
    {
        public string ClassificationIdentifier { get; set; }
        public List<GroupPermission> Permissions{ get; set; }
    }
}
