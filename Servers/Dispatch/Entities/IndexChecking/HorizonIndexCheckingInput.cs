using System.Collections.Generic;
using GPAS.Dispatch.Entities.Concepts;

namespace GPAS.Dispatch.Entities.IndexChecking
{
    public class HorizonIndexCheckingInput
    {
        public HorizonIndexCheckingInput()
        {
            Properties = new List<KProperty>();
            RelationsIds = new List<long>();
        }

        public long ObjectId { set; get; }

        public string ObjectTypeUri { set; get; }

        public long ResultLimit { set; get; }
        
        public List<KProperty> Properties { set; get; }
        
        public List<long> RelationsIds { set; get; }
    }
}
