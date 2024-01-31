using GPAS.AccessControl;
using GPAS.Dispatch.Entities.Concepts.Geo;
using GPAS.Horizon.Logic;
using GPAS.Horizon.Server.LoadTests.Properties;
using GPAS.Logger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GPAS.Horizon.Logic.GraphRepositoryProvider;

namespace GPAS.Horizon.Server.LoadTests
{
    public class BaseLoadTest
    {
        public static int MaxObjectId = 1;
        public static int MaxPropertyId = 1;
        public static int MaxRelationId = 1;
        public static int PropertiesCount = 20;
        public const int RelationCounts = 5;
        public const int BatchItems = 1000;
        public const int DataSourceId = 300;
        public long resultLimit = 10000;
        public static string ExtensionFileToSave;

        protected Ontology.Ontology ontology = null;
        protected ACL currentACL;
        protected OntologyMaterial ontologyMaterial = null;
        protected Dictionary<string, Ontology.BaseDataTypes> allProperties = new Dictionary<string, Ontology.BaseDataTypes>()
        {
            ["label"] = Ontology.BaseDataTypes.String,
            ["نام"] = Ontology.BaseDataTypes.String,
            ["آدرس"] = Ontology.BaseDataTypes.String,
            ["سن"] = Ontology.BaseDataTypes.String,
            ["نام_خانوادگی"] = Ontology.BaseDataTypes.String,
        };
        protected List<string> allObjects = new List<string>() { "شخص", "مکالمه_تلفنی", "سند" };
        protected List<string> allRelations = new List<string>() { "حضور_در"};
        protected List<string> allEvents = new List<string>() { "مکالمه_تلفنی" };

        private const int MaxPropertyValuesLength = 25;

        public string LogMessage = "";
        public static string ClearAllDataTime = "";
        public static string PublishDataTime = "";
        public string TestTime = "";
        public string OutPutTimeFormat = @"hh\:mm\:ss\.fff";

        public static long BatchCount { get; set; } = 100;

        public static int RetrieveItemsCount { get; set; } = 200;

        public readonly Random Random = new Random();
        public readonly Service HorizonService = new Service();

        private static ProcessLogger logger;        

        protected Ontology.Ontology SetOntology()
        {
            ontology = new Ontology.Ontology();
            return ontology;
        }
        public string startRunTestsTime;

        protected OntologyMaterial GenerateDefaultOntologyMaterial()
        {
            Dictionary<string, Dictionary<string, GraphRepositoryBaseDataTypes>> ObjAndRelatedPropTypes = GenerateObjectAndPropertiesTypeFromOntology();
            List<string> RelationTypes = allRelations;
            List<string> EventTypes = allEvents;
            ontologyMaterial = new OntologyMaterial(ObjAndRelatedPropTypes, RelationTypes, EventTypes);
            return ontologyMaterial;
        }

        protected void GenerateDefaultOntology(List<string> properties)
        {
            ontology = new Ontology.Ontology()
            {
                baseUri = "ontology",
            };

            if (properties == null || properties.Count == 0)
            {
                ontology.metadataDictionary = new Dictionary<string, Ontology.metadata>()
                {
                    ["label"] = new Ontology.metadata()
                    {
                        isDeprecated = false,
                        searchable = true,
                    },
                };
            }
            else
            {
                ontology.metadataDictionary = new Dictionary<string, Ontology.metadata>();
                foreach (var pName in properties)
                {
                    ontology.metadataDictionary.Add(pName, new Ontology.metadata()
                    {
                        isDeprecated = false,
                        searchable = true,
                    });
                }
            }
        }

        private Dictionary<string, Dictionary<string, GraphRepositoryBaseDataTypes>> GenerateObjectAndPropertiesTypeFromOntology()
        {
            Dictionary<string, Dictionary<string, GraphRepositoryBaseDataTypes>> result = new Dictionary<string, Dictionary<string, GraphRepositoryBaseDataTypes>>();

            Dictionary<string, GraphRepositoryBaseDataTypes> personProp = new Dictionary<string, GraphRepositoryBaseDataTypes>();
            personProp.Add("نام", GraphRepositoryBaseDataTypes.String);
            personProp.Add("label", GraphRepositoryBaseDataTypes.String);
            personProp.Add("نام_خانوادگی", GraphRepositoryBaseDataTypes.String);
            personProp.Add("سن", GraphRepositoryBaseDataTypes.Int);
            personProp.Add("آدرس", GraphRepositoryBaseDataTypes.String);


            Dictionary<string, GraphRepositoryBaseDataTypes> phoneProp = new Dictionary<string, GraphRepositoryBaseDataTypes>();
            phoneProp.Add("نام", GraphRepositoryBaseDataTypes.String);
            phoneProp.Add("label", GraphRepositoryBaseDataTypes.String);

            Dictionary<string, GraphRepositoryBaseDataTypes>docProp = new Dictionary<string, GraphRepositoryBaseDataTypes>();
            docProp.Add("نام", GraphRepositoryBaseDataTypes.String);
            docProp.Add("label", GraphRepositoryBaseDataTypes.String);

            result.Add("شخص", personProp);
            result.Add("مکالمه_تلفنی", phoneProp);
            result.Add("سند", docProp);

            return result;
        }

