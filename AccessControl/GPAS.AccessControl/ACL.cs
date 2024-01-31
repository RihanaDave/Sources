using Newtonsoft.Json;
using System;
using System.IO;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GPAS.Utility;

namespace GPAS.AccessControl
{
    [Serializable]
    [DataContract]
    public class ACL
    {
        public ACL()
        {
            Permissions = new List<ACI>();
        }

        [DataMember]
        public string Classification { set; get; }

        [DataMember]
        public List<ACI> Permissions { set; get; }

        public string ToJsonString()
        {
            string jsonString = JsonConvert.SerializeObject(this, Formatting.None,
              new JsonSerializerSettings()
              {
                  ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
              });
            return jsonString;
        }

        public Stream ToJsonStream()
        {
            StreamUtility utilities = new StreamUtility();
            return utilities.GenerateStreamFromString(ToJsonString());
        }
    }
}
