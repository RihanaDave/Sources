using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Horizon.Entities.Graph
{
 public class VertexProperty
    {
        public string TypeUri { set; get; }
        public string Value { set; get; }
        public long OwnerVertexID { set; get; }
        public string OwnerVertexTypeURI{ get; set; }

    }
}
