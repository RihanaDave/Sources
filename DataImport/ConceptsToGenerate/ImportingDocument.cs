using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataImport.ConceptsToGenerate
{
    [Serializable]
    public class ImportingDocument : ImportingObject
    {
        protected ImportingDocument()
            : base()
        { }

        public ImportingDocument(string typeUri, ImportingProperty labelProperty)
            : base(typeUri, labelProperty)
        {
            
        }

        public string DocumentPath { get; set; }
    }
}
