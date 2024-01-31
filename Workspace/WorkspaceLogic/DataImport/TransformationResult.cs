using System.Collections.Generic;
using GPAS.DataImport.ConceptsToGenerate;

namespace GPAS.Workspace.Logic.DataImport
{
    public class TransformationResult
    {
        public List<ImportingObject> GeneratingObjects { get; internal set; }
        public List<ImportingRelationship> GeneratingRelationships { get; internal set; }
        public long InvalidLinesCount { get; internal set; }
    }
}