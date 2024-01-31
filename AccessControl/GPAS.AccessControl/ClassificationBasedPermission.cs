using System.Runtime.Serialization;

namespace GPAS.AccessControl
{
    [DataContract]
    public class ClassificationBasedPermission
    {
        [DataMember]
        public ClassificationEntry Classification { get; set; }
        [DataMember]
        public Permission AccessLevel { get; set; }
    }
}
