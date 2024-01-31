using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchAround.DataMapping
{
    [Serializable]
    public class SourceSetObjectMapping : ObjectMapping
    {
        private SourceSetObjectMapping()
        {

        }
        public SourceSetObjectMapping(OntologyTypeMappingItem objectType, string mappingTitle, List<ObjectMapping> subGroupObjects)
            : base(objectType, mappingTitle)
        {
            SubGroupObjects = subGroupObjects;
        }
        public List<ObjectMapping> SubGroupObjects = new List<ObjectMapping>();
    }
}
