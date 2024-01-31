using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Horizon.GraphRepositoryPlugins.OrientRestV3
{
    public class ODocument
    {
        public List<OProperty> Properties { get; set; }

        public ODocument()
        {
            Properties = new List<OProperty>();
        }

        internal bool HasField(string propertyType)
        {
            bool result = false;

            foreach (var currentProperty in Properties)
            {
                if (currentProperty.Type.Equals(propertyType))
                {
                    result = true;
                    break;
                }
            }

            return result;
        }

        internal List<object> GetPropertyValue(string propertyType)
        {
            List<object> result = null;
            foreach (var currentProperty in Properties)
            {
                if (currentProperty.Type.Equals(propertyType))
                {
                    result = currentProperty.Values;
                    break;
                }
            }
            return result;
        }
    }
}
