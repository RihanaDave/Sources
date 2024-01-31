using GPAS.Dispatch.Entities.Concepts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataImport.ConceptsToGenerate.Serialization
{
    public class MappingEntity
    {
        protected MappingEntity()
        { }

        public MappingEntity(ImportingObject importingObj, KObject kobj)
        {
            ImportingObj = importingObj;
            Kobj = kobj;
        }

        public ImportingObject ImportingObj;
        public KObject Kobj;
     
    }
}
