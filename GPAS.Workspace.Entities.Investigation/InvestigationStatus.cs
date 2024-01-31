using GPAS.Dispatch.Entities.Concepts;
using GPAS.Workspace.Entities.Investigation.UnpublishedChanges;
using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace GPAS.Workspace.Entities.Investigation
{
    [Serializable]
    public class InvestigationStatus
    {
        public GraphApplicationStatus GraphStatus { get; set; }
        public BrowserApplicationStatus BrowserStatus { get; set; }
        public MapApplicationStatus MapStatus { get; set; }
        // public ObjectExplorerApplicationStatus ObjectExplorerStatus { get; set; }
        public SaveInvestigationUnpublishedConcepts SaveInvestigationUnpublishedConcepts { get; set; }

        public string UnpublishedConcepts { get; set; }

        public InvestigationStatus()
        { }

        private static Type[] serializationPreipheralTypes =
        {
            typeof(GraphApplicationStatus),
            typeof(BrowserApplicationStatus),
            typeof(MapApplicationStatus),
            typeof(HeatMapStatus),
            typeof(SaveInvestigationUnpublishedConcepts),
            typeof(CachedObjectMetadata),
            typeof(CachedPropertyMetadata),
            typeof(CachedMediaMetadata),
            typeof(CachedRelationshipMetadata),
            typeof(KObject),
            typeof(KProperty),
            typeof(KMedia),
            typeof(KRelationship),

        };

        public static void Serialize(Stream streamWriter, InvestigationStatus investigationStatusToSerialize)
        {
            if (streamWriter == null)
                throw new ArgumentNullException("streamWriter");
            if (investigationStatusToSerialize == null)
                throw new ArgumentNullException("queryToSerialize");

            XmlTextWriter xmlTextWriter = new XmlTextWriter(streamWriter, Encoding.UTF8);
            XmlSerializer serializer = new XmlSerializer(typeof(InvestigationStatus), serializationPreipheralTypes);
            serializer.Serialize(xmlTextWriter, investigationStatusToSerialize);
        }

        public static void SerializeToFile(string filePath, InvestigationStatus investigationStatusToSerialize)
        {
            if (filePath == null)
                throw new ArgumentNullException("filePath");
            if (investigationStatusToSerialize == null)
                throw new ArgumentNullException("queryToSerialize");
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Invalid argument", "filePath");

            StreamWriter sw = new StreamWriter(filePath);
            try
            {
                Serialize(sw.BaseStream, investigationStatusToSerialize);
            }
            finally
            {
                sw.Close();
            }
        }

        public static InvestigationStatus Deserialize(Stream streamReader)
        {
            if (streamReader == null)
                throw new ArgumentNullException("streamReader");

            MemoryStream serializedPrimaryQueryStream = new MemoryStream();
            using (MemoryStream resultQueryStream = new MemoryStream(serializedPrimaryQueryStream.ToArray()))
            {
                using (StreamReader xmlStreamReader = new StreamReader(resultQueryStream, Encoding.UTF8))
                {
                    XmlReader xr = XmlReader.Create(streamReader);
                    XmlSerializer xs = new XmlSerializer(typeof(InvestigationStatus), serializationPreipheralTypes);
                    return (InvestigationStatus)xs.Deserialize(xr);
                }
            }
        }

        /// <summary>
        /// یک نمونه از کلاس «درخواست جستجوی فیلتری» را براساس اطلاعات سری شده‌ی ذخیره شده در فایل، برمی‌گرداند
        /// </summary>
        public static InvestigationStatus DeserializeFromFile(string filePath)
        {
            if (filePath == null)
                throw new ArgumentNullException("filePath");
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("Invalid argument", "filePath");

            FileStream fs = new FileStream(filePath, FileMode.Open);
            try
            {
                return Deserialize(fs);
            }
            finally
            {
                fs.Close();
            }
        }
    }
}
