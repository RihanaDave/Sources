using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataImport.InternalResolve
{
    public class IRObjectsCollection
    {
        private Dictionary<MustMatchPropertiesCollection, IRObject> objectsWithMustMatchProperties
            = new Dictionary<MustMatchPropertiesCollection, IRObject>();
        private HashSet<IRObject> objectsWithoutMustMatchProperties
            = new HashSet<IRObject>();

        public void Add(IRObject obj)
        {
            if (obj.MustMatchProperties.Length > 0)
            {
                MustMatchPropertiesCollection objMMPropertiesCollection
                    = new MustMatchPropertiesCollection(obj.MustMatchProperties);
                objectsWithMustMatchProperties.Add(objMMPropertiesCollection, obj);
            }
            else
                objectsWithoutMustMatchProperties.Add(obj);
            TotalAddedObjectsCount++;
        }

        public int TotalAddedObjectsCount
        {
            get;
            private set;
        }

        internal IEnumerable<IRObject> GetObjects()
        {
            return objectsWithMustMatchProperties.Values
                .Concat(objectsWithoutMustMatchProperties);
        }

        internal List<IRRelationshipEnd> GetRelationshipEnds()
        {            
            List<IRRelationshipEnd> result = new List<IRRelationshipEnd>
                (objectsWithMustMatchProperties.Count
                + objectsWithoutMustMatchProperties.Count);
            result.AddRange(objectsWithMustMatchProperties.Values.Select(endObj => new IRRelationshipPropertyBasedEnd(endObj.MustMatchProperties)));
            result.AddRange(objectsWithoutMustMatchProperties.Select(endObj => new IRRelationshipObjectBasedEnd(endObj)));
            return result;
        }

        internal bool TryGetSameMustMatchObject(IRObject checkingObject, out IRObject existObjectWithSameProperties)
        {
            existObjectWithSameProperties = null;
            MustMatchPropertiesCollection objMMPropertiesCollection
                = new MustMatchPropertiesCollection(checkingObject.MustMatchProperties);
            if (!objectsWithMustMatchProperties.ContainsKey(objMMPropertiesCollection))
                return false;
            else
            {
                existObjectWithSameProperties = objectsWithMustMatchProperties[objMMPropertiesCollection];
                return true;
            }
        }
    }
}