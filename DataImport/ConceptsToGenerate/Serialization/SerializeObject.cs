using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataImport.ConceptsToGenerate.Serialization
{
    public class SerializeObject
    {
        protected SerializeObject()
        { }

        public SerializeObject(ImportingObject baseInstance, long internalId)
        {
            BaseInstance = baseInstance;
            InternalId = internalId;
        }

        public ImportingObject BaseInstance;
        public long InternalId;
    }
}
