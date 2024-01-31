using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Horizon.GraphRepositoryPlugins.OrientRestV3
{
    public class OProperty
    {
        public string Type { get; set; }
        public List<object> Values { get; set; }
    }
}
