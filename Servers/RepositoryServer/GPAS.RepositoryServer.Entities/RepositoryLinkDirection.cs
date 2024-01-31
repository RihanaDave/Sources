using System.Runtime.Serialization;

namespace GPAS.RepositoryServer.Entities
{
    [DataContract]
    public enum RepositoryLinkDirection
    {
        [EnumMember]
        SourceToTarget = 1,
        [EnumMember]
        TargetToSource = 2,
        [EnumMember]
        Bidirectional = 3
    }
}
