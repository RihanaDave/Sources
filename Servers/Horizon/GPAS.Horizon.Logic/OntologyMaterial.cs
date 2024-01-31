using GPAS.Ontology;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GPAS.Horizon.Logic.GraphRepositoryProvider;

namespace GPAS.Horizon.Logic
{
    public class OntologyMaterial
    {
        public Dictionary<string, Dictionary<string, GraphRepositoryBaseDataTypes>> ObjAndRelatedPropTypes { get; private set; }
        public List<string> RelationTypes { get; private set; }
        public List<string> EventTypes { get; private set; }

        public OntologyMaterial(Dictionary<string, Dictionary<string, GraphRepositoryBaseDataTypes>> ontologyObjAndRelatedPropTypes,
            List<string> ontologyRelationTypes, List<string> ontologyEventTypes)
        {
            ObjAndRelatedPropTypes = ontologyObjAndRelatedPropTypes;
            RelationTypes = ontologyRelationTypes;
            EventTypes = ontologyEventTypes;
        }

        public bool IsEvent(string typeURI)
        {
            bool result = false;

            if (EventTypes.Contains(typeURI))
            {
                result = true;
            }

            return result;
        }

        public GraphRepositoryBaseDataTypes GetBaseDataTypeOfProperty(string propertyTypeURI)
        {
            GraphRepositoryBaseDataTypes result = GraphRepositoryBaseDataTypes.None;

            foreach (var currentObj in ObjAndRelatedPropTypes)
            {
                foreach (var currentProp in currentObj.Value)
                {
                    if (currentProp.Key.Equals(propertyTypeURI))
                    {
                        result = currentProp.Value;
                        break;
                    }
                }
            }

            return result;
        }

        public string GetDefaultRelationshipTypeForEventBasedLink(string domain, string intermidateEvent, string range)
        {
            return ("حضور_در");
        }

        public static OntologyMaterial GetOntologyMaterial(Ontology.Ontology localOntology)
        {
            Dictionary<string, Dictionary<string, GraphRepositoryBaseDataTypes>> objToPropAndTypeMapping = GenerateObjectAndPropertiesTypeFromOntology(localOntology);
            List<string> relationshipTypes = GenerateRelatioinshipTypes(localOntology);
            List<string> eventTypes = GenerateEventTypes(localOntology);

            return new OntologyMaterial(objToPropAndTypeMapping, relationshipTypes, eventTypes);
        }
        private static List<string> GenerateRelatioinshipTypes(Ontology.Ontology localOntology)
        {
            List<string> result = new List<string>();

            List<string> allOntologyRelationships = localOntology.GetAllOntologyRelationships();
            foreach (var currentRelation in allOntologyRelationships)
            {
                result.Add(localOntology.GetTypeName(currentRelation));
            }

            return result;
        }
        private static List<string> GenerateEventTypes(Ontology.Ontology localOntology)
        {
            List<string> result = new List<string>();

            string[] objectTypeURIs = localOntology.GetAllObjectTypeURIs();
            foreach (var currentObject in objectTypeURIs)
            {
                if (localOntology.IsEvent(currentObject))
                {
                    result.Add(currentObject);
                }
            }

            return result;
        }
        private static Dictionary<string, Dictionary<string, GraphRepositoryBaseDataTypes>> GenerateObjectAndPropertiesTypeFromOntology(Ontology.Ontology localOntology)
        {
            Dictionary<string, Dictionary<string, GraphRepositoryBaseDataTypes>> result = new Dictionary<string, Dictionary<string, GraphRepositoryBaseDataTypes>>();

            string[] objectTypeURIs = localOntology.GetAllObjectTypeURIs();
            foreach (var currentObjTypeUri in objectTypeURIs)
            {
                if (ObjectPropertiesList.Count > 0)
                    ObjectPropertiesList.Clear();

                GetHierarchyPropertiesName(OntologyProvider.GetOntology().GetHierarchyPropertiesOfObject(currentObjTypeUri));

                foreach (var currentProp in ObjectPropertiesList)
                {
                    if (result.ContainsKey(currentObjTypeUri))
                    {
                        if (!result[currentObjTypeUri].ContainsKey(currentProp.TypeName))
                        {
                            result[currentObjTypeUri].Add(currentProp.TypeName, ConvertOntologyDataTypeToGraphRepoDataType(currentProp.BaseDataType));
                        }
                    }
                    else
                    {
                        Dictionary<string, GraphRepositoryBaseDataTypes> temp = new Dictionary<string, GraphRepositoryBaseDataTypes>();
                        temp.Add(currentProp.TypeName, ConvertOntologyDataTypeToGraphRepoDataType(currentProp.BaseDataType));
                        result.Add(currentObjTypeUri, temp);
                    }
                }
            }

            return result;
        }

        static List<DataType> ObjectPropertiesList = new List<DataType>();

        private static void GetHierarchyPropertiesName(ObservableCollection<OntologyNode> properties)
        {
            foreach (var item in properties)
            {
                if (!(item is PropertyNode))
                    continue;

                PropertyNode property = item as PropertyNode;

                if (property.IsLeaf)
                {
                    ObjectPropertiesList.Add(new DataType() { TypeName = property.TypeUri, BaseDataType = property.BaseDataType });
                }
                else
                {
                    if (property.Children.Count <= 0)
                        return;

                    GetHierarchyPropertiesName(property.Children);
                }
            }
        }

        private static GraphRepositoryBaseDataTypes ConvertOntologyDataTypeToGraphRepoDataType(BaseDataTypes ontologyBaseDataType)
        {
            GraphRepositoryBaseDataTypes result = GraphRepositoryBaseDataTypes.None;

            switch (ontologyBaseDataType)
            {
                case BaseDataTypes.Int:
                    result = GraphRepositoryBaseDataTypes.Int;
                    break;
                case BaseDataTypes.Boolean:
                    result = GraphRepositoryBaseDataTypes.Boolean;
                    break;
                case BaseDataTypes.DateTime:
                    result = GraphRepositoryBaseDataTypes.DateTime;
                    break;
                case BaseDataTypes.String:
                    result = GraphRepositoryBaseDataTypes.String;
                    break;
                case BaseDataTypes.Double:
                    result = GraphRepositoryBaseDataTypes.Double;
                    break;
                case BaseDataTypes.HdfsURI:
                    result = GraphRepositoryBaseDataTypes.HdfsURI;
                    break;
                case BaseDataTypes.Long:
                    result = GraphRepositoryBaseDataTypes.Long;
                    break;
                case BaseDataTypes.None:
                    result = GraphRepositoryBaseDataTypes.None;
                    break;
                case BaseDataTypes.GeoTime:
                    result = GraphRepositoryBaseDataTypes.GeoTime;
                    break;
                case BaseDataTypes.GeoPoint:
                    result = GraphRepositoryBaseDataTypes.GeoPoint;
                    break;
                default:
                    break;
            }

            return result;
        }
    }
}
