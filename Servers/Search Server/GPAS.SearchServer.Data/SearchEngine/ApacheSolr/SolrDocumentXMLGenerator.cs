using System.Xml;

namespace GPAS.SearchServer.Access.SearchEngine.ApacheSolr
{
    public class SolrDocumentXMLGenerator
    {
        private XmlDocument document;
        private XmlNode addNode;

        public SolrDocumentXMLGenerator()
        {
            document = new XmlDocument();
            addNode = document.CreateElement("add");
            document.AppendChild(addNode);
        }

        public string GetXML()
        {
            return document.OuterXml;
        }

        public XmlDocument CreateDocument()
        {
            XmlNode docNode = document.CreateElement("doc");
            addNode.AppendChild(docNode);
            return document;
        }

        public void AddField(string fieldName, string value)
        {
            XmlNode fieldNode = document.CreateElement("field");
            XmlAttribute nameAttribute = document.CreateAttribute("name");
            nameAttribute.Value = fieldName;
            fieldNode.Attributes.Append(nameAttribute);
            fieldNode.InnerText = value;
            document.DocumentElement.LastChild.AppendChild(fieldNode);
        }

        public void AddMultiValueField(string fieldName, string value)
        {
            XmlNode fieldNode = document.CreateElement("field");
            XmlAttribute nameAttribute = document.CreateAttribute("name");
            nameAttribute.Value = fieldName;
            fieldNode.Attributes.Append(nameAttribute);

            XmlAttribute updateAttribute = document.CreateAttribute("update");
            updateAttribute.Value = "add";
            fieldNode.Attributes.Append(updateAttribute);

            fieldNode.InnerText = value;
            document.DocumentElement.LastChild.AppendChild(fieldNode);
        }
        public void RemoveMultiValueField(string fieldName, string value)
        {
            XmlNode fieldNode = document.CreateElement("field");
            XmlAttribute nameAttribute = document.CreateAttribute("name");
            nameAttribute.Value = fieldName;
            fieldNode.Attributes.Append(nameAttribute);

            XmlAttribute updateAttribute = document.CreateAttribute("update");
            updateAttribute.Value = "remove";
            fieldNode.Attributes.Append(updateAttribute);

            fieldNode.InnerText = value;
            document.DocumentElement.LastChild.AppendChild(fieldNode);
        }

        public void UpdateMultiValueField(string fieldName, string value)
        {
            XmlNode fieldNode = document.CreateElement("field");
            XmlAttribute nameAttribute = document.CreateAttribute("name");
            nameAttribute.Value = fieldName;
            fieldNode.Attributes.Append(nameAttribute);

            XmlAttribute updateAttribute = document.CreateAttribute("update");
            updateAttribute.Value = "set";
            fieldNode.Attributes.Append(updateAttribute);

            fieldNode.InnerText = value;
            document.DocumentElement.LastChild.AppendChild(fieldNode);
        }


        public void UpdateField(string fieldName, string value)
        {
            XmlNode fieldNode = document.CreateElement("field");
            XmlAttribute nameAttribute = document.CreateAttribute("name");
            nameAttribute.Value = fieldName;
            fieldNode.Attributes.Append(nameAttribute);

            XmlAttribute updateAttribute = document.CreateAttribute("update");
            updateAttribute.Value = "set";
            fieldNode.Attributes.Append(updateAttribute);

            fieldNode.InnerText = value;
            document.DocumentElement.LastChild.AppendChild(fieldNode);
        }
    }

}
