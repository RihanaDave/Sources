using System;
using System.Collections.Generic;

namespace GPAS.DataImport.InternalResolve.InterTypeResolve
{
    internal class ITIRObjectsCollection
    {
        private Dictionary<ITIRPropertiesCollection, ITIRObject> objectsPerProperties
            = new Dictionary<ITIRPropertiesCollection, ITIRObject>();

        internal void Add(ITIRObject obj)
        {
            objectsPerProperties.Add(obj.InterTypeMustMatchProperties, obj);
        }
        internal void UpdateBaseObjectChanges(ITIRObject obj, HashSet<string> interTypeMustMatchPropertiesTypeURI)
        {
            objectsPerProperties.Remove(obj.InterTypeMustMatchProperties);
            ITIRObject newObj = new ITIRObject(obj.BaseObject, interTypeMustMatchPropertiesTypeURI);
            Add(newObj);
        }

        internal bool TryGetSameITIRObject(ITIRObject checkingObject, out ITIRObject existObjectWithSameProperties)
        {
            return objectsPerProperties.TryGetValue(checkingObject.InterTypeMustMatchProperties, out existObjectWithSameProperties);
        }

        internal IEnumerable<ITIRObject> GetObjects()
        {
            return objectsPerProperties.Values;
        }
    }
}