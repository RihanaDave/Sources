using System;
using System.Runtime.Serialization;

namespace GPAS.AccessControl
{
    [Serializable]
    [DataContract]
    public class ACI
    {
        public ACI()
        { }

        [DataMember]
        public string GroupName { set; get; }

        [DataMember]
        public Permission AccessLevel { set; get; }
    }
}