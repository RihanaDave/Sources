using GPAS.Dispatch.Entities.Concepts;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPAS.Dispatch.Entities.Publish
{
    [DataContract]
    public class ModifiedConcepts
    {
        [DataMember]
        public List<ModifiedProperty> ModifiedProperties { set; get; }

        [DataMember]
        public List<KMedia> DeletedMedias { set; get; }
    }
}