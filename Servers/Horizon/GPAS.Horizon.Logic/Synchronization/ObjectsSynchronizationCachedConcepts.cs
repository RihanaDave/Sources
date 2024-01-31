using GPAS.DataSynchronization;
using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.Entities.Publish;
using GPAS.Horizon.Entities;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.Horizon.Logic.Synchronization
{
    internal class ObjectsSynchronizationCachedConcepts : CachedConcepts
    {
        public ObjectsSynchronizationCachedConcepts
            (List<KObject> addedObjects, List<KProperty> addedProperties
            , List<ModifiedProperty> modifiedProperties)
        {
            AddedObjects = addedObjects;
            AddedProperties = addedProperties;
            ModifiedProperties = modifiedProperties;
        }

        internal List<KObject> AddedObjects { get; private set; }
        internal List<KProperty> AddedProperties { get; private set; }
        internal List<ModifiedProperty> ModifiedProperties { get; private set; }

        public override IEnumerable<long> GetCachedConceptIDs()
        {
            return AddedObjects.Select(o => o.Id)
                .Concat(AddedProperties.Select(p => p.Owner.Id))
                .Concat(ModifiedProperties.Select(mp => mp.OwnerObjectID))
                .Distinct();
        }

        public List<KObject> GetAddedObjectForSpecificObject(long objID)
        {
            foreach (KObject obj in AddedObjects)
            {
                if (obj.Id.Equals(objID))
                {
                    return new List<KObject>() { obj };
                }
            }
            return new List<KObject>();
        }
        public List<KProperty> GetAddedPropertiesForSpecificObject(long objID)
        {
            return AddedProperties.Where(p => p.Owner.Id == objID).ToList();
        }
        public List<ModifiedProperty> GetModifiedPropertiesForSpecificObject(long objID)
        {
            return ModifiedProperties.Where(mp => mp.OwnerObjectID == objID).ToList();
        }
    }
}
