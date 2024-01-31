using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataImport.ConceptsToGenerate
{
    [Serializable]
    public class ImportingProperty
    {
        public ImportingProperty()
        { }

        public string TypeURI
        {
            get;
            set;
        }
        public string Value
        {
            get;
            set;
        }

        public ImportingProperty(string typeURI, string value)
        {
            TypeURI = typeURI;
            Value = value;
        }
    }
}
