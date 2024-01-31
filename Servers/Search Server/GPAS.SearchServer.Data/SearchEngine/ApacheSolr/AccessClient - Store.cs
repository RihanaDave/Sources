using GPAS.AccessControl;
using GPAS.Dispatch.Entities.Concepts.Geo;
using GPAS.Logger;
using GPAS.Ontology;
using GPAS.PropertiesValidation;
using GPAS.SearchServer.Entities;
using GPAS.SearchServer.Entities.SearchEngine.Documents;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace GPAS.SearchServer.Access.SearchEngine.ApacheSolr
{
    public partial class AccessClient
    {
        JArray UploadList = new JArray();

        const char PropertyDocIdSeparator = '!';
        const string RelationshipDocIdSeparator = "!R";

        public void AddFileDocument(File fileDoc)
        {
            TryUploadDocumentContent(fileDoc);
            UploadDocumentFeatures(fileDoc);
        }

        public void AddImageDocument(ImageDocument imageDoc)
        {
            UploadImageDocument(imageDoc);
        }

        private void UploadImageDocument(ImageDocument imageDoc)
        {
            JArray jsonArray = CreateJsonForImageDoc(imageDoc);
            RestClient client = new RestClient(ImageCollection.SolrUrl);
            RestRequest request = new RestRequest($"/update?commit=true", Method.POST);
            request.AddParameter("application/json", jsonArray.ToString(), ParameterType.RequestBody);
            try
            {
                ExecuteRestRequest(client, request);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to upload content of image document with ID '{imageDoc.ImageId}'", ex);
            }
        }

        private void UploadDocumentFeatures(File fileDoc)
        {
            JArray jsonArray = CreateJsonForFileDoc(fileDoc);
            RestClient client = new RestClient(FileCollection.SolrUrl);
            RestRequest request = new RestRequest($"/update?commit=true", Method.POST);
            request.AddParameter("application/json", jsonArray.ToString(), ParameterType.RequestBody);
            try
            {
                ExecuteRestRequest(client, request);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to upload features of document with ID '{fileDoc.Id}'", ex);
            }
        }

        private void TryUploadDocumentContent(File fileDoc)
        {
            if (fileDoc == null)
                throw new NullReferenceException(nameof(fileDoc));
            if (fileDoc.Content == null)
                fileDoc.Content = new byte[] { };

            RestClient client = new RestClient(FileCollection.SolrUrl);
            RestRequest request = new RestRequest($"/update/extract?commit=true&literal.id={fileDoc.Id}", Method.POST);
            request.AddFileBytes(fileDoc.Id, fileDoc.Content, fileDoc.Id);
            try
            {
                ExecuteRestRequest(client, request);
            }
            catch /*(Exception ex)*/
            {
                // فایل توسط زیرساخت قابل پذیرش نیست
                // TODO: ثبت لاگ مدیریتی
                //throw new Exception($"Unable to upload content of document with ID '{fileDoc.Id}'", ex);
            }
        }

        private bool ExecuteRestRequest(RestClient client, RestRequest request)
        {
            IRestResponse response = client.Execute(request);

            if (!response.IsSuccessful)
            {
                throw response.ErrorException;
            }

            switch (response.ResponseStatus)
            {
                case ResponseStatus.Completed:
                    JObject responseContent = JObject.Parse(response.Content);
                    var status = (long)JObject.Parse(responseContent["responseHeader"].ToString())["status"];
                    return true;
                case ResponseStatus.Error:
                case ResponseStatus.TimedOut:
                    throw response.ErrorException;
                case ResponseStatus.Aborted:
                    throw new Exception("Request aborted!");
                case ResponseStatus.None:
                default:
                    throw new Exception("Unknown response.");
            }
        }

        private JArray CreateJsonForImageDoc(ImageDocument imageDoc)
        {
            JArray result = new JArray();
            foreach (var face in imageDoc.Faces)
            {
                JObject json = new JObject();
                json.Add(new JProperty("id", face.FaceId));
                json.Add(new JProperty(nameof(imageDoc.ImageId), imageDoc.ImageId));
                if (!string.IsNullOrEmpty(imageDoc.Description))
                {
                    json.Add(new JProperty(nameof(imageDoc.Description), imageDoc.Description));
                }

                //ACL
                json.Add(new JProperty(nameof(imageDoc.ACL.ClassificationIdentifier), imageDoc.ACL.ClassificationIdentifier));
                foreach (var aci in imageDoc.ACL.Permissions)
                {
                    json.Add(new JProperty(aci.GroupName, (byte)aci.AccessLevel));
                }

                //boundig box
                int[] boundingBox = new int[4];
                boundingBox[0] = face.BoundingBox.topLeft.X;
                boundingBox[1] = face.BoundingBox.topLeft.Y;
                boundingBox[2] = face.BoundingBox.width;
                boundingBox[3] = face.BoundingBox.height;
                json.Add(new JProperty(new JProperty(nameof(face.BoundingBox), JArray.FromObject(boundingBox))));

                //vectorFeatue
                for (int i = 1; i <= face.VectorFeatue.Count; i++)
                {
                    json.Add(new JProperty(ImageDocument.GetFieldName(i), face.VectorFeatue[i - 1]));
                }
                result.Add(json);
            }
            return result;
        }

        private JArray CreateJsonForFileDoc(File fileDoc)
        {
            JObject json = new JObject();
            json.Add(new JProperty("id", fileDoc.Id));
            json.Add(new JProperty(nameof(fileDoc.CreatedTime), ConvertDatePropertyToSolrLongDate(fileDoc.CreatedTime.Value)));
            json.Add(new JProperty(nameof(fileDoc.FileSize), fileDoc.FileSize));
            json.Add(new JProperty(nameof(fileDoc.FileType), fileDoc.FileType));
            json.Add(new JProperty(nameof(fileDoc.ImportDate), ConvertDatePropertyToSolrLongDate(fileDoc.ImportDate.Value)));
            json.Add(new JProperty(nameof(fileDoc.RelatedWord), fileDoc.RelatedWord));
            if (!string.IsNullOrEmpty(fileDoc.Description))
            {
                json.Add(new JProperty(nameof(fileDoc.Description), new JObject(new JProperty("add", fileDoc.Description))));
            }

            JArray ownerObjectIdJArray = new JArray();
            foreach (var ownerObjectId in fileDoc.OwnerObjectIds)
            {
                ownerObjectIdJArray.Add(fileDoc.OwnerObjectIds);
            }
            json.Add(new JProperty(nameof(fileDoc.OwnerObjectIds), new JObject(new JProperty("add", ownerObjectIdJArray))));
            //ACL Information
            json.Add(new JProperty(nameof(fileDoc.Acl.ClassificationIdentifier), /*new JObject(new JProperty("add", */fileDoc.Acl.ClassificationIdentifier/*))*/));
            foreach (var aci in fileDoc.Acl.Permissions)
            {
                json.Add(new JProperty(aci.GroupName, /*new JObject(new JProperty("add", */(byte)aci.AccessLevel/*))*/));
            }
            return new JArray(json);
        }

        public void AddObjectDocument(ObjectDocument objDoc)
        {
            JObject jsonParsedDocument = new JObject();
            jsonParsedDocument.Add(new JProperty("id", objDoc.Id));
            jsonParsedDocument.Add(new JProperty("TypeUri", objDoc.TypeUri));
            jsonParsedDocument.Add(new JProperty("IsGroup", objDoc.IsGroup));
            jsonParsedDocument.Add(new JProperty("LabelPropertyID", objDoc.LabelPropertyID));
            jsonParsedDocument.Add(new JProperty("ResolvedTo", objDoc.ResolvedTo));
            jsonParsedDocument.Add(new JProperty("ParentDocument", "true"));
            JArray properties = ParsePropertiesOfObjectToJson(objDoc.Properties);
            jsonParsedDocument.Add(new JProperty("_childDocuments_", properties));
            AddToUploadList(jsonParsedDocument);
            AddRelationshipDocument(objDoc.Relationships);
        }

        private void AddToUploadList(JObject jsonObject)
        {
            UploadList.Add(jsonObject);
            if (AutoCommitEnabled && UploadList.Count >= AutoCommitMaximumPendingDocumentsCount)
            {
                ApplyChanges(ObjectCollection.SolrUrl);
                ApplyChanges(FileCollection.SolrUrl);
                ApplyChanges(DataSourceCollection.SolrUrl);
                ApplyChanges(ImageCollection.SolrUrl);
                ApplyChanges(DataSourceAciCollection.SolrUrl);
            }
        }

        private JArray ParsePropertiesOfObjectToJson(List<Property> properties)
        {
            JArray jsonParsedProperties = new JArray();
            foreach (var property in properties)
            {
                jsonParsedProperties.Add(CreatePropetyJsonObject(property));
            }
            return jsonParsedProperties;
        }

        private JObject CreatePropetyJsonObject(Property property)
        {
            JObject jsonProperty = new JObject();
            jsonProperty.Add(new JProperty("id", GetPropertyValueForID(property.OwnerObjectID, property.Id)));
            jsonProperty.Add(new JProperty(nameof(property.TypeUri), property.TypeUri));
            jsonProperty.Add(new JProperty(nameof(property.BaseType), property.BaseType));
            jsonProperty.Add(new JProperty(nameof(property.OwnerObjectID), property.OwnerObjectID));
            jsonProperty.Add(new JProperty(nameof(property.OwnerObjectTypeUri), property.OwnerObjectTypeUri));
            jsonProperty.Add(new JProperty(nameof(property.DataSourceId), property.DataSourceId));
            if (property.StringValue != null)
            {
                jsonProperty.Add(new JProperty(nameof(property.StringValue), property.StringValue));
                // توکن کننده‌ی مبتنی بر کلیدواژه رشته‌های طولانی را پشتیبانی نمی‌کند؛
                // اندیس‌گذاری مبتنی بر کلیدواژه برای طول کمتر از طول اصلی هم بی معنا است؛
                // به همین خاطر از ورود این فقره داده صرف‌نظر شد
                // TODO: ثبت لاگ مدیریتی افزوده شود
                if (System.Text.Encoding.UTF8.GetByteCount(property.KeywordTokenizedStringValue) <= 32766)
                {
                    jsonProperty.Add(new JProperty(nameof(property.KeywordTokenizedStringValue), property.KeywordTokenizedStringValue));
                }
            }
            if (property.LongValue != null)
                jsonProperty.Add(new JProperty(nameof(property.LongValue), property.LongValue));
            if (property.DoubleValue != null)
                jsonProperty.Add(new JProperty(nameof(property.DoubleValue), property.DoubleValue));
            if (property.DateTimeValue != null)
            {
                jsonProperty.Add(new JProperty(nameof(property.LongValue), ConvertDatePropertyToSolrLongDate(property.DateTimeValue)));
                jsonProperty.Add(new JProperty(nameof(property.DateTimeValue), ConvertDatePropertyToSolrStringDate(property.DateTimeValue)));
            }
            if (property.BooleanValue != null)
                jsonProperty.Add(new JProperty(nameof(property.BooleanValue), property.BooleanValue));
            if (property.GeoValue != null)
            {
                jsonProperty.Add(new JProperty(nameof(property.GeoValue), property.GeoValue));
                jsonProperty.Add(new JProperty(nameof(property.DateRangeValue), property.DateRangeValue));
            }

            //Add ACL Information
            jsonProperty.Add(new JProperty(nameof(property.Acl.ClassificationIdentifier), property.Acl.ClassificationIdentifier));
            foreach (var aci in property.Acl.Permissions)
            {
                jsonProperty.Add(new JProperty(aci.GroupName, (byte)aci.AccessLevel));
            }
            return jsonProperty;
        }

        public void AddPropertyDocument(SearchObject objDoc, List<Property> newProperties)
        {
            JToken retrivedProperties = RetriveObjectsFromSolrByQuery($"id:{objDoc.Id}*", 1000);
            ObjectDocument documentObject = new ObjectDocument
            {
                Id = objDoc.Id.ToString(CultureInfo.InstalledUICulture),
                LabelPropertyID = objDoc.LabelPropertyID,
                TypeUri = objDoc.TypeUri,
                Properties = new List<Property>(newProperties)
            };
            AddObjectDocumentWithJsonProperties(documentObject, retrivedProperties);
        }

        public void AddRelationshipDocument(List<Relationship> newRelationships)
        {
            foreach (Relationship rel in newRelationships)
            {
                JObject relJObj = CreateRelationshipJsonObject(rel);
                AddToUploadList(relJObj);
            }
        }

        private JObject CreateRelationshipJsonObject(Relationship rel)
        {
            JObject jsonRel = new JObject();
            jsonRel.Add(new JProperty("id", $"{rel.SourceObjectId}{RelationshipDocIdSeparator}{rel.Id}"));
            jsonRel.Add(new JProperty(nameof(rel.LinkTypeUri), rel.LinkTypeUri));
            jsonRel.Add(new JProperty(nameof(rel.SourceObjectId), rel.SourceObjectId));
            jsonRel.Add(new JProperty(nameof(rel.SourceObjectTypeUri), rel.SourceObjectTypeUri));
            jsonRel.Add(new JProperty(nameof(rel.TargetObjectId), rel.TargetObjectId));
            jsonRel.Add(new JProperty(nameof(rel.TargetObjectTypeUri), rel.TargetObjectTypeUri));
            jsonRel.Add(new JProperty(nameof(rel.DataSourceId), rel.DataSourceId));
            jsonRel.Add(new JProperty(nameof(rel.Direction), rel.Direction));
            //Add ACL Information
            jsonRel.Add(new JProperty(nameof(rel.Acl.ClassificationIdentifier), rel.Acl.ClassificationIdentifier));
            foreach (var aci in rel.Acl.Permissions)
            {
                jsonRel.Add(new JProperty(aci.GroupName, (byte)aci.AccessLevel));
            }
            return jsonRel;
        }

        private void AddObjectDocumentWithJsonProperties(ObjectDocument objectDoc, JToken retrivedProperties)
        {
            JObject jsonParsedDocument = new JObject();
            jsonParsedDocument.Add(new JProperty("id", objectDoc.Id));
            jsonParsedDocument.Add(new JProperty("TypeUri", objectDoc.TypeUri));
            jsonParsedDocument.Add(new JProperty("IsGroup", objectDoc.IsGroup));
            jsonParsedDocument.Add(new JProperty("LabelPropertyID", objectDoc.LabelPropertyID));
            jsonParsedDocument.Add(new JProperty("ParentDocument", "true"));
            JArray properties = ParsePropertiesOfObjectToJson(objectDoc.Properties);
            properties = AppendPropertiesToJArray(properties, retrivedProperties);
            jsonParsedDocument.Add(new JProperty("_childDocuments_", properties));
            AddToUploadList(jsonParsedDocument);
        }

        private JArray AppendPropertiesToJArray(JToken retrivedProperties, string label)
        {
            JArray properties = new JArray();
            foreach (JToken property in retrivedProperties.Children())
            {
                var labelProperty = (string)property["TypeUri"];
                if (!(labelProperty == label))
                {
                    properties.Add(property);
                }
            }
            return properties;
        }

        private JArray AppendPropertiesToJArray(JArray properties, JToken retrivedProperties)
        {
            foreach (var property in retrivedProperties.Children())
            {
                properties.Add(property);
            }
            return properties;
        }


        public void AddValueToFileDocumentMultiValue(string fileDocID, string multiValueFieldName, List<string> valuesToAdd)
        {
            JArray updateJson = CreateJsonMultiValueFileDocumentUpdate(fileDocID, multiValueFieldName, valuesToAdd);
            UpdateFileDocument(updateJson);
        }

        private void UpdateFileDocument(JArray updateJson)
        {
            var client = new RestClient(FileCollection.SolrUrl);
            RestRequest request;
            request = new RestRequest($"/update?commit=true", Method.POST);
            request.AddParameter("application/json; charset=utf-8", updateJson.ToString(), ParameterType.RequestBody);
            ExecuteRestRequest(client, request);
        }

        private JArray CreateJsonMultiValueFileDocumentUpdate(string fileDocID, string multiValueFieldName, List<string> valuesToAdd)
        {
            JArray updateJson = new JArray();
            updateJson.Add(new JObject(new JProperty("id", fileDocID)));
            JArray valuesJArray = new JArray();
            foreach (var value in valuesToAdd)
            {
                valuesJArray.Add(value);
            }
            updateJson.Add(new JObject(new JProperty(multiValueFieldName, new JObject(new JProperty("add", valuesJArray)))));
            return updateJson;
        }

        public void ApplyChanges(string collectionUrl, bool alsoCommitChanges = true)
        {
            if (UploadList.Count == 0)
                return;
            var client = new RestClient(collectionUrl);
            RestRequest request;
            if (alsoCommitChanges)
                request = new RestRequest($"/update?commit=true", Method.POST);
            else
                request = new RestRequest($"/update?commit=false", Method.POST);
            request.AddParameter("application/json; charset=utf-8", UploadList.ToString(), ParameterType.RequestBody);
            if (ExecuteRestRequest(client, request))
            {
                UploadList = new JArray();
            }
        }

        public void ChangeValueFromFileDocumentMultiField(string multiValueFieldName, string currentValue, string newValue)
        {
            List<string> ids = RetriveDocumentsHasMultiValueFieldInFileCollection(multiValueFieldName, currentValue);
            JArray changeValueJson = new JArray();
            foreach (var id in ids)
            {
                changeValueJson.Add(CreateJsonForRemoveMultiValueField(id, multiValueFieldName, currentValue));
                changeValueJson.Add(CreateJsonForAddMultiValueField(id, multiValueFieldName, newValue));
            }
            UpdateFileDocument(changeValueJson);
        }

        private JToken CreateJsonForAddMultiValueField(string id, string multiValueFieldName, string newValue)
        {
            JObject jsonForAddMultiValueField = new JObject();
            jsonForAddMultiValueField.Add(new JProperty("id", id));
            jsonForAddMultiValueField.Add(new JProperty(multiValueFieldName, new JObject(new JProperty("add", newValue))));
            return jsonForAddMultiValueField;
        }

        private JToken CreateJsonForRemoveMultiValueField(string id, string multiValueFieldName, string currentValue)
        {
            JObject jsonForRemoveMultiValueField = new JObject();
            jsonForRemoveMultiValueField.Add(new JProperty("id", id));
            jsonForRemoveMultiValueField.Add(new JProperty(multiValueFieldName, new JObject(new JProperty("remove", currentValue))));
            return jsonForRemoveMultiValueField;
        }

        public void Commit()
        {
            JArray emptyJArray = new JArray();
            ExecuteJsonQueryOnSpecificCollectionAndCommit(FileCollection.SolrUrl, emptyJArray);
            ExecuteJsonQueryOnSpecificCollectionAndCommit(ObjectCollection.SolrUrl, emptyJArray);
            ExecuteJsonQueryOnSpecificCollectionAndCommit(DataSourceCollection.SolrUrl, emptyJArray);
            ExecuteJsonQueryOnSpecificCollectionAndCommit(ImageCollection.SolrUrl, emptyJArray);
            ExecuteJsonQueryOnSpecificCollectionAndCommit(DataSourceAciCollection.SolrUrl, emptyJArray);
        }

        private void ExecuteJsonQueryOnSpecificCollectionAndCommit(string collectionUrl, JToken jsonQuery)
        {
            var client = new RestClient(collectionUrl);
            RestRequest request;
            request = new RestRequest($"/update?commit=true", Method.POST);
            request.AddParameter("application/json; charset=utf-8", jsonQuery.ToString(), ParameterType.RequestBody);
            ExecuteRestRequest(client, request);
        }

        public void DeleteAllDocuments()
        {
            JObject deleteAllDocumentJson = new JObject();
            deleteAllDocumentJson.Add(new JProperty("delete", new JObject(new JProperty("query", "*:*"))));
            ExecuteJsonQueryOnSpecificCollectionAndCommit(ObjectCollection.SolrUrl, deleteAllDocumentJson);
            ExecuteJsonQueryOnSpecificCollectionAndCommit(ResolveCollection.SolrUrl, deleteAllDocumentJson);
            ExecuteJsonQueryOnSpecificCollectionAndCommit(FileCollection.SolrUrl, deleteAllDocumentJson);
            ExecuteJsonQueryOnSpecificCollectionAndCommit(DataSourceCollection.SolrUrl, deleteAllDocumentJson);
            ExecuteJsonQueryOnSpecificCollectionAndCommit(ImageCollection.SolrUrl, deleteAllDocumentJson);
            ExecuteJsonQueryOnSpecificCollectionAndCommit(DataSourceAciCollection.SolrUrl, deleteAllDocumentJson);
            ExecuteJsonQueryOnSpecificCollectionAndCommit(GraphCollection.SolrUrl, deleteAllDocumentJson);
        }

        public void DeleteFileDocuments(List<string> fileDocIDs)
        {
            JObject deleteJsonQuery = CreateDeleteDocumentJsonQuery(fileDocIDs);
            ExecuteJsonQueryOnSpecificCollectionAndCommit(FileCollection.SolrUrl, deleteJsonQuery);
        }

        private JObject CreateDeleteDocumentJsonQuery(List<string> docIDs)
        {
            JObject deleteJsonQuery = new JObject();
            string ids = "(";
            foreach (var id in docIDs)
            {
                ids += $" {id}* ";
            }
            ids += ")";
            deleteJsonQuery.Add(new JProperty("delete", new JObject(new JProperty("query", $"id : {ids}"))));
            return deleteJsonQuery;
        }

        public void DeleteObjectDocument(List<string> objDocIDList)
        {
            if (objDocIDList.Count == 0)
                return;
            JObject deleteJsonQuery = CreateDeleteDocumentJsonQuery(objDocIDList);
            ExecuteJsonQueryOnSpecificCollectionAndCommit(ObjectCollection.SolrUrl, deleteJsonQuery);
        }

        public void MovePropertyDocuments(List<string> currentOwnerObjDocIDs, string newOwnerObjID)
        {
            JArray entireProperties = new JArray();
            foreach (var resolvedObjDocID in currentOwnerObjDocIDs)
            {
                JToken sourceProperties = RetrieveEntireDocumentObjectFromSolr(resolvedObjDocID);
                JToken propertiesOfObject = AsignNewIdAndOwnerId(sourceProperties, newOwnerObjID);
                entireProperties = AppendPropertiesToJArray(entireProperties, propertiesOfObject);
            }
            //asign new ownerobjectid and correct id for property

            JToken targetProperties = RetrieveEntireDocumentObjectFromSolr(newOwnerObjID);
            JObject targetDocumentJsonToAdd = new JObject();
            targetDocumentJsonToAdd.Add(new JProperty("id", newOwnerObjID));
            targetDocumentJsonToAdd.Add(new JProperty("ParentDocument", "true"));
            targetDocumentJsonToAdd.Add(new JProperty("_childDocuments_", entireProperties));
            AddToUploadList(targetDocumentJsonToAdd);
        }

        private const int MoveRelationshipDocumentsRetriveBatchSize = 100000;

        public void MoveRelationshipDocuments(List<string> currentSourceObjDocIDs, string newSourceObjDocID)
        {
            foreach (string currentSourceID in currentSourceObjDocIDs)
            {
                bool IsMoreRelationshipExistanceForTheSourcePossible = true;
                while (IsMoreRelationshipExistanceForTheSourcePossible)
                {
                    JToken retrivedRelationships = RetriveObjectsFromSolrByQuery($"id:{currentSourceID}{RelationshipDocIdSeparator}*", MoveRelationshipDocumentsRetriveBatchSize);
                    List<string> idOfRelationhipsToRemove = new List<string>(10);
                    List<JObject> relationshipJTokensToAdd = new List<JObject>(10);
                    foreach (JToken relDoc in retrivedRelationships.Children())
                    {
                        idOfRelationhipsToRemove.Add((string)relDoc["id"]);
                        JObject tempObj = JObject.Parse(relDoc.ToString());
                        tempObj.Remove("_version_");
                        AssignNewRelSourceID(ref tempObj, newSourceObjDocID);
                        relationshipJTokensToAdd.Add(tempObj);
                    }
                    if (idOfRelationhipsToRemove.Count < MoveRelationshipDocumentsRetriveBatchSize)
                        IsMoreRelationshipExistanceForTheSourcePossible = false;
                    DeleteObjectDocument(idOfRelationhipsToRemove);
                    AddRelationshipDocument(relationshipJTokensToAdd);
                }
            }
        }

        private void AddRelationshipDocument(List<JObject> relationshipJObjsToAdd)
        {
            foreach (JObject relJObj in relationshipJObjsToAdd)
            {
                AddToUploadList(relJObj);
            }
        }

        private void AssignNewRelSourceID(ref JObject relDoc, string newSourceObjID)
        {
            string relID = (string)relDoc["id"];
            relDoc["id"] = $"{newSourceObjID}{RelationshipDocIdSeparator}{relID.Substring(relID.IndexOf(RelationshipDocIdSeparator) + RelationshipDocIdSeparator.Length)}";
            relDoc[nameof(Relationship.SourceObjectId)] = newSourceObjID;
        }

        private string GetPropertyValueForID(string ownerObjDocID, string propertyDocID)
        {
            if (ownerObjDocID == null)
            {
                throw new ArgumentNullException(nameof(ownerObjDocID));
            }
            if (propertyDocID == null)
            {
                throw new ArgumentNullException(nameof(propertyDocID));
            }
            return $"{ownerObjDocID}{PropertyDocIdSeparator}{propertyDocID}";
        }

        private JToken AsignNewIdAndOwnerId(JToken entireProperties, string targetObjID)
        {
            foreach (JToken item in entireProperties)
            {
                string propertyId = ((string)item["id"]).Split(PropertyDocIdSeparator)[1];

                item["id"] = GetPropertyValueForID(targetObjID, propertyId);
                item[nameof(Property.OwnerObjectID)] = targetObjID;
            }
            return entireProperties;
        }

        public void RemoveValueFromFileDocumentMultiField(string fileDocID, string multiValueFieldName, string valueToRemove, bool removedocumentIfNoMoreValueRemainsForField)
        {
            List<string> retriveFields = new List<string>();
            bool isRetrivedMultiValueFieldSingle = false;
            retriveFields.Add(multiValueFieldName);
            JToken fileDocumentJson = RetriveFromCollectionByIdAndFilterFields(fileDocID, FileCollection.SolrUrl, retriveFields);
            List<string> multiValues = new List<string>();
            foreach (var multiValueFieldJson in fileDocumentJson["doc"][multiValueFieldName].Children())
            {
                multiValues.Add(multiValueFieldJson.ToString());
            }
            if (multiValues.Count <= 1)
            {
                isRetrivedMultiValueFieldSingle = true;
            }
            if (removedocumentIfNoMoreValueRemainsForField && isRetrivedMultiValueFieldSingle)
            {
                List<string> removeIds = new List<string>();
                removeIds.Add(fileDocID);
                DeleteFileDocuments(removeIds);
            }
            else
            {
                JArray jsonQuery = new JArray();
                JToken removeMultiValueFieldJson = CreateJsonForRemoveMultiValueField(fileDocID, multiValueFieldName, valueToRemove);
                jsonQuery.Add(removeMultiValueFieldJson);
                UpdateFileDocument(jsonQuery);
            }
        }

        public void UpdateObjectDocumentField(string objDocID, string fieldName, string newValue)
        {
            var retrivedProperties = RetrieveEntireDocumentObjectFromSolr(objDocID);

            JObject jsonParsedDocument = new JObject();
            jsonParsedDocument.Add(new JProperty("id", objDocID));
            jsonParsedDocument.Add(new JProperty("ParentDocument", "true"));
            jsonParsedDocument.Add(new JProperty(fieldName, newValue));

            JArray properties = new JArray();
            properties = AppendPropertiesToJArray(properties, retrivedProperties);
            jsonParsedDocument.Add(new JProperty("_childDocuments_", properties));
            AddToUploadList(jsonParsedDocument);

        }

        public void UpdatePropertyDocumentField(string propertyDocID, string ownerObjDocID, string fieldName, string newValue)
        {
            var retrivedProperties = RetrieveEntireDocumentObjectFromSolr(ownerObjDocID);
            JArray newProperties = new JArray();
            foreach (var property in retrivedProperties)
            {
                if (property["id"].ToString() == GetPropertyValueForID(ownerObjDocID, propertyDocID))
                {
                    if (fieldName == nameof(Property.GeoTime))
                    {
                        object obj = null;
                        if (ValueBaseValidation.TryParsePropertyValue(BaseDataTypes.GeoTime, newValue, out obj).Status == ValidationStatus.Valid ||
                            ValueBaseValidation.TryParsePropertyValue(BaseDataTypes.GeoTime, newValue, out obj).Status == ValidationStatus.Warning
                            )
                        {
                            GeoTimeEntity geoTimeEntity = (GeoTimeEntity)obj;
                            property[nameof(Property.GeoValue)] = $"{geoTimeEntity.Location.Latitude}, {geoTimeEntity.Location.Longitude}";
                            property[nameof(Property.DateRangeValue)] = $"[{ConvertDatePropertyToSolrLongDate(geoTimeEntity.DateRange.TimeBegin)} TO {ConvertDatePropertyToSolrLongDate(geoTimeEntity.DateRange.TimeEnd)}]";
                        }
                    }
                    else if (fieldName == nameof(Property.GeoPoint))
                    {
                        object obj = null;
                        if (ValueBaseValidation.TryParsePropertyValue(BaseDataTypes.GeoPoint, newValue, out obj).Status == ValidationStatus.Valid ||
                            ValueBaseValidation.TryParsePropertyValue(BaseDataTypes.GeoPoint, newValue, out obj).Status == ValidationStatus.Warning
                            )
                        {
                            GeoLocationEntity geoTimeEntity = (GeoLocationEntity)obj;
                            property[nameof(Property.GeoValue)] = $"{geoTimeEntity.Latitude}, {geoTimeEntity.Longitude}";
                        }
                    }
                    else
                    {
                        if (fieldName.Equals(nameof(Property.DateTimeValue)))
                        {
                            newValue = ConvertDatePropertyToSolrLongDate(newValue).ToString();
                        }
                        property[fieldName] = newValue;
                    }
                }
                newProperties.Add(property);
            }

            SearchObject ownerObject = GetObject(long.Parse(ownerObjDocID));

            JObject jsonParsedDocument = new JObject();
            jsonParsedDocument.Add(new JProperty("id", ownerObjDocID));
            jsonParsedDocument.Add(new JProperty("TypeUri", ownerObject.TypeUri));
            jsonParsedDocument.Add(new JProperty("LabelPropertyID", ownerObject.LabelPropertyID));
            jsonParsedDocument.Add(new JProperty("ParentDocument", "true"));
            JArray properties = new JArray();
            properties = AppendPropertiesToJArray(properties, newProperties);
            jsonParsedDocument.Add(new JProperty("_childDocuments_", properties));
            AddToUploadList(jsonParsedDocument);
            UpdateCollection(ObjectCollection.SolrUrl);
        }

        private void UpdateCollection(string collectionUrl, bool alsoCommitChanges = true)
        {
            if (UploadList.Count == 0)
                throw new InvalidOperationException();
            var client = new RestClient(collectionUrl);
            RestRequest request;
            if (alsoCommitChanges)
                request = new RestRequest($"/update?commit=true", Method.POST);
            else
                request = new RestRequest($"/update?commit=false", Method.POST);
            request.AddParameter("application/json; charset=utf-8", UploadList.ToString(), ParameterType.RequestBody);
            if (ExecuteRestRequest(client, request))
            {
                UploadList = new JArray();
            }
        }

        private long ConvertDatePropertyToSolrLongDate(string datetimeValue)
        {
            try
            {
                DateTime date = DateTime.Parse(datetimeValue, CultureInfo.InvariantCulture);
                long dateValue = date.Ticks / 10000;
                return dateValue;
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                return 0;
            }
        }

        private const string DefaultDateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";

        private string ConvertDatePropertyToSolrStringDate(DateTime? datetimeValue)
        {
            try
            {
                string dateValue = (datetimeValue == null) ? string.Empty : ((DateTime)datetimeValue).ToString(DefaultDateTimeFormat,
                  CultureInfo.InvariantCulture);
                return dateValue;
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                return null;
            }
        }

        private long ConvertDatePropertyToSolrLongDate(DateTime? datetimeValue)
        {
            try
            {
                DateTime utcTime1 = new DateTime(datetimeValue.Value.Year, datetimeValue.Value.Month, datetimeValue.Value.Day, datetimeValue.Value.Hour, datetimeValue.Value.Minute, datetimeValue.Value.Second);
                utcTime1 = DateTime.SpecifyKind(utcTime1, DateTimeKind.Utc);
                DateTimeOffset utcTime2 = utcTime1;
                return utcTime2.ToUnixTimeMilliseconds();
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                return 0;
            }
        }

        public void AddDataSourceDocument(DataSourceDocument dataSource)
        {

            JObject jsonParsedDocument = new JObject();
            jsonParsedDocument.Add(new JProperty("id", dataSource.Id));
            jsonParsedDocument.Add(new JProperty(nameof(DataSourceDocument.Name), dataSource.Name));
            jsonParsedDocument.Add(new JProperty(nameof(DataSourceDocument.Description), dataSource.Description));
            jsonParsedDocument.Add(new JProperty(nameof(DataSourceDocument.Type), dataSource.Type));
            jsonParsedDocument.Add(new JProperty(nameof(DataSourceDocument.CreatedBy), dataSource.CreatedBy));
            jsonParsedDocument.Add(new JProperty(nameof(DataSourceDocument.CreatedTime), dataSource.CreatedTime));
            jsonParsedDocument.Add(new JProperty(nameof(Entities.SearchEngine.Documents.ACL.ClassificationIdentifier), dataSource.Acl.ClassificationIdentifier));
            foreach (var item in dataSource.Acl.Permissions)
            {
                jsonParsedDocument.Add(new JProperty(item.GroupName, (byte)item.AccessLevel));
            }
            AddToDataSourcUploadList(jsonParsedDocument);
        }

        private void AddToDataSourcUploadList(JObject jsonObject)
        {
            UploadList.Add(jsonObject);
        }


        public void RegisterNewDataSource(long dsId, string name, DataSourceType type, AccessControl.ACL acl, string description, string createBy, string createdTime)
        {
            AddNewDataSource(dsId, name, type, acl, description, createBy, createdTime);
            AddDataSourceAcis(dsId, acl.Permissions);
        }

        private void AddNewDataSource(long dsId, string name, DataSourceType type, AccessControl.ACL acl, string description, string createBy, string createdTime)
        {
            JObject jsonParsedDocument = new JObject();
            jsonParsedDocument.Add(new JProperty("id", dsId));
            jsonParsedDocument.Add(new JProperty("Name", name));
            jsonParsedDocument.Add(new JProperty("Description", description));
            jsonParsedDocument.Add(new JProperty("Type", (byte)type));
            jsonParsedDocument.Add(new JProperty("CreatedBy", createBy));
            jsonParsedDocument.Add(new JProperty("CreatedTime", createdTime));
            jsonParsedDocument.Add(new JProperty("ClassificationIdentifier", acl.Classification));
            //foreach (var item in acl.Permissions)
            //{
            //jsonParsedDocument.Add(new JProperty("Administrators", (byte)item.AccessLevel));
            //}

            jsonParsedDocument.Add(new JProperty("Administrators", string.Join(",", acl.Permissions.Select(s => (byte)s.AccessLevel).ToList())));

            AddToDataSourcUploadList(jsonParsedDocument);
        }

        private void AddDataSourceAcis(long dsId, List<ACI> acis)
        {
            if (acis.Count < 1)
                return;

            JArray jArray = new JArray();

            for (int i = 0; i < acis.Count; i++)
            {
                ACI aci = acis[i];

                JObject jsonParsedDocument = new JObject();
                jsonParsedDocument.Add(new JProperty("id", dsId));
                jsonParsedDocument.Add(new JProperty("dsid", dsId));
                jsonParsedDocument.Add(new JProperty("GroupName", aci.GroupName));
                jsonParsedDocument.Add(new JProperty("Permission", (byte)aci.AccessLevel));

                jArray.Add(jsonParsedDocument);
                //AddToDataSourcUploadList(jsonParsedDocument);
            }

            var client = new RestClient(DataSourceAciCollection.SolrUrl);
            RestRequest request = new RestRequest($"/update?commit=true", Method.POST);
            request.AddParameter("application/json; charset=utf-8", jArray.ToString(), ParameterType.RequestBody);
            ExecuteRestRequest(client, request);
        }

        public SearchGraphArrangement AddSearchGraph(SearchGraphArrangement searchGraphArrangement)
        {
            JArray jArray = new JArray();

            JObject jsonParsedDocument = new JObject();
            jsonParsedDocument.Add(new JProperty("id", searchGraphArrangement.Id));
            jsonParsedDocument.Add(new JProperty("dsid", searchGraphArrangement.Id));
            jsonParsedDocument.Add(new JProperty("Title", searchGraphArrangement.Title));
            jsonParsedDocument.Add(new JProperty("Description", searchGraphArrangement.Description));
            jsonParsedDocument.Add(new JProperty("TimeCreated", searchGraphArrangement.TimeCreated));
            jsonParsedDocument.Add(new JProperty("GraphImage_txt", Convert.ToBase64String(searchGraphArrangement.GraphImage)));
            jsonParsedDocument.Add(new JProperty("GraphArrangement_txt", Convert.ToBase64String(searchGraphArrangement.GraphArrangementXML)));
            jsonParsedDocument.Add(new JProperty("NodesCount", searchGraphArrangement.NodesCount));

            jArray.Add(jsonParsedDocument);


            var client = new RestClient(GraphCollection.SolrUrl);
            RestRequest request = new RestRequest($"/update?commit=true", Method.POST);
            request.AddParameter("application/json; charset=utf-8", jArray.ToString(), ParameterType.RequestBody);
            ExecuteRestRequest(client, request);

            return searchGraphArrangement;
        }
    }
}