        protected object GenerateValueProperty(string propertyName)
        {
            switch (allProperties[propertyName])
            {
                case Ontology.BaseDataTypes.Int:
                    return Random.Next();
                case Ontology.BaseDataTypes.Boolean:
                    return Random.NextDouble() > .5;
                case Ontology.BaseDataTypes.DateTime:
                    return RandomDateTime();
                case Ontology.BaseDataTypes.String:
                    return RandomString();
                case Ontology.BaseDataTypes.Double:
                    return Random.NextDouble();
                case Ontology.BaseDataTypes.Long:
                    return (long)Random.Next();
                case Ontology.BaseDataTypes.HdfsURI:
                case Ontology.BaseDataTypes.None:
                default:
                    return RandomString();
            }
        }

        protected DateTime RandomDateTime()
        {
            return new DateTime(Random.Next(1, 9999), Random.Next(1, 12), Random.Next(1, 28), Random.Next(0, 23),
                                                       Random.Next(0, 59), Random.Next(0, 59), Random.Next(0, 999));
        }

        protected string RandomString()
        {
            const string charsWithNumbers = "AaBbCcDdEeFfGgHhIiJjKkLlMmNnOoPpQqRrSsTtUuVvWwXxYyZz0123456789";
            return new string(Enumerable.Repeat(charsWithNumbers, Random.Next(0, MaxPropertyValuesLength)).Select(s => s[Random.Next(s.Length)]).ToArray());
        }

        protected double RandomDouble(double min, double max)
        {
            if (min > max)
                throw new ArgumentOutOfRangeException("min Larger than max");

            double x = Random.NextDouble();
            double d = max - min;
            return (d * x) + min;
        }

        protected List<T> GetRandomElements<T>(List<T> list)
        {
            int elementsCount = Random.Next(1, list.Count);
            return list.OrderBy(arg => Guid.NewGuid()).Take(elementsCount).ToList();
        }

        public BaseLoadTest()
        {
            currentACL = new ACL()
            {
                Classification = Classification.EntriesTree.First().IdentifierString,
                Permissions = new List<ACI>()
                {
                    new ACI
                    {
                         AccessLevel = Permission.Owner,
                         GroupName = AccessControl.Groups.NativeGroup.Administrators.ToString()
                    },
                }
            };            
        }

        protected void WriteLog(string message)
        {
            if (!Directory.Exists(Resources.String_LogFilePath))
            {
                Directory.CreateDirectory(Resources.String_LogFilePath);
            }

            if (logger == null)
            {
                logger = new ProcessLogger();

                string fileName = Resources.String_LogFilePath + string.Format(Resources.String_LogFileName, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
                logger.Initialization(fileName);
            }

            Debug.WriteLine(message);
            logger.WriteLog(message);
        }

        //public void WriteLog(string message)
        //{
        //    try
        //    {
        //        Debug.WriteLine(message);

        //        string resultFolderPath = Resources.String_LogFilePath;
        //        string resultFilePath = Path.Combine(resultFolderPath, "Load test results_" + ExtensionFileToSave);

        //        if (!Directory.Exists(resultFolderPath))
        //        {
        //            Directory.CreateDirectory(resultFolderPath);
        //        }

        //        if (!File.Exists(resultFilePath))
        //        {
        //            using (StreamWriter streamWriter = File.CreateText(resultFilePath))
        //            {
        //                streamWriter.WriteLine("");
        //                streamWriter.WriteLine(message);
        //            }

        //            return;
        //        }

        //        using (StreamWriter streamWriter = File.AppendText(resultFilePath))
        //        {
        //            streamWriter.WriteLine("");
        //            streamWriter.WriteLine(message);
        //        }

        //    }
        //    catch (Exception e)
        //    {
        //        // Do nothing
        //    }
        //}

    }
}
