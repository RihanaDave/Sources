using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Horizon.Entities.Graph
{
  public  class Vertex
    {
        public long ID { set; get; }
        public string TypeUri { set; get; }
        public List<VertexProperty> Properties { set; get; }
    }
}
