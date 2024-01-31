using System.Collections.Generic;
using GPAS.Dispatch.Entities.Concepts;

namespace GPAS.Dispatch.Entities.IndexChecking
{
    public class SearchIndexCheckingInput
    {
        public SearchIndexCheckingInput()
        {
            Properties = new List<KProperty>();
            RelationsIds = new List<long>();
        }

        public long ObjectId { set; get; }

        public string ObjectType { set; get; }

        public byte[] DocumentContent { set; get; }

        public List<KProperty> Properties { set; get; }

        public List<long> RelationsIds { set; get; }
    }
}
