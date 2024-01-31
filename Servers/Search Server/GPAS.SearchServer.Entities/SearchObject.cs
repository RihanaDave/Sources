using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPAS.SearchServer.Entities
{
    [DataContract]
    public class SearchObject
    {
        [DataMember]
        public string TypeUri { set; get; }

        [DataMember]
        public long? LabelPropertyID { set; get; }

        [DataMember]
        public long Id { set; get; }

        [DataMember]
        public int IsMaster { get; set; }

        [DataMember]
        public SearchObjectMaster SearchObjectMaster { get; set; }

        [DataMember]
        public List<SearchObject> Slaves
        {
            get;
            set;
        }
    }
}
