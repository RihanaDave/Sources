using GPAS.DataImport.ConceptsToGenerate;
using GPAS.DataImport.InternalResolve;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataImport.Transformation
{
    internal class ImportingObjectsCollection
    {
        private Dictionary<IRRelationshipPropertyBasedEnd, ImportingObject> objectsWithMustMatchProperties
            = new Dictionary<IRRelationshipPropertyBasedEnd, ImportingObject>();
        private Dictionary<IRRelationshipObjectBasedEnd, ImportingObject> objectsWithoutMustMatchProperties
            = new Dictionary<IRRelationshipObjectBasedEnd, ImportingObject>();

        public void Add(IRRelationshipEnd relEnd, ImportingObject obj)
        {
            if (relEnd is IRRelationshipPropertyBasedEnd)
            {
                if (!(objectsWithMustMatchProperties.ContainsKey(relEnd as IRRelationshipPropertyBasedEnd)))
                {
                    objectsWithMustMatchProperties.Add(relEnd as IRRelationshipPropertyBasedEnd, obj);
                }
            }
            else if (relEnd is IRRelationshipObjectBasedEnd)
            {
                if (!(objectsWithoutMustMatchProperties.ContainsKey(relEnd as IRRelationshipObjectBasedEnd)))
                {
                    objectsWithoutMustMatchProperties.Add(relEnd as IRRelationshipObjectBasedEnd, obj);
                }
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        internal bool TryGetMatchObject(IRRelationshipEnd relEnd, out ImportingObject matchedObject)
        {
            if (relEnd is IRRelationshipPropertyBasedEnd)
                return objectsWithMustMatchProperties.TryGetValue(relEnd as IRRelationshipPropertyBasedEnd, out matchedObject);
            else if (relEnd is IRRelationshipObjectBasedEnd)
                return objectsWithoutMustMatchProperties.TryGetValue(relEnd as IRRelationshipObjectBasedEnd, out matchedObject);
            else
                throw new NotSupportedException();
        }

        internal IEnumerable<ImportingObject> GetObjects()
        {
            return objectsWithMustMatchProperties.Values
                .Concat(objectsWithoutMustMatchProperties.Values);
        }
    }
}
