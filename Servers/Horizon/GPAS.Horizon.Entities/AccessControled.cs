using GPAS.AccessControl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Horizon.Entities
{
   public class AccessControled <T>
    {
        public ACL Acl;
        public T ConceptInstance;
    }
}
