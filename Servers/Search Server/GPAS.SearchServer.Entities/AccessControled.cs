using GPAS.AccessControl;

namespace GPAS.SearchServer.Entities
{
    public class AccessControled<T>
    {
        public ACL Acl { get; set; }
        public T ConceptInstance { get; set; }
    }
}
