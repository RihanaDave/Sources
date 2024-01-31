using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataImport.ConceptsToGenerate
{
    [Serializable]
    public enum ImportingRelationshipDirection
    {
        SourceToTarget = 1,
        TargetToSource = 2,
        Bidirectional = 3
    }
}
