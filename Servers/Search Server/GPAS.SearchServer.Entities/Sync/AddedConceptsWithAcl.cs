using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Entities.Sync
{
    public class AddedConceptsWithAcl
    {
        public List<SearchObject> AddedNonDocumentObjects { get; set; }

        public List<AccessControled<SearchObject>> AddedDocuments { get; set; }

        public List<AccessControled<SearchProperty>> AddedProperties { set; get; }

        public List<AccessControled<SearchRelationship>> AddedRelationships { set; get; }

        public List<AccessControled<SearchMedia>> AddedMedias { get; set; }
    }
}
