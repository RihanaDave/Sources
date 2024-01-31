using GPAS.AccessControl;
using GPAS.Dispatch.Entities.Concepts.Geo;
using GPAS.Ontology;
using GPAS.PropertiesValidation;
using GPAS.SearchServer.Entities;
using GPAS.SearchServer.Entities.SearchEngine.Documents;
using GPAS.Utility;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace GPAS.SearchServer.Logic
{
    public class SearchEngineDocumentConvertor
    {
        public File GetFileDocument(AccessControled<SearchMedia> addedMedia, byte[] mediaContent)
        {
            return new File()
            {
                Id = EncodingConverter.GetBase64Encode(addedMedia.ConceptInstance.URI),
                Content = mediaContent,
                Description = addedMedia.ConceptInstance.Description,
                OwnerObjectIds = new List<string> { addedMedia.ConceptInstance.OwnerObjectId.ToString(CultureInfo.InstalledUICulture) },
                Acl = GetSearchEngineDocumentAclFromConceptsAcl(addedMedia.Acl)
            };
        }

        public File GetFileDocument(AccessControled<SearchObject> addedDoc, byte[] documentContent)
        {
            SearchServer.Access.FileRepositoryService.ServiceClient proxy = null;
            
            File file = new File();
            try
            {
                proxy = new Access.FileRepositoryService.ServiceClient();
                long fileSize = proxy.GetDataSourceAndDocumentFileSizeInBytes(addedDoc.ConceptInstance.Id.ToString());

                file.Id = addedDoc.ConceptInstance.Id.ToString(CultureInfo.InvariantCulture);
                file.Content = documentContent;
                file.OwnerObjectIds = new List<string> { addedDoc.ConceptInstance.Id.ToString(CultureInfo.InvariantCulture) };
                file.Acl = GetSearchEngineDocumentAclFromConceptsAcl(addedDoc.Acl);
                file.CreatedTime = DateTime.Now;
                file.FileSize = fileSize;
                file.ImportDate = DateTime.Now;
                file.RelatedWord = "RelatedWord";
                file.FileType = addedDoc.ConceptInstance.TypeUri;
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
            return file;

        }

        public bool GetPropertyDocument(AccessControled<SearchProperty> addedProp, Ontology.Ontology ontology, out Property property)
        {
            bool isParsed = false;
            property = new Property()
            {
                Id = addedProp.ConceptInstance.Id.ToString(CultureInfo.InvariantCulture),
                TypeUri = addedProp.ConceptInstance.TypeUri,
                BaseType = (byte)ontology.GetBaseDataTypeOfProperty(addedProp.ConceptInstance.TypeUri),
                OwnerObjectID = addedProp.ConceptInstance.OwnerObject.Id.ToString(CultureInfo.InvariantCulture),
                OwnerObjectTypeUri = addedProp.ConceptInstance.OwnerObject.TypeUri,
                 DataSourceId = addedProp.ConceptInstance.DataSourceID,
                Acl = GetSearchEngineDocumentAclFromConceptsAcl(addedProp.Acl)
            };
            if (addedProp.ConceptInstance.TypeUri == ontology.GetDateRangeAndLocationPropertyTypeUri())
            {
                isParsed = SetPropertyDocumentValueField(ref property, BaseDataTypes.GeoTime, addedProp.ConceptInstance.Value);
            }
            else
            {
                isParsed = SetPropertyDocumentValueField(ref property, (BaseDataTypes)property.BaseType, addedProp.ConceptInstance.Value);
            }
            return isParsed;
        }

        public Relationship GetRelationshipDocument(AccessControled<SearchRelationship> addedRel)
        {
            return new Relationship()
            {
                Id = addedRel.ConceptInstance.Id.ToString(CultureInfo.InvariantCulture),
                LinkTypeUri = addedRel.ConceptInstance.TypeUri,
                SourceObjectId = addedRel.ConceptInstance.SourceObjectId.ToString(CultureInfo.InvariantCulture),
                SourceObjectTypeUri = addedRel.ConceptInstance.SourceObjectTypeUri,
                TargetObjectId = addedRel.ConceptInstance.TargetObjectId.ToString(CultureInfo.InvariantCulture),
                TargetObjectTypeUri = addedRel.ConceptInstance.TargetObjectTypeUri,
                DataSourceId = addedRel.ConceptInstance.DataSourceID,
                Direction = addedRel.ConceptInstance.Direction,
                Acl = GetSearchEngineDocumentAclFromConceptsAcl(addedRel.Acl)
            };
        }
        public bool ConvertValidationToBoolean(BaseDataTypes baseType, string value, out object parsedValue)
        {
            if (ValueBaseValidation.TryParsePropertyValue(baseType, value, out parsedValue).Status == ValidationStatus.Valid ||
                ValueBaseValidation.TryParsePropertyValue(baseType, value, out parsedValue).Status == ValidationStatus.Warning)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool SetPropertyDocumentValueField(ref Property changingProp, BaseDataTypes baseType, string value)
        {
            object parsedValue = null;
            bool isParsed = ConvertValidationToBoolean(baseType, value, out parsedValue);
            if (isParsed)
            {
                switch (baseType)
                {
                    case BaseDataTypes.Int:
                    case BaseDataTypes.Long:
                        changingProp.LongValue = string.IsNullOrEmpty(parsedValue.ToString()) ? (long?)null : long.Parse(parsedValue.ToString());
                        break;
                    case BaseDataTypes.Boolean:
                        changingProp.BooleanValue = string.IsNullOrEmpty(parsedValue.ToString()) ? (bool?)null : bool.Parse(parsedValue.ToString());
                        break;
                    case BaseDataTypes.DateTime:
                        changingProp.DateTimeValue = string.IsNullOrEmpty(parsedValue.ToString()) ? (DateTime?)null : (DateTime?)(parsedValue);
                        break;
                    case BaseDataTypes.String:
                    case BaseDataTypes.HdfsURI:
                        changingProp.StringValue = (string)parsedValue;
                        break;
                    case BaseDataTypes.Double:
                        changingProp.DoubleValue = string.IsNullOrEmpty(parsedValue.ToString()) ? (double?)null : double.Parse(parsedValue.ToString());
                        break;
                    case BaseDataTypes.GeoTime:
                        object obj = null;
                        if (ConvertValidationToBoolean(baseType, value, out obj))
                        {
                            GeoTimeEntity geoTimeEntity = (GeoTimeEntity)obj;
                            changingProp.GeoValue = $"{geoTimeEntity.Location.Latitude}, {geoTimeEntity.Location.Longitude}";
                            changingProp.DateRangeValue = $"[{ConvertDatePropertyToSolrDate(geoTimeEntity.DateRange.TimeBegin)} TO {ConvertDatePropertyToSolrDate(geoTimeEntity.DateRange.TimeEnd)}]";
                        }
                        break;
                    case BaseDataTypes.GeoPoint:
                        if (ConvertValidationToBoolean(baseType, value, out obj))
                        {
                            GeoLocationEntity geoLocationEntity = (GeoLocationEntity)obj;
                            changingProp.GeoValue = $"{geoLocationEntity.Latitude}, {geoLocationEntity.Longitude}";
                        }
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
            return isParsed;
        }

        private const string DefaultDateTimeFormat = "yyyy-MM-dd'T'HH:mm:ss.fff'Z'";

        private string ConvertDatePropertyToSolrDate(DateTime datetimeValue)
        {
            var dateValue = datetimeValue.ToString(DefaultDateTimeFormat,
                 CultureInfo.InvariantCulture);
            return dateValue;

        }

        public Entities.SearchEngine.Documents.ACL GetSearchEngineDocumentAclFromConceptsAcl(AccessControl.ACL acl)
        {
            return new Entities.SearchEngine.Documents.ACL()
            {
                ClassificationIdentifier = acl.Classification,
                Permissions = GetPermissionsFromAcl(acl)
            };
        }

        public AccessControl.ACL GetConceptsAclFromSearchEngineDocumentAcl(Entities.SearchEngine.Documents.ACL acl)
        {
            return new AccessControl.ACL()
            {
                Classification = acl.ClassificationIdentifier,
                Permissions = GetPermissionsFromEngineDocumentAcl(acl)
            };
        }
        public List<AccessControl.ACI> GetPermissionsFromEngineDocumentAcl(Entities.SearchEngine.Documents.ACL acl)
        {
            List<AccessControl.ACI> result = new List<AccessControl.ACI>();
            foreach (Entities.SearchEngine.Documents.GroupPermission aci in acl.Permissions)
            {
                result.Add(new AccessControl.ACI()
                {
                    GroupName = aci.GroupName,
                    AccessLevel = aci.AccessLevel
                });
            }
            return result;
        }



        public List<Entities.SearchEngine.Documents.GroupPermission> GetPermissionsFromAcl(AccessControl.ACL acl)
        {
            List<GroupPermission> result = new List<GroupPermission>(acl.Permissions.Count);
            foreach (ACI aci in acl.Permissions)
            {
                result.Add(new GroupPermission()
                {
                    GroupName = aci.GroupName,
                    AccessLevel = aci.AccessLevel
                });
            }
            return result;
        }

        public ObjectDocument GetObjectDocument(SearchObject addedObj)
        {
            return new ObjectDocument()
            {
                Id = addedObj.Id.ToString(CultureInfo.InvariantCulture),
                TypeUri = addedObj.TypeUri,
                 LabelPropertyID = addedObj.LabelPropertyID,
                Properties = new List<Property>()
            };
        }

        public DataSourceDocument ConvertDataSourceInfoToDataSource(DataSourceInfo dataSourceInfo)
        {
            return new DataSourceDocument()
            {
                Acl = GetSearchEngineDocumentAclFromConceptsAcl(dataSourceInfo.Acl),
                Description = dataSourceInfo.Description,
                Id = dataSourceInfo.Id.ToString(),
                Name = dataSourceInfo.Name,
                Type = dataSourceInfo.Type,
                CreatedBy = dataSourceInfo.CreatedBy,
                CreatedTime = dataSourceInfo.CreatedTime
            };
        }

        public List<DataSourceInfo> ConvertDataSourceInfoToDataSource(List<DataSourceDocument> dataSources)
        {
            List<DataSourceInfo> dataSourceInfos = new List<DataSourceInfo>();
            if (dataSources.Count == 0)
                return dataSourceInfos;
            foreach (var dataSource in dataSources)
            {
                dataSourceInfos.Add(new DataSourceInfo()
                {
                    Acl = GetConceptsAclFromSearchEngineDocumentAcl(dataSource.Acl),
                    Description = dataSource.Description,
                    Id = long.Parse(dataSource.Id),
                    Name = dataSource.Name,
                    Type = dataSource.Type,
                    CreatedBy = dataSource.CreatedBy,
                    CreatedTime = dataSource.CreatedTime
                });
            }
            return dataSourceInfos;
        }
    }
}
