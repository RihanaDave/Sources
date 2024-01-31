using GPAS.DataImport.StructuredToSemiStructuredConvertors.EmlToCsv;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Controls.DataImport.Model;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.DataSourceField;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.Map;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.Permission;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport
{
    public partial class DataImportUtility : Utility.Utility
    {
        public static readonly string emlDirectoryPath =
            $"{ConfigurationManager.AppSettings["WorkspaceTempFolderPath"]}{"Eml Files Attachments"}";

        public static IEnumerable<IDataSource> SpecifyDataSourceType(IEnumerable<string> filePath)
        {
            string[] supportedDocumentFileExtensions = OntologyProvider.GetOntology().GetTextDocumentFileTypes();
            string[] supportedImageFileExtensions = OntologyProvider.GetOntology().GetImageFileTypes();
            string[] supportedVideoFileExtensions = OntologyProvider.GetOntology().GetVideoFileTypes();
            string[] supportedAudioFileExtensions = OntologyProvider.GetOntology().GetAudioFileTypes();
            string[] supportedStructuredFileExtensions = OntologyProvider.GetOntology().GetTabularFileTypes();

            List<IDataSource> dataSources = new List<IDataSource>();
            List<string> emlFiles = filePath.Where(fp => Path.GetExtension(fp)?.ToUpper().Replace(".", "") == "EML").ToList();
            if (emlFiles.Count > 1)
            {
                filePath = filePath.Except(emlFiles);
                TabularGroupDataSourceModel emlGroupDataSource = new TabularGroupDataSourceModel()
                {
                    Title = "Eml Group",
                };

                List<EmlDataSourceModel> internalDataSources = new List<EmlDataSourceModel>();

                foreach (string emlFile in emlFiles)
                {
                    EmlDataSourceModel emlDataSource = CreateEmlDataSourceModel(emlFile);
                    internalDataSources.Add(emlDataSource);
                }

                emlGroupDataSource.AddRangeDataSources(internalDataSources);
                CreateDefultMapForEmlDataSource(emlGroupDataSource);
                dataSources.Add(emlGroupDataSource);
            }

            foreach (string path in filePath)
            {
                string extenstion = Path.GetExtension(path)?.ToUpper().Replace(".", "");

                if (supportedStructuredFileExtensions.Contains(extenstion))
                {
                    switch (extenstion)
                    {
                        case "CSV":
                            dataSources.Add(CreateCsvDataSourceModel(path));
                            break;
                        case "XLSX":
                            dataSources.AddRange(CreateExcelDataSourceModel(path));
                            break;
                        case "EML":
                            EmlDataSourceModel emlDataSource = CreateEmlDataSourceModel(path);
                            CreateDefultMapForEmlDataSource(emlDataSource);
                            dataSources.Add(emlDataSource);
                            break;
                        case "ACCDB":
                        case "MDB":
                            dataSources.AddRange(CreateAccessDataSourceModel(path));
                            break;
                    }
                }
                else
                {
                    UnstructuredDataSourceModel unstructuredDataSource = null;

                    if (supportedAudioFileExtensions.Contains(extenstion))
                    {
                        unstructuredDataSource = CreateAudioDataSourceModel(path);
                    }
                    else if (supportedImageFileExtensions.Contains(extenstion))
                    {
                        unstructuredDataSource = CreateImageDataSourceModel(path);
                    }
                    else if (supportedVideoFileExtensions.Contains(extenstion))
                    {
                        unstructuredDataSource = CreateVideoDataSourceModel(path);
                    }
                    else if (supportedDocumentFileExtensions.Contains(extenstion))
                    {
                        switch (extenstion)
                        {
                            case "PDF":
                            case "DOC":
                            case "DOCX":
                            case "RTF":
                            case "PPT":
                            case "PPTX":
                                unstructuredDataSource = CreateComplexTextBaseDataSourceModel(path);
                                break;
                            case "HTM":
                            case "HTML":
                            case "XHTML":
                            case "TXT":
                                unstructuredDataSource = CreateSimpleTextBaseDataSourceModel(path);
                                break;
                        }
                    }
                    else
                    {
                        throw new NotSupportedException(nameof(extenstion));
                    }

                    CreateDefultMapForUnstructuredDataSource(unstructuredDataSource);

                    dataSources.Add(unstructuredDataSource);
                }
            }

            return dataSources;
        }

        public static void CreateDefultMapForEmlDataSource(IDataSource dataSource)
        {
            MappingViewModel mappingViewModel = new MappingViewModel(dataSource.Map);

            if (!ExistEmlMapConceptsInOntology(mappingViewModel))
                return;
            Ontology.Ontology ontology = OntologyProvider.GetOntology();

            //sender email
            string personObjectTypeUri = ontology.GetPersonObjectTypeURI();
            mappingViewModel.AddObject(personObjectTypeUri);
            AddPropertiesToEmlMapSender(mappingViewModel);

            //receiver email
            mappingViewModel.AddObject(personObjectTypeUri);
            AddPropertiesToEmlMapReceiver(mappingViewModel);

            //email event
            string emailObjectTypeUri = ontology.GetEmailObjectTypeURI();
            mappingViewModel.AddLink(emailObjectTypeUri,
                OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(emailObjectTypeUri),
                Entities.KWLinks.LinkDirection.SourceToTarget,
                mappingViewModel.Map.ObjectCollection[0],
                mappingViewModel.Map.ObjectCollection[1]
                );
            AddPropertiesToEmlMapEmailObject(mappingViewModel);

            //document
            mappingViewModel.AddObject(ontology.GetDocumentTypeURI());
            DocumentMapModel document = (DocumentMapModel)mappingViewModel.Map.ObjectCollection[3];
            document.PathOption = DocumentPathOption.FolderWithSubFolder;
            AddPropertiesToEmlMapDocument(mappingViewModel);

            //attachment relationship
            string attachmentRelationshipTypeUri = ontology.GetAttachmentRelationshipTypeURI();
            mappingViewModel.AddLink(attachmentRelationshipTypeUri,
                OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(attachmentRelationshipTypeUri),
                Entities.KWLinks.LinkDirection.TargetToSource,
                mappingViewModel.Map.ObjectCollection[2],
                mappingViewModel.Map.ObjectCollection[3]
                );

            foreach (ObjectMapModel objectMap in mappingViewModel.Map.ObjectCollection)
            {
                objectMap.IsSelected = false;
            }
        }

        private static void AddPropertyWithValue(MappingViewModel mappingViewModel, ObjectMapModel objectMap, string propertyTypeUri, string fieldTitle, FieldType fieldType)
        {
            Ontology.Ontology ontology = OntologyProvider.GetOntology();
            IEnumerable<DataSourceFieldModel> fields = mappingViewModel?.CategoryFieldCollection?.FirstOrDefault(c => c.Category == fieldType)
                       ?.DataSourceCollection;
            mappingViewModel.AddPropertyToObject(objectMap, propertyTypeUri);
            SinglePropertyMapModel property = (SinglePropertyMapModel)objectMap.Properties.LastOrDefault();
            DataSourceFieldModel field = fields.FirstOrDefault(tf => tf.Title == fieldTitle);
            mappingViewModel.AddValueToProperty(property, field);
        }

        private static void AddPropertiesToEmlMapSender(MappingViewModel mappingViewModel)
        {
            Ontology.Ontology ontology = OntologyProvider.GetOntology();
            ExtractedEmailingEvent extractedEmailingEvent = new ExtractedEmailingEvent();
            ObjectMapModel sender = mappingViewModel.Map.ObjectCollection[0];

            string namePropertyTypeUri = ontology.GetNamePropertyTypeURI();
            AddPropertyWithValue(mappingViewModel, sender, namePropertyTypeUri, nameof(extractedEmailingEvent.SenderDisplayName), FieldType.Tabular);

            string emailAddressPropertyTypeUri = ontology.GetEmailAddressPropertyTypeURI();
            AddPropertyWithValue(mappingViewModel, sender, emailAddressPropertyTypeUri, nameof(extractedEmailingEvent.SenderEmail), FieldType.Tabular);
            sender.Properties.LastOrDefault().HasResolution = true;
        }

        private static void AddPropertiesToEmlMapReceiver(MappingViewModel mappingViewModel)
        {
            Ontology.Ontology ontology = OntologyProvider.GetOntology();
            ExtractedEmailingEvent extractedEmailingEvent = new ExtractedEmailingEvent();
            ObjectMapModel receiver = mappingViewModel.Map.ObjectCollection[1];

            string namePropertyTypeUri = ontology.GetNamePropertyTypeURI();
            AddPropertyWithValue(mappingViewModel, receiver, namePropertyTypeUri, nameof(extractedEmailingEvent.ReceiverDisplayName), FieldType.Tabular);

            string emailAddressPropertyTypeUri = ontology.GetEmailAddressPropertyTypeURI();
            AddPropertyWithValue(mappingViewModel, receiver, emailAddressPropertyTypeUri, nameof(extractedEmailingEvent.ReceiverEmail), FieldType.Tabular);
            receiver.Properties.LastOrDefault().HasResolution = true;
        }

        private static void AddPropertiesToEmlMapEmailObject(MappingViewModel mappingViewModel)
        {
            Ontology.Ontology ontology = OntologyProvider.GetOntology();
            ExtractedEmailingEvent extractedEmailingEvent = new ExtractedEmailingEvent();
            ObjectMapModel emailObject = mappingViewModel.Map.ObjectCollection[2];

            string emailSubjectPropertyTypeUri = ontology.GetEmailSubjectPropertyTypeURI();
            AddPropertyWithValue(mappingViewModel, emailObject, emailSubjectPropertyTypeUri, nameof(extractedEmailingEvent.Subject), FieldType.Tabular);

            string emailBodyPropertyTypeUri = ontology.GetEmailBodyPropertyTypeURI();
            AddPropertyWithValue(mappingViewModel, emailObject, emailBodyPropertyTypeUri, nameof(extractedEmailingEvent.TextBody), FieldType.Tabular);

            string emailIdPropertyTypeUri = ontology.GetEmailIdPropertyTypeURI();
            AddPropertyWithValue(mappingViewModel, emailObject, emailIdPropertyTypeUri, nameof(extractedEmailingEvent.MessageId), FieldType.Tabular);
            emailObject.Properties.LastOrDefault().HasResolution = true;

            string emailReceiveMethodPropertyTypeUri = ontology.GetEmailReceiveMethodPropertyTypeURI();
            AddPropertyWithValue(mappingViewModel, emailObject, emailReceiveMethodPropertyTypeUri, nameof(extractedEmailingEvent.SendingMethod), FieldType.Tabular);

            string sentTimePropertyTypeUri = ontology.GetSentTimePropertyTypeURI();
            AddPropertyWithValue(mappingViewModel, emailObject, sentTimePropertyTypeUri, nameof(extractedEmailingEvent.DateSent), FieldType.Tabular);
            DateTimePropertyMapModel dateTimeProperty = (DateTimePropertyMapModel)emailObject.Properties.LastOrDefault();
            dateTimeProperty.Configuration.StringFormat = "M/d/yyyy h:m:s tt";

            string receiverAddressPropertyTypeUri = ontology.GetReceiverAddressPropertyTypeURI();
            AddPropertyWithValue(mappingViewModel, emailObject, receiverAddressPropertyTypeUri, nameof(extractedEmailingEvent.ReceiverEmail), FieldType.Tabular);

            string senderAddressPropertyTypeUri = ontology.GetSenderAddressPropertyTypeURI();
            AddPropertyWithValue(mappingViewModel, emailObject, senderAddressPropertyTypeUri, nameof(extractedEmailingEvent.SenderEmail), FieldType.Tabular);
        }

        private static void AddPropertiesToEmlMapDocument(MappingViewModel mappingViewModel)
        {
            DocumentMapModel document = (DocumentMapModel)mappingViewModel.Map.ObjectCollection[3];
            ExtractedEmailingEvent extractedEmailingEvent = new ExtractedEmailingEvent();

            mappingViewModel.AddValueToProperty(document.Path,
                mappingViewModel?.CategoryFieldCollection?.FirstOrDefault(c => c.Category == FieldType.Tabular)
                    ?.DataSourceCollection.FirstOrDefault(tf => tf.Title == nameof(extractedEmailingEvent.AttachmentsRelativePath)));
            document.Path.HasResolution = true;
        }

        private static bool ExistEmlMapConceptsInOntology(MappingViewModel mappingViewModel)
        {
            Ontology.Ontology ontology = OntologyProvider.GetOntology();

            ExtractedEmailingEvent extractedEmailingEvent = new ExtractedEmailingEvent();
            IEnumerable<DataSourceFieldModel> fields = mappingViewModel?.CategoryFieldCollection?.FirstOrDefault(c => c.Category == FieldType.Tabular)
                       ?.DataSourceCollection;

            return !(
                ontology == null || fields == null || fields.Count() == 0 ||
                ontology.GetPersonObjectTypeURI() == null ||
                ontology.GetEmailObjectTypeURI() == null ||
                ontology.GetAttachmentRelationshipTypeURI() == null ||
                ontology.GetNamePropertyTypeURI() == null ||
                ontology.GetEmailAddressPropertyTypeURI() == null ||
                ontology.GetEmailSubjectPropertyTypeURI() == null ||
                ontology.GetEmailBodyPropertyTypeURI() == null ||
                ontology.GetEmailIdPropertyTypeURI() == null ||
                ontology.GetEmailReceiveMethodPropertyTypeURI() == null ||
                ontology.GetSentTimePropertyTypeURI() == null ||
                ontology.GetReceiverAddressPropertyTypeURI() == null ||
                ontology.GetSenderAddressPropertyTypeURI() == null ||
                fields.FirstOrDefault(tf => tf.Title == nameof(extractedEmailingEvent.Subject)) == null ||
                fields.FirstOrDefault(tf => tf.Title == nameof(extractedEmailingEvent.TextBody)) == null ||
                fields.FirstOrDefault(tf => tf.Title == nameof(extractedEmailingEvent.MessageId)) == null ||
                fields.FirstOrDefault(tf => tf.Title == nameof(extractedEmailingEvent.SendingMethod)) == null ||
                fields.FirstOrDefault(tf => tf.Title == nameof(extractedEmailingEvent.DateSent)) == null ||
                fields.FirstOrDefault(tf => tf.Title == nameof(extractedEmailingEvent.ReceiverEmail)) == null ||
                fields.FirstOrDefault(tf => tf.Title == nameof(extractedEmailingEvent.SenderEmail)) == null ||
                fields.FirstOrDefault(tf => tf.Title == nameof(extractedEmailingEvent.ReceiverDisplayName)) == null ||
                fields.FirstOrDefault(tf => tf.Title == nameof(extractedEmailingEvent.SenderDisplayName)) == null ||
                fields.FirstOrDefault(tf => tf.Title == nameof(extractedEmailingEvent.AttachmentsRelativePath)) == null
            );
        }

        private static void CreateDefultMapForUnstructuredDataSource(UnstructuredDataSourceModel dataSource)
        {
            Ontology.Ontology ontology = OntologyProvider.GetOntology();
            MappingViewModel mappingViewModel = new MappingViewModel(dataSource.Map);
            mappingViewModel.AddObject(ontology.GetDocumentTypeURI());
            DocumentMapModel document = (DocumentMapModel)dataSource.Map.ObjectCollection[0];
            mappingViewModel.AddValueToProperty(document.Path,
                mappingViewModel?.CategoryFieldCollection?.FirstOrDefault(c => c.Category == FieldType.Path)
                    ?.DataSourceCollection.FirstOrDefault(x => ((PathPartFieldModel)x).PartType == PathPartType.FullPath));
        }

        private static CsvDataSourceModel CreateCsvDataSourceModel(string filePath)
        {
            return new CsvDataSourceModel
            {
                FileInfo = new FileInfoModel
                {
                    FullPath = filePath
                },
                Title = Path.GetFileNameWithoutExtension(filePath)
            };
        }

        private static IEnumerable<ExcelDataSourceModel> CreateExcelDataSourceModel(string filePath)
        {
            GPAS.DataImport.Transformation.Utility util = new GPAS.DataImport.Transformation.Utility();
            Dictionary<string, DataTable> excelSheetToDataTableMapping = util.GetExcelSheetToDataTableMapping(filePath,
                Properties.Settings.Default.ImportPreview_MaximumSampleRows, true);

            return excelSheetToDataTableMapping.Select(sheet => new ExcelDataSourceModel
            { Title = sheet.Key, FileInfo = new FileInfoModel { FullPath = filePath }, Preview = sheet.Value })
                .Cast<ExcelDataSourceModel>().ToList();
        }

        private static IEnumerable<AccessDataSourceModel> CreateAccessDataSourceModel(string filePath)
        {
            GPAS.DataImport.Transformation.Utility util = new GPAS.DataImport.Transformation.Utility();
            Dictionary<string, DataTable> accessTableToDataTableMapping = util.GetAccessTableToDataTableMapping(filePath,
                Properties.Settings.Default.ImportPreview_MaximumSampleRows, true);

            return accessTableToDataTableMapping.Select(sheet => new AccessDataSourceModel
            { FileInfo = new FileInfoModel { FullPath = filePath }, Title = sheet.Key, Preview = sheet.Value })
                .Cast<AccessDataSourceModel>().ToList();
        }

        private static EmlDataSourceModel CreateEmlDataSourceModel(string filePath)
        {
            return new EmlDataSourceModel
            {
                FileInfo = new FileInfoModel
                {
                    FullPath = filePath
                },
                Title = Path.GetFileNameWithoutExtension(filePath)
            };
        }

        private static SimpleTextBaseDataSourceModel CreateSimpleTextBaseDataSourceModel(string filePath)
        {
            return new SimpleTextBaseDataSourceModel
            {
                FileInfo = new FileInfoModel
                {
                    FullPath = filePath
                },
                Title = Path.GetFileNameWithoutExtension(filePath)
            };
        }

        private static ComplexTextBaseDataSourceModel CreateComplexTextBaseDataSourceModel(string filePath)
        {
            return new ComplexTextBaseDataSourceModel
            {
                FileInfo = new FileInfoModel
                {
                    FullPath = filePath
                },
                Title = Path.GetFileNameWithoutExtension(filePath)
            };
        }

        private static ImageBaseDataSourceModel CreateImageDataSourceModel(string filePath)
        {
            return new ImageBaseDataSourceModel
            {
                FileInfo = new FileInfoModel
                {
                    FullPath = filePath
                },
                Title = Path.GetFileNameWithoutExtension(filePath)
            };
        }

        private static VideoBaseDataSourceModel CreateVideoDataSourceModel(string filePath)
        {
            return new VideoBaseDataSourceModel
            {
                FileInfo = new FileInfoModel
                {
                    FullPath = filePath
                },
                Title = Path.GetFileNameWithoutExtension(filePath)
            };
        }

        private static AudioBaseDataSourceModel CreateAudioDataSourceModel(string filePath)
        {
            return new AudioBaseDataSourceModel
            {
                FileInfo = new FileInfoModel
                {
                    FullPath = filePath
                },
                Title = Path.GetFileNameWithoutExtension(filePath)
            };
        }

        public static async Task<IDataSource> CopyDataSourceAsync(IDataSource dataSource)
        {
            IDataSource copied = null;
            await Task.Run(() => copied = CopyDataSource(dataSource));
            return copied;
        }

        private static IDataSource CopyDataSource(IDataSource dataSource)
        {
            if (dataSource is SQLServerDataSourceModel sQLDataSource)
            {
                return CreateSQlServerDataSource(sQLDataSource.Title, sQLDataSource.FileInfo?.FullPath, sQLDataSource.Preview);
            }
            else
            {
                List<string> files = new List<string>();

                if (dataSource is ISingleDataSource singleDataSource)
                {
                    files.Add(singleDataSource.FileInfo.FullPath);
                }
                else if (dataSource is IGroupDataSource groupDataSource)
                {
                    files.AddRange(groupDataSource.DataSourceCollection.OfType<EmlDataSourceModel>().Select(eml => eml.FileInfo.FullPath));
                }

                List<IDataSource> dataSources = DataImportUtility.SpecifyDataSourceType(files).ToList();

                if (dataSources?.Count > 0)
                    return dataSources[0];
                else
                    return null;
            }
        }

        public static SQLServerDataSourceModel CreateSQlServerDataSource(string title, string databaseUri, DataTable preview)
        {
            return new SQLServerDataSourceModel()
            {
                FileInfo = new FileInfoModel()
                {
                    FullPath = databaseUri,
                },
                Title = title,
                Preview = preview
            };
        }

        /// <summary>
        /// بارگذاری نگاشت برای منبع داده انتخاب شده
        /// </summary>
        /// <param name="targetDataSource">منبع داده انتخاب شده</param>
        /// <param name="newMap">نگاشت</param>
        /// <returns></returns>
        public static OperationResultModel LoadMap(IDataSource targetDataSource, MapModel newMap)
        {
            OperationResultModel result = new OperationResultModel();

            OperationResultModel matchResult = IsMapMatchWithDataSource(newMap, targetDataSource);

            if (matchResult.IsOk)
            {
                newMap.OwnerDataSource = targetDataSource;

                newMap.ObjectCollection.First().IsSelected = true;

                foreach (ObjectMapModel objectMapModel in newMap.ObjectCollection)
                {
                    objectMapModel.OwnerMap = newMap;

                    if (objectMapModel is DocumentMapModel documentMapModel)
                    {
                        PrepareDocumentObjectForLoad(documentMapModel, targetDataSource);
                    }

                    foreach (PropertyMapModel property in objectMapModel.GetAllProperties())
                    {
                        property.OwnerObject = objectMapModel;

                        if (property.IsDisplayName)
                        {
                            property.IsDisplayName = false;
                            property.IsDisplayName = true;
                        }

                        if (property is DateTimePropertyMapModel dateTimeProperty)
                        {
                        }

                        if (property is MultiPropertyMapModel multiProperty)
                        {
                            if (multiProperty.InnerProperties == null)
                                multiProperty.InnerProperties = new ObservableCollection<PropertyMapModel>();

                            RemoveExtraInnerProperties(multiProperty);
                        }

                        SetValuesOwnerProperty(property);
                    }
                }

                foreach (RelationshipMapModel relationship in newMap.RelationshipCollection)
                {
                    relationship.OwnerMap = newMap;
                }

                targetDataSource.Map = newMap;
                result.IsOk = true;
            }
            else
            {
                StringBuilder message = new StringBuilder();

                message.Append($"`{targetDataSource.Title}` problems:");
                message.Append(Environment.NewLine);
                message.Append(matchResult.ErrorMessage);

                result.IsOk = false;
                result.ErrorMessage = message.ToString();
            }

            return result;
        }

        private static void RemoveExtraInnerProperties(MultiPropertyMapModel multiProperty)
        {
            List<PropertyMapModel> extraProperties = new List<PropertyMapModel>();
            if (multiProperty.InnerProperties == null)
                return;

            IEnumerable<IGrouping<string, PropertyMapModel>> groups = multiProperty.InnerProperties.GroupBy(ip => ip.TypeUri);
            foreach (var group in groups)
            {
                List<PropertyMapModel> groupList = group.ToList();
                if (groupList.Count > 1)
                {
                    for (int i = 0; i < groupList.Count - 1; i++)
                    {
                        extraProperties.Add(groupList[i]);
                    }
                }

                if (groupList.LastOrDefault() is MultiPropertyMapModel innerMultiProperty)
                {
                    RemoveExtraInnerProperties(innerMultiProperty);
                }
            }

            multiProperty.InnerProperties = new ObservableCollection<PropertyMapModel>(multiProperty.InnerProperties.Except(extraProperties));
        }

        internal static void LoadPermission(IDataSource dataSource, ACLModel newPermission)
        {
            newPermission.Classification.OwnerACL = newPermission;

            foreach (ACIModel item in newPermission.Permissions)
            {
                item.OwnerACL = newPermission;
            }

            dataSource.Acl = newPermission;
        }

        private static void SetValuesOwnerProperty(PropertyMapModel property)
        {
            if (property is SinglePropertyMapModel singleProperty)
            {
                foreach (ValueMapModel valueMapModel in singleProperty.ValueCollection)
                {
                    valueMapModel.OwnerProperty = property;
                }
            }
            else if (property is MultiPropertyMapModel multiProperty)
            {
                foreach (PropertyMapModel innerProperty in multiProperty.InnerProperties)
                {
                    innerProperty.ParentProperty = multiProperty;
                    SetValuesOwnerProperty(innerProperty);
                }
            }
        }

        private static void PrepareDocumentObjectForLoad(DocumentMapModel document, IDataSource targetDataSource)
        {
            document.Properties.RemoveAt(0);

            if (targetDataSource is UnstructuredDataSourceModel)
            {
                ObjectMapModel documentNameProperty = targetDataSource.Map.ObjectCollection.FirstOrDefault();
                document.Path = ((DocumentMapModel)documentNameProperty)?.Path;
            }
        }

        public static OperationResultModel CheckMapMatchingWithDataSource(MapModel newMap, IDataSource targetDataSource)
        {
            return IsMapMatchWithDataSource(newMap, targetDataSource);
        }

        private static OperationResultModel IsMapMatchWithDataSource(MapModel mapModel, IDataSource targetDataSource)
        {
            if (mapModel.ObjectCollection.Count == 0)
                throw new Exception();

            List<ValueMapModel> tabularValues = new List<ValueMapModel>();
            List<ValueMapModel> metaDataValues = new List<ValueMapModel>();
            List<ValueMapModel> pathValues = new List<ValueMapModel>();
            List<ValueMapModel> constValues = new List<ValueMapModel>();

            foreach (ObjectMapModel objectMapModel in mapModel.ObjectCollection)
            {
                foreach (PropertyMapModel property in objectMapModel.Properties)
                {
                    GetValuesWithSpecificFieldType(property, FieldType.Tabular, ref tabularValues);
                    GetValuesWithSpecificFieldType(property, FieldType.MetaData, ref metaDataValues);
                    GetValuesWithSpecificFieldType(property, FieldType.Path, ref pathValues);
                    GetValuesWithSpecificFieldType(property, FieldType.Const, ref constValues);
                }
            }

            OperationResultModel tabularValuesMatchingResult = CheckPropertiesValueMatching(tabularValues,
                targetDataSource.FieldCollection.Where(x => x.Type == FieldType.Tabular));

            OperationResultModel metadataValuesMatchingResult = CheckPropertiesValueMatching(metaDataValues,
                targetDataSource.FieldCollection.Where(x => x.Type == FieldType.MetaData));

            OperationResultModel pathValuesMatchingResult = CheckPropertiesValueMatching(pathValues,
                targetDataSource.FieldCollection.Where(x => x.Type == FieldType.Path));

            foreach (ValueMapModel constValue in constValues)
            {
                constValue.Field.SampleValue = constValue.SampleValue;
            }

            List<ValueMatchingResult> matchingResults = new List<ValueMatchingResult>
            {
                new ValueMatchingResult
                {
                    FieldType = FieldType.Tabular,
                    OperationResultResult = tabularValuesMatchingResult
                },
                new ValueMatchingResult
                {
                    FieldType = FieldType.MetaData,
                    OperationResultResult = metadataValuesMatchingResult
                },
                new ValueMatchingResult
                {
                    FieldType = FieldType.Path,
                    OperationResultResult = pathValuesMatchingResult
                }
            };

            return CreateFinalMatchingResult(matchingResults);
        }

        private static OperationResultModel CreateFinalMatchingResult(List<ValueMatchingResult> results)
        {
            OperationResultModel finalResult = new OperationResultModel
            {
                IsOk = results.All(x => x.OperationResultResult.IsOk)
            };

            if (!finalResult.IsOk)
            {
                StringBuilder errorMessage = new StringBuilder();

                foreach (ValueMatchingResult matchingResult in results.Where(x => x.OperationResultResult.IsOk == false))
                {
                    switch (matchingResult.FieldType)
                    {
                        case FieldType.Tabular:
                            errorMessage.Append(
                                $"These columns not found in this data source:\n {matchingResult.OperationResultResult.ErrorMessage}");
                            break;
                        case FieldType.MetaData:
                            errorMessage.Append(
                                $"\nThe data source metadata does not match the selected mapping:\n " +
                                $"{matchingResult.OperationResultResult.ErrorMessage}");
                            break;
                        case FieldType.Path:
                            errorMessage.Append(
                                $"These data source:\n {matchingResult.OperationResultResult.ErrorMessage}");
                            break;
                    }
                }

                finalResult.ErrorMessage = errorMessage.ToString();
            }

            return finalResult;
        }

        /// <summary>
        /// پیدا کردن مقادیر با نوع خاص در یک ویژگی
        /// </summary>
        /// <param name="property">ویژگی موزد نظر</param>
        /// <param name="fieldType">نوع مقادیر</param>
        /// <param name="valueList">لیستی که می‌خواهیم مقادیر پیدا شده به آن اضافه شوند</param>
        private static void GetValuesWithSpecificFieldType(PropertyMapModel property, FieldType fieldType,
            ref List<ValueMapModel> valueList)
        {
            if (property is SinglePropertyMapModel singleProperty)
            {
                valueList.AddRange(singleProperty.ValueCollection.Where(x => x.Field.Type == fieldType));
            }
            else if (property is MultiPropertyMapModel multiProperty)
            {
                foreach (PropertyMapModel innerProperty in multiProperty.InnerProperties)
                {
                    GetValuesWithSpecificFieldType(innerProperty, fieldType, ref valueList);
                }
            }
        }

        /// <summary>
        /// بررسی تطابق نوع مقادیر با نوع مقادیر قابل قبول برای منبع داده جاری
        /// </summary>
        /// <param name="loadedValues">مقادیر بارگزاری شده</param>
        /// <param name="dataSourceFields">نوع مقادیر منبع داده جاری</param>
        /// <returns></returns>
        private static OperationResultModel CheckPropertiesValueMatching(List<ValueMapModel> loadedValues,
            IEnumerable<DataSourceFieldModel> dataSourceFields)
        {
            IList<string> mismatchFieldTypesTitle = new List<string>();

            foreach (ValueMapModel property in loadedValues)
            {
                bool found = false;

                foreach (DataSourceFieldModel fieldModel in dataSourceFields.Where(fieldModel => property.Field.Title.Equals(fieldModel.Title)))
                {
                    property.Field = fieldModel;
                    found = true;
                }

                if (!found)
                {
                    mismatchFieldTypesTitle.Add(property.Field.Title);
                }
            }

            OperationResultModel result = new OperationResultModel();

            if (mismatchFieldTypesTitle.Count == 0)
            {
                result.IsOk = true;
                result.ErrorMessage = string.Empty;
            }
            else
            {
                result.IsOk = false;
                result.ErrorMessage = string.Join(",", mismatchFieldTypesTitle);
            }

            return result;
        }

        private struct ValueMatchingResult
        {
            public OperationResultModel OperationResultResult;
            public FieldType FieldType;
        }

        /// <summary>
        /// بررسی اینکه آیا نگاشت انتخاب شده برای بارگذاری با هستان‌شناسی موجود
        /// مطابقت دارد یا نه
        /// </summary>
        /// <param name="mapModel">نگاشت انتخاب شده</param>
        /// <returns></returns>
        public static OperationResultModel IsMapMatchWithOntology(MapModel mapModel)
        {
            OperationResultModel result = new OperationResultModel();

            List<string> mismatchMapObjectTypeUris = new List<string>();
            List<string> mismatchMapRelationshipTypeUris = new List<string>();
            List<string> mismatchMapPropertyTypeUris = new List<string>();

            foreach (ObjectMapModel objectMapModel in mapModel.ObjectCollection)
            {
                if (!OntologyProvider.GetOntology().IsObject(objectMapModel.TypeUri))
                    mismatchMapObjectTypeUris.Add(objectMapModel.TypeUri);
            }

            foreach (RelationshipMapModel relationshipMapModel in mapModel.RelationshipCollection)
            {
                if (!OntologyProvider.GetOntology().IsRelationship(relationshipMapModel.TypeUri))
                    mismatchMapRelationshipTypeUris.Add(relationshipMapModel.TypeUri);
            }

            foreach (ObjectMapModel objectMapModel in mapModel.ObjectCollection)
            {
                foreach (PropertyMapModel propertyMapModel in objectMapModel.Properties)
                {
                    if (!OntologyProvider.GetOntology().IsProperty(propertyMapModel.TypeUri))
                        mismatchMapPropertyTypeUris.Add(propertyMapModel.TypeUri);
                }
            }

            mismatchMapObjectTypeUris = mismatchMapObjectTypeUris.Distinct().ToList();
            mismatchMapRelationshipTypeUris = mismatchMapRelationshipTypeUris.Distinct().ToList();
            mismatchMapPropertyTypeUris = mismatchMapPropertyTypeUris.Distinct().ToList();

            StringBuilder mismatchTypesDescription = new StringBuilder(mismatchMapObjectTypeUris.Any()
                ? string.Format(Properties.Resources.Mismatch_Object_types_0,
                    SeparateTypesByComma(mismatchMapObjectTypeUris))
                : string.Empty);

            if (mismatchTypesDescription.Length != 0)
                mismatchTypesDescription.Append(Environment.NewLine);

            mismatchTypesDescription.Append(mismatchMapPropertyTypeUris.Any()
                ? string.Format(Properties.Resources.Mismatch_Property_types_0,
                    SeparateTypesByComma(mismatchMapPropertyTypeUris))
                : string.Empty);

            if (mismatchMapPropertyTypeUris.Count != 0)
                mismatchTypesDescription.Append(Environment.NewLine);

            mismatchTypesDescription.Append(mismatchMapRelationshipTypeUris.Any()
                ? string.Format(Properties.Resources.Mismatch_Relationship_types_0,
                    SeparateTypesByComma(mismatchMapRelationshipTypeUris))
                : string.Empty);

            if (mismatchTypesDescription.Length != 0)
            {
                result.IsOk = false;
                result.ErrorMessage = !mismatchTypesDescription.ToString().Equals(string.Empty)
                    ? $"{Properties.Resources.Selected_Map_Is_Not_Match_With_Current_Ontology_}" +
                      $"{Environment.NewLine}{mismatchTypesDescription}" : string.Empty;
            }
            else
            {
                result.IsOk = true;
                result.ErrorMessage = string.Empty;
            }

            return result;
        }

        private static string SeparateTypesByComma(IEnumerable<string> typeUris)
        {
            string result = typeUris.Aggregate(string.Empty, (current, typeUri) =>
                current + $" \"{OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(typeUri)}\",");

            if (!result.Equals(string.Empty))
                result = result.TrimEnd(',');

            return result;
        }

        public static bool TabularDataSourceCanImportWorkSpaceSide(ITabularDataSource tabularDataSource)
        {
            if (tabularDataSource == null)
                return false;

            return TabularDataSourceRowCountCanImportWorkSpaceSide(tabularDataSource) && TabularDataSourceFileSizCanImportWorkSpaceSide(tabularDataSource);
        }

        public static bool TabularDataSourceRowCountCanImportWorkSpaceSide(ITabularDataSource tabularDataSource)
        {
            if (tabularDataSource == null)
                return false;

            long dsRowCount = (long)tabularDataSource.RowCountMetaDataItem.Value;
            int mapObjectCount = 0;
            if (tabularDataSource.Map != null && tabularDataSource.Map.ObjectCollection != null)
                mapObjectCount = tabularDataSource.Map.ObjectCollection.Count;

            if (!tabularDataSource.RowCountMetaDataItem.NeedsRecalculation &&
                dsRowCount * mapObjectCount <= Properties.Settings.Default.WorkspaceSideImport_MaximumImportingObjects)
            {
                return true;
            }

            return false;
        }

        public static bool TabularDataSourceFileSizCanImportWorkSpaceSide(ITabularDataSource tabularDataSource)
        {
            if (tabularDataSource == null)
                return false;

            long maxFileSize = Properties.Settings.Default.WorkspaceSideImport_MaximumSemiStructuredImportingFilesSizeInMegaBytes * 1024 * 1024;
            long fileSize = 0;
            if (tabularDataSource is ISingleDataSource singleDataSource)
                fileSize = singleDataSource.FileInfo.Size;
            else if (tabularDataSource is IGroupDataSource groupDataSource)
                fileSize = groupDataSource.GetAllDataSourcesFileSize();
            else
                throw new NotSupportedException();

            return fileSize <= maxFileSize;
        }

        /// <summary>
        /// بررسی می‌کند که آیا منابع داده ورودی با گروه انتخاب شده سازگار هستند یا نه
        /// </summary>
        /// <param name="dataSources">منابع ورودی</param>
        /// <param name="tabularDataSource">گروه انتخاب شده</param>
        /// <returns>لیست منابع داده سازگار</returns>
        public static IEnumerable<ITabularDataSource> GetMatchDataSourcesWithTabularDataSource(IEnumerable<IDataSource> dataSources, ITabularDataSource tabularDataSource)
        {
            List<ITabularDataSource> matchedDataSources = new List<ITabularDataSource>();

            if (tabularDataSource.Preview == null)
                return dataSources.OfType<ITabularDataSource>();

            DataTable groupPreview = tabularDataSource.Preview;

            foreach (ITabularDataSource tabularDataSource2 in dataSources.OfType<ITabularDataSource>())
            {
                if (IsMatchTables(tabularDataSource.Preview, tabularDataSource2.Preview))
                    matchedDataSources.Add(tabularDataSource2);
            }

            return matchedDataSources;
        }

        public static bool IsMatchTables(DataTable dataTable1, DataTable dataTable2)
        {
            if (dataTable1 == null || dataTable2 == null)
                return false;

            if (dataTable1.Columns.Count != dataTable2.Columns.Count)
                return false;

            for (int i = 0; i < dataTable1.Columns.Count; i++)
            {
                if (!dataTable1.Columns[i].ColumnName.Equals(dataTable2.Columns[i].ColumnName))
                    return false;
            }

            return true;
        }
    }
}
