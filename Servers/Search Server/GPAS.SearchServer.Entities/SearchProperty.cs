using System.Runtime.Serialization;

namespace GPAS.SearchServer.Entities
{
    [DataContract]
    public class SearchProperty
    {
        [DataMember]
        public string TypeUri { set; get; }

        [DataMember]
        public string Value { set; get; }

        [DataMember]
        public long Id { set; get; }

        [DataMember]
        public SearchObject OwnerObject { set; get; }
        [DataMember]
        public long DataSourceID { set; get; }
    }
}
