using GPAS.Dispatch.Entities.Concepts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataImport.ConceptsToGenerate.Serialization
{
    [Serializable]
    public class SerializeMappings
    {
        public List<MappingEntity> records;

        protected SerializeMappings()
        { }

        public SerializeMappings(Dictionary<ImportingObject, KObject> mapping)
        {
            records = new List<MappingEntity>();

            foreach (var currentMapping in mapping.Keys)
            {
                records.Add(new MappingEntity(currentMapping, mapping[currentMapping]));
            }
        }

        internal static Dictionary<ImportingObject, KObject> GetMappingDictionaryFromSerializeMappings(SerializeMappings serializeMappings)
        {
            Dictionary<ImportingObject, KObject> result = new Dictionary<ImportingObject, KObject>();

            foreach (var currentEntity in serializeMappings.records)
            {
                result.Add(currentEntity.ImportingObj, currentEntity.Kobj);
            }

            return result;
        }
    }
}
