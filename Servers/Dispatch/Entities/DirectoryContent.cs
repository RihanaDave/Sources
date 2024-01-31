using System.Runtime.Serialization;

namespace GPAS.Dispatch.Entities
{
    [DataContract]
    public class DirectoryContent
    {
        [DataMember]
        public DirectoryContentType ContentType { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string UriAddress { get; set; }
    }
}