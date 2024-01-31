using GPAS.StatisticalQuery.Formula;
using GPAS.StatisticalQuery.ObjectSet;
using System.Collections.Generic;

namespace GPAS.StatisticalQuery
{
    public class Query
    {
        public Query()
        {
            SourceSet =  StartingObjectSet.AllObjects;
            FormulaSequence = new List<FormulaStep>();
        }
        public StartingObjectSet SourceSet { get; set; }
        public List<FormulaStep> FormulaSequence { get; set; }
    }
}
