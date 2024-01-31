using System;

namespace GPAS.DataImport.ConceptsToGenerate.Serialization
{
    [Serializable]
    public class SerializeRelationship
    {
        protected SerializeRelationship()
        { }

        public SerializeRelationship(ImportingRelationship baseInstance, long sourceInternalId, long targetInternalId)
        {
            BaseInstance = baseInstance;
            SourceInternalID = sourceInternalId;
            TargetInternalID = targetInternalId;
        }

        public ImportingRelationship BaseInstance;
        public long SourceInternalID;
        public long TargetInternalID;
    }
}
