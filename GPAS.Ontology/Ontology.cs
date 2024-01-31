using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Windows.Forms;
using VDS.RDF;
using VDS.RDF.Query;

namespace GPAS.Ontology
{
    public class Ontology
    {
        private IGraph graph;
        public string baseUri;

        public Dictionary<string, metadata> metadataDictionary = new Dictionary<string, metadata>();

        /// <summary>
        /// نمونه خالی از ساختار نوع داده؛
        /// این نمونه برای نشان دادن مواقعی است که نمونه ای از این ساختار، به نوع خاصی اشاره ندارد.
        /// </summary>
        public readonly static DataType EmptyDataType = new DataType { TypeName = "", BaseDataType = BaseDataTypes.None };

        ///<summary>
        ///از این تابع برای حذف کرکترهای خاصی از پر‌س‌وجوهای اسپارکل جهت جلوگیری از تزریق اسپارکل استفاده می‌شود
        ///</summary>
        private string MakeSafeString(string inputStr)
        {
            try
            {
                string outputStr = inputStr.Replace("{", "");
                outputStr = outputStr.Replace("}", "");
                outputStr = outputStr.Replace(":", "");
                outputStr = outputStr.Replace(";", "");
                outputStr = outputStr.Replace(".", "");
                outputStr = outputStr.Replace("/", "");
                outputStr = outputStr.Replace("\\", "");
                outputStr = outputStr.Replace(">", "");
                outputStr = outputStr.Replace("<", "");
                outputStr = outputStr.Replace("?", "");
                outputStr = outputStr.Replace("-", "");
                outputStr = outputStr.Replace("]", "");
                outputStr = outputStr.Replace("[", "");
                outputStr = outputStr.Replace("~", "");
                outputStr = outputStr.Replace("!", "");
                outputStr = outputStr.Replace("&", "");
                outputStr = outputStr.Replace("$", "");
                outputStr = outputStr.Replace("%", "");
                outputStr = outputStr.Replace("\"", "");
                outputStr = outputStr.Replace("'", "");
                outputStr = outputStr.Replace("(", "");
                outputStr = outputStr.Replace(")", "");
                outputStr = outputStr.Replace("*", "");
                outputStr = outputStr.Replace("+", "");
                outputStr = outputStr.Replace(",", "");
                outputStr = outputStr.Replace("=", "");
                outputStr = outputStr.Replace("#", "");
                outputStr = outputStr.Replace("@", "");
                return (outputStr);
            }
            catch
            {
                throw new ApplicationException("String Is Null");

            }
        }

        ///<summary>
        ///این تابع یک "یو آر آی" دریافت می‌کند و نام نوع مورد نظر را برمی‌گرداند
        ///</summary>
        public string GetTypeName(string uri)
        {
            string typeName = HttpUtility.UrlDecode(uri);
            typeName = typeName.Remove(0, typeName.IndexOf('#') + 1);
            return (typeName);
        }

        ///<summary>
        ///این تابع نام یک نوع را دریافت می‌کند و یو آر آی آن نوع را برمی‌گرداند
        ///</summary>
        public string GetTypeURI(string name)
        {
            string typeURI = baseUri + "#" + name;
            return (typeURI);
        }

        ///<summary>
        ///این تابع یک آنتلوژی را از مسیر ورودی بارگذاری می‌کند و یک گراف از روی آن ایجاد می‌نماید
        ///</summary>
        ///<summary>
        ///این تابع یک آنتلوژی را از مسیر ورودی بارگذاری می‌کند و یک گراف از روی آن ایجاد می‌نماید
        ///</summary>
        public IGraph LoadOntology(string ontologyPath, string ontologyMetadataPath = "")
        {
            graph = new Graph();

            graph.LoadFromFile(ontologyPath);
            baseUri = GetBaseUri(ontologyPath);

            if (ontologyMetadataPath != "")
            {
                using (StreamReader streamReader = new StreamReader(ontologyMetadataPath))
                {
                    metadataDictionary.Clear();
                    streamReader.ReadLine();
                    while (!streamReader.EndOfStream)
                    {
                        string splitMe = streamReader.ReadLine();
                        string[] lineSplits = splitMe.Split(new char[] { ',' });

                        if (lineSplits.Length == 3)
                        {
                            metadata newMetadata = new metadata();
                            newMetadata.isDeprecated = Convert.ToBoolean(lineSplits[1].Trim());
                            newMetadata.searchable = Convert.ToBoolean(lineSplits[2].Trim());
                            metadataDictionary.Add(lineSplits[0].Trim(), newMetadata);
                        }
                    }
                }
            }

            PreProcessTopLevelTypeSubsets();

            return graph;
        }

        HashSet<string> entitySubTypeNames = new HashSet<string>();
        HashSet<string> eventSubTypeNames = new HashSet<string>();
        static HashSet<string> documentSubTypeNames = new HashSet<string>();
        HashSet<string> textDocumentSubTypeNames = new HashSet<string>();
        HashSet<string> imageDocumentSubTypeNames = new HashSet<string>();
        HashSet<string> audioDocumentSubTypeNames = new HashSet<string>();
        HashSet<string> multimediaSubTypeNames = new HashSet<string>();
        HashSet<string> tabularTypeNames = new HashSet<string> { "CSV" }; //, "XLSX", "EML", "MDB", "ACCDB"
        Dictionary<string, DataType> allProperties = new Dictionary<string, DataType>();
        private void PreProcessTopLevelTypeSubsets()
        {
            RefetchSubTypes(GetEntityTypeURI(), entitySubTypeNames);
            RefetchSubTypes(GetEventTypeURI(), eventSubTypeNames);
            RefetchSubTypes(GetDocumentTypeURI(), documentSubTypeNames);
            
            //Disabe in new ETL. After activate unstructured import must uncomment below lines.
            //RefetchOnlyLeafSubTypes(GetTextDocumentTypeURI(), textDocumentSubTypeNames);
            //RefetchOnlyLeafSubTypes(GetImageDocumentTypeURI(), imageDocumentSubTypeNames);
            //RefetchOnlyLeafSubTypes(GetAudioDocumentTypeURI(), audioDocumentSubTypeNames);
            //RefetchOnlyLeafSubTypes(GetMultimediaDocumentTypeURI(), multimediaSubTypeNames);
            PrefetchAllProperties();
        }

        private void RefetchSubTypes(string typeToFetchSubTypes, HashSet<string> hashSetToFetchSubTypes)
        {
            hashSetToFetchSubTypes.Clear();
            foreach (string type in GetAllChilds(typeToFetchSubTypes))
            {
                hashSetToFetchSubTypes.Add(type);
            }
        }

        private void RefetchOnlyLeafSubTypes(string typeToFetchSubTypes, HashSet<string> hashSetToFetchSubTypes)
        {
            hashSetToFetchSubTypes.Clear();
            foreach (string type in GetOnlyAllChilds(typeToFetchSubTypes))
            {
                hashSetToFetchSubTypes.Add(type);
            }
        }

