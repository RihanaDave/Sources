using GPAS.DataImport.DataMapping.SemiStructured;
using GPAS.DataImport.InternalResolve;
using System.Collections.Generic;

namespace GPAS.DataImport.Transformation
{
    internal class ExtractedConcepts
    {
        internal Dictionary<ObjectMapping, IRObjectsCollection> IRObjectsPerMappings;
        internal Dictionary<RelationshipMapping, HashSet<IRRelationship>> IRRelationshipsPerMappings;
    }
}
