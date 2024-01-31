using GPAS.AccessControl;

namespace GPAS.SearchServer.Entities.SearchEngine.Documents
{
    public class GroupPermission
    {
        public string GroupName { get; set; }
        public Permission AccessLevel { get; set; }
    }
}
