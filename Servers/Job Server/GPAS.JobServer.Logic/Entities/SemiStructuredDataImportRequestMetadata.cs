using System.Runtime.Serialization;

namespace GPAS.JobServer.Logic.Entities
{
    [DataContract]
    public class SemiStructuredDataImportRequestMetadata
    {
        [DataMember]
        public byte[] serializedMaterialBase { get; set; }

        [DataMember]
        public byte[] serializedTypeMapping { get; set; }

        [DataMember]
        public byte[] serializedACL { get; set; }
    }
}
