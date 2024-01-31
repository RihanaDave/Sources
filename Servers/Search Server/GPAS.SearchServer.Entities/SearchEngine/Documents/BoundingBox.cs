using System.Drawing;
using System.Runtime.Serialization;

namespace GPAS.SearchServer.Entities.SearchEngine.Documents
{
    [DataContract]
    public class BoundingBox
    {
        [DataMember]
        public Point topLeft { get; set; }
        
        [DataMember]
        public int width { get; set; }

        [DataMember]
        public int height { get; set; }

        [DataMember]
        public Landmarks landmarks { get; set; }
    }
}