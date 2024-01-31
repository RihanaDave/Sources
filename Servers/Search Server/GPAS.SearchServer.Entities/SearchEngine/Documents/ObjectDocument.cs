
using System.Collections.Generic;

namespace GPAS.SearchServer.Entities.SearchEngine.Documents
{
    public class ObjectDocument
    {
        public string Id { get; set; }

        public string TypeUri { get; set; }

        public long? LabelPropertyID { set; get; }

        public bool IsGroup { set; get; }

        public long? ResolvedTo { set; get; }

        public List<Property> Properties { get; set; }
        public List<Relationship> Relationships { get; set; }
        public ObjectDocument()
        {
            Properties = new List<Property>();
            Relationships = new List<Relationship>();
        }
    }
}
