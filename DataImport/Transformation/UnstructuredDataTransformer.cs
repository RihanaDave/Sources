using GPAS.DataImport.ConceptsToGenerate;
using GPAS.DataImport.DataMapping.Unstructured;
using GPAS.Logger;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.DataImport.Transformation
{
    public class UnstructuredDataTransformer
    {
        ProcessLogger Logger;
        Ontology.Ontology CurrentOntology;
        private bool importPerformed = false;
        private bool ReportFullDetails = true;
        private int MinimumIntervalBetwweenIterrativeLogsReportInSeconds;

        TypeMapping ImportMapping;
        ObjectMapping[] ObjectsMappingArray;
        RelationshipMapping[] RelationshipsMappingArray;

        private List<ImportingObject> generatingObjects;
        public List<ImportingObject> GeneratingObjects
        {
            get
            {
                CheckImportToBePerform();
                return generatingObjects;
            }
        }

        private List<ImportingRelationship> generatingRelationships;
        public List<ImportingRelationship> GeneratingRelationships
        {
            get
            {
                CheckImportToBePerform();
                return generatingRelationships;
            }
        }

        public UnstructuredDataTransformer(Ontology.Ontology ontology, ProcessLogger logger = null)
        {
            Logger = logger;
            CurrentOntology = ontology;
        }

        private void CheckImportToBePerform()
        {
            if (!importPerformed)
                throw new InvalidOperationException("Import may perform first!");
        }       
       
        public void TransformConcepts(TypeMapping importMapping)
        {
            if (importMapping == null)
                throw new ArgumentNullException(nameof(importMapping));            

            generatingObjects = new List<ImportingObject>();
            generatingRelationships = new List<ImportingRelationship>();
            ReportFullDetails = bool.Parse(ConfigurationManager.AppSettings["ReportFullDetailsInImportLog"]);
            MinimumIntervalBetwweenIterrativeLogsReportInSeconds = int.Parse(ConfigurationManager.AppSettings["MinimumIntervalBetwweenIterrativeLogsReportInSeconds"]);

            // به خاطر امکان نغییر نگاشت در این کلاس و تاثیر ناخواسته بر فراخواننده ی آن
            // از نگاشت کپی گرفته می شود
            ImportMapping = importMapping.Copy(importMapping.ObjectsMapping.First().MappingTitle);
            // توابع زیر برای حفظ کارایی و عدم پردازش تکراری، اینجا پیش‌پردازش‌ها را انجام می‌دهند
            ObjectsMappingArray = ImportMapping.ObjectsMapping.ToArray();
            RelationshipsMappingArray = ImportMapping.RelationshipsMapping.ToArray();

            GenerateObjectsAndTheirProperties();

            SaveLog(string.Format("Internal resolution completed; {0:N0} Objects with totally {1:N0} Properties and {2:N0} Relationships are condidate to be generate.", generatingObjects.Count, generatingObjects.Sum(go => go.GetProperties().Count()), generatingRelationships.Count));
            importPerformed = true;
        }

        private List<ImportingObject> GenerateObjectsAndTheirProperties()
        {
            List<ImportingObject> result = new List<ImportingObject>();
            foreach (var currentObjectMapping in ObjectsMappingArray)
            {
                List<ImportingProperty> generatingProperties = new List<ImportingProperty>();
                foreach (var currentPropertyMapping in currentObjectMapping.Properties)
                {
                    var generatingProperty = new ImportingProperty(currentPropertyMapping.PropertyType, currentPropertyMapping.Value.ConstValue);
                    generatingProperties.Add(generatingProperty);
                }
                
                string labelPropertyTypeUri = CurrentOntology.GetDefaultDisplayNamePropertyTypeUri();

                ImportingObject generatingObject = new ImportingObject(currentObjectMapping.ObjectType, new ImportingProperty(labelPropertyTypeUri, currentObjectMapping.MappingTitle));

                generatingObject.AddPropertyRangeForObject(generatingProperties);

                generatingObjects.Add(generatingObject);                
            }
            return result;
        }

        public List<ImportingObject> GetGeneratingObjectsForMapping(ObjectMapping objectMapping)
        {
            if (objectMapping == null)
            {
                throw new ArgumentNullException(nameof(objectMapping));
            }
            if (!ImportMapping.ObjectsMapping.Any(om => om.ID.Equals(objectMapping.ID)))
            {
                throw new ArgumentException("Object mapping not presented in the transformer maps");
            }

            List<ImportingObject> result = new List<ImportingObject>();
            if (generatingObjects.Where(go=>go.TypeUri.Equals(objectMapping.ObjectType)).Any())
            {
                result.Add(generatingObjects.Where(go => go.TypeUri.Equals(objectMapping.ObjectType)).First());
            }
            return result;
        }

        private void SaveLog(string logContent, bool storeInusePrivateMemorySize = true)
        {
            if (Logger != null)
                Logger.WriteLog(logContent, storeInusePrivateMemorySize);
        }

    }
}
