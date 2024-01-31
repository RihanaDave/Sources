using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Entities.ImageProcessing
{
    public class RetrievedFaceKWObject
    {
        public KWObject kwObject { get; set; }

        public double distance { get; set; }

        public List<KWBoundingBox> boundingBox { get; set; }

    }    
}
