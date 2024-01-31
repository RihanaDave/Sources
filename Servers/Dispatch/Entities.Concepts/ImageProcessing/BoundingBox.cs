using System.Drawing;
using System.Runtime.Serialization;
using System.Windows.Input;

namespace GPAS.Dispatch.Entities.Concepts.ImageProcessing
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