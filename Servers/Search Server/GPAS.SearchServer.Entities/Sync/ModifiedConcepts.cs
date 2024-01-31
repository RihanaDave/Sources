using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPAS.SearchServer.Entities.Sync
{
    [DataContract]
    public class ModifiedConcepts
    {
        [DataMember]
        public ModifiedProperty[] ModifiedProperties { set; get; }

        [DataMember]
        public List<SearchMedia> DeletedMedias { set; get; }
    }
}
