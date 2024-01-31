using GPAS.Dispatch.Entities.Concepts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Horizon.Entities
{
    public class AddedConceptsWithAcl
    {
        public List<KObject> AddedObjects { set; get; }

        public List<KProperty> AddedProperties { set; get; }

        public List<AccessControled<RelationshipBaseKlink>> AddedRelationshipsWithAcl { set; get; }

        public List<KMedia> AddedMedias { set; get; }
    }
}
