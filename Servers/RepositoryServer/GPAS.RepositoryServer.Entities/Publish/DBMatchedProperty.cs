using System.Runtime.Serialization;

namespace GPAS.RepositoryServer.Entities.Publish
{
    [DataContract]
    public class DBMatchedProperty
    {
        [DataMember]
        public string TypeUri { get; set; }
        [DataMember]
        public string Value { get; set; }
    }
}