using GPAS.AccessControl;
using GPAS.Logger;
using GPAS.SearchServer.Entities.SearchEngine;
using GPAS.SearchServer.Entities.SearchEngine.Documents;
using Newtonsoft.Json.Linq;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace GPAS.SearchServer.Access.SearchEngine.ApacheSolr
{
    public partial class AccessClient : IAccessClient
    {
        public static readonly string SolrURL = ConfigurationManager.AppSettings["SolrURL"];
        public static readonly string SolrConfigSetPath = ConfigurationManager.AppSettings["SolrConfigSetPath"];
        public const bool StoreFieldsData = true;
        public bool AutoCommitEnabled { get; set; }
        public long AutoCommitMaximumPendingDocumentsCount { get; set; }

        public void AddFieldToSchema(string fieldName, DataType fieldType, bool isMultiValue = false)
        {
            AddFieldToSchema(fieldName, fieldType, Collections.ObjectCollection, isMultiValue, true);
            AddFieldToSchema(fieldName, fieldType, Collections.FileCollection, isMultiValue, true);
            AddFieldToSchema(fieldName, fieldType, Collections.DataSourceCollection, isMultiValue, true);
        }
        private void AddFieldToSchema(string fieldName, DataType fieldType, Collections target, bool isMultiValue, bool isIndexed = true, bool docValues = false)
        {
            ValidateSchemaNewFieldName(fieldName);
            if (IsFieldExistsInSchema(fieldName, target))
            {
                return;
            }
            var newField = new SchemaField()
            {
                Name = fieldName,
                IsIndexed = isIndexed,
                IsMultiValue = isMultiValue,
                IsStored = StoreFieldsData,
                Type = GetSolrTypeFromDataType(fieldType),
                DocValues = docValues
            };
            AddFieldToSchema(newField, target);
        }

        private void ValidateSchemaNewFieldName(string fieldName)
        {
            if (fieldName.ToLower().Equals("id"))
            {
                throw new InvalidOperationException("ID is a build-in field name");
            }
            if (string.IsNullOrWhiteSpace(fieldName))
            {
                throw new InvalidOperationException("Invalid ID value");
            }
        }

        private void AddFieldToSchema(SchemaField field, Collections target)
        {
            JObject fieldJson = new JObject();
            fieldJson.Add(new JProperty("name", field.Name));
            fieldJson.Add(new JProperty("type", field.Type));
            fieldJson.Add(new JProperty("multiValued", field.IsMultiValue.ToString()));
            fieldJson.Add(new JProperty("indexed", field.IsIndexed.ToString()));
            fieldJson.Add(new JProperty("stored", field.IsStored.ToString()));
            fieldJson.Add(new JProperty("docValues", field.DocValues.ToString()));
            JObject addFieldJson = new JObject();
            addFieldJson.Add(new JProperty("add-field", fieldJson));
            if (target == Collections.ObjectCollection)
            {
                PostToSchema(addFieldJson.ToString(), ObjectCollection.SolrUrl);
            }
            if (target == Collections.FileCollection)
            {
                PostToSchema(addFieldJson.ToString(), FileCollection.SolrUrl);
            }
            if (target == Collections.DataSourceCollection)
            {
                PostToSchema(addFieldJson.ToString(), DataSourceCollection.SolrUrl);
            }
            if (target == Collections.ImageCollection)
            {
                PostToSchema(addFieldJson.ToString(), ImageCollection.SolrUrl);
            }
            if (target == Collections.DataSourceAciCollection)
            {
                PostToSchema(addFieldJson.ToString(), DataSourceAciCollection.SolrUrl);
            }
            if (target == Collections.GraphCollection)
            {
                PostToSchema(addFieldJson.ToString(), GraphCollection.SolrUrl);
                if (target == Collections.ResolveCollection)
                {
                    PostToSchema(addFieldJson.ToString(), ResolveCollection.SolrUrl);
                }
            }
        }
        public void PostToSchema(string jsonSchema, string collectionSolrUrl)
        {
            var client = new RestClient(collectionSolrUrl);
            client.Authenticator = new HttpBasicAuthenticator("solr", "SolrRocks");
            var request = new RestRequest("/schema", Method.POST);
            request.AddParameter("application/json", jsonSchema, ParameterType.RequestBody);
            var response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                throw response.ErrorException;
            }

        }
        private string GetSolrTypeFromDataType(DataType fieldType)
        {
            switch (fieldType)
            {
                case DataType.Int:
                case DataType.Long:
                    return "long";
                case DataType.Bool:
                    return "boolean";
                case DataType.String:
                    return "text_general";
                case DataType.Double:
                    return "double";
                case DataType.DateTime:
                    return "date";
                case DataType.GeoLocation:
                    return "geo_rpt";
                case DataType.DateRange:
                    return "date_range";
                case DataType.PInts:
                    return "pints";
                case DataType.KeywordTokenizedString:
                    return "text_keyword_tokenized";
                default:
                    throw new NotSupportedException();
            }
        }

        public void Init(string[] groupNames)
        {
            AutoCommitEnabled = true;
            AutoCommitMaximumPendingDocumentsCount = 1000000;
            //UploadConfigSets();
            //BuildCollections();
            //ApplyDocumentFieldsToRelatedSchema();
            //ApplyGroupsLastChangesToSchema(groupNames);
        }

        private void ApplyDocumentFieldsToRelatedSchema()
        {
            //Apply Fields To ObjectCollection
            ApplyObjectDocumentStaticFieldsToObjectCollectionSchema();
            ApplyPropertyDocumentStaticFieldsToObjectCollectionSchema();
            ApplyRelationshipDocumentStaticFieldsToObjectCollectionSchema();

            //Apply Fields To DataSourceCollection
            ApplyPropertyDocumentStaticFieldsToDataSourceCollectionSchema();

            //Apply Fields To FileCollection
            ApplyFileDocumentStaticFieldsToFileCollectionSchema();

            //Apply Fields To ImageCollection
            ApplyImageDocumentStaticFieldsToImageCollectionSchema();

            //Apply Fields To DataSourceAci
            ApplyDataSourceAciDocumentStaticFieldsToDataSourceAciCollectionSchema();

            //Apply Fields To Graph
            ApplyDataSourceAciDocumentStaticFieldsToGraphCollectionSchema();

            //Apply Fields To ResolveCollection
            ApplyImageDocumentStaticFieldsToResolveCollectionSchema();
        }

        private void ApplyDataSourceAciDocumentStaticFieldsToGraphCollectionSchema()
        {
            AddFieldToSchema(nameof(GraphDocument.dsid), DataType.Long, Collections.GraphCollection, false);
            AddFieldToSchema(nameof(GraphDocument.Title), DataType.String, Collections.GraphCollection, false);
            AddFieldToSchema(nameof(GraphDocument.Description), DataType.String, Collections.GraphCollection, false);
            AddFieldToSchema(nameof(GraphDocument.TimeCreated), DataType.String, Collections.GraphCollection, false);
            //AddFieldToSchema("GraphImage_txt", DataType.String, Collections.GraphCollection, false);
            //AddFieldToSchema("GraphArrangement_txt", DataType.String, Collections.GraphCollection, false);
            AddFieldToSchema(nameof(GraphDocument.NodesCount), DataType.Long, Collections.GraphCollection, false);
        }

        private void ApplyDataSourceAciDocumentStaticFieldsToDataSourceAciCollectionSchema()
        {
            AddFieldToSchema(nameof(DataSourceAciDocument.dsid), DataType.Long, Collections.DataSourceAciCollection, false);
            AddFieldToSchema(nameof(DataSourceAciDocument.GroupName), DataType.String, Collections.DataSourceAciCollection, false);
            AddFieldToSchema(nameof(DataSourceAciDocument.Permission), DataType.Int, Collections.DataSourceAciCollection, false);
        }

        private void ApplyImageDocumentStaticFieldsToResolveCollectionSchema()
        {
            AddFieldToSchema("ResolveTo", DataType.Long, Collections.ResolveCollection, true, false, true);
            AddFieldToSchema("MasterId", DataType.Long, Collections.ResolveCollection, false, false, true);
        }

        private void ApplyImageDocumentStaticFieldsToImageCollectionSchema()
        {
            AddFieldToSchema(nameof(ImageDocument.Description), DataType.String, Collections.ImageCollection, false);
            AddFieldToSchema(nameof(ImageDocument.ImageId), DataType.String, Collections.ImageCollection, false);
            AddFieldToSchema(nameof(FaceSpecification.BoundingBox), DataType.PInts, Collections.ImageCollection, true, false);
            AddFieldToSchema(nameof(Entities.SearchEngine.Documents.ACL.ClassificationIdentifier), DataType.String, Collections.ImageCollection, false);

            //for (int i = 1; i <= ImageDocument.NumberOfFeatue; i++)
            //{
            //    AddFieldToSchema(ImageDocument.GetFieldName(i), DataType.Double, Collections.ImageCollection, false);
            //}
        }

        private void ApplyPropertyDocumentStaticFieldsToDataSourceCollectionSchema()
        {
            AddFieldToSchema(nameof(DataSourceDocument.Name), DataType.String, Collections.DataSourceCollection, false);
            AddFieldToSchema(nameof(DataSourceDocument.Description), DataType.String, Collections.DataSourceCollection, false);
            AddFieldToSchema(nameof(DataSourceDocument.Type), DataType.Long, Collections.DataSourceCollection, false);
            AddFieldToSchema(nameof(DataSourceDocument.CreatedBy), DataType.String, Collections.DataSourceCollection, false);
            AddFieldToSchema(nameof(DataSourceDocument.CreatedTime), DataType.String, Collections.DataSourceCollection, false);
            AddFieldToSchema(nameof(Entities.SearchEngine.Documents.ACL.ClassificationIdentifier), DataType.String, Collections.DataSourceCollection, false);
        }

        private void ApplyFileDocumentStaticFieldsToFileCollectionSchema()
        {
            //AddFieldToSchema(nameof(File.Id), DataType.String, SchemaFieldTarget.FileCollection, false);
            AddFieldToSchema(nameof(File.OwnerObjectIds), DataType.String, Collections.FileCollection, true);
            AddFieldToSchema(nameof(File.CreatedTime), DataType.DateTime, Collections.FileCollection, false);
            AddFieldToSchema(nameof(File.ImportDate), DataType.DateTime, Collections.FileCollection, false);
            AddFieldToSchema(nameof(File.FileSize), DataType.Long, Collections.FileCollection, false);
            AddFieldToSchema(nameof(File.RelatedWord), DataType.String, Collections.FileCollection, false);
            AddFieldToSchema(nameof(Entities.SearchEngine.Documents.ACL.ClassificationIdentifier), DataType.String, Collections.FileCollection, false);
            AddFieldToSchema(nameof(File.FileType), DataType.String, Collections.FileCollection, false);
        }

        private void ApplyRelationshipDocumentStaticFieldsToObjectCollectionSchema()
        {
            //AddFieldToSchema(nameof(Relationship.Id), DataType.String, SchemaFieldTarget.ObjectCollection, false, true);
            AddFieldToSchema(nameof(Relationship.LinkTypeUri), DataType.String, Collections.ObjectCollection, false, true);
            AddFieldToSchema(nameof(Relationship.SourceObjectId), DataType.String, Collections.ObjectCollection, false, true);
            AddFieldToSchema(nameof(Relationship.SourceObjectTypeUri), DataType.String, Collections.ObjectCollection, false, true);
            AddFieldToSchema(nameof(Relationship.TargetObjectId), DataType.String, Collections.ObjectCollection, false, true);
            AddFieldToSchema(nameof(Relationship.TargetObjectTypeUri), DataType.String, Collections.ObjectCollection, false, true);
            AddFieldToSchema(nameof(Relationship.DataSourceId), DataType.Int, Collections.ObjectCollection, false, true);
            AddFieldToSchema(nameof(Relationship.Direction), DataType.Int, Collections.ObjectCollection, false, true);
        }

        private void ApplyPropertyDocumentStaticFieldsToObjectCollectionSchema()
        {
            //AddFieldToSchema(nameof(Property.Id), DataType.String, SchemaFieldTarget.ObjectCollection, false, true);
            AddFieldToSchema(nameof(Property.TypeUri), DataType.String, Collections.ObjectCollection, false, true);
            AddFieldToSchema(nameof(Property.BaseType), DataType.Int, Collections.ObjectCollection, false);
            AddFieldToSchema(nameof(Property.OwnerObjectID), DataType.String, Collections.ObjectCollection, false, true);
            AddFieldToSchema(nameof(Property.OwnerObjectTypeUri), DataType.String, Collections.ObjectCollection, false, true);
            AddFieldToSchema(nameof(Property.BooleanValue), DataType.Bool, Collections.ObjectCollection, false, true);
            AddFieldToSchema(nameof(Property.DateTimeValue), DataType.DateTime, Collections.ObjectCollection, false, true);
            AddFieldToSchema(nameof(Property.DoubleValue), DataType.Double, Collections.ObjectCollection, false, true);
            AddFieldToSchema(nameof(Property.LongValue), DataType.Long, Collections.ObjectCollection, false, true);
            AddFieldToSchema(nameof(Property.StringValue), DataType.String, Collections.ObjectCollection, false, true);
            AddFieldToSchema(nameof(Property.KeywordTokenizedStringValue), DataType.KeywordTokenizedString, Collections.ObjectCollection, false);
            AddFieldToSchema(nameof(Property.GeoValue), DataType.GeoLocation, Collections.ObjectCollection, false);
            AddFieldToSchema(nameof(Property.DateRangeValue), DataType.DateRange, Collections.ObjectCollection, false);
            AddFieldToSchema(nameof(Property.DataSourceId), DataType.Long, Collections.ObjectCollection, false);
        }

        private void ApplyObjectDocumentStaticFieldsToObjectCollectionSchema()
        {
            //AddFieldToSchema(nameof(DocumentObject.Id), DataType.String, SchemaFieldTarget.ObjectCollection, false, true);
            AddFieldToSchema("ParentDocument", DataType.Bool, Collections.ObjectCollection, false, true);

            AddFieldToSchema("TypeUri", DataType.String, Collections.ObjectCollection, false, true);
            AddFieldToSchema("IsGroup", DataType.Bool, Collections.ObjectCollection, false, true);
            AddFieldToSchema("LabelPropertyID", DataType.String, Collections.ObjectCollection, false, true);
            //AddFieldToSchema("ResolvedTo", DataType.Long, Collections.ObjectCollection, false, true);

            // Collection common field(s)
            AddFieldToSchema(nameof(Entities.SearchEngine.Documents.ACL.ClassificationIdentifier), DataType.String, Collections.ObjectCollection, false);
        }

        private void ApplyGroupsLastChangesToSchema(string[] groupNames)
        {
            foreach (var groupName in groupNames)
            {
                AddFieldToSchema(groupName, DataType.Long, Collections.ObjectCollection, false);
                AddFieldToSchema(groupName, DataType.Long, Collections.DataSourceCollection, false);
                AddFieldToSchema(groupName, DataType.Long, Collections.FileCollection, false);
                AddFieldToSchema(groupName, DataType.Long, Collections.ImageCollection, false);
                AddFieldToSchema(groupName, DataType.Long, Collections.DataSourceAciCollection, false);
            }
        }

        private void BuildCollections()
        {
            int numberOfShards = 10;
            int numberOfLiveNodes = GetDistributeStatus();
            BuildCollection(ObjectCollection.Name, ObjectCollection.ConfigSetName, numberOfShards, numberOfLiveNodes);
            BuildCollection(ResolveCollection.Name, ResolveCollection.ConfigSetName, numberOfShards, numberOfLiveNodes);
            BuildCollection(DataSourceCollection.Name, DataSourceCollection.ConfigSetName, numberOfShards, numberOfLiveNodes);
            BuildCollection(FileCollection.Name, FileCollection.ConfigSetName, numberOfShards, numberOfLiveNodes);
            BuildCollection(ImageCollection.Name, ImageCollection.ConfigSetName, numberOfShards, numberOfLiveNodes);
            BuildCollection(DataSourceAciDocument.Name, DataSourceAciDocument.ConfigSetName, numberOfShards, numberOfLiveNodes);
            BuildCollection(GraphDocument.Name, GraphDocument.ConfigSetName, numberOfShards, numberOfLiveNodes);

            //BuildCollection("TestObjectCollection", ObjectCollection.ConfigSetName, numberOfShards, numberOfLiveNodes);
            //BuildCollection("TestDatasourceCollection", DataSourceCollection.ConfigSetName, numberOfShards, numberOfLiveNodes);
        }

        private int GetDistributeStatus()
        {

            string solrCollectionURI = $"{SolrURL}/admin/collections?action=clusterstatus&wt=json";
            var client = new RestClient(solrCollectionURI);
            client.Authenticator = new HttpBasicAuthenticator("solr", "SolrRocks");
            var request = new RestRequest(Method.GET);
            var result = client.Execute(request);

            if (!result.IsSuccessful)
            {
                throw result.ErrorException;
            }

            var jsonResult = JObject.Parse(result.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            var nodes = jsonResult["cluster"];
            nodes = nodes["live_nodes"];
            int numberOfLiveNodes = 0;
            foreach (var node in nodes)
            {
                numberOfLiveNodes++;
            }
            return numberOfLiveNodes;
        }

        private void BuildCollection(string collectionName, string configSetName, int numberOfShards, int numberOfLiveNodes)
        {
            if (!IsCollectionExists(collectionName))
            {
                string solrUrl = SolrURL;
                string uri = $"{SolrURL}/admin/collections?action=CREATE&name={collectionName}&numShards={numberOfShards}&replicationFactor={numberOfLiveNodes}&maxShardsPerNode={numberOfShards * numberOfLiveNodes}&collection.configName={configSetName}&wt=json";
                var client = new RestClient(uri);
                client.Authenticator = new HttpBasicAuthenticator("solr", "SolrRocks");
                var request = new RestRequest(Method.GET);

                var result = client.Execute(request);

                if (!result.IsSuccessful)
                {
                    throw result.ErrorException;
                }
            }
        }

        private bool IsCollectionExists(string collectionName)
        {

            string solrCollectionURI = $"{SolrURL}/admin/collections?action=LIST&wt=json";
            var client = new RestClient(solrCollectionURI);
            client.Authenticator = new HttpBasicAuthenticator("solr", "SolrRocks");
            var request = new RestRequest(Method.GET);
            var result = client.Execute(request);
            if (!result.IsSuccessful)
            {
                throw result.ErrorException;
            }

            var jsonResult = JObject.Parse(result.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            var configSets = jsonResult["collections"];
            foreach (var configSet in configSets)
            {
                if (configSet.ToString().Equals(collectionName))
                    return true;
            }
            return false;
        }

        private void UploadConfigSets()
        {
            var files = System.IO.Directory.GetFiles(SolrConfigSetPath);
            foreach (var filePath in files)
            {
                var fileInfo = new System.IO.FileInfo(filePath);
                if (!fileInfo.Name.EndsWith(".zip"))
                    continue;
                string fileName = fileInfo.Name.Substring(0, fileInfo.Name.Length - 4);
                if (!IsConfigSetExists(fileName))
                {
                    UploadConfigSetToSolr(fileName, filePath);
                }
            }
        }
        private bool IsConfigSetExists(string fileName)
        {
            string solrConfigsetURI = $"{SolrURL}/admin/configs?action=LIST&wt=json";
            var client = new RestClient(solrConfigsetURI);
            client.Authenticator = new HttpBasicAuthenticator("solr", "SolrRocks");
            var request = new RestRequest(Method.GET);
            var result = client.Execute(request);

            if (!result.IsSuccessful)
            {
                throw result.ErrorException;
            }

            var jsonResult = JObject.Parse(result.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            var configSets = jsonResult["configSets"];
            foreach (var configSet in configSets)
            {
                if (configSet.ToString().Equals(fileName))
                    return true;
            }
            return false;
        }
        private bool IsFieldExistsInSchema(string fieldName, Collections targetSchema)
        {
            StringBuilder collectionUriBuilder = new StringBuilder();
            switch (targetSchema)
            {
                case Collections.ObjectCollection:
                    collectionUriBuilder.Append(ObjectCollection.SolrUrl);
                    break;
                case Collections.FileCollection:
                    collectionUriBuilder.Append(FileCollection.SolrUrl);
                    break;
                case Collections.DataSourceCollection:
                    collectionUriBuilder.Append(DataSourceCollection.SolrUrl);
                    break;
                case Collections.ImageCollection:
                    collectionUriBuilder.Append(ImageCollection.SolrUrl);
                    break;
                case Collections.DataSourceAciCollection:
                    collectionUriBuilder.Append(DataSourceAciCollection.SolrUrl);
                    break;
                case Collections.GraphCollection:
                    collectionUriBuilder.Append(GraphCollection.SolrUrl);
                    break;
                case Collections.ResolveCollection:
                    collectionUriBuilder.Append(ResolveCollection.SolrUrl);
                    break;
                default:
                    throw new NotSupportedException();
            }
            collectionUriBuilder.Append("/schema/fields");

            var client = new RestClient(collectionUriBuilder.ToString());
            client.Authenticator = new HttpBasicAuthenticator("solr", "SolrRocks");
            var request = new RestRequest(Method.GET);
            var result = client.Execute(request);

            if (!result.IsSuccessful)
            {
                throw result.ErrorException;
            }

            if (result.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            var jsonResult = JObject.Parse(result.Content);
            var status = (long)JObject.Parse(jsonResult["responseHeader"].ToString())["status"];
            var fields = jsonResult["fields"];
            foreach (var field in fields)
            {
                if (field["name"].ToString().Equals(fieldName))
                    return true;
            }
            return false;
        }
        private void UploadConfigSetToSolr(string configSetName, string configSetPath)
        {
            string solrConfigsetURI = $"{SolrURL}/admin/configs?action=UPLOAD&name={configSetName}";
            try
            {
                using (var client = new System.Net.WebClient())
                {
                    var ISO_8859_1 = Encoding.GetEncoding("ISO-8859-1");
                    var svcCredentials = Convert.ToBase64String(ISO_8859_1.GetBytes("solr" + ":" + "SolrRocks"));

                    client.Headers.Add("Authorization", "Basic " + svcCredentials);

                    byte[] result = client.UploadData(solrConfigsetURI, "POST", System.IO.File.ReadAllBytes(configSetPath));
                }
            }
            catch (System.Net.WebException ex)
            {
                ExceptionDetailGenerator exDetailGenerator = new ExceptionDetailGenerator();
                throw exDetailGenerator.AppendWebExceptionResonse(ex);
            }
        }

        public void Optimize()
        {
            Optimize(ObjectCollection.SolrUrl);
            Optimize(FileCollection.SolrUrl);
        }

        private void Optimize(string collectionUrl)
        {
            var client = new RestClient(collectionUrl);
            client.Authenticator = new HttpBasicAuthenticator("solr", "SolrRocks");
            var request = new RestRequest("/update", Method.POST);
            request.AddParameter("text/xml", "<optimize><query>*:*</query></optimize>", ParameterType.RequestBody);
            var response = client.Execute(request);
            if (!response.IsSuccessful)
            {
                throw response.ErrorException;
            }
        }

        public void AddFieldToSchema(string fieldName, Ontology.DataType fieldType, bool isMultiValue = false)
        {
            throw new NotImplementedException();
        }

        public List<long> GetEventDocumentIDsForMatchedKeyword(string keyword, AuthorizationParametters authorizationParametters, int quickSearchResultsTreshould, Ontology.Ontology ontology)
        {
            return GetEventDocumentIDsForMatchedKeyword(keyword, authorizationParametters, quickSearchResultsTreshould, ontology);
        }

        public bool DeleteGraph(int id)
        {
            try
            {
                JObject deleteAllDocumentJson = new JObject();
                deleteAllDocumentJson.Add(new JProperty("delete", new JObject(new JProperty("id", id.ToString()))));
                ExecuteJsonQueryOnSpecificCollectionAndCommit(GraphCollection.SolrUrl, deleteAllDocumentJson);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
