using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Entities
{
    public class PropertiesMatchingResults
    {
        public long SearchedPropertyID { get; set; }
        public long[] ResultPropertiesID { get; set; }
    }
}