        public string GetBaseUri(string ontologyPath)
        {
            StreamReader reader = null;
            try
            {
                reader = new StreamReader(ontologyPath);
                string baseUri = " ";
                while (reader.Peek() != -1)
                {
                    string currentLine = reader.ReadLine();
                    if (currentLine.Contains("xml:base="))
                    {
                        currentLine = currentLine.Split('\"', '\"')[1];
                        baseUri = currentLine;
                        return baseUri;
                    }
                }
                return baseUri;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
        }

        ///<summary>
        ///این تابع نام یک لینک یا شیء را دریافت و پدر بلافصل آن گره را به عنوان خروجی ارسال می‌نماید
        ///</summary>
        public string GetParent(string child)
        {
            child = MakeSafeString(child);
            object results = new object();

            if (IsRelationship(child))
            {
                results = graph.ExecuteQuery(@"  PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                                                        PREFIX type: <" + baseUri + "#>   " +
                                                                       @"
                                                            SELECT ?parent
	                                                              WHERE { type:" + child + " rdfs:subPropertyOf ?parent}");
            }
            else
            {
                results = graph.ExecuteQuery(@"  PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                                                        PREFIX type: <" + baseUri + "#>   " +
                                                       @"
                                                            SELECT ?parent
	                                                              WHERE { type:" + child + " rdfs:subClassOf ?parent}");
            }

            if (results is SparqlResultSet)
            {
                SparqlResultSet resultSet = (SparqlResultSet)results;
                string s = resultSet.Results[0].ToString();
                return (GetTypeName(s));
            }
            else
                return (null);
        }

        ///<summary>
        ///این تابع نام یک شیء را به عنوان پدر دریافت کرده و فرزندان آن گره را به عنوان خروجی ارسال می‌نماید
        ///</summary>
        public List<string> GetAllChilds(string parent)
        {
            parent = MakeSafeString(parent);
            List<string> finalResult = new List<string>();
            object results = new object();

            results = graph.ExecuteQuery(@"  PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                                             PREFIX type: <" + baseUri + "#>" +
                                               @"  SELECT ?child
	                                               WHERE { ?child rdfs:subClassOf* type:" + parent + "}");

            if (results is SparqlResultSet)
            {
                SparqlResultSet resultSet = (SparqlResultSet)results;
                foreach (SparqlResult result in resultSet)
                {
                    finalResult.Add(GetTypeName(result.ToString()));
                }
                return (finalResult);
            }
            else
                return (null);
        }

        public ArrayList GetOnlyAllChilds(string parent)
        {
            parent = MakeSafeString(parent);
            ArrayList finalResult = new ArrayList();
            object results = new object();

            results = graph.ExecuteQuery(@"  PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                                             PREFIX type: <" + baseUri + "#>" +
                                               @"  SELECT ?child
	                                               WHERE { ?child rdfs:subClassOf type:" + parent + "}");

            if (results is SparqlResultSet)
            {
                SparqlResultSet resultSet = (SparqlResultSet)results;
                foreach (SparqlResult result in resultSet)
                {
                    finalResult.Add(GetTypeName(result.ToString()));
                }
                return (finalResult);
            }
            else
                return (null);
        }

        ///<summary>
        ///این تابع نام یک لینک یا شیء را به عنوان فرزند دریافت کرده و اجداد آن گره را به عنوان خروجی ارسال می‌نماید
        ///</summary>
        public ArrayList GetAllParents(string child)
        {
            child = MakeSafeString(child);
            ArrayList finalResult = new ArrayList();
            object results = new object();

            if (IsRelationship(child))
            {
                results = graph.ExecuteQuery(@"  PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                                                    PREFIX type: <" + baseUri + "#>   " +
                                                                    @"
                                                        SELECT ?parent
	                                                            WHERE { type:" + child + " rdfs:subPropertyOf* ?parent}");
            }
            else
            {
                results = graph.ExecuteQuery(@"  PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                                                    PREFIX type: <" + baseUri + "#>" +
                                                @"  SELECT ?parent
	                                                            WHERE { type:" + child + " rdfs:subClassOf* ?parent}");
            }
            if (results is SparqlResultSet)
            {
                SparqlResultSet resultSet = (SparqlResultSet)results;
                foreach (SparqlResult result in resultSet)
                {
                    finalResult.Add(GetTypeName(result.ToString()));
                }
                return (finalResult);
            }
            else return (null);
        }

        public ArrayList GetAllParentsForObjects(List<string> typeUriList)
        {
            ArrayList finalResult = new ArrayList();

            StringBuilder query = new StringBuilder(string.Empty);

            query.Append(@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                           PREFIX type: <" + baseUri + "#>" +
                         @"SELECT DISTINCT ?parent WHERE {
                                                {type:" + typeUriList.First() + " rdfs:subClassOf* ?parent}");

            foreach (var typeUri in typeUriList)
            {
                query.Append(" UNION {type:" + typeUri + " rdfs:subClassOf* ?parent}");
            }

            query.Append("}");

            var results = graph.ExecuteQuery(query.ToString());

            if (results is SparqlResultSet resultSet)
            {
                foreach (SparqlResult result in resultSet)
                {
                    finalResult.Add(GetTypeName(result.ToString()));
                }
                return (finalResult);
            }

            return null;
        }

        ///<summary>
        ///این تابع سلسله‌مراتب تمامی اشیاء موجود در آنتولوژی که قبلاً بارگذاری شده است را به یک درخت تبدیل می‌کند و به عنوان خروجی ارسال می‌نماید
        ///</summary>
        public ObservableCollection<OntologyNode> GetOntologyObjectsHierarchy()
        {
            OntologyNode t1 = new OntologyNode();
            ObservableCollection<OntologyNode> ontoTree = t1.Children;


            Object results = graph.ExecuteQuery(@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                                                  PREFIX type: <" + baseUri + "#>" +
                                                  @"SELECT ?subject WHERE { ?subject rdfs:subClassOf type:شیء}");

            if (results is SparqlResultSet)
            {
                //SELECT/ASK queries give a SparqlResultSet
                SparqlResultSet resultSet = (SparqlResultSet)results;
                foreach (SparqlResult result in resultSet)
                {
                    OntologyNode parentNode = new OntologyNode();
                    ontoTree.Add(CreateObjectNodeType(parentNode, GetTypeName(result.ToString())));
                }
                return (ontoTree);
            }
            else
            {
                return (null);
            }
        }

        public ObservableCollection<OntologyNode> GetOntologyCompleteObjectsHierarchy()
        {
            OntologyNode t1 = new OntologyNode();
            ObservableCollection<OntologyNode> ontoTree = t1.Children;
            Object results = graph.ExecuteQuery(@"      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                                                        PREFIX type: <" + baseUri + "#>" +
                                                             @"   SELECT ?subject
	                                                              WHERE { ?subject rdfs:subClassOf* type:شیء}");

            if (results is SparqlResultSet)
            {
                //SELECT/ASK queries give a SparqlResultSet
                SparqlResultSet resultSet = (SparqlResultSet)results;
                foreach (SparqlResult result in resultSet)
                {
                    OntologyNode parentNode = new OntologyNode();
                    ontoTree.Add(CreateObjectNodeType(parentNode, GetTypeName(result.ToString())));
                }
                return (ontoTree);
            }
            else
            {
                return (null);
            }
        }

        ///<summary>
        ///این تابع برای ساخت درخت سلسله‌مراتب اشیاء آنتولوژی توسط توابع دیگر فراخوانی می‌شود
        ///</summary>
        private OntologyNode CreateObjectNodeType(OntologyNode parentNode, string currentonto)
        {
            parentNode.TypeUri = currentonto;
            string temp = currentonto;

            object results = graph.ExecuteQuery(@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                                                  PREFIX type: <" + baseUri + "#>" +
                                                  @"SELECT ?subject WHERE { ?subject rdfs:subClassOf type:" + temp + "}");

            if (results is SparqlResultSet)
            {
                //SELECT/ASK queries give a SparqlResultSet
                SparqlResultSet resultSet = (SparqlResultSet)results;
                resultSet = (SparqlResultSet)results;
                foreach (SparqlResult result in resultSet)
                {
                    OntologyNode childNode = new OntologyNode();
                    parentNode.Children.Add(childNode);
                    CreateObjectNodeType(childNode, GetTypeName(result.ToString()));
                }
                return parentNode;
            }
            else
            {
                return null;
            }
        }

        ///<summary>
        ///این تابع سلسله‌مراتب تمامی رخدادهای موجود در آنتولوژی که قبلاً بارگذاری شده است را به یک درخت تبدیل می‌کند و به عنوان خروجی ارسال می‌نماید
        ///</summary>
        public OntologyNode GetOntologyEventsHierarchy()
        {
            OntologyNode ontoTree = new OntologyNode();
            ontoTree.TypeUri = GetEventTypeURI();
            object results = graph.ExecuteQuery(@"
                                                PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                                                PREFIX type: <" + baseUri + "#>" +
                                                            @"   SELECT ?subject
	                                                        WHERE { ?subject rdfs:subClassOf type:" + GetEventTypeURI() + "}");

            if (results is SparqlResultSet)
            {
                //SELECT/ASK queries give a SparqlResultSet
                SparqlResultSet resultSet = (SparqlResultSet)results;
                foreach (SparqlResult result in resultSet)
                {
                    OntologyNode parentNode = new OntologyNode();
                    ontoTree.Children.Add(CreateObjectNodeType(parentNode, GetTypeName(result.ToString())));
                }

                return (ontoTree);
            }
            else
            {
                return (null);
            }
        }

        ///<summary>
        ///این تابع سلسله‌مراتب تمامی روابط یا لینک‌های موجود در آنتولوژی که قبلاً بارگذاری شده است را به یک درخت تبدیل می‌کند و به عنوان خروجی ارسال می‌نماید
        ///</summary>
        private OntologyNode GetOntologyRelationshipsHierarchy(ArrayList leaves)
        {
            OntologyNode ontoTree = new OntologyNode();
            ontoTree.TypeUri = GetRelationshipTypeURI();
            object results = graph.ExecuteQuery(@"
                                                    PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                                                    PREFIX type: <" + baseUri + "#>" +
                                                                @"   SELECT ?subject
	                                                            WHERE { ?subject rdfs:subPropertyOf type:" + GetRelationshipTypeURI() + "}");

            if (results is SparqlResultSet)
            {
                //SELECT/ASK queries give a SparqlResultSet
                SparqlResultSet resultSet = (SparqlResultSet)results;
                foreach (SparqlResult result in resultSet)
                {
                    OntologyNode parentNode = new OntologyNode();
                    ontoTree.Children.Add(CreateRelationshipNodeType(parentNode, leaves, GetTypeName(result.ToString())));
                }

                return (ontoTree);
            }
            else
            {
                return (null);
            }
        }

        public List<string> GetAllRelationshipChilds(string parent)
        {
            parent = MakeSafeString(parent);
            List<string> finalResult = new List<string>();
            object results = new object();
            string query = @"  PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                                             PREFIX type: <" + baseUri + "#>" +
                                               @"  SELECT ?child
	                                               WHERE {{ ?child rdfs:subPropertyOf* type:" + GetRelationshipTypeURI() + @" .}
                                                          { ?child rdfs:subPropertyOf* type:" + parent + "}}";
            results = graph.ExecuteQuery(query);

            if (results is SparqlResultSet)
            {
                SparqlResultSet resultSet = (SparqlResultSet)results;
                foreach (SparqlResult result in resultSet)
                {
                    finalResult.Add(GetTypeName(result.ToString()));
                }
                return (finalResult);
            }
            else
                return (null);
        }

        public List<string> GetAllOntologyRelationships()
        {
            List<string> relationship = new List<string>();
            object results = graph.ExecuteQuery(@"
                                                    PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                                                    PREFIX type: <" + baseUri + "#>" +
                                                                @"   SELECT ?subject
	                                                            WHERE { ?subject rdfs:subPropertyOf* type:" + GetRelationshipTypeURI() + "}");

            if (results is SparqlResultSet)
            {
                //SELECT/ASK queries give a SparqlResultSet
                SparqlResultSet resultSet = (SparqlResultSet)results;
                foreach (SparqlResult result in resultSet)
                {
                    relationship.Add(result.ToString());
                }

                return (relationship);
            }
            else
            {
                return (null);
            }
        }

        ///<summary>
        ///این تابع برای ایجاد سلسله‌مراتب تمامی لینک‌های موجود در آنتولوژی که قبلاً بارگذاری شده است، توسط توابع دیگر فراخوانی می‌گردد
        ///</summary>
        private OntologyNode CreateRelationshipNodeType(OntologyNode parentNode, ArrayList leaves, string currentonto)
        {
            parentNode.TypeUri = currentonto;
            string temp = currentonto;

            object results = graph.ExecuteQuery(@"      PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                                                    PREFIX type: <" + baseUri + "#>" +
                                                   @"   SELECT ?subject
	                                                          WHERE { ?subject rdfs:subPropertyOf type:" + temp + "}");

            if (results is SparqlResultSet)
            {
                //SELECT/ASK queries give a SparqlResultSet
                SparqlResultSet resultSet = (SparqlResultSet)results;
                resultSet = (SparqlResultSet)results;
                if (resultSet.Count == 0)
                    leaves.Add(temp);

                foreach (SparqlResult result in resultSet)
                {
                    OntologyNode childNode = new OntologyNode();
                    parentNode.Children.Add(childNode);
                    CreateRelationshipNodeType(childNode, leaves, GetTypeName(result.ToString()));
                }

                return parentNode;
            }
            else
            {
                return null;
            }
        }

        ///<summary>
        ///این تابع سلسله‌مراتبی از تمامی لینک‌های معتبری را که می‌توان بین دو شیء انتخاب شده ایجاد نمود، را به شکل یک درخت برمی‌گرداند.
        ///ورودی‌های این تابع دامنه و برد و یا در واقع نوع اشیاء دو طرف لینک می‌باشد
        ///البته در صورتی که توسط ورودی سوم اعلام بشود که مقصد لینک یک گروه است، آنگاه در این تابع بررسی می‌شود که آیا می‌توان ارتباطی از نوع "عضو_گروه" هم ایجاد نمود یا خیر
        ///</summary>
        public OntologyNode GetValidRelationshipsHierarchy(string domain, string range, bool isGroup)
        {
            ArrayList leaves = new ArrayList();
            OntologyNode ontologyRelationshipTree = GetOntologyRelationshipsHierarchy(leaves);
            if (ontologyRelationshipTree != null)
                return (GetValidOntologyTreeAfterPruning(ontologyRelationshipTree, GetAllValidLink(domain, range, isGroup)));
            else
                throw new ApplicationException("SPARQL Query Or Ontology Configuration Is Wrong");
        }

        private OntologyNode GetValidOntologyTreeAfterPruning(OntologyNode tree, List<string> validLinks)
        {
            try
            {
                int k = 0;
                OntologyNode child = tree.Children.FirstOrDefault();

                while (child != null)
                {
                    if (!child.IsLeaf)
                    {
                        GetValidOntologyTreeAfterPruning(child, validLinks);
                    }

                    if (child.IsLeaf)
                    {
                        bool removeLeaf = false;

                        if (!validLinks.Contains(child.TypeUri))
                            removeLeaf = true;

                        if (removeLeaf)
                        {
                            tree.Children.Remove(child);
                        }
                        else
                        {
                            k++;
                        }
                    }
                    else
                    {
                        k++;
                    }

                    if (tree.Children.Count > k)
                        child = tree.Children[k];
                    else
                        child = null;
                }

                return tree;
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                return null;
            }
        }

        ///<summary>
        ///این تابع لیستی از تمامی لینک‌های معتبری را که می‌توان بین دو شیء انتخاب شده ایجاد نمود، برمی‌گرداند
        ///</summary>
        private List<string> GetAllValidLink(string domain, string range, bool isGroup)
        {
            List<string> resualt = new List<string>();
            ArrayList domains = new ArrayList();
            ArrayList ranges = new ArrayList();

            domains = GetAllParents(domain);
            ranges = GetAllParents(range);

            foreach (string domainVar in domains)
            {
                foreach (string rangeVar in ranges)
                {
                    List<string> temp = new List<string>();
                    temp = GetSpecificAllValidLink(domainVar, rangeVar);
                    resualt.AddRange(temp);
                }
            }

            if (isGroup)
            {
                ArrayList parents = GetAllParents(domain);
                if (parents.IndexOf(range) != -1)
                    resualt.Add("عضو_گروه");
            }

            return resualt;
        }

        private List<string> GetSpecificAllValidLink(string domain, string range)
        {
            domain = MakeSafeString(domain);
            range = MakeSafeString(range);

            List<string> finalResult = new List<string>();

            object results = graph.ExecuteQuery(@" 
                                                        PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                                                        PREFIX type: <" + baseUri + "#>" +
                                                   @"   SELECT ?link
	                                                              WHERE { {?link rdfs:domain type:" + domain +
                                                                      ";    rdfs:range  type:" + range + @"}  
                                                                          UNION 
                                                                          {?link rdfs:domain type:" + range +
                                                                      ";    rdfs:range  type:" + domain +
                                                                      "} }");

            if (results is SparqlResultSet)
            {
                SparqlResultSet resultSet = (SparqlResultSet)results;
                foreach (SparqlResult result in resultSet)
                {

                    string s = result.ToString();
                    finalResult.Add(GetTypeName(s));
                }
                return (finalResult);
            }
            else return (null);
        }

        private List<string> GetAllValidLinkForADomain(string domain)
        {
            List<string> result = new List<string>();
            ArrayList domains = new ArrayList();

            domains = GetAllParents(domain);

            foreach (string domainVar in domains)
            {
                List<string> temp = new List<string>();
                temp = GetSpecificValidLinkForDomainObject(domainVar);
                result.AddRange(temp);
            }

            return result;
        }

        public OntologyNode GetAllValidRelationshipsHierarchyForDomainSet(List<string> domainNames)
        {
            ArrayList leaves = new ArrayList();
            OntologyNode ontologyRelationshipTree = GetOntologyRelationshipsHierarchy(leaves);
            if (ontologyRelationshipTree != null)
            {
                List<string> allValidLinks = new List<string>();
                foreach (var domain in domainNames)
                {
                    allValidLinks.AddRange(GetAllValidLinkForADomain(domain));
                }
                return (GetValidOntologyTreeAfterPruning(ontologyRelationshipTree, allValidLinks));
            }
            else
                throw new ApplicationException("SPARQL Query Or Ontology Configuration Is Wrong");
        }

        public List<string> GetSpecificValidLinkForDomainObject(string domain)
        {
            domain = MakeSafeString(domain);
            List<string> finalResult = new List<string>();
            ArrayList leaves = new ArrayList(GetAllOntologyRelationships());

            object results = graph.ExecuteQuery(@" 
                                                        PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                                                        PREFIX type: <" + baseUri + "#>" +
                                                   @"   SELECT ?link
	                                                              WHERE { {?link rdfs:domain type:" + domain + @" ; rdfs:subPropertyOf* type:" + GetRelationshipTypeURI() + @" }  
                                                                          UNION 
                                                                          {?link rdfs:range  type:" + domain + "; rdfs:subPropertyOf* type:" + GetRelationshipTypeURI() + " } }");

            if (results is SparqlResultSet)
            {
                SparqlResultSet resultSet = (SparqlResultSet)results;
                foreach (SparqlResult result in resultSet)
                {

                    string s = result.ToString();
                    finalResult.Add(GetTypeName(s));
                }
                OntologyNode ontologyRelationshipTree = GetOntologyRelationshipsHierarchy(leaves);

                //return (GetValidRelationshipsTree(ontologyRelationshipTree, finalResult));
                return finalResult;
            }
            else return (null);
        }

        public ObservableCollection<OntologyNode> GetEntitiesTreeForASpecificDomainAndLink(List<string> domainNames, string link)
        {
            OntologyNode ontologyObjecthipTree = GetOntologyCompleteObjectsHierarchy().FirstOrDefault();
            if (ontologyObjecthipTree != null)
            {
                List<string> entities = new List<string>();
                foreach (var domain in domainNames)
                {
                    entities.AddRange(GetAllEntitiesForASpecificDomainAndLink(domain, link));
                }

                List<string> entitiesHierarchy = new List<string>();
                entitiesHierarchy.AddRange(entities);
                foreach (var entity in entities)
                    entitiesHierarchy.AddRange(GetAllChilds(entity.ToString()));


                ObservableCollection<OntologyNode> ontoTree = null;
                if (entitiesHierarchy.Contains("شیء"))
                    ontoTree = ontologyObjecthipTree.Children;
                else
                {
                    OntologyNode ontologyTree = GetValidOntologyTreeAfterPruning(ontologyObjecthipTree, entitiesHierarchy);
                    ontoTree = ontologyTree.Children;
                }

                return ontoTree;
            }
            else
                throw new ApplicationException("SPARQL Query Or Ontology Configuration Is Wrong");
        }

        private List<string> GetAllEntitiesForASpecificDomainAndLink(string domain, string link)
        {
            domain = MakeSafeString(domain);
            link = MakeSafeString(link);

            string domains = string.Empty;
            ArrayList parents = GetAllParents(domain);
            foreach (var parent in parents)
            {
                domains += ":" + parent + ", ";
            }
            domains = domains.Remove(domains.Length - 2, 2);

            string links = string.Empty;
            List<string> linkParents = GetAllRelationshipChilds(link);
            foreach (var parent in linkParents)
            {
                links += ":" + parent + ", ";
            }
            links = links.Remove(links.Length - 2, 2);


            List<string> finalResult = new List<string>();
            object results = new object();
            string sparqlQuery = @"  PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                                     PREFIX owl: <http://www.w3.org/2002/07/owl#>
                                                 PREFIX : <" + baseUri + "#>" +
                                               @"SELECT ?x
	                                                WHERE {{?y a owl:ObjectProperty . 
                                                            FILTER (?y in(" + links + @" )) .} 
                                                            {?y rdfs:domain ?t . FILTER(?t in(" + domains + @")) .}  
                                                            {?y rdfs:range ?x} }";
            results = graph.ExecuteQuery(sparqlQuery);

            if (results is SparqlResultSet)
            {
                SparqlResultSet resultSet = (SparqlResultSet)results;
                foreach (SparqlResult result in resultSet)
                {
                    finalResult.Add(GetTypeName(result.ToString()));
                }
            }

            sparqlQuery = @"  PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                                     PREFIX owl: <http://www.w3.org/2002/07/owl#>
                                                 PREFIX : <" + baseUri + "#>" +
                                               @"SELECT ?x
	                                                WHERE {{?y a owl:ObjectProperty . 
                                                            FILTER (?y in(" + links + @" )) .} 
                                                            {?y rdfs:range ?t . FILTER(?t in(" + domains + @")) .}  
                                                            {?y rdfs:domain ?x} }";
            results = graph.ExecuteQuery(sparqlQuery);

            if (results is SparqlResultSet)
            {
                SparqlResultSet resultSet = (SparqlResultSet)results;
                foreach (SparqlResult result in resultSet)
                {
                    finalResult.Add(GetTypeName(result.ToString()));
                }
                return (finalResult);
            }


            else return (null);
        }

        /**<summary>
      *این تابع با دریافت دامنه، برد و نوع لینک انتخاب شده، جهت‌های ممکن برای آن لینک را مشخص می‌نماید
      *خروجی‌های تابع از قرار زیر است 
               *DomainToRange یا RanegToDomain => در صورتی که لینک یکطرفه باشد
               *Bidurectional => در صورتی که لینک دوطرفه باشد
               *DomainToRangeOrRangeToDomain => در صورتی که لینک مورد نظر در هر دو سمت امکانپذیر باشد ولی دوطرفه نباشد
               *Neutral => در صورتی که برای لینک هیچ جهتی شناسایی نشود
         *نکته: نوع گروه همیشه باید به عنوان برد، به تابع ارسال بشود
      *</summary>
       **/
        public Direction GetLinkDirection(string linkType, string domain, string range, bool isGroup)
        {
            Direction direction = Direction.Neutral;

            if (IsEvent(linkType))
                return Direction.Bidirectionl;
            if (!IsRelationship(linkType))
                throw new ApplicationException();
            if (isGroup && linkType == "عضو_گروه")
                return Direction.RangeToDomain;

            object results = graph.ExecuteQuery(@" 
                                                    PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                                                    PREFIX type: <" + baseUri + "#>" +
                                                @"   SELECT distinct ?range ?domain
	                                                            WHERE {type:" + linkType + " rdfs:domain ?domain;	rdfs:range  ?range}");

            object isFunc = graph.ExecuteQuery(@"PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
                                                        PREFIX owl: <http://www.w3.org/2002/07/owl#>
                                                        PREFIX type: <" + baseUri + "#>" +
                                                        "ASK  {type:" + linkType + " rdf:type owl:FunctionalProperty}");

            SparqlResultSet isFunctional = (SparqlResultSet)isFunc;


            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    string dom, rng = "";
                    dom = (r.Value("domain")).ToString();
                    dom = GetTypeName(dom);

                    rng = (r.Value("range")).ToString();
                    rng = GetTypeName(rng);

                    if (IsA(domain, dom) && IsA(range, rng))
                    {
                        if (direction == Direction.Neutral || direction == Direction.DomainToRange)
                            direction = Direction.DomainToRange;
                        else if (isFunctional.Result == false)
                        {
                            direction = Direction.Bidirectionl;
                            return (direction);
                        }
                        else
                        {
                            direction = Direction.DomainToRangeOrRangeToDomain;
                            return (direction);
                        }
                    }

                    if (IsA(domain, rng) && IsA(range, dom))
                    {
                        if (direction == Direction.Neutral || direction == Direction.RangeToDomain)
                            direction = Direction.RangeToDomain;
                        else if (isFunctional.Result == false)
                        {
                            direction = Direction.Bidirectionl;
                            return (direction);
                        }
                        else
                        {
                            direction = Direction.DomainToRangeOrRangeToDomain;
                            return (direction);
                        }

                    }
                }
                return (direction);
            }
            else
            {
                direction = Direction.Neutral;
                return (direction);
            }
        }

        ///<summary>
        ///این تابع دو نوع دریافت می‌کند و بررسی می‌کند که آیا نوع دوم جزء پدران نوع اول می‌باشد یا خیر
        ///</summary>
        private bool IsA(string type1, string type2)
        {
            ArrayList parents = GetAllParents(type1);
            return (parents.Contains(type2));
        }

        ///<summary>
        ///این تابع یک نوع دریافت می‌کند و بررسی می‌کند که این نوع آیا یک ویژگی می‌باشد یا خیر
        ///</summary>
        public bool IsProperty(string typeUri)
        {
            return allProperties.ContainsKey(typeUri);
        }

        ///<summary>
        ///این تابع یک نوع دریافت می‌کند و بررسی می‌کند که این نوع آیا یک لینک می‌باشد یا خیر
        ///</summary>
        public bool IsRelationship(string type)
        {
            // TODO: کارایی | هستان شناسی - می‌توان با پیش‌پردازش بازیابی روابط (مانند دیگر مفاهیم) کارایی این تابع را به طرز چشم‌گیری افزایش داد
            object isExist = graph.ExecuteQuery(@"PREFIX rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>
                                                            PREFIX owl: <http://www.w3.org/2002/07/owl#>
                                                            PREFIX type: <" + baseUri + "#>" +
                                 "ASK  {type:" + type + " rdf:type owl:ObjectProperty}");

            SparqlResultSet LinkExistance = (SparqlResultSet)isExist;
            return (LinkExistance.Result);
        }

        ///<summary>
        ///این تابع یک نوع دریافت می‌کند و بررسی می‌کند که این نوع آیا یک رخداد می‌باشد یا خیر
        ///</summary>
        public bool IsEvent(string type)
        {
            return eventSubTypeNames.Contains(type);
        }

        ///<summary>
        ///این تابع یک نوع دریافت می‌کند و بررسی می‌کند که این نوع آیا یک موجودیت می‌باشد یا خیر
        ///</summary>
        public bool IsEntity(string type)
        {
            return entitySubTypeNames.Contains(type);
        }

        ///<summary>
        ///این تابع یک نوع دریافت می‌کند و بررسی می‌کند که این نوع آیا یک سند می‌باشد یا خیر
        ///</summary>

        public bool IsDocument(string type)
        {
            return documentSubTypeNames.Contains(type);
        }

        public bool IsTextDocument(string type)
        {
            return textDocumentSubTypeNames.Contains(type);
        }

        public bool IsMultimediaDocument(string type)
        {
            return multimediaSubTypeNames.Contains(type);
        }

        public bool IsImageDocument(string type)
        {
            return imageDocumentSubTypeNames.Contains(type);
        }

        public bool IsAudioDocument(string type)
        {
            return audioDocumentSubTypeNames.Contains(type);
        }

        public List<string> GetStringTypeProperties(List<string> objectTypes)
        {
            List<string> properties = new List<string>();
            foreach (var currentType in objectTypes)
            {
                List<DataType> allProperties = GetAllPropertiesOfObject(currentType);
                foreach (var currentProperty in allProperties)
                {
                    if (currentProperty.BaseDataType == BaseDataTypes.String
                        && !currentProperty.TypeName.Equals(GetDateRangeAndLocationPropertyTypeUri()))
                    {
                        if (!properties.Contains(currentProperty.TypeName))
                        {
                            properties.Add(currentProperty.TypeName);
                        }
                    }
                }
            }

            return properties;
        }

        public string InferGroupType(IEnumerable<string> groupMembersTypes)
        {
            return InferGroupType(new ArrayList(new ObservableCollection<string>(groupMembersTypes)));
        }

        /// <summary>
        /// این تابع یک لیست از انواع اعضاء گروه را دریافت کرده و اولین پدر مشترک این اعضاء را یافته و به عنوان نوع گروه ارسال می‌نماید
        /// </summary>
        /// <param name="groupMembersTypes"></param>
        /// <returns></returns>
        public string InferGroupType(ArrayList groupMembersTypes)
        {
            ArrayList allParentsList = new ArrayList();
            int minLength = Int16.MaxValue;
            int index = 0;
            foreach (string memberType in groupMembersTypes)
            {
                ArrayList temp = GetAllParents(memberType);
                if (temp.Count < minLength)
                {
                    minLength = temp.Count;
                    index = groupMembersTypes.IndexOf(memberType);
                }
                allParentsList.Add(temp);
            }

            ArrayList selectedParents = (ArrayList)allParentsList[index];
            allParentsList.RemoveAt(index);
            bool sw = new bool();
            foreach (string parent in selectedParents)
            {
                sw = true;
                foreach (ArrayList parentList in allParentsList)
                {
                    if (parentList.IndexOf(parent) == -1)
                    {
                        sw = false;
                        break;
                    }
                }
                if (sw)
                    return (parent);
            }
            return ("شیء");
        }

        public ObservableCollection<OntologyNode> GetHierarchyPropertiesOfObjects(List<string> objectTypeList)
        {
            StringBuilder query = new StringBuilder(string.Empty);
            var parentsList = GetAllParentsForObjects(objectTypeList);

            query.Append(@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                               PREFIX type: <" + baseUri + "#>" +
                         @"SELECT DISTINCT ?subject WHERE {
                                  {?subject rdfs:subPropertyOf type:ویژگی; rdfs:domain type:" + objectTypeList.First() + "}");

            foreach (var objectType in objectTypeList)
            {
                query.Append(" UNION {?subject rdfs:subPropertyOf type:ویژگی; rdfs:domain type:" + objectType + "}");
            }

            foreach (var parent in parentsList)
            {
                query.Append(" UNION {?subject rdfs:subPropertyOf type:ویژگی; rdfs:domain type:" + parent + "}");
            }

            query.Append("}");

            OntologyNode tree = new PropertyNode();
            ObservableCollection<OntologyNode> ontoTree = tree.Children;

            object results = graph.ExecuteQuery(query.ToString());

            if (!(results is SparqlResultSet))
                return null;

            SparqlResultSet resultSet = (SparqlResultSet)results;
            foreach (SparqlResult result in resultSet)
            {
                PropertyNode parentNode = new PropertyNode();
                ontoTree.Add(CreatePropertyNodeType(parentNode, GetTypeName(result.ToString()), objectTypeList,
                    parentsList));
            }

            return SetPropertiesBaseDataType(ontoTree);
        }

        public ObservableCollection<OntologyNode> GetHierarchyPropertiesOfObject(string objectType)
        {
            StringBuilder query = new StringBuilder(string.Empty);
            ArrayList parentsList = null;

            if (string.IsNullOrEmpty(objectType))
            {
                query.Append(@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                               PREFIX type: <" + baseUri + "#>" +
                             @"SELECT ?subject WHERE {?subject rdfs:subPropertyOf type:ویژگی}");
            }
            else
            {
                query.Append(@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                               PREFIX type: <" + baseUri + "#>" +
                             @"SELECT DISTINCT ?subject WHERE {
                                  {?subject rdfs:subPropertyOf type:ویژگی; rdfs:domain type:" + objectType + "}");

                parentsList = GetAllParents(objectType);

                foreach (var parent in parentsList)
                {
                    query.Append(" UNION {?subject rdfs:subPropertyOf type:ویژگی; rdfs:domain type:" + parent + "}");
                }

                query.Append("}");
            }

            OntologyNode tree = new OntologyNode();
            ObservableCollection<OntologyNode> ontoTree = tree.Children;

            object results = graph.ExecuteQuery(query.ToString());

            if (!(results is SparqlResultSet))
                return null;

            SparqlResultSet resultSet = (SparqlResultSet)results;
            foreach (SparqlResult result in resultSet)
            {
                PropertyNode parentNode = new PropertyNode();
                ontoTree.Add(CreatePropertyNodeType(parentNode, GetTypeName(result.ToString()), objectType, parentsList));
            }

            return SetPropertiesBaseDataType(ontoTree);
        }

        private OntologyNode CreatePropertyNodeType(OntologyNode parentNode, string currentOnto, string objectType,
            ArrayList parentsList)
        {
            parentNode.TypeUri = currentOnto;
            string temp = currentOnto;

            StringBuilder query = new StringBuilder(string.Empty);

            if (string.IsNullOrEmpty(objectType))
            {
                query.Append(@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                               PREFIX type: <" + baseUri + "#>" +
                             @"SELECT ?subject WHERE {
                                 ?subject rdfs:subPropertyOf type:" + temp + "}");
            }
            else
            {
                query.Append(@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                               PREFIX type: <" + baseUri + "#>" +
                             @"SELECT DISTINCT ?subject WHERE {
                                 {?subject rdfs:subPropertyOf type:" + temp + "; rdfs:domain type:" + objectType + "}");

                foreach (var parent in parentsList)
                {
                    query.Append(" UNION {?subject rdfs:subPropertyOf type:" + temp + "; rdfs:domain type:" + parent + "}");
                }

                query.Append("}");
            }

            //TODO: change subject to property in query
            object results = graph.ExecuteQuery(query.ToString());

            if (!(results is SparqlResultSet))
                return null;

            var resultSet = (SparqlResultSet)results;
            foreach (SparqlResult result in resultSet)
            {
                PropertyNode childNode = new PropertyNode();
                parentNode.Children.Add(childNode);
                CreatePropertyNodeType(childNode, GetTypeName(result.ToString()), objectType, parentsList);
            }
            return parentNode;
        }

        private OntologyNode CreatePropertyNodeType(OntologyNode parentNode, string currentOnto, List<string> objectTypes,
            ArrayList parentsList)
        {
            parentNode.TypeUri = currentOnto;
            string temp = currentOnto;

            StringBuilder query = new StringBuilder(string.Empty);

            query.Append(@"PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                               PREFIX type: <" + baseUri + "#>" +
                         @"SELECT DISTINCT ?subject WHERE {
                                 {?subject rdfs:subPropertyOf type:" + temp + "; rdfs:domain type:" + objectTypes.First() + "}");

            foreach (var objectType in objectTypes)
            {
                query.Append(" UNION {?subject rdfs:subPropertyOf type:" + temp + "; rdfs:domain type:" + objectType + "}");
            }

            foreach (var parent in parentsList)
            {
                query.Append(" UNION {?subject rdfs:subPropertyOf type:" + temp + "; rdfs:domain type:" + parent + "}");
            }

            query.Append("}");


            //TODO: change subject to property in query
            object results = graph.ExecuteQuery(query.ToString());

            if (!(results is SparqlResultSet))
                return null;

            var resultSet = (SparqlResultSet)results;
            foreach (SparqlResult result in resultSet)
            {
                PropertyNode childNode = new PropertyNode();
                parentNode.Children.Add(childNode);
                CreatePropertyNodeType(childNode, GetTypeName(result.ToString()), objectTypes, parentsList);
            }
            return parentNode;
        }

        private ObservableCollection<OntologyNode> SetPropertiesBaseDataType(ObservableCollection<OntologyNode> treeCollection)
        {
            var allProperties = graph.ExecuteQuery(@"PREFIX owl: <http://www.w3.org/2002/07/owl#>
                                                     PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                                                     SELECT DISTINCT ?Property ?DataType
	                                                 WHERE { ?Property a owl:DatatypeProperty;
			                                         rdfs:range ?DataType.}");


            SparqlResultSet allPropertiesResult = (SparqlResultSet)allProperties;
            var typeList = allPropertiesResult.Results;
            foreach (var node in treeCollection)
            {
                if (!(node is PropertyNode))
                    continue;

                PropertyNode child = node as PropertyNode;

                SparqlResult typeResult = new SparqlResult();
                string typeName = GetTypeName(child.TypeUri);

                foreach (var item in typeList)
                {
                    if (typeName == GetTypeName(item[0].ToString()))
                    {
                        if (typeName.Equals(GetDateRangeAndLocationPropertyTypeUri()))
                        {
                            child.BaseDataType = BaseDataTypes.GeoTime;
                        }
                        else
                        {
                            switch (GetTypeName(item[1].ToString()))
                            {
                                case "int":
                                    child.BaseDataType = BaseDataTypes.Int;
                                    break;
                                case "integer":
                                    child.BaseDataType = BaseDataTypes.Int;
                                    break;
                                case "double":
                                    child.BaseDataType = BaseDataTypes.Double;
                                    break;
                                case "boolean":
                                    child.BaseDataType = BaseDataTypes.Boolean;
                                    break;
                                case "string":
                                    child.BaseDataType = BaseDataTypes.String;
                                    break;
                                case "dateTime":
                                    child.BaseDataType = BaseDataTypes.DateTime;
                                    break;
                                case "anyURI":
                                    child.BaseDataType = BaseDataTypes.HdfsURI;
                                    break;
                                case "long":
                                    child.BaseDataType = BaseDataTypes.Long;
                                    break;
                                case "geoPoint":
                                    child.BaseDataType = BaseDataTypes.GeoPoint;
                                    break;
                                default:
                                    throw new NotSupportedException($"Invalid base type: {GetTypeName(item[1].ToString())}");
                            }
                        }
                    }
                }

                var nodes = SetPropertiesBaseDataType(child.Children);
            }

            return treeCollection;
        }

        /// <summary>
        /// این تابع با دریافت یک نوع شیء، تمامی ویژگی‌های مربوط به آن شیء به همراه انواع آن‌ها را بر می‌گرداند
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public List<DataType> GetAllPropertiesOfObject(string objectType)
        {
            List<DataType> properties = new List<DataType>();

            Object AllProperties = graph.ExecuteQuery(@"PREFIX owl: <http://www.w3.org/2002/07/owl#>
                                                            PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                                                            SELECT DISTINCT ?Property ?DataType
	                                                            WHERE { ?Property a owl:DatatypeProperty;
			                                                                rdfs:range ?DataType.}");

            SparqlResultSet results = new SparqlResultSet();

            foreach (var item in GetAllParents(objectType))
            {
                object temp = graph.ExecuteQuery(@"  PREFIX owl: <http://www.w3.org/2002/07/owl#>
                                                        PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                                                        PREFIX type: <" + baseUri + "#>" +
                                                    @"   SELECT distinct ?dataProperty
                                                                   WHERE {?dataProperty a owl:DatatypeProperty;
                                                                            rdfs:domain type:" + item + "}");

                results.Results.AddRange((SparqlResultSet)temp);
            }

            if (results is SparqlResultSet)
            {
                SparqlResultSet AllPropertiesResult = (SparqlResultSet)AllProperties;
                List<SparqlResult> typeList = new List<SparqlResult>();
                typeList = AllPropertiesResult.Results;
                SparqlResultSet resultSet = results;
                foreach (SparqlResult result in resultSet)
                {
                    SparqlResult typeResult = new SparqlResult();
                    DataType tempType = new DataType
                    {
                        TypeName = GetTypeName(result.ToString())
                    };

                    foreach (var item in typeList)
                    {
                        if (tempType.TypeName == GetTypeName(item[0].ToString()))
                        {
                            if (tempType.TypeName.Equals(GetDateRangeAndLocationPropertyTypeUri()))
                            {
                                tempType.BaseDataType = BaseDataTypes.GeoTime;
                            }
                            else
                            {
                                switch (GetTypeName(item[1].ToString()))
                                {
                                    case "int":
                                        tempType.BaseDataType = BaseDataTypes.Int;
                                        break;
                                    case "integer":
                                        tempType.BaseDataType = BaseDataTypes.Int;
                                        break;
                                    case "double":
                                        tempType.BaseDataType = BaseDataTypes.Double;
                                        break;
                                    case "boolean":
                                        tempType.BaseDataType = BaseDataTypes.Boolean;
                                        break;
                                    case "string":
                                        tempType.BaseDataType = BaseDataTypes.String;
                                        break;
                                    case "dateTime":
                                        tempType.BaseDataType = BaseDataTypes.DateTime;
                                        break;
                                    case "anyURI":
                                        tempType.BaseDataType = BaseDataTypes.HdfsURI;
                                        break;
                                    case "long":
                                        tempType.BaseDataType = BaseDataTypes.Long;
                                        break;
                                    case "geoPoint":
                                        tempType.BaseDataType = BaseDataTypes.GeoPoint;
                                        break;
                                    default:
                                        throw new NotSupportedException(string.Format("Invalid base type: {0}", GetTypeName(item[1].ToString())));
                                }
                            }
                        }
                    }
                    properties.Add(tempType);
                }
            }
            return (properties);
        }

        /// <summary>
        /// این تابع با دریافت یک شیء، تمامی ویژگی‌های پدر این شیء را به عنوان خروجی ارسال می‌نماید
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public List<DataType> GetAllPropertiesOfParentOfObject(string objectType)
        {
            string parentName = GetParent(objectType);
            return GetAllPropertiesOfObject(parentName);
        }

        /// <summary>
        ///   این تابع با دریافت پدر یک شیء، اجتماع ویژگی‌های اشیاء همزاد با این شیء را محاسبه و به عنوان خروجی ارسال می‌کند.
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public List<DataType> GetUnionOfPropertiesOfSiblingsOfObject(string parentName)
        {
            object results = new object();

            results = graph.ExecuteQuery(@"  PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                                             PREFIX type: <" + baseUri + "#>   " +
                                       @"
                                             SELECT ?parent
	                                                WHERE { ?parent rdfs:subClassOf type:" + parentName + "}");

            if (((SparqlResultSet)results).Count == 0)
            {
                List<DataType> IntersectionPropertiesList = new List<DataType>();
                IntersectionPropertiesList = GetAllPropertiesOfObject(parentName);
                return IntersectionPropertiesList;
            }

            List<DataType> unionPropertiesSet = new List<DataType>();

            if (results is SparqlResultSet)
            {
                SparqlResultSet resualtSet = (SparqlResultSet)results;
                foreach (SparqlResult result in resualtSet)
                {
                    List<DataType> temp = new List<DataType>();
                    temp = GetAllPropertiesOfObject(GetTypeName(result.ToString()));
                    unionPropertiesSet = unionPropertiesSet.Union(temp).ToList();
                }
                return (unionPropertiesSet);
            }
            else return (null);
        }

        /// <summary>
        ///   این تابع با دریافت پدر یک شیء، اشتراک ویژگی‌های اشیاء همزاد با این شیء را محاسبه و به عنوان خروجی ارسال می‌کند.
        /// </summary>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public List<DataType> GetIntersectOfPropertiesOfSiblingsOfObject(string parentName)
        {
            object results = new object();

            results = graph.ExecuteQuery(@"  PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                                             PREFIX type: <" + baseUri + "#>   " +
                                       @"
                                             SELECT ?parent
	                                                WHERE { ?parent rdfs:subClassOf type:" + parentName + "}");

            if (((SparqlResultSet)results).Count == 0)
            {
                List<DataType> IntersectionPropertiesList = new List<DataType>();
                IntersectionPropertiesList = GetAllPropertiesOfObject(parentName);
                return IntersectionPropertiesList;
            }

            List<DataType> IntersectionPropertiesSet = new List<DataType>();
            IntersectionPropertiesSet = GetAllProperties();

            if (results is SparqlResultSet)
            {
                SparqlResultSet resultSet = (SparqlResultSet)results;
                foreach (SparqlResult result in resultSet)
                {
                    List<DataType> temp = new List<DataType>();
                    temp = GetAllPropertiesOfObject(GetTypeName(result.ToString()));
                    IntersectionPropertiesSet = IntersectionPropertiesSet.Intersect(temp).ToList();
                }
                return (IntersectionPropertiesSet);
            }
            else return (null);
        }

        private void PrefetchAllProperties()
        {
            object AllProperties = graph.ExecuteQuery(@"PREFIX owl: <http://www.w3.org/2002/07/owl#>
                                                            PREFIX rdfs: <http://www.w3.org/2000/01/rdf-schema#>
                                                            SELECT DISTINCT ?Property ?DataType
	                                                            WHERE { ?Property a owl:DatatypeProperty;
			                                                                rdfs:range ?DataType.}");
            if (AllProperties is SparqlResultSet)
            {
                SparqlResultSet AllPropertiesResult = (SparqlResultSet)AllProperties;
                List<SparqlResult> typeList = new List<SparqlResult>();
                typeList = AllPropertiesResult.Results;
                foreach (SparqlResult r in typeList)
                {
                    SparqlResult typeResult = new SparqlResult();
                    DataType tempType = new DataType();
                    string s = r[0].ToString();
                    tempType.TypeName = GetTypeName(s);

                    if (tempType.TypeName.Equals(GetDateRangeAndLocationPropertyTypeUri()))
                    {
                        tempType.BaseDataType = BaseDataTypes.GeoTime;
                    }
                    else
                    {
                        switch (GetTypeName(r[1].ToString()))
                        {
                            case "int":
                                tempType.BaseDataType = BaseDataTypes.Int;
                                break;
                            case "integer":
                                tempType.BaseDataType = BaseDataTypes.Int;
                                break;
                            case "double":
                                tempType.BaseDataType = BaseDataTypes.Double;
                                break;
                            case "boolean":
                                tempType.BaseDataType = BaseDataTypes.Boolean;
                                break;
                            case "string":
                                tempType.BaseDataType = BaseDataTypes.String;
                                break;
                            case "dateTime":
                                tempType.BaseDataType = BaseDataTypes.DateTime;
                                break;
                            case "anyURI":
                                tempType.BaseDataType = BaseDataTypes.HdfsURI;
                                break;
                            case "long":
                                tempType.BaseDataType = BaseDataTypes.Long;
                                break;
                            case "geoPoint":
                                tempType.BaseDataType = BaseDataTypes.GeoPoint;
                                break;
                            default:
                                throw new NotSupportedException(string.Format("Invalid base type: {0}", GetTypeName(r[1].ToString())));
                        }
                    }
                    allProperties.Add(tempType.TypeName, tempType);
                }
            }
        }

        /// <summary>
        /// این تابع تمامی ویژگی‌های تعریف شده در هستان‌شناسی را به همراه انواع آن‌ها، بر می‌گرداند
        /// </summary>
        /// <returns></returns>
        public List<DataType> GetAllProperties()
        {
            return allProperties.Values.ToList();
        }

        /// <summary>
        /// این تابع نوع یک ویژگی را دریافت کرده و نوع پایه برای آن ویژگی را به عنوان خروجی برمی‌گرداند
        /// </summary>
        /// <param name="propertyTypeUri"></param>
        /// <returns></returns>
        public BaseDataTypes GetBaseDataTypeOfProperty(string propertyTypeUri)
        {
            if (allProperties.ContainsKey(propertyTypeUri))
            {
                return allProperties[propertyTypeUri].BaseDataType;
            }
            else
            {
                throw new ArgumentException(string.Format("Property type \"{0}\" not defined in the ontology", propertyTypeUri));
            }
        }

        /// <summary>
        /// این تابع تمام ویژگی‌های موجود از یک نوع پایه را برمی‌گرداند
        /// </summary>
        /// <param name="dataType">نوع پایه</param>
        /// <returns>مجموعه ای از ویژگی ها</returns>
        public IEnumerable<string> GetAllPropertiesOfBaseDataType(BaseDataTypes dataType)
        {
            return allProperties.GroupBy(p => p.Value.BaseDataType)?.
                FirstOrDefault(g => g.Key == dataType)?.
                Select(x => x.Key);
        }

        public bool IsObject(string objTypeUri)
        {
            return IsEntity(objTypeUri)
                || IsEvent(objTypeUri)
                || IsDocument(objTypeUri);
        }

        /// <summary>
        /// این تابع بر اساس هستان‌شناسی موجود، فایل پیش‌فرضی را به عنوان فراداده هستان‌شناسی، در مسیری که به عنوان ورودی به‌آن داده شده است، ایجاد می‌کند
        /// </summary>
        /// <param name="filePath"></param>
        public void GenerateDefaultOntologyMetadata(string filePath)
        {
            ObservableCollection<OntologyNode> ontologyTree = GetOntologyObjectsHierarchy();

            TreeNode ontoNode = new TreeNode();
            TreeNodeCollection ontoCollection = ontoNode.Nodes;

            FileStream metadataFile = new FileStream(filePath + "\\Ontology Metadata.csv", FileMode.OpenOrCreate, FileAccess.Write);
            StreamWriter streamWriter = new StreamWriter(metadataFile);
            try
            {
                foreach (var tree in ontologyTree)
                {
                    TreeNode ontoNode2 = new TreeNode();
                    ontoCollection.Add(ParseOntologyTree(tree, ontoNode2, streamWriter));
                }
            }
            finally
            {
                streamWriter.Close();
            }
        }

        /// <summary>
        /// این تابع برای پویش درخت هستان‌شناسی و ایجاد فایل فراداده از روی آن استفاده می‌شود.
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="ontoNode"></param>
        /// <param name="sr"></param>
        /// <returns></returns>
        private TreeNode ParseOntologyTree(OntologyNode tree, TreeNode ontoNode, StreamWriter sr)
        {
            ontoNode.Name = tree.TypeUri;
            ontoNode.Text = tree.TypeUri;
            foreach (OntologyNode item in tree.Children)
            {
                TreeNode child = new TreeNode();
                ontoNode.Nodes.Add(child);
                ParseOntologyTree(item, child, sr);
                byte[] stringBytes = Encoding.Unicode.GetBytes(GetTypeName(child.Name));
                sr.WriteLine(Encoding.Unicode.GetString(stringBytes) + ", false, true");
            }
            return ontoNode;
        }

        /// <summary>
        /// این تابع نام یک شیء را دریافت کرده و مشخص می‌نماید که آیا این شیء از هستان‌شناسی حذف شده است یا نه.
        /// </summary>
        /// <param name="conceptName"></param>
        /// <returns></returns>
        public bool IsDeprecated(string conceptName)
        {
            if (metadataDictionary.ContainsKey(conceptName))
            {
                return metadataDictionary[conceptName].isDeprecated;
            }

            return false;
        }

        /// <summary>
        /// این تابع نام یک شیء را دریافت کرده و مشخص می‌نماید که آیا این شیء قابل جستجو است یا خیر.
        /// </summary>
        /// <param name="conceptName"></param>
        /// <returns></returns>
        public bool IsSearchable(string conceptName)
        {
            if (metadataDictionary.ContainsKey(conceptName))
            {
                return metadataDictionary[conceptName].searchable;
            }

            return false;
        }

        public string GetAllConceptsTypeURI()
        {
            return ("شیء");
        }

        public string GetEntityTypeURI()
        {
            return ("موجودیت");
        }

        public string GetEventTypeURI()
        {
            return ("رخداد");
        }

        public string GetDocumentTypeURI()
        {
            return ("سند");
        }

        public string GetPersonObjectTypeURI()
        {
            if (GetAllObjectTypeURIs().Contains("شخص"))
                return "شخص";
            else
                return null;
        }

        public string GetEmailObjectTypeURI()
        {
            if (GetAllObjectTypeURIs().Contains("پست_الکترونیک"))
                return "پست_الکترونیک";
            else
                return null;
        }

        public string GetNamePropertyTypeURI()
        {
            if (GetAllProperties().FirstOrDefault(p => p.TypeName == "نام") == null)
                return null;
            else
                return "نام";
        }

        public string GetEmailAddressPropertyTypeURI()
        {
            if (GetAllProperties().FirstOrDefault(p => p.TypeName == "آدرس_ایمیل") == null)
                return null;
            else
                return "آدرس_ایمیل";
        }

        public string GetEmailSubjectPropertyTypeURI()
        {
            if (GetAllProperties().FirstOrDefault(p => p.TypeName == "موضوع_ایمیل") == null)
                return null;
            else
                return "موضوع_ایمیل";
        }

        public string GetEmailBodyPropertyTypeURI()
        {
            if (GetAllProperties().FirstOrDefault(p => p.TypeName == "متن_ایمیل") == null)
                return null;
            else
                return "متن_ایمیل";
        }

        public string GetEmailIdPropertyTypeURI()
        {
            if (GetAllProperties().FirstOrDefault(p => p.TypeName == "شناسه_ایمیل") == null)
                return null;
            else
                return "شناسه_ایمیل";
        }

        public string GetEmailReceiveMethodPropertyTypeURI()
        {
            if (GetAllProperties().FirstOrDefault(p => p.TypeName == "روش_دریافت_ایمیل") == null)
                return null;
            else
                return "روش_دریافت_ایمیل";
        }

        public string GetSentTimePropertyTypeURI()
        {
            if (GetAllProperties().FirstOrDefault(p => p.TypeName == "زمان_ارسال") == null)
                return null;
            else
                return "زمان_ارسال";
        }

        public string GetReceiverAddressPropertyTypeURI()
        {
            if (GetAllProperties().FirstOrDefault(p => p.TypeName == "آدرس_گیرنده") == null)
                return null;
            else
                return "آدرس_گیرنده";
        }

        public string GetSenderAddressPropertyTypeURI()
        {
            if (GetAllProperties().FirstOrDefault(p => p.TypeName == "آدرس_فرستنده") == null)
                return null;
            else
                return "آدرس_فرستنده";
        }

        public string GetDocumentNamePropertyTypUri()
        {
            return "نام_فایل";
        }

        public string GetDefaultIpPropertyTypeURI()
        {
            return ("آدرس_آی_پی");
        }

        private string GetMultimediaDocumentTypeURI()
        {
            return ("چند_رسانه_ای");
        }

        private string GetTextDocumentTypeURI()
        {
            return ("متنی");
        }

        private string GetImageDocumentTypeURI()
        {
            return ("تصویری");
        }

        private string GetAudioDocumentTypeURI()
        {
            return ("صوتی");
        }

        public string GetRelationshipTypeURI()
        {
            return "رابطه";
        }

        public string GetAttachmentRelationshipTypeURI()
        {
            if (GetAllRelationships().Contains("الصاق"))
                return "الصاق";
            else
                return null;
        }

        private IEnumerable<string> GetAllRelationships()
        {
            return GetAllOntologyRelationships().Select(r => GetTypeName(r));
        }

        public string GetDefaultRelationshipTypeForEventBasedLink(string domain, string intermidateEvent, string range)
        {
            return ("حضور_در");
        }

        public string DefaultGroupRelationshipType()
        {
            return ("عضو_گروه");
        }

        public string[] GetDocumentSubTypeURIs()
        {
            return textDocumentSubTypeNames
                .Union(multimediaSubTypeNames)
                .Union(audioDocumentSubTypeNames)
                .Union(imageDocumentSubTypeNames).ToArray();
        }

        public string[] GetTextDocumentFileTypes()
        {
            return textDocumentSubTypeNames.ToArray();
        }

        public string[] GetVideoFileTypes()
        {
            return multimediaSubTypeNames.ToArray();
        }

        public string[] GetAudioFileTypes()
        {
            return audioDocumentSubTypeNames.ToArray();
        }

        public string[] GetImageFileTypes()
        {
            return imageDocumentSubTypeNames.ToArray();
        }

        public string[] GetTabularFileTypes()
        {
            return tabularTypeNames.ToArray();
        }


        public string[] GetAllObjectTypeURIs()
        {
            return entitySubTypeNames
                .Union(eventSubTypeNames)
                .Union(documentSubTypeNames)
                .Union(new string[] { GetEntityTypeURI(), GetEventTypeURI(), GetDocumentTypeURI() })
                .ToArray();
        }

        public string GetDateRangeAndLocationPropertyTypeUri()
        {
            return "زمان_و_موقعیت_جغرافیایی";
        }

        public string GetLocationPropertyTypeUri()
        {
            return "موقعیت_جغرافیایی";
        }

        public string GetDefaultDisplayNamePropertyTypeUri()
        {
            return "label";
        }

        public string GetDocumentTypeUriByFileExtension(string fileExtension)
        {
            if (fileExtension == null)
                throw new ArgumentNullException(nameof(fileExtension));

            string fileExtensionToUpper = fileExtension.ToUpper();
            if (IsDocument(fileExtensionToUpper))
            {
                return fileExtensionToUpper;
            }
            else
            {
                return GetDocumentTypeURI();
            }
        }
    }
}
