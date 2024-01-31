using System.Runtime.Serialization;

namespace GPAS.FileRepository.Logic.Entities
{
    [DataContract]
    public class DirectoryContent
    {
        [DataMember]
        public DirectoryContentType ContentType { set; get; }
        [DataMember]
        public string UriAddress { set; get; }
        [DataMember]
        public string DisplayName { set; get; }
    }
}
