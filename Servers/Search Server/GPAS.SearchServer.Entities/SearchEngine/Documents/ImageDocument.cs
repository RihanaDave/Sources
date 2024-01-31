using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Entities.SearchEngine.Documents
{
    public class ImageDocument
    {
        public static readonly int NumberOfFeatue = 512;

        public static string GetFieldName(int featueNumber)
        {
            return "e" + featueNumber.ToString();
        }

        public ACL ACL { get; set; }

        public string ImageId { get; set; }

        public string Description { get; set; }

        public List<FaceSpecification> Faces { get; set; }
    }
}
