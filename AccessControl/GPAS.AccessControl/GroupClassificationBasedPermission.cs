using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPAS.AccessControl
{
    [DataContract]
    public class GroupClassificationBasedPermission
    {
        [DataMember]
        public string GroupName { get; set; }
        [DataMember]
        public List<ClassificationBasedPermission> Permissions { get; set; }
    }
}
