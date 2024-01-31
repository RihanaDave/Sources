using System;
using GPAS.DataImport.InternalResolve;

namespace GPAS.DataImport.ConceptsToGenerate
{
    public partial class ImportingRelationship
    {
        protected ImportingRelationship()
        { }

        public string Description;
        public ImportingRelationshipDirection Direction;
        public DateTime? TimeBegin;
        public DateTime? TimeEnd;
        [NonSerialized]
        public ImportingObject Source;
        [NonSerialized]
        public ImportingObject Target;
        public string TypeURI;

        public ImportingRelationship(ImportingObject source, ImportingObject target, string typeURI, IRRelationshipDirection direction, DateTime? timeBegin, DateTime? timeEnd, string description)
        {
            Source = source;
            Target = target;
            TypeURI = typeURI;
            Direction = ConvertIRRelationshipDirectionToImportingDirection(direction);
            TimeBegin = timeBegin;
            TimeEnd = timeEnd;
            Description = description;
        }

        private ImportingRelationshipDirection ConvertIRRelationshipDirectionToImportingDirection(IRRelationshipDirection direction)
        {
            switch (direction)
            {
                case IRRelationshipDirection.SourceToTarget:
                    return ImportingRelationshipDirection.SourceToTarget;
                case IRRelationshipDirection.TargetToSource:
                    return ImportingRelationshipDirection.TargetToSource;
                case IRRelationshipDirection.Bidirectional:
                    return ImportingRelationshipDirection.Bidirectional;
                default:
                    throw new InvalidOperationException("Unknown direction for relationship");
            }
        }
    }
}
