using GPAS.Dispatch.Entities.Concepts.Geo;
using GPAS.PropertiesValidation;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.FeatureTest.GeoSearchTests
{
    public class KWObjectWithPropertiesForTest
    {
        public KWObject KWObject { set; get; }
        public List<KWProperty> Properties { set; get; }
        public static async Task<KWObjectWithPropertiesForTest> Create(string objTypeURI, string label, Dictionary<string, List<string>> properties, List<GeoTimeEntityRawData> geoTimeProperties)
        {
            KWObject kWObject = await ObjectManager.CreateNewObject(objTypeURI, label);
            List<KWProperty> outProp = new List<KWProperty>();

            foreach (var prp in properties)
            {
                foreach (string val in prp.Value)
                {
                    KWProperty kWProperty = PropertyManager.CreateNewPropertyForObject(kWObject, prp.Key.ToString(), val.ToString());
                    outProp.Add(kWProperty);
                }
            }

            foreach (GeoTimeEntityRawData gtprp in geoTimeProperties)
            {
                KWProperty kWProperty = PropertyManager.CreateNewPropertyForObject(kWObject, OntologyProvider.GetOntology().GetDateRangeAndLocationPropertyTypeUri(), GeoTime.GetGeoTimeStringValue(gtprp));
                outProp.Add(kWProperty);
            }

            return new KWObjectWithPropertiesForTest() { KWObject = kWObject, Properties = outProp };
        }
    }
}
