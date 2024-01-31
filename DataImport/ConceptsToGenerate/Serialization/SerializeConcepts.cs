using GPAS.AccessControl;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.DataImport.ConceptsToGenerate.Serialization
{
    [Serializable]
    public class SerializeConcepts
    {
        protected SerializeConcepts()
        { }

        public SerializeConcepts(IEnumerable<ImportingObject> importingObjects, IEnumerable<ImportingRelationship> importingRelationships, long dataSourceID)
        {
            var importingObjectsInternalIDsDictionary = new Dictionary<ImportingObject, long>();
            ImportingObjectsInternalIDs = new List<SerializeObject>();
            long internalIdCounter = 0;
            foreach (var item in importingObjects)
            {
                importingObjectsInternalIDsDictionary.Add(item, ++internalIdCounter);
                ImportingObjectsInternalIDs.Add(new SerializeObject(item, internalIdCounter));
            }

            ImportingRelationships = new List<SerializeRelationship>();
            foreach (var item in importingRelationships)
            {
                long itemSourceInternalId = importingObjectsInternalIDsDictionary[item.Source];
                long itemTargetInternalId = importingObjectsInternalIDsDictionary[item.Target];
                var newItem = new SerializeRelationship(item, itemSourceInternalId, itemTargetInternalId);
                ImportingRelationships.Add(newItem);
            }

            DataSourceID = dataSourceID;
        }

        public List<SerializeObject> ImportingObjectsInternalIDs;
        public List<SerializeRelationship> ImportingRelationships;
        public long DataSourceID;

        internal static Tuple<List<ImportingObject>, List<ImportingRelationship>, long> GetImportingConceptsFromDeserializedInstance(SerializeConcepts importingConcepts)
        {
            var importingObjectsInternalIDsDictionary = new Dictionary<long, ImportingObject>();
            foreach (var item in importingConcepts.ImportingObjectsInternalIDs)
            {
                importingObjectsInternalIDsDictionary.Add(item.InternalId, item.BaseInstance);
            }

            foreach (var item in importingConcepts.ImportingRelationships)
            {
                item.BaseInstance.Source = importingObjectsInternalIDsDictionary[item.SourceInternalID];
                item.BaseInstance.Target = importingObjectsInternalIDsDictionary[item.TargetInternalID];
            }
            return new Tuple<List<ImportingObject>, List<ImportingRelationship>, long>
                (importingObjectsInternalIDsDictionary.Values.ToList()
                , importingConcepts.ImportingRelationships.Select(r => r.BaseInstance).ToList()
                , importingConcepts.DataSourceID);
        }
    }
}
