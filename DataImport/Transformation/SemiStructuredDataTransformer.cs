using GPAS.DataImport.ConceptsToGenerate;
using GPAS.DataImport.DataMapping;
using GPAS.DataImport.DataMapping.SemiStructured;
using GPAS.DataImport.InternalResolve;
using GPAS.DataImport.InternalResolve.InterTypeResolve;
using GPAS.Dispatch.Entities.Concepts.Geo;
using GPAS.Logger;
using GPAS.PropertiesValidation;
using GPAS.PropertiesValidation.Geo.Formats;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GPAS.DataImport.Transformation
{
    public class SemiStructuredDataTransformer
    {
        ProcessLogger Logger;

        string[][] DataSourceFields;
        TypeMapping ImportMapping;
        ObjectMapping[] ObjectsMappingArray;
        RelationshipMapping[] RelationshipsMappingArray;
        Dictionary<ObjectMapping, bool> ObjectsMappingNonResolvablityDictionary;
        HashSet<ObjectMapping> FullyConstValueObjectMappings;
        HashSet<RelationshipMapping> RelationshipMappingsWithFullyConstValueObjectEnds;
        ExtractedConcepts TotalExtractedConcepts;
        Dictionary<int, int> ConvertedColumnIndexMappedToBaseDataSourceColumnIndex = new Dictionary<int, int>();
        int MappingLastRequiredColumnIndex;
        private bool ReportFullDetails = true;
        int DataSourceColumnsCount;
        private int MinimumIntervalBetwweenIterrativeLogsReportInSeconds;
        Ontology.Ontology CurrentOntology;

        private int GetMappingLastRequiredColumnIndex()
        {
            int mappingLastRequiredColumnIndex = 0;
            for (int i = 0; i < ObjectsMappingArray.Length; i++)
            {
                ObjectMapping objectMapping = ObjectsMappingArray[i];
                foreach (PropertyMapping property in objectMapping.Properties)
                {
                    if (property.Value is TableColumnMappingItem)
                    {
                        IncreaseLastRequiredColumnIndexByValueMapping(ref mappingLastRequiredColumnIndex, (property.Value as TableColumnMappingItem));
                    }
                    else if (property.Value is MultiValueMappingItem)
                    {
                        foreach (SingleValueMappingItem subValue in (property.Value as MultiValueMappingItem).MultiValues)
                        {
                            IncreaseLastRequiredColumnIndexByValueMapping(ref mappingLastRequiredColumnIndex, subValue);
                        }
                    }
                    else if (property.Value is GeoTimeValueMappingItem)
                    {
                        IncreaseLastRequiredColumnIndexByValueMapping(ref mappingLastRequiredColumnIndex, (property.Value as GeoTimeValueMappingItem).Latitude);
                        IncreaseLastRequiredColumnIndexByValueMapping(ref mappingLastRequiredColumnIndex, (property.Value as GeoTimeValueMappingItem).Longitude);
                        IncreaseLastRequiredColumnIndexByValueMapping(ref mappingLastRequiredColumnIndex, (property.Value as GeoTimeValueMappingItem).TimeBegin);
                        IncreaseLastRequiredColumnIndexByValueMapping(ref mappingLastRequiredColumnIndex, (property.Value as GeoTimeValueMappingItem).TimeEnd);
                    }
                    else if (property.Value is ConstValueMappingItem)
                    {
                        if (property.Value is PathPartMappingItem)
                        {

                        }
                    }
                    else
                    {
                        throw new NotSupportedException("Unknown value-mapping item type");
                    }
                }
            }
            return mappingLastRequiredColumnIndex;
        }

        private void IncreaseLastRequiredColumnIndexByValueMapping(ref int lastRequiredColumnIndex, SingleValueMappingItem valueMapping)
        {
            if (valueMapping is TableColumnMappingItem)
            {
                lastRequiredColumnIndex = Math.Max(lastRequiredColumnIndex, (valueMapping as TableColumnMappingItem).ColumnIndex);
            }
        }

        public SemiStructuredDataTransformer(Ontology.Ontology ontology, ProcessLogger logger = null)
        {
            Logger = logger;
            CurrentOntology = ontology;
        }

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

        private long totalInvalidRowsCount = 0;
        public long InvalidRowsCount
        {
            get
            {
                CheckImportToBePerform();
                return totalInvalidRowsCount;
            }
        }

        private Dictionary<ObjectMapping, ImportingObjectsCollection> generatingObjectsPerMapping = new Dictionary<ObjectMapping, ImportingObjectsCollection>();
        private Dictionary<string, ImportingObjectsCollection> generatingObjectsPerTypeURIs = new Dictionary<string, ImportingObjectsCollection>();

        private bool importPerformed = false;
        private const string DocumentsPathFakeTypeUriAlternative = "___document_path___";

        private void CheckImportToBePerform()
        {
            if (!importPerformed)
                throw new InvalidOperationException("Import may perform first!");
        }

        public void TransformConcepts(string excelFilePath, string sheetName, TypeMapping importMapping)
        {
            Utility util = new Utility();
            SaveLog("Reading Excel worksheet fileds ...");
            string[][] dataSourceFields = util.GetParsableFieldsFromExcelSheet(excelFilePath, sheetName);
            SaveLog("Excel worksheet read completed");
            TransformConcepts(ref dataSourceFields, importMapping);
        }
        public void TransformConceptsFromAccessTable(string accessFilePath, string accessTable, TypeMapping importMapping)
        {
            Utility util = new Utility();
            SaveLog("Reading Access worksheet fileds ...");
            string[][] dataSourceFields = util.GetParsableFieldsFromAccessTable(accessFilePath, accessTable);
            SaveLog("Access worksheet read completed");
            TransformConcepts(ref dataSourceFields, importMapping);
        }

        public void TransformConcepts(DataTable sourceTable, TypeMapping importMapping)
        {
            Utility util = new Utility();
            SaveLog("Reading table fileds ...");
            string[][] dataSourceFields = util.GetFieldsFromTable(sourceTable);
            SaveLog("Table read completed");
            TransformConcepts(ref dataSourceFields, importMapping);
        }

        public void TransformConcepts(Stream sourceCsvStream, char separator, TypeMapping importMapping)
        {
            Utility util = new Utility();
            SaveLog("Parsing CSV file ...");
            string[][] dataSourceFields = util.GetParsableFieldsFromCsvContentStream(sourceCsvStream, separator, Logger);
            SaveLog("CSV file parsed");
            TransformConcepts(ref dataSourceFields, importMapping);
        }

        public IEnumerable<ImportingObject> GetGeneratingObjectsForMapping(ObjectMapping objectMapping)
        {
            if (objectMapping == null)
            {
                throw new ArgumentNullException(nameof(objectMapping));
            }
            if (!ImportMapping.ObjectsMapping.Any(om => om.ID.Equals(objectMapping.ID)))
            {
                throw new ArgumentException("Object mapping not presented in the transformer maps");
            }

            if (ImportMapping.InterTypeAutoInternalResolution)
            {
                return (generatingObjectsPerTypeURIs[objectMapping.ObjectType.TypeUri]).GetObjects();
            }
            else
            {
                return generatingObjectsPerMapping.Single(ot => ot.Key.ID.Equals(objectMapping.ID)).Value.GetObjects();
            }
        }

        /// <summary></summary>
        /// <param name="dataSourceFields">
        /// داده‌های این آرایه جهت حفظ حافظه، حذف می‌شوند و پس از فراخوانی تابع، مقدار قبل از فراخوانی آن را نخواهند داشت؛
        /// اولین ردیف این داده‌ها، ردیف «عنوان ستون‌ها» در نظر گرفته می‌شود
        /// </param>
        public void TransformConcepts(ref string[][] dataSourceFields, TypeMapping importMapping)
        {
            if (dataSourceFields == null)
                throw new ArgumentNullException("dataSourceFields");
            if (importMapping == null)
                throw new ArgumentNullException("importMapping");

            ValidateTypeMapping(importMapping);

            generatingObjects = new List<ImportingObject>();
            generatingRelationships = new List<ImportingRelationship>();
            totalInvalidRowsCount = 0;
            ReportFullDetails = bool.Parse(ConfigurationManager.AppSettings["ReportFullDetailsInImportLog"]);
            MinimumIntervalBetwweenIterrativeLogsReportInSeconds = int.Parse(ConfigurationManager.AppSettings["MinimumIntervalBetwweenIterrativeLogsReportInSeconds"]);

            DataSourceFields = dataSourceFields;
            // به خاطر امکان نغییر نگاشت در این کلاس و تاثیر ناخواسته بر فراخواننده ی آن
            // از نگاشت کپی گرفته می شود
            ImportMapping = importMapping.Copy();
            // توابع زیر برای حفظ کارایی و عدم پردازش تکراری، اینجا پیش‌پردازش‌ها را انجام می‌دهند
            ObjectsMappingArray = ImportMapping.ObjectsMapping.ToArray();
            RelationshipsMappingArray = ImportMapping.RelationshipsMapping.ToArray();
            // TODO: Remove unused columns from 'Data Source fields' and reindex Mappings
            ApplyConversationsOnDataSourceFields();
            // TODO: Remove unused columns ... after conversations
            FillObjectsMappingNonResolvablityDictionary();
            FillFullyConstValueObjectMappingsHashSet();
            MappingLastRequiredColumnIndex = GetMappingLastRequiredColumnIndex();

            FillIRConceptListsUsingInputMaterial();
            SaveLog(string.Format
                ("Extracted Objects: {0:N0} | Extracted Relationships: {1:N0}"
                , TotalExtractedConcepts.IRObjectsPerMappings.Sum(i => i.Value.TotalAddedObjectsCount), TotalExtractedConcepts.IRRelationshipsPerMappings.Sum(i => i.Value.Count)));

            // حافظه‌ی مورد استفاده توسط آرایه آزاد می‌شود؛ این کار جهت استفاده بهینه از فضا
            // در مرحله‌ی بعد که اشیاء در حافظه ایجاد می‌شوند صورت می‌گیرد
            Array.Resize(ref dataSourceFields, 0);
            GC.Collect();
            SaveLog("Memory optimization performed.");

            SaveLog(string.Format("Finalizing Internal Resolution (assign Objects to Relationships ends) \"{0}\" inter-type auto resolution option ...", (importMapping.InterTypeAutoInternalResolution) ? "with" : "without"));

            if (ImportMapping.InterTypeAutoInternalResolution == true)
            {
                GenerateConceptsWithInterTypeAutoInternalResolution();
            }
            else
            {
                GenerateConceptsWithoutInterTypeAutoInternalResolution();
            }

            SaveLog(string.Format("Internal resolution completed; {0:N0} Objects with totally {1:N0} Properties and {2:N0} Relationships are condidate to be generate.", generatingObjects.Count, generatingObjects.Sum(go => go.GetProperties().Count()), generatingRelationships.Count));
            importPerformed = true;
        }

        private void ValidateTypeMapping(TypeMapping importMapping)
        {
            if (!importMapping.IsMappingValid())
            {
                throw new InvalidOperationException("Invalid import type mapping; Mapping may contains atleast one object mapping and one of properties per each object mappings may defined as display name");
            }
            foreach (ObjectMapping objMapping in importMapping.ObjectsMapping)
            {
                if (objMapping is DocumentMapping)
                {
                    foreach (PropertyMapping propMapping in objMapping.Properties)
                    {
                        if (propMapping.PropertyType.TypeUri.Equals(DocumentsPathFakeTypeUriAlternative))
                        {
                            throw new InvalidOperationException($"Unable to use a property of type '{DocumentsPathFakeTypeUriAlternative}' for document mapping");
                        }
                    }
                }
            }
        }

        private void ApplyConversationsOnDataSourceFields()
        {
            // ApplyGeoConversations
            ApplyDefinedRegularExpressionsOnDataSourceFields();
            ApplyDefinedGeoSpecialConversaionsOnDataSourceFields();
        }

        private void ApplyDefinedGeoSpecialConversaionsOnDataSourceFields()
        {
            foreach (ObjectMapping objMap in ObjectsMappingArray)
            {
                foreach (GeoTimePropertyMapping propMap in objMap.Properties.Where(pm => pm is GeoTimePropertyMapping))
                {
                    ApplyGeoConvertAndAddToNewColumn(propMap);
                }
            }
        }

        private void ApplyGeoConvertAndAddToNewColumn(GeoTimePropertyMapping propMap)
        {
            string failedRowNumbers = string.Empty;
            GeoTimeValueMappingItem geoTimeValueMapping = (GeoTimeValueMappingItem)propMap.Value;

            int processedLatitudeFieldIndex = DataSourceFields[0].Length;
            int processedLongitudeFieldIndex = DataSourceFields[0].Length + 1;
            int rowsNewSize = DataSourceFields[0].Length + 2;
            // Add column header
            AppendValueMappingConvertedColumnToDataSourceHeader(geoTimeValueMapping.Latitude, "Lat_converted");
            AppendValueMappingConvertedColumnToDataSourceHeader(geoTimeValueMapping.Longitude, "Long_converted");
            // Add row fields
            GeoSpecialFormats convertor = new GeoSpecialFormats();
            for (int rowNumber = 1; rowNumber < DataSourceFields.Length; rowNumber++)
            {
                string latitudeFieldRawValue = GetPropertyValue(geoTimeValueMapping.Latitude, ref DataSourceFields[rowNumber]);
                string latConvertedValue = string.Empty;
                double? latConvertedDoubleValue = null;

                string longitudeFieldRawValue = GetPropertyValue(geoTimeValueMapping.Longitude, ref DataSourceFields[rowNumber]);
                string longConvertedValue = string.Empty;
                double? longConvertedDoubleValue = null;

                if (convertor.GeoSpecialConvertor(latitudeFieldRawValue, GeoComponentType.Latitude, propMap.GeoSpecialFormat, out latConvertedDoubleValue)
                    && convertor.GeoSpecialConvertor(longitudeFieldRawValue, GeoComponentType.Longitude, propMap.GeoSpecialFormat, out longConvertedDoubleValue))
                {
                    latConvertedValue = latConvertedDoubleValue.Value.ToString(CultureInfo.InvariantCulture);
                    longConvertedValue = longConvertedDoubleValue.Value.ToString(CultureInfo.InvariantCulture);
                }
                else
                {
                    failedRowNumbers = string.Concat(failedRowNumbers, string.Format("\t{0}", rowNumber));
                }

                Array.Resize(ref DataSourceFields[rowNumber], rowsNewSize);
                DataSourceFields[rowNumber][processedLatitudeFieldIndex] = latConvertedValue;
                DataSourceFields[rowNumber][processedLongitudeFieldIndex] = longConvertedValue;
            }
            if (!string.IsNullOrWhiteSpace(failedRowNumbers))
            {
                SaveLog(string.Format("Unable to convert geo-special data on for rows:{0}.", failedRowNumbers));
            }

            UpdatePropertyMappingByNewlyAddedColumn(geoTimeValueMapping, processedLatitudeFieldIndex, processedLongitudeFieldIndex);

            GC.Collect();
        }

        private void AppendValueMappingConvertedColumnToDataSourceHeader(SingleValueMappingItem valueMapping, string columnHeader)
        {
            if (valueMapping is ConstValueMappingItem)
            {
                AppendConvertedColumnToDataSourceHeader(-1, columnHeader);
            }
            else if (valueMapping is TableColumnMappingItem)
            {
                AppendConvertedColumnToDataSourceHeader(((TableColumnMappingItem)valueMapping).ColumnIndex, columnHeader);
            }
            else
            {
                throw new NotSupportedException("Unknown value-mapping item type");
            }
        }

        private static void UpdatePropertyMappingByNewlyAddedColumn(GeoTimeValueMappingItem geoTimeValueMap, int latitudeFieldIndex, int longitudeFieldIndex)
        {
            geoTimeValueMap.Latitude = new TableColumnMappingItem(latitudeFieldIndex, "Lat_converted", geoTimeValueMap.ResolutionOption);
            geoTimeValueMap.Longitude = new TableColumnMappingItem(longitudeFieldIndex, "Long_converted", geoTimeValueMap.ResolutionOption);
        }

        private void ApplyDefinedRegularExpressionsOnDataSourceFields()
        {
            foreach (ObjectMapping objMap in ObjectsMappingArray)
            {
                foreach (PropertyMapping propMap in objMap.Properties)
                {
                    if (propMap.Value is TableColumnMappingItem)
                    {
                        ApplyRegExIfNeededAndAddToNewColumn(propMap.Value as TableColumnMappingItem);
                    }
                    else if (propMap.Value is MultiValueMappingItem)
                    {
                        foreach (SingleValueMappingItem subValue in (propMap.Value as MultiValueMappingItem).MultiValues)
                        {
                            ApplyRegExIfNeededAndAddToNewColumn(subValue);
                        }
                    }
                    else if (propMap.Value is GeoTimeValueMappingItem)
                    {
                        ApplyRegExIfNeededAndAddToNewColumn((propMap.Value as GeoTimeValueMappingItem).Latitude);
                        ApplyRegExIfNeededAndAddToNewColumn((propMap.Value as GeoTimeValueMappingItem).Longitude);
                        ApplyRegExIfNeededAndAddToNewColumn((propMap.Value as GeoTimeValueMappingItem).TimeBegin);
                        ApplyRegExIfNeededAndAddToNewColumn((propMap.Value as GeoTimeValueMappingItem).TimeEnd);
                    }
                }
            }
        }

        private void ApplyRegExIfNeededAndAddToNewColumn(SingleValueMappingItem valueMappingItem)
        {
            if (valueMappingItem is TableColumnMappingItem)
            {
                TableColumnMappingItem tableColumnMappingItem = (TableColumnMappingItem)valueMappingItem;
                if (!string.IsNullOrWhiteSpace(tableColumnMappingItem.RegularExpressionPattern))
                {
                    RegexOptions options = RegexOptions.Compiled;
                    Regex regex = new Regex(tableColumnMappingItem.RegularExpressionPattern, options);
                    string failedRowNumbers = string.Empty;

                    // Add column header
                    AppendConvertedColumnToDataSourceHeader(tableColumnMappingItem.ColumnIndex, GetPropertyValue(tableColumnMappingItem, ref DataSourceFields[0]) + "_RegEx_Applied");
                    int processedFieldIndex = DataSourceFields[0].Length - 1;
                    int rowsNewSize = DataSourceFields[0].Length;
                    // Add row fields
                    for (int rowNumber = 1; rowNumber < DataSourceFields.Length; rowNumber++)
                    {
                        string fieldRowData = GetPropertyValue(tableColumnMappingItem, ref DataSourceFields[rowNumber]);
                        MatchCollection regexResult = regex.Matches(fieldRowData);
                        string fieldProccessedData;
                        if (regexResult.Count == 0)
                        {
                            fieldProccessedData = string.Empty;
                            failedRowNumbers = string.Concat(failedRowNumbers, string.Format("\t{0}", rowNumber));
                        }
                        else
                        {
                            fieldProccessedData = regexResult[0].Value;
                        }
                        Array.Resize(ref DataSourceFields[rowNumber], rowsNewSize);
                        DataSourceFields[rowNumber][processedFieldIndex] = fieldProccessedData;
                    }
                    if (!string.IsNullOrWhiteSpace(failedRowNumbers))
                    {
                        SaveLog(string.Format("Unable to apply regular expression pattern on column '{0}' for rows:{1}."
                            , GetPropertyValue(tableColumnMappingItem, ref DataSourceFields[0]), failedRowNumbers));
                    }

                    tableColumnMappingItem.ColumnIndex = processedFieldIndex;

                    GC.Collect();
                }
            }
        }

        private void AppendConvertedColumnToDataSourceHeader(int conversationSourceColumnIndex, string newColumneHeader)
        {
            int newHeaderIndex = DataSourceFields[0].Length;
            Array.Resize(ref DataSourceFields[0], newHeaderIndex + 1);
            DataSourceFields[0][newHeaderIndex] = newColumneHeader;

            int realSourceColumnIndex = GetRealSourceColumnIndex(conversationSourceColumnIndex);
            ConvertedColumnIndexMappedToBaseDataSourceColumnIndex.Add(newHeaderIndex, realSourceColumnIndex);
        }

        private int GetRealSourceColumnIndex(int checkingColumnIndex)
        {
            if (ConvertedColumnIndexMappedToBaseDataSourceColumnIndex.ContainsKey(checkingColumnIndex))
            {
                return GetRealSourceColumnIndex(ConvertedColumnIndexMappedToBaseDataSourceColumnIndex[checkingColumnIndex]);
            }
            else
            {
                return checkingColumnIndex;
            }
        }

        private void FillObjectsMappingNonResolvablityDictionary()
        {
            ObjectsMappingNonResolvablityDictionary = new Dictionary<ObjectMapping, bool>();
            for (int i = 0; i < ObjectsMappingArray.Length; i++)
            {
                ObjectMapping objectMapping = ObjectsMappingArray[i];
                bool isNonResolvable = true;
                if (objectMapping is DocumentMapping)
                {
                    if ((objectMapping as DocumentMapping).DocumentPathMapping is TableColumnMappingItem)
                    {
                        if (((objectMapping as DocumentMapping).DocumentPathMapping as TableColumnMappingItem).ResolutionOption != PropertyInternalResolutionOption.Ignorable)
                        {
                            isNonResolvable = false;
                            ObjectsMappingNonResolvablityDictionary.Add(objectMapping, isNonResolvable);
                            continue;
                        }
                    }
                    else if ((objectMapping as DocumentMapping).DocumentPathMapping is MultiValueMappingItem)
                    {
                        if (((objectMapping as DocumentMapping).DocumentPathMapping as MultiValueMappingItem).ResolutionOption != PropertyInternalResolutionOption.Ignorable)
                        {
                            isNonResolvable = false;
                            ObjectsMappingNonResolvablityDictionary.Add(objectMapping, isNonResolvable);
                            continue;
                        }
                    }
                }
                foreach (var propertyMapping in objectMapping.Properties.Where(p => p.Value is IResolvableValueMappingItem))
                {
                    if ((propertyMapping.Value as IResolvableValueMappingItem).ResolutionOption != PropertyInternalResolutionOption.Ignorable)
                    {
                        isNonResolvable = false;
                        break;
                    }
                }
                ObjectsMappingNonResolvablityDictionary.Add(objectMapping, isNonResolvable);
            }
        }

        private void FillFullyConstValueObjectMappingsHashSet()
        {
            FullyConstValueObjectMappings = new HashSet<ObjectMapping>();
            for (int i = 0; i < ObjectsMappingArray.Length; i++)
            {
                ObjectMapping objectMapping = ObjectsMappingArray[i];
                if(objectMapping.IsFullyConstMapping())
                {
                    FullyConstValueObjectMappings.Add(objectMapping);
                }
            }
            RelationshipMappingsWithFullyConstValueObjectEnds = new HashSet<RelationshipMapping>();
            for (int i = 0; i < RelationshipsMappingArray.Length; i++)
            {
                RelationshipMapping relMap = RelationshipsMappingArray[i];
                ObjectMapping relMapSource = GetSourceObjectMappingForRelationshipMapping(relMap, ImportMapping);
                ObjectMapping relMapTarget = GetTargetObjectMappingForRelationshipMapping(relMap, ImportMapping);

                if (IsFullyConstObjectMapping(relMapSource)
                 && IsFullyConstObjectMapping(relMapTarget))
                {
                    RelationshipMappingsWithFullyConstValueObjectEnds.Add(relMap);
                }
            }
        }

        private void GenerateConceptsWithoutInterTypeAutoInternalResolution()
        {
            Dictionary<ObjectMapping, IRObjectsCollection> iRObjectsPerMappings = TotalExtractedConcepts.IRObjectsPerMappings;
            Dictionary<RelationshipMapping, HashSet<IRRelationship>> iRRelationshipsPerMappings = TotalExtractedConcepts.IRRelationshipsPerMappings;

            foreach (var iRObjectsPerMapping in iRObjectsPerMappings)
            {
                ImportingObjectsCollection generatedObjectsPerRelationshipEnds
                    = GenerateObjectsAndTheirProperties(iRObjectsPerMapping.Value.GetObjects());
                generatingObjectsPerMapping.Add(iRObjectsPerMapping.Key, generatedObjectsPerRelationshipEnds);
            }

            GenerateRelationshipsUsingGeneratedObjectsPerObjectMappings
                (iRRelationshipsPerMappings, ImportMapping, generatingObjectsPerMapping);
        }

        private void GenerateRelationshipsUsingGeneratedObjectsPerObjectMappings
            (Dictionary<RelationshipMapping, HashSet<IRRelationship>> iRRelationshipsPerMappings
            , TypeMapping importMapping
            , Dictionary<ObjectMapping, ImportingObjectsCollection> generatingObjectsPerMapping)
        {
            foreach (var iRRelationshipsPerMapping in iRRelationshipsPerMappings)
            {
                var currentRelationshipMapping = iRRelationshipsPerMapping.Key;
                var sourceObjectMapping = GetSourceObjectMappingForRelationshipMapping(currentRelationshipMapping, importMapping);
                var targetObjectMapping = GetTargetObjectMappingForRelationshipMapping(currentRelationshipMapping, importMapping);

                var sourceGeneratedObjects
                    = generatingObjectsPerMapping[sourceObjectMapping];
                var targetGeneratedObjects
                    = generatingObjectsPerMapping[targetObjectMapping];
                generatingRelationships.AddRange
                    (GenerateRelationships(iRRelationshipsPerMapping.Value, sourceGeneratedObjects, targetGeneratedObjects));
            }
        }

        private void GenerateConceptsWithInterTypeAutoInternalResolution()
        {
            Dictionary<ObjectMapping, IRObjectsCollection> iRObjectsPerMappings = TotalExtractedConcepts.IRObjectsPerMappings;
            Dictionary<RelationshipMapping, HashSet<IRRelationship>> iRRelationshipsPerMappings = TotalExtractedConcepts.IRRelationshipsPerMappings;

            var iRObjectsPerTypeURIs = new Dictionary<string, IEnumerable<IRObject>>();
            var generatedObjectsPerIRObjects = new Dictionary<IRObject, ImportingObject>();
            foreach (KeyValuePair<string, List<ObjectMapping>> objectMappingsPerTypeUri
                in ImportMapping.GetObjectTypeMappingsByTypeUri())
            {
                string objTypeUri = objectMappingsPerTypeUri.Key;
                List<ObjectMapping> objMappings = objectMappingsPerTypeUri.Value;

                IEnumerable<IRObject> iRObjectsPerCurrentTypeURI;
                if (objMappings.Count == 1)
                {
                    iRObjectsPerCurrentTypeURI =
                        iRObjectsPerMappings[objMappings[0]].GetObjects();
                }
                else
                {
                    SaveLog(string.Format("Start inter-type mapping internal resolution for objects of type: '{0}' ...", objTypeUri));
                    iRObjectsPerCurrentTypeURI =
                        GetIRObjectsPerMultipleMappingsUsingInternalResolution
                            (ref iRObjectsPerMappings, objMappings);
                    SaveLog("Inter-type mapping internal resolution completed.");
                }
                iRObjectsPerTypeURIs.Add(objTypeUri, iRObjectsPerCurrentTypeURI);

                var generatedObjectsPerRelationshipEnd
                    = GenerateObjectsAndTheirProperties(iRObjectsPerCurrentTypeURI);
                generatingObjectsPerTypeURIs
                    .Add(objTypeUri, generatedObjectsPerRelationshipEnd);
            }

            generatingRelationships.AddRange
                (GenerateRelationshipsUsingGeneratedObjectsPerObjectTypeURIs
                    (iRRelationshipsPerMappings, ImportMapping, generatingObjectsPerTypeURIs));
        }

        private List<ImportingRelationship> GenerateRelationshipsUsingGeneratedObjectsPerObjectTypeURIs(Dictionary<RelationshipMapping, HashSet<IRRelationship>> iRRelationshipsPerMappings, TypeMapping importMapping, Dictionary<string, ImportingObjectsCollection> generatingObjectsPerTypeURIs)
        {
            var totalGeneratingRelationships = new List<ImportingRelationship>();
            foreach (var iRRelationshipsPerMapping in iRRelationshipsPerMappings)
            {
                var currentRelationshipMapping = iRRelationshipsPerMapping.Key;
                var sourceObjectMapping = GetSourceObjectMappingForRelationshipMapping(currentRelationshipMapping, importMapping);
                var targetObjectMapping = GetTargetObjectMappingForRelationshipMapping(currentRelationshipMapping, importMapping);

                var sourceGeneratedObjects
                    = generatingObjectsPerTypeURIs[sourceObjectMapping.ObjectType.TypeUri];
                var targetGeneratedObjects
                    = generatingObjectsPerTypeURIs[targetObjectMapping.ObjectType.TypeUri];

                var generatingRelationships = GenerateRelationships(iRRelationshipsPerMapping.Value, sourceGeneratedObjects, targetGeneratedObjects);
                totalGeneratingRelationships.AddRange(generatingRelationships);
            }
            return totalGeneratingRelationships;
        }

        private List<ImportingRelationship> GenerateRelationships(IEnumerable<IRRelationship> iRRelationships, ImportingObjectsCollection sourceGeneratingObjects, ImportingObjectsCollection targetGeneratingObjects)
        {
            var generatingRelationships = new List<ImportingRelationship>();
            foreach (var iRRelationship in iRRelationships)
            {
                ImportingObject source;
                if (!sourceGeneratingObjects.TryGetMatchObject(iRRelationship.SourceEnd, out source))
                    continue;
                ImportingObject target;
                if (!targetGeneratingObjects.TryGetMatchObject(iRRelationship.TargetEnd, out target))
                    continue;
                var generatingRelationship = GenerateImportingRelationship(iRRelationship, source, target);
                generatingRelationships.Add(generatingRelationship);
            }
            return generatingRelationships;
        }

        private IEnumerable<IRObject> GetIRObjectsPerMultipleMappingsUsingInternalResolution
            (ref Dictionary<ObjectMapping, IRObjectsCollection> iRObjectsPerMappings
            , List<ObjectMapping> objectMappingsToResolveOn)
        {
            // Declaring "Must-Match" Properties for inter-type resolution
            var InterObjectMappingsMustMatchPropertiesTypeURI = new HashSet<string>();
            foreach (var objectMapping in objectMappingsToResolveOn)
            {
                var objectMappingMustMatchProperties = objectMapping.Properties
                    .Where(p => p.Value is IResolvableValueMappingItem
                        && ((p.Value as IResolvableValueMappingItem).ResolutionOption == PropertyInternalResolutionOption.FindMatch
                            || (p.Value as IResolvableValueMappingItem).ResolutionOption == PropertyInternalResolutionOption.MustMatch));
                foreach (var propertyMapping in objectMappingMustMatchProperties)
                {
                    if (!InterObjectMappingsMustMatchPropertiesTypeURI.Contains(propertyMapping.PropertyType.TypeUri))
                        InterObjectMappingsMustMatchPropertiesTypeURI.Add(propertyMapping.PropertyType.TypeUri);
                }
            }
            // Performing inter-type resolution
            var overallIRObjectsPerMappings = new ITIRObjectsCollection();

            foreach (IRObject irObj in iRObjectsPerMappings[objectMappingsToResolveOn[0]].GetObjects())
            {
                overallIRObjectsPerMappings.Add(new ITIRObject(irObj, InterObjectMappingsMustMatchPropertiesTypeURI));
            }

            for (int i = 1; i < objectMappingsToResolveOn.Count; i++)
            {
                SaveLog(string.Format("Inter-type resolution on mapping {0} of {1}", i, objectMappingsToResolveOn.Count - 1));
                ObjectMapping objectMapping = objectMappingsToResolveOn[i];
                InterObjectMappingsInternalResolution
                    (ref overallIRObjectsPerMappings
                    , iRObjectsPerMappings[objectMapping]
                    , InterObjectMappingsMustMatchPropertiesTypeURI);
            }
            return overallIRObjectsPerMappings.GetObjects().Select(itirObj => itirObj.BaseObject);
        }

        private ImportingRelationship GenerateImportingRelationship(IRRelationship iRRelationship, ImportingObject source, ImportingObject target)
        {
            return new ImportingRelationship
                (source, target, iRRelationship.TypeURI
                , iRRelationship.Direction
                , null, null, iRRelationship.Description);
        }

        private IRRelationshipDirection ConvertLinkMappingDirectionToIRRelationshipDirection(RelationshipBaseLinkMappingRelationDirection direction)
        {
            switch (direction)
            {
                case RelationshipBaseLinkMappingRelationDirection.PrimaryToSecondary:
                    return IRRelationshipDirection.SourceToTarget;
                case RelationshipBaseLinkMappingRelationDirection.SecondaryToPrimary:
                    return IRRelationshipDirection.TargetToSource;
                case RelationshipBaseLinkMappingRelationDirection.Bidirectional:
                    return IRRelationshipDirection.Bidirectional;
                default:
                    throw new InvalidOperationException("Unknown direction for relationship mapping"); ;
            }
        }

        private ObjectMapping GetTargetObjectMappingForRelationshipMapping(RelationshipMapping relationshipMapping, TypeMapping overallTypeMapping)
        {
            return overallTypeMapping.ObjectsMapping.Single(om => om.ID == relationshipMapping.TargetId);
        }

        private ObjectMapping GetSourceObjectMappingForRelationshipMapping(RelationshipMapping relationshipMapping, TypeMapping overallTypeMapping)
        {
            return overallTypeMapping.ObjectsMapping.Single(om => om.ID == relationshipMapping.SourceId);
        }

        private ImportingObjectsCollection GenerateObjectsAndTheirProperties(IEnumerable<IRObject> iRObjects)
        {
            var result = new ImportingObjectsCollection();
            foreach (var currentIRObject in iRObjects)
            {
                List<ImportingProperty> generatingProperties = new List<ImportingProperty>();
                foreach (var currentIRMMProperty in currentIRObject.MustMatchProperties)
                {
                    var generatingProperty = new ImportingProperty(currentIRMMProperty.TypeURI, currentIRMMProperty.Value);
                    generatingProperties.Add(generatingProperty);
                }
                foreach (var currentIRIProperty in currentIRObject.IgnorableProperties)
                {
                    foreach (var currentValue in currentIRIProperty.Values)
                    {
                        var generatingProperty = new ImportingProperty(currentIRIProperty.TypeURI, currentValue);
                        generatingProperties.Add(generatingProperty);
                    }
                }

                ImportingProperty[] documentPathFakeProperties = generatingProperties.Where(p => p.TypeURI.Equals(DocumentsPathFakeTypeUriAlternative)).ToArray();
                if (documentPathFakeProperties.Length > 1)
                {
                    throw new InvalidOperationException("Document temperory fake property to specify the path, may not generate more than once");
                }

                Ontology.Ontology ontology = new Ontology.Ontology();
                string labelPropertyTypeUri = ontology.GetDefaultDisplayNamePropertyTypeUri();

                ImportingObject generatingObject;
                if (documentPathFakeProperties.Length == 1)
                {
                    ImportingProperty documentPathFakeProperty = documentPathFakeProperties[0];
                    generatingObject = new ImportingDocument(currentIRObject.TypeUri, new ImportingProperty(labelPropertyTypeUri, currentIRObject.DisplayName));
                    (generatingObject as ImportingDocument).DocumentPath = documentPathFakeProperty.Value;
                    generatingProperties.Remove(documentPathFakeProperty);
                }
                else
                {
                    generatingObject = new ImportingObject(currentIRObject.TypeUri, new ImportingProperty(labelPropertyTypeUri, currentIRObject.DisplayName));
                }

                generatingObject.AddPropertyRangeForObject(generatingProperties);

                generatingObjects.Add(generatingObject);

                if (currentIRObject.MustMatchProperties.Length == 0)
                {
                    result.Add(new IRRelationshipObjectBasedEnd(currentIRObject), generatingObject);
                }
                else
                {
                    result.Add(new IRRelationshipPropertyBasedEnd(currentIRObject.MustMatchProperties), generatingObject);
                }
            }
            return result;
        }

        private void InterObjectMappingsInternalResolution(ref ITIRObjectsCollection totalIRObjects, IRObjectsCollection iRObjectsToResolve, HashSet<string> interTypeMustMatchPropertiesTypeURI)
        {
            int currentIRObjectIndex = 0;
            double lastExtractionLogReportedPercentage = 0;
            DateTime lastLogStoreTime = DateTime.MinValue;

            foreach (var currentIRObjectToResolve in iRObjectsToResolve.GetObjects())
            {
                ITIRObject currentNotResolvedObj = new ITIRObject(currentIRObjectToResolve, interTypeMustMatchPropertiesTypeURI);
                ITIRObject matchedResolveObj;
                if (totalIRObjects.TryGetSameITIRObject(currentNotResolvedObj, out matchedResolveObj))
                {
                    IRObject matchedResolveBaseObj = matchedResolveObj.BaseObject;
                    ResolveSameMMPropertiesIRObjects(ref matchedResolveBaseObj, currentNotResolvedObj.BaseObject);
                    totalIRObjects.UpdateBaseObjectChanges(matchedResolveObj, interTypeMustMatchPropertiesTypeURI);
                }
                else
                {
                    totalIRObjects.Add(currentNotResolvedObj);
                }

                double percentages = currentIRObjectIndex * 100 / iRObjectsToResolve.TotalAddedObjectsCount;
                if (lastExtractionLogReportedPercentage != percentages
                    && lastLogStoreTime.AddSeconds(MinimumIntervalBetwweenIterrativeLogsReportInSeconds) <= DateTime.Now)
                {
                    SaveLog(string.Format("Inter-type Internal Resolution for the type mapping: {0:N0}%", percentages));
                    lastLogStoreTime = DateTime.Now;
                    lastExtractionLogReportedPercentage = percentages;
                }

                currentIRObjectIndex++;
            }
        }

        private void ResolveProbablyConflictInIRIProperties(ref List<IRIgnorableProperty> baseIgnorableProperties, IRIgnorableProperty ignorablePropertyToResolveWithBase)
        {
            if (IsIRObjectIPropertiesIncludesOnePropertyWithType(ref baseIgnorableProperties, ignorablePropertyToResolveWithBase.TypeURI))
            {
                int indexOfBaseIRIPropertyWithSameTypeURI = GetIndexOfIgnorablePropertyWithType(ref baseIgnorableProperties, ignorablePropertyToResolveWithBase.TypeURI);
                var existingValues = baseIgnorableProperties[indexOfBaseIRIPropertyWithSameTypeURI].Values;
                var valuesToAdd = new List<string>();
                foreach (var checkingValue in ignorablePropertyToResolveWithBase.Values)
                {
                    if (!existingValues.Contains(checkingValue))
                        valuesToAdd.Add(checkingValue);
                }
                baseIgnorableProperties[indexOfBaseIRIPropertyWithSameTypeURI].Values.AddRange(valuesToAdd);
            }
            else
                baseIgnorableProperties.Add(ignorablePropertyToResolveWithBase);
        }

        private bool IsIRObjectIPropertiesIncludesOnePropertyWithType(ref List<IRIgnorableProperty> properties, string typeURI)
        {
            int count = properties.Where(p => p.TypeURI.Equals(typeURI)).Count();
            if (count > 1)
                throw new InvalidOperationException("There are more than one Ignorable Properties exist in given list");
            else if (count == 1)
                return true;
            else
                return false;
        }

        private int GetIndexOfIgnorablePropertyWithType(ref List<IRIgnorableProperty> properties, string typeURI)
        {
            for (int i = 0; i < properties.Count; i++)
            {
                if (properties[i].TypeURI.Equals(typeURI))
                    return i;
            }
            throw new InvalidOperationException("No item with wanted type");
        }

        private void ResolveProbablyConflictInDisplayName(ref string baseDisplayName, string displayNameToResolveWithBase)
        {
            if (!string.IsNullOrWhiteSpace(displayNameToResolveWithBase))
                baseDisplayName = displayNameToResolveWithBase;
        }

        private void FillIRConceptListsUsingInputMaterial()
        {
            // صرف نظر از خط اول فایل که حاوی عنوان ستون‌های فایل سی.اس.وی می باشد
            if (DataSourceFields.Length < 1)
            {
                throw new ArgumentException("Data source has no content");
            }

            SaveLog(string.Format("Start processing semi-struectured data ..."));

            DataSourceColumnsCount = DataSourceFields[0].Length;
            if (MappingLastRequiredColumnIndex > DataSourceColumnsCount - 1)
            {
                throw new InvalidOperationException("Data source columns are less than required columns for the mapping");
            }

            InitiateExtractedConceptsInstanceByConstMappings();
            ExtractConceptsFromDataRows(1, DataSourceFields.Length - 1);

            SaveLog(string.Format("Semi-struectured data processing completed."));
        }

        private void InitiateExtractedConceptsInstanceByConstMappings()
        {
            TotalExtractedConcepts = new ExtractedConcepts()
            {
                IRObjectsPerMappings = GenerateIRObjectEmptyListsByMapping(),
                IRRelationshipsPerMappings = GenerateIRRelationshipEmptyListsByMapping()
            };

            foreach (ObjectMapping constObjMapping in FullyConstValueObjectMappings)
            {
                GenerateNewIRObjectForFullConstObjectMapping(constObjMapping);
            }
            foreach (RelationshipMapping relMapping in RelationshipMappingsWithFullyConstValueObjectEnds)
            {
                ObjectMapping relSourceObjectMapping = GetSourceObjectMappingForRelationshipMapping(relMapping, ImportMapping);
                ObjectMapping relTargetObjectMapping = GetTargetObjectMappingForRelationshipMapping(relMapping, ImportMapping);
                string relDescription;
                if (relMapping.RelationshipDescription is ConstValueMappingItem)
                {
                    relDescription = (relMapping.RelationshipDescription as ConstValueMappingItem).ConstValue;
                }
                else
                {
                    if (ReportFullDetails)
                        SaveLog("Warning: Unable to set non-Const-Value description to relationships between Full-Const object mappings; Rel. type-uri replaced!");
                    relDescription = relMapping.RelationshipType.TypeUri;
                }
                GenerateIRRelationshipAndAddToExtractedConcepts
                    (relMapping, relDescription
                    , TotalExtractedConcepts.IRObjectsPerMappings[relSourceObjectMapping].GetObjects().Select(o => (IRRelationshipEnd)(new IRRelationshipObjectBasedEnd(o))).ToList()
                    , TotalExtractedConcepts.IRObjectsPerMappings[relTargetObjectMapping].GetObjects().Select(o => (IRRelationshipEnd)(new IRRelationshipObjectBasedEnd(o))).ToList());
            }
        }

        private void GenerateNewIRObjectForFullConstObjectMapping(ObjectMapping fullConstObjectMapping)
        {
            IEnumerable<IGrouping<string, PropertyMapping>> relatedObjectPropertiesPerPropertyTypes
                = (fullConstObjectMapping.Properties.GroupBy(p => p.PropertyType.TypeUri));
            var relatedObjectProperties = new List<IRIgnorableProperty>(fullConstObjectMapping.Properties.Count);
            foreach (IGrouping<string, PropertyMapping> item in relatedObjectPropertiesPerPropertyTypes)
            {
                IRIgnorableProperty ignorableProperty = new IRIgnorableProperty()
                {
                    TypeURI = item.Key,
                    Values = new List<string>()
                };
                foreach (PropertyMapping propertyMapping in item.AsEnumerable())
                {
                    ignorableProperty.Values.Add(GetFullyConstPropertyValue(propertyMapping.Value));
                }
                relatedObjectProperties.Add(ignorableProperty);
            }

            string objectDisplayName = GetFullyConstPropertyValue(fullConstObjectMapping.GetDisplayName());
            Dictionary<ObjectMapping, IRObjectsCollection> tempDict = null;
            if (fullConstObjectMapping is DocumentMapping)
            {
                DocumentMapping documentMapping = fullConstObjectMapping as DocumentMapping;
                string path = GetFullyConstPropertyValue(documentMapping.DocumentPathMapping);
                GenerateIRDocumentAndAddToDictionaries
                    (documentMapping, objectDisplayName, path
                    , new List<IRMustMatchProperty>(0), new List<IRMustMatchProperty>(0), relatedObjectProperties
                    , ref tempDict, ref tempDict);
            }
            else
            {
                GenerateIRObjectAndAddToDictionaries
                    (fullConstObjectMapping, objectDisplayName
                    , new List<IRMustMatchProperty>(0), new List<IRMustMatchProperty>(0), relatedObjectProperties
                    , ref tempDict, ref tempDict);
            }
        }

        private static void CleansupStringArray(string[] fields)
        {
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i] == null)
                {
                    fields[i] = string.Empty;
                    continue;
                }
                //fields[i] = ConvertToUtf8(fields[i]);
                fields[i] = RemoveInvalidXmlCharacters(fields[i]);
            }
        }

        private static string ConvertToUtf8(string text)
        {
            byte[] utf16Bytes = Encoding.Unicode.GetBytes(text);
            byte[] utf8Bytes = Encoding.Convert(Encoding.Unicode, Encoding.UTF8, utf16Bytes);
            return Encoding.UTF8.GetString(utf8Bytes);
        }

        /// <summary>
        /// حذف کاراکترهای غیرمجاز xml را انجام می دهد.
        /// https://stackoverflow.com/questions/730133/invalid-characters-in-xml/5110103#5110103
        /// http://blog.mark-mclaren.info/2007/02/invalid-xml-characters-when-valid-utf8_5873.html
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private static string RemoveInvalidXmlCharacters(string text)
        {
            // From xml spec valid chars: 
            // #x9 | #xA | #xD | [#x20-#xD7FF] | [#xE000-#xFFFD] | [#x10000-#x10FFFF]     
            // any Unicode character, excluding the surrogate blocks, FFFE, and FFFF. 
            text = text.Replace('"', '”');
            string re = @"[^\x09\x0A\x0D\x20-\uD7FF\uE000-\uFFFD\u10000-\u10FFFF]";
            return Regex.Replace(text, re, "");
        }

        private void ExtractConceptsFromDataRows(int dataSourceStartRowIndex, int processingRowsCount)
        {
            if (dataSourceStartRowIndex < 1)
                throw new ArgumentOutOfRangeException("dataSourceStartRowIndex");
            if (processingRowsCount < 1)
                throw new ArgumentOutOfRangeException("processingRowsCount");

            SaveLog(string.Format("Start data extraction from row {0:N0} to {1:N0}"
                    , dataSourceStartRowIndex, dataSourceStartRowIndex + processingRowsCount));

            totalInvalidRowsCount = 0;
            List<KeyValuePair<long, List<int>>> invalidFieldsPerExtractedRows = new List<KeyValuePair<long, List<int>>>();

            double lastExtractionLogReportedPercentage = 0;
            DateTime lastExtractionLogStoreTime = DateTime.MinValue;

            SaveLog("Prepairing Extract and Internal Resolution concepts ...");
            // خواندن بقیه خطوط فایل که شامل داده‌های موردنظر می‌باشد
            for (int rowIndex = dataSourceStartRowIndex; rowIndex < (dataSourceStartRowIndex + processingRowsCount); rowIndex++)
            {
                // خواندن یک خط و تجزیه‌ی آن - براساس کراکتر جداکننده فرهنگ یکسان
                CleansupStringArray(DataSourceFields[rowIndex]);
                //if (ReportFullDetails && DataSourceLines[rowIndex].Length > currentReadLine.Length)
                //{
                //    SaveLog(string.Format("Invalid XML unicode character(s) at row #{0:N0} detected and removed; input: {1} | output: {2}", rowIndex, DataSourceLines[rowIndex], currentReadLine));
                //}
                try
                {
                    string[] currentRowFields = DataSourceFields[rowIndex];

                    // تعداد فیلدها با تعداد ستون‌های منبع داده سازگار نیست
                    if (currentRowFields.Length != DataSourceColumnsCount)
                    {
                        totalInvalidRowsCount++;
                        if (ReportFullDetails)
                        {
                            SaveLog(string.Format("Row #{0:N0} transformation failed | Row fields count not equals Data Source columns count", rowIndex));
                        }
                    }
                    else
                    {
                        for (int i = 0; i < currentRowFields.Length; i++)
                        {
                            currentRowFields[i] = currentRowFields[i].Trim();
                        }
                        // ورود داده‌های این خط براساس نگاشت داده‌شده
                        DataRowFieldsExtractionResult extractionResult = FillIRConceptListsUsingSemiStructuredDataRecord(currentRowFields);
                        if (extractionResult.InvalidFieldsIndex.Count > 0)
                        {
                            invalidFieldsPerExtractedRows.Add(new KeyValuePair<long, List<int>>(rowIndex, extractionResult.InvalidFieldsIndex));
                        }

                        double percentages = (rowIndex - dataSourceStartRowIndex) * 100 / (dataSourceStartRowIndex + processingRowsCount);
                        if (lastExtractionLogReportedPercentage != percentages
                            && lastExtractionLogStoreTime.AddSeconds(MinimumIntervalBetwweenIterrativeLogsReportInSeconds) <= DateTime.Now)
                        {
                            SaveLog(string.Format("Extracting and Internal Resolving concepts: {0:N0}%", percentages));
                            lastExtractionLogStoreTime = DateTime.Now;
                            lastExtractionLogReportedPercentage = percentages;
                        }
                    }
                }
                catch (Exception ex)
                {
                    totalInvalidRowsCount++;
                    ExceptionDetailGenerator detailGenerator = new ExceptionDetailGenerator();
                    string ExceptionDetails = detailGenerator.GetDetails(ex);
                    SaveLog(string.Format("Data extraction failed at row #{0:N0}{1}Exception Details:{1}{2}"
                            , rowIndex.ToString(), Environment.NewLine, ExceptionDetails));
                }
            }
            SaveLog("Extract and Internal Resolution concepts completed.");

            // ثبت گزارش برای هر دسته
            StringBuilder logMessage = new StringBuilder();
            if (totalInvalidRowsCount == 0)
                logMessage.AppendFormat("Data row {0:N0} to {1:N0} are extracted and internally resolved successfully"
                    , dataSourceStartRowIndex, dataSourceStartRowIndex + processingRowsCount);
            else if (totalInvalidRowsCount == processingRowsCount)
                logMessage.AppendFormat("Data row {0:N0} to {1:N0} extraction and/or internal resolution failed"
                    , dataSourceStartRowIndex, dataSourceStartRowIndex + processingRowsCount);
            else
            {
                logMessage.AppendFormat("Data row {0:N0} to {1:N0} are extracted and internally resolved with {2} fail(s)"
                    , dataSourceStartRowIndex, dataSourceStartRowIndex + processingRowsCount, totalInvalidRowsCount);
            }

            if (invalidFieldsPerExtractedRows.Count > 0)
            {
                logMessage.AppendFormat("; Total rows with must-match properties where not have valid value: {0:N0}", invalidFieldsPerExtractedRows.Count);
                if (ReportFullDetails)
                {
                    logMessage.AppendFormat("{0}Row#{1}{1}Invalid field(s) index", Environment.NewLine, '\t');
                    logMessage.AppendFormat("{0}----{1}{1}----------------------", Environment.NewLine, '\t');
                    foreach (KeyValuePair<long, List<int>> invalidFieldsPerRow in invalidFieldsPerExtractedRows)
                    {
                        logMessage.AppendFormat("{0}{1:N0}{2}{2}", Environment.NewLine, invalidFieldsPerRow.Key, '\t');
                        foreach (int fieldIndex in invalidFieldsPerRow.Value)
                        {
                            logMessage.AppendFormat("{0}, ", fieldIndex.ToString());
                        }
                        logMessage.Remove(logMessage.Length - 2, 2);
                    }
                }

            }

            SaveLog(logMessage.ToString());

            // کدهای ثبت لاگ و نیز شمارنده‌های ورود دسته‌ای داده‌ها، به خاطر
            // عدم استفاده، فعلا غیر فعال شده‌اند

            //if ((double)totalSuccessfullImportedLines / dataLineCounter * 100 >= PercentOfAcceptableSuccessfulSemistructuredDataImport)
            //{
            //    SaveLog(string.Format("Import completed successfully; Imported data lines {0} of {1}", totalSuccessfullImportedLines, dataLineCounter));
            //}
            //else
            //{
            //    SaveLog(string.Format("Import complete fail, imported lines percents < {0}; Imported data lines {1} of {2}", PercentOfAcceptableSuccessfulSemistructuredDataImport, totalSuccessfullImportedLines, dataLineCounter));
            //}
        }

        private void SaveLog(string logContent, bool storeInusePrivateMemorySize = true)
        {
            if (Logger != null)
                Logger.WriteLog(logContent, storeInusePrivateMemorySize);
        }

        private class DataRowFieldsExtractionResult
        {
            internal List<int> InvalidFieldsIndex = new List<int>();
        }

        // TODO: عملکرد این متد به خاطر پیچیدگی، قابل انتقال به بک کلاس مجزاست

        private DataRowFieldsExtractionResult FillIRConceptListsUsingSemiStructuredDataRecord(string[] dataRow)
        {
            if (dataRow == null)
                throw new ArgumentNullException("dataRow");
            if (dataRow.Length <= 0)
                throw new ArgumentException("No data fields");

            var extractionResult = new DataRowFieldsExtractionResult();

            var RecordMMPropertiesPerNonIgnorableObjectMappings = new Dictionary<ObjectMapping, IRObjectsCollection>();
            var RecordIRObjectPerFullIgnorableObjectMappings = new Dictionary<ObjectMapping, IRObjectsCollection>();

            for (int i = 0; i < ObjectsMappingArray.Length; i++)
            {
                ObjectMapping objectMapping = ObjectsMappingArray[i];

                if (IsFullyConstObjectMapping(objectMapping))
                {
                    // در صورتی که تمام ویژگی‌های شئ از نوع مقدار ثابت باشند، نیازی به
                    // جایگذاری شي در دیکشنری‌ها نیست و از دیکشنری پر شده در ابتدای فرایند
                    // که یکبار برای همیشه پرشده است استفاده خواهد شد
                    continue;
                }

                string objectDisplayName = string.Empty;
                // لیست اول صرفا برای نگهداری ویژگی‌هایی ست که میخواهیم مبنای
                // Resolve
                // داخلی باشند، و محتوای آن همزمان با پر شدن، در لیست دوم نیز
                // افزوده خواهد شد
                var objectFindMatchProperties = new List<IRMustMatchProperty>();
                var objectNonFindMatchMustMatchProperties = new List<IRMustMatchProperty>();
                var objectIgnorableProperties = new List<IRIgnorableProperty>();

                bool hasInvalidProperty = HasObjectInvalidMustMatchProperty(ref dataRow, ref extractionResult, objectMapping);

                foreach (var propertyMapping in objectMapping.Properties)
                {
                    PropertyInternalResolutionOption? resolutionOption = null;

                    if (hasInvalidProperty || (propertyMapping.Value is ConstValueMappingItem))
                    {
                        resolutionOption = PropertyInternalResolutionOption.Ignorable;
                    }
                    else
                    {
                        if (propertyMapping.Value is IResolvableValueMappingItem)
                        {
                            resolutionOption = ((IResolvableValueMappingItem)(propertyMapping.Value)).ResolutionOption;
                        }
                    }

                    string fieldValue = GetPropertyValue(propertyMapping.Value, ref dataRow);

                    AddNewPropertyToApproperiateList(propertyMapping, fieldValue, resolutionOption.Value, ref objectFindMatchProperties, ref objectNonFindMatchMustMatchProperties, ref objectIgnorableProperties);

                    if (propertyMapping.IsSetAsDisplayName)
                    {
                        objectDisplayName = fieldValue;
                    }
                }

                if (IsNonResolvableObjectMapping(objectMapping))
                {
                    RecordIRObjectPerFullIgnorableObjectMappings.Add(objectMapping, new IRObjectsCollection());
                }
                else
                {
                    RecordMMPropertiesPerNonIgnorableObjectMappings.Add(objectMapping, new IRObjectsCollection());
                }

                if (objectMapping is DocumentMapping)
                {
                    DocumentMapping documentMapping = objectMapping as DocumentMapping;
                    string path = GetPropertyValue(documentMapping.DocumentPathMapping, ref dataRow);
                    GenerateIRDocumentAndAddToDictionaries
                        (documentMapping, objectDisplayName, path
                        , objectFindMatchProperties, objectNonFindMatchMustMatchProperties, objectIgnorableProperties
                        , ref RecordMMPropertiesPerNonIgnorableObjectMappings, ref RecordIRObjectPerFullIgnorableObjectMappings);
                }
                else
                {
                    GenerateIRObjectAndAddToDictionaries
                        (objectMapping, objectDisplayName
                        , objectFindMatchProperties, objectNonFindMatchMustMatchProperties, objectIgnorableProperties
                        , ref RecordMMPropertiesPerNonIgnorableObjectMappings, ref RecordIRObjectPerFullIgnorableObjectMappings);
                }
            }
            // TODO: امکان حذف روابط تکراری بین اشیاء (که ممکن است براثر ادغام اشیاء ایجاد شود) افزوده شود
            for (int i = 0; i < RelationshipsMappingArray.Length; i++)
            {
                RelationshipMapping relationshipMap = RelationshipsMappingArray[i];
                if(RelationshipMappingsWithFullyConstValueObjectEnds.Contains(relationshipMap))
                {
                    continue;
                }
                ObjectMapping relationshipSourceObjectMapping = GetSourceObjectMappingForRelationshipMapping(relationshipMap, ImportMapping);
                ObjectMapping relationshipTargetObjectMapping = GetTargetObjectMappingForRelationshipMapping(relationshipMap, ImportMapping);
                
                string description = GetRelatinshipDescription(relationshipMap, dataRow);
                List<IRRelationshipEnd> sourceEnds = GetRelationshipEndFromFilledDictionaries
                    (relationshipSourceObjectMapping, ref RecordMMPropertiesPerNonIgnorableObjectMappings
                    , ref RecordIRObjectPerFullIgnorableObjectMappings);
                List<IRRelationshipEnd> targetEnds = GetRelationshipEndFromFilledDictionaries
                    (relationshipTargetObjectMapping, ref RecordMMPropertiesPerNonIgnorableObjectMappings
                    , ref RecordIRObjectPerFullIgnorableObjectMappings);

                if (sourceEnds == null || sourceEnds.Count == 0
                 || targetEnds == null || targetEnds.Count == 0)
                {
                    // در صورتی که به دلیل معتبر نبودن حداقلی ویژگی‌های
                    // Must-Match
                    // شئی به عنوان مبدا رابطه ایجاد نشده باشد، رابطه‌ای برای استخراج وجود نخواهد داشت
                    continue;
                }

                GenerateIRRelationshipAndAddToExtractedConcepts(relationshipMap, description, sourceEnds, targetEnds);
            }
            return extractionResult;
        }

        private void GenerateIRRelationshipAndAddToExtractedConcepts(RelationshipMapping relationshipMap, string description, List<IRRelationshipEnd> sourceEnds, List<IRRelationshipEnd> targetEnds)
        {
            foreach (IRRelationshipEnd source in sourceEnds)
            {
                foreach (IRRelationshipEnd target in targetEnds)
                {
                    var iRRelationshipPerMapping = new IRRelationship()
                    {
                        TypeURI = relationshipMap.RelationshipType.TypeUri,
                        Direction = ConvertLinkMappingDirectionToIRRelationshipDirection(relationshipMap.RelationshipDirection),
                        Description = description,
                        SourceEnd = source,
                        TargetEnd = target
                    };
                    if (!TotalExtractedConcepts.IRRelationshipsPerMappings[relationshipMap].Contains(iRRelationshipPerMapping))
                    {
                        TotalExtractedConcepts.IRRelationshipsPerMappings[relationshipMap].Add(iRRelationshipPerMapping);
                    }
                }
            }
        }

        private void GenerateIRDocumentAndAddToDictionaries
            (DocumentMapping documentMapping, string objectDisplayName, string path
            , List<IRMustMatchProperty> objectFindMatchProperties
            , List<IRMustMatchProperty> objectNonFindMatchMustMatchProperties
            , List<IRIgnorableProperty> objectIgnorableProperties
            , ref Dictionary<ObjectMapping, IRObjectsCollection> RecordMMPropertiesPerNonIgnorableObjectMappings
            , ref Dictionary<ObjectMapping, IRObjectsCollection> RecordIRObjectPerFullIgnorableObjectMappings)
        {
            bool isAnyFileAdded = false;

            if (documentMapping.PathOptions.SingleFile == true)
            {
                FileInfo fileInfo = new FileInfo(path);
                isAnyFileAdded = AddNewIRObjectForFile
                    (fileInfo, documentMapping, objectDisplayName
                    , objectFindMatchProperties, objectNonFindMatchMustMatchProperties, objectIgnorableProperties
                    , ref RecordMMPropertiesPerNonIgnorableObjectMappings, ref RecordIRObjectPerFullIgnorableObjectMappings);
            }
            if (documentMapping.PathOptions.FolderContent == true)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(path);
                bool addResult = AddNewIRObjectsForDirectoryFiles
                    (dirInfo, documentMapping, objectDisplayName
                    , objectFindMatchProperties, objectNonFindMatchMustMatchProperties, objectIgnorableProperties
                    , ref RecordMMPropertiesPerNonIgnorableObjectMappings, ref RecordIRObjectPerFullIgnorableObjectMappings);
                if (isAnyFileAdded || addResult)
                {
                    isAnyFileAdded = true;
                }
            }
            if (documentMapping.PathOptions.SubFoldersContent == true)
            {
                DirectoryInfo dirInfo = new DirectoryInfo(path);
                if (dirInfo.Exists)
                {
                    foreach (DirectoryInfo subDirectory in dirInfo.GetDirectories())
                    {
                        bool addResult = AddNewIRObjectsForDirectoryFilesAndSubDirectoryFiles
                            (subDirectory, documentMapping, objectDisplayName
                            , objectFindMatchProperties, objectNonFindMatchMustMatchProperties, objectIgnorableProperties
                            , ref RecordMMPropertiesPerNonIgnorableObjectMappings, ref RecordIRObjectPerFullIgnorableObjectMappings);
                        if (isAnyFileAdded || addResult)
                        {
                            isAnyFileAdded = true;
                        }
                    }
                }
            }

            if (!isAnyFileAdded && ReportFullDetails)
            {
                SaveLog($"Can not read any file from \"{path}\" by the mapping definitions");
            }
        }

        private void GenerateIRObjectAndAddToDictionaries
            (ObjectMapping objectMapping, string objectDisplayName
            , List<IRMustMatchProperty> objectFindMatchProperties
            , List<IRMustMatchProperty> objectNonFindMatchMustMatchProperties
            , List<IRIgnorableProperty> objectIgnorableProperties
            , ref Dictionary<ObjectMapping, IRObjectsCollection> RecordMMPropertiesPerNonIgnorableObjectMappings
            , ref Dictionary<ObjectMapping, IRObjectsCollection> RecordIRObjectPerFullIgnorableObjectMappings)
        {
            IRObject newIRObj = GenerateNewIRObject
                (objectMapping.ObjectType.TypeUri, objectDisplayName
                , objectFindMatchProperties, objectNonFindMatchMustMatchProperties, objectIgnorableProperties);
            AddIRObjectToDictionaries
                (newIRObj, objectMapping
                , ref RecordMMPropertiesPerNonIgnorableObjectMappings
                , ref RecordIRObjectPerFullIgnorableObjectMappings);
        }

        private bool HasObjectInvalidMustMatchProperty(ref string[] dataRow, ref DataRowFieldsExtractionResult extractionResult, ObjectMapping objectMapping)
        {
            bool hasInvalidProperty = false;

            foreach (var propertyMapping in objectMapping.Properties)
            {
                PropertyInternalResolutionOption? resolutionOption = null;

                if (propertyMapping.Value is ConstValueMappingItem)
                {
                    resolutionOption = PropertyInternalResolutionOption.Ignorable;
                }
                else
                {
                    if (propertyMapping.Value is IResolvableValueMappingItem)
                    {
                        resolutionOption = ((IResolvableValueMappingItem)(propertyMapping.Value)).ResolutionOption;
                    }
                }

                string fieldValue = GetPropertyValue(propertyMapping.Value, ref dataRow);
                if (resolutionOption == PropertyInternalResolutionOption.FindMatch
                || resolutionOption == PropertyInternalResolutionOption.MustMatch)
                {
                    if (!TryValidateMustMatchPropertyValue(fieldValue, propertyMapping.Value, resolutionOption.Value, ref extractionResult))
                    {
                        hasInvalidProperty = true;
                        break;
                    }
                }
            }

            return hasInvalidProperty;
        }

        private bool TryValidateMustMatchPropertyValue(string extractedValue, ValueMappingItem valueMapping, PropertyInternalResolutionOption resolutionOption, ref DataRowFieldsExtractionResult extractionResult)
        {

            if (IsMustMatchPropertyValueMinimallyValid(extractedValue))
            {
                if (valueMapping is GeoTimeValueMappingItem)
                {
                    GeoTimeEntityRawData geoTimeRawData = GeoTime.GeoTimeEntityRawData(extractedValue);
                    bool invalidLatOrLongDetected = false;

                    if (!IsMustMatchPropertyValueMinimallyValid(geoTimeRawData.Latitude))
                    {
                        AddFieldToReportAsInvalid(((GeoTimeValueMappingItem)valueMapping).Latitude, ref extractionResult);
                        invalidLatOrLongDetected = true;
                    }
                    if (!IsMustMatchPropertyValueMinimallyValid(geoTimeRawData.Longitude))
                    {
                        AddFieldToReportAsInvalid(((GeoTimeValueMappingItem)valueMapping).Longitude, ref extractionResult);
                        invalidLatOrLongDetected = true;
                    }

                    if (invalidLatOrLongDetected)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                return true;
            }
            else
            {
                AddFieldToReportAsInvalid(valueMapping, ref extractionResult);
                return false;
            }
        }
        private void AddFieldToReportAsInvalid(ValueMappingItem propertyValue, ref DataRowFieldsExtractionResult extractionResult)
        {
            List<int> invalidFieldIndices = new List<int>();
            if (propertyValue is TableColumnMappingItem)
            {
                invalidFieldIndices.Add(GetRealSourceColumnIndex(((TableColumnMappingItem)propertyValue).ColumnIndex));
            }
            else if (propertyValue is MultiValueMappingItem)
            {
                invalidFieldIndices.AddRange
                    (((MultiValueMappingItem)propertyValue).MultiValues
                        .Where(v => v is TableColumnMappingItem)
                        .Select(v => GetRealSourceColumnIndex(((TableColumnMappingItem)v).ColumnIndex)));
            }
            //else if (propertyValue is GeoTimeValueMappingItem)
            //{
            //    //نگاشت‌های از این نوع باید به صورت جداگانه تجزیه‌ شده و اجزای آن برای گزارش به این تابع پاس داده شوند
            //}
            else
            {
                throw new NotSupportedException("Unknown value-mapping item");
            }

            foreach (int invalidIndex in invalidFieldIndices)
            {
                if (!extractionResult.InvalidFieldsIndex.Contains(invalidIndex))
                {
                    extractionResult.InvalidFieldsIndex.Add(invalidIndex);
                }
            }
        }

        private string GetFullyConstPropertyValue(ValueMappingItem valueMapping)
        {
            if (valueMapping is SingleValueMappingItem)
            {
                return GetFullyConstPropertyValue(valueMapping as SingleValueMappingItem);
            }
            else if (valueMapping is MultiValueMappingItem)
            {
                return GetFullyConstPropertyValue(valueMapping as MultiValueMappingItem);
            }
            else if (valueMapping is GeoTimeValueMappingItem)
            {
                return GetFullyConstPropertyValue(valueMapping as GeoTimeValueMappingItem);
            }
            else
            {
                throw new NotSupportedException("Unknown value-mapping type");
            }
        }
        private string GetFullyConstPropertyValue(GeoTimeValueMappingItem propertyValue)
        {
            GeoTimeEntityRawData valueRawData = new GeoTimeEntityRawData()
            {
                Latitude = GetFullyConstPropertyValue(propertyValue.Latitude),
                Longitude = GetFullyConstPropertyValue(propertyValue.Longitude),
                TimeBegin = GetFullyConstPropertyValue(propertyValue.TimeBegin),
                TimeEnd = GetFullyConstPropertyValue(propertyValue.TimeEnd)
            };
            return GeoTime.GetGeoTimeStringValue(valueRawData);
        }
        private string GetFullyConstPropertyValue(MultiValueMappingItem propertyValue)
        {
            StringBuilder fieldsTotalValue = new StringBuilder(string.Empty);
            foreach (SingleValueMappingItem currentValue in propertyValue.MultiValues)
            {
                fieldsTotalValue.Append(GetFullyConstPropertyValue(currentValue));
            }
            return fieldsTotalValue.ToString();
        }
        private string GetFullyConstPropertyValue(SingleValueMappingItem valueMapping)
        {
            if (valueMapping is PathPartMappingItem)
            {
                return GetFullyConstPropertyValue(valueMapping as PathPartMappingItem);
            }
            else if (valueMapping is ConstValueMappingItem)
            {
                return GetFullyConstPropertyValue(valueMapping as ConstValueMappingItem);
            }
            else if (valueMapping is TableColumnMappingItem)
            {
                throw new InvalidOperationException("A Table-Column based mapping can not defined in a Fully-Const Property mapping");
            }
            else
            {
                throw new NotSupportedException("Unknown single-value mapping item type");
            }
        }
        private string GetFullyConstPropertyValue(ConstValueMappingItem valueMapping)
        {
            return valueMapping.ConstValue;
        }

        private string GetPropertyValue(ValueMappingItem valueMapping, ref string[] dataRow)
        {
            if (valueMapping is SingleValueMappingItem)
            {
                return GetPropertyValue(valueMapping as SingleValueMappingItem, ref dataRow);
            }
            else if (valueMapping is MultiValueMappingItem)
            {
                return GetPropertyValue(valueMapping as MultiValueMappingItem, ref dataRow);
            }
            else if (valueMapping is GeoTimeValueMappingItem)
            {
                return GetPropertyValue(valueMapping as GeoTimeValueMappingItem, ref dataRow);
            }
            else
            {
                throw new NotSupportedException("Unknown value-mapping type");
            }
        }
        private string GetPropertyValue(GeoTimeValueMappingItem propertyValue, ref string[] dataRow)
        {
            GeoTimeEntityRawData valueRawData = new GeoTimeEntityRawData()
            {
                Latitude = GetPropertyValue(propertyValue.Latitude, ref dataRow),
                Longitude = GetPropertyValue(propertyValue.Longitude, ref dataRow),
                TimeBegin = GetPropertyValue(propertyValue.TimeBegin, ref dataRow),
                TimeEnd = GetPropertyValue(propertyValue.TimeEnd, ref dataRow)
            };
            return GeoTime.GetGeoTimeStringValue(valueRawData);
        }
        private string GetPropertyValue(MultiValueMappingItem propertyValue, ref string[] dataRow)
        {
            StringBuilder fieldsTotalValue = new StringBuilder(string.Empty);
            foreach (SingleValueMappingItem currentValue in propertyValue.MultiValues)
            {
                fieldsTotalValue.Append(GetPropertyValue(currentValue, ref dataRow));
            }
            return fieldsTotalValue.ToString();
        }
        private string GetPropertyValue(SingleValueMappingItem valueMapping, ref string[] dataRow)
        {
            if (valueMapping is TableColumnMappingItem)
            {
                return GetPropertyValue((valueMapping as TableColumnMappingItem), ref dataRow);
            }
            else if (valueMapping is PathPartMappingItem)
            {
                return GetPropertyValue(valueMapping as PathPartMappingItem);
            }
            else if (valueMapping is ConstValueMappingItem)
            {
                return GetPropertyValue(valueMapping as ConstValueMappingItem);
            }
            else
            {
                throw new NotSupportedException("Unknown single-value mapping item type");
            }
        }
        private string GetPropertyValue(TableColumnMappingItem valueMapping, ref string[] dataRow)
        {
            if (valueMapping is DateTimeTableColumnMappingItem)
            {
                return GetPropertyValue((valueMapping as DateTimeTableColumnMappingItem), ref dataRow);
            }
            else
            {
                return dataRow[valueMapping.ColumnIndex];
            }
        }
        private string GetPropertyValue(DateTimeTableColumnMappingItem valueMapping, ref string[] dataRow)
        {
            string invaliantCultureValue = string.Empty;
            DateTimeTableColumnMappingItem.ParseDateTimeValue
                (dataRow[valueMapping.ColumnIndex], valueMapping.DateTimeCultureName, valueMapping.DateTimeStyles, out invaliantCultureValue);
            return invaliantCultureValue;
        }
        private string GetPropertyValue(ConstValueMappingItem valueMapping)
        {
            return valueMapping.ConstValue;
        }

        /// <summary></summary>
        /// <returns>Returns 'true' if atleast one file exist in sub-directory of the directory and add to dictionaries, otherwise returns 'false'</returns>
        private bool AddNewIRObjectsForDirectoryFilesAndSubDirectoryFiles
            (DirectoryInfo dirInfo, DocumentMapping documentMapping, string displayNameFromProperties
            , List<IRMustMatchProperty> objectFindMatchProperties
            , List<IRMustMatchProperty> objectNonFindMatchMustMatchProperties
            , List<IRIgnorableProperty> objectIgnorableProperties
            , ref Dictionary<ObjectMapping, IRObjectsCollection> RecordMMPropertiesPerNonIgnorableObjectMappings
            , ref Dictionary<ObjectMapping, IRObjectsCollection> RecordIRObjectPerFullIgnorableObjectMappings)
        {
            if (!dirInfo.Exists)
            {
                return false;
            }
            bool anyFilePathAdded = false;
            anyFilePathAdded = AddNewIRObjectsForDirectoryFiles
                (dirInfo, documentMapping, displayNameFromProperties
                , objectFindMatchProperties, objectNonFindMatchMustMatchProperties, objectIgnorableProperties
                , ref RecordMMPropertiesPerNonIgnorableObjectMappings, ref RecordIRObjectPerFullIgnorableObjectMappings);
            foreach (DirectoryInfo subDir in dirInfo.GetDirectories())
            {
                bool addResult = AddNewIRObjectsForDirectoryFilesAndSubDirectoryFiles
                    (subDir, documentMapping, displayNameFromProperties
                    , objectFindMatchProperties, objectNonFindMatchMustMatchProperties, objectIgnorableProperties
                    , ref RecordMMPropertiesPerNonIgnorableObjectMappings, ref RecordIRObjectPerFullIgnorableObjectMappings);
                if (anyFilePathAdded || addResult)
                {
                    anyFilePathAdded = true;
                }
            }
            return anyFilePathAdded;
        }

        /// <summary></summary>
        /// <returns>Returns 'true' if atleast one file exist in the directory and add to dictionaries, otherwise returns 'false'</returns>
        private bool AddNewIRObjectsForDirectoryFiles
            (DirectoryInfo dirInfo, DocumentMapping documentMapping, string displayNameFromProperties
            , List<IRMustMatchProperty> objectFindMatchProperties
            , List<IRMustMatchProperty> objectNonFindMatchMustMatchProperties
            , List<IRIgnorableProperty> objectIgnorableProperties
            , ref Dictionary<ObjectMapping, IRObjectsCollection> RecordMMPropertiesPerNonIgnorableObjectMappings
            , ref Dictionary<ObjectMapping, IRObjectsCollection> RecordIRObjectPerFullIgnorableObjectMappings)
        {
            if (!dirInfo.Exists)
            {
                return false;
            }
            bool anyFilePathAdded = false;
            foreach (FileInfo subFile in dirInfo.GetFiles())
            {
                bool addResult = AddNewIRObjectForFile
                    (subFile, documentMapping, displayNameFromProperties
                    , objectFindMatchProperties, objectNonFindMatchMustMatchProperties, objectIgnorableProperties
                    , ref RecordMMPropertiesPerNonIgnorableObjectMappings, ref RecordIRObjectPerFullIgnorableObjectMappings);
                if (anyFilePathAdded || addResult)
                {
                    anyFilePathAdded = true;
                }
            }
            return anyFilePathAdded;
        }

        /// <summary></summary>
        /// <returns>Returns 'true' if the file exist and add to dictionaries, otherwise returns 'false'</returns>
        private bool AddNewIRObjectForFile
            (FileInfo fileInfo, DocumentMapping documentMapping, string displayNameFromProperties
            , List<IRMustMatchProperty> objectFindMatchProperties
            , List<IRMustMatchProperty> objectNonFindMatchMustMatchProperties
            , List<IRIgnorableProperty> objectIgnorableProperties
            , ref Dictionary<ObjectMapping, IRObjectsCollection> RecordMMPropertiesPerNonIgnorableObjectMappings
            , ref Dictionary<ObjectMapping, IRObjectsCollection> RecordIRObjectPerFullIgnorableObjectMappings)
        {
            if (!fileInfo.Exists)
            {
                return false;
            }
            if (fileInfo.Length == 0)
            {
                if (ReportFullDetails)
                {
                    SaveLog($"File \"{fileInfo.FullName}\" is empty and ignored");
                }
                return false;
            }
            string typeUri = CurrentOntology.GetDocumentTypeUriByFileExtension(fileInfo.Extension.TrimStart('.'));
            string displayName;
            if (documentMapping.IsDocumentNameAsDisplayName)
            {
                displayName = fileInfo.Name;
            }
            else
            {
                displayName = displayNameFromProperties;
            }
            IRObject newIRObj;
            switch ((documentMapping.DocumentPathMapping as IResolvableValueMappingItem).ResolutionOption)
            {
                case PropertyInternalResolutionOption.FindMatch:
                    List<IRMustMatchProperty> documentFindMatchProperties = new List<IRMustMatchProperty>(objectFindMatchProperties);
                    documentFindMatchProperties.Add(new IRMustMatchProperty(DocumentsPathFakeTypeUriAlternative, fileInfo.FullName));
                    newIRObj = GenerateNewIRObject(typeUri, displayName, documentFindMatchProperties, objectNonFindMatchMustMatchProperties, objectIgnorableProperties);
                    break;
                case PropertyInternalResolutionOption.MustMatch:
                    List<IRMustMatchProperty> documentNonFindMatchMustMatchProperties = new List<IRMustMatchProperty>(objectNonFindMatchMustMatchProperties);
                    documentNonFindMatchMustMatchProperties.Add(new IRMustMatchProperty(DocumentsPathFakeTypeUriAlternative, fileInfo.FullName));
                    newIRObj = GenerateNewIRObject(typeUri, displayName, objectFindMatchProperties, documentNonFindMatchMustMatchProperties, objectIgnorableProperties);
                    break;
                case PropertyInternalResolutionOption.Ignorable:
                    List<IRIgnorableProperty> documentIgnorableProperties = new List<IRIgnorableProperty>(objectIgnorableProperties);
                    documentIgnorableProperties.Add(new IRIgnorableProperty() { TypeURI = DocumentsPathFakeTypeUriAlternative, Values = new List<string>() { fileInfo.FullName } });
                    newIRObj = GenerateNewIRObject(typeUri, displayName, objectFindMatchProperties, objectNonFindMatchMustMatchProperties, documentIgnorableProperties);
                    break;
                default:
                    throw new NotSupportedException("Unknown resolution option");
            }
            AddIRObjectToDictionaries
                (newIRObj, documentMapping
                , ref RecordMMPropertiesPerNonIgnorableObjectMappings
                , ref RecordIRObjectPerFullIgnorableObjectMappings);
            if (ReportFullDetails)
            {
                SaveLog($"File \"{fileInfo.FullName}\" will import as a new Document");
            }
            return true;
        }

        private void AddIRObjectToDictionaries
            (IRObject newIRObj, ObjectMapping objectMapping
            , ref Dictionary<ObjectMapping, IRObjectsCollection> RecordMMPropertiesPerNonIgnorableObjectMappings
            , ref Dictionary<ObjectMapping, IRObjectsCollection> RecordIRObjectPerFullIgnorableObjectMappings)
        {
            if (IsNonResolvableObjectMapping(objectMapping))
            {
                AddNonResolvableIRObjectToDictionaries(newIRObj, objectMapping, ref RecordIRObjectPerFullIgnorableObjectMappings);
            }
            else
            {
                AddResolvableIRObjectToDictionaries(newIRObj, objectMapping, ref RecordMMPropertiesPerNonIgnorableObjectMappings);
            }
        }

        private void AddResolvableIRObjectToDictionaries
            (IRObject iRObj, ObjectMapping objectMapping
            , ref Dictionary<ObjectMapping, IRObjectsCollection> RecordMMPropertiesPerNonIgnorableObjectMappings)
        {
            ObjectMappingInnerInternalResolution(iRObj, objectMapping);
            if (RecordMMPropertiesPerNonIgnorableObjectMappings != null)
            {
                RecordMMPropertiesPerNonIgnorableObjectMappings[objectMapping].Add(iRObj);
            }
        }

        private void AddNonResolvableIRObjectToDictionaries
            (IRObject iRObj, ObjectMapping objectMapping
            , ref Dictionary<ObjectMapping, IRObjectsCollection> RecordIRObjectPerFullIgnorableObjectMappings)
        {
            TotalExtractedConcepts.IRObjectsPerMappings[objectMapping].Add(iRObj);
            if (RecordIRObjectPerFullIgnorableObjectMappings != null)
            {
                RecordIRObjectPerFullIgnorableObjectMappings[objectMapping].Add(iRObj);
            }
        }

        private static IRObject GenerateNewIRObject(string objectTypeUri, string objectDisplayName, List<IRMustMatchProperty> objectFindMatchProperties, List<IRMustMatchProperty> objectNonFindMatchMustMatchProperties, List<IRIgnorableProperty> objectIgnorableProperties)
        {
            return new IRObject(objectFindMatchProperties, objectNonFindMatchMustMatchProperties)
            {
                DisplayName = objectDisplayName,
                TypeUri = objectTypeUri,
                IgnorableProperties = objectIgnorableProperties
            };
        }

        private void AddNewPropertyToApproperiateList(PropertyMapping propertyMapping, string fieldValue, PropertyInternalResolutionOption resolutionOption, ref List<IRMustMatchProperty> objectFindMatchProperties, ref List<IRMustMatchProperty> objectNonFindMatchMustMatchProperties, ref List<IRIgnorableProperty> objectIgnorableProperties)
        {
            switch (resolutionOption)
            {
                case PropertyInternalResolutionOption.FindMatch:
                    AddNewMustMatchProperty(ref objectFindMatchProperties, propertyMapping.PropertyType.TypeUri, fieldValue);
                    break;
                case PropertyInternalResolutionOption.MustMatch:
                    AddNewMustMatchProperty(ref objectNonFindMatchMustMatchProperties, propertyMapping.PropertyType.TypeUri, fieldValue);
                    break;
                case PropertyInternalResolutionOption.Ignorable:
                    AddNewIgnorableProperty(ref objectIgnorableProperties, propertyMapping.PropertyType.TypeUri, fieldValue);
                    break;
                default:
                    throw new NotSupportedException("Unknown internal resolution option");
            }
        }

        private string GetRelatinshipDescription(RelationshipMapping relationshipMap, string[] dataRow)
        {
            string description;
            if (relationshipMap.RelationshipDescription == null)
                description = string.Empty;
            else if (relationshipMap.RelationshipDescription is SingleValueMappingItem)
                description = GetPropertyValue((SingleValueMappingItem)relationshipMap.RelationshipDescription, ref dataRow);
            else if (relationshipMap.RelationshipDescription is MultiValueMappingItem)
                description = GetPropertyValue(((MultiValueMappingItem)relationshipMap.RelationshipDescription), ref dataRow);
            //else if (relationshipMap.RelationshipDescription is GeoTimeValueMappingItem)
            // امکان انتساب توضیحات از این نوع وجود ندارد
            else
                throw new NotSupportedException("Unknown Relationship description mapping-value type");
            return description;
        }

        private List<IRRelationshipEnd> GetRelationshipEndFromFilledDictionaries
            (ObjectMapping relationshipEndObjectMapping
            , ref Dictionary<ObjectMapping, IRObjectsCollection> RecordMMPropertiesPerNonIgnorableObjectMappings
            , ref Dictionary<ObjectMapping, IRObjectsCollection> RecordIRObjectPerFullIgnorableObjectMappings)
        {
            if (IsFullyConstObjectMapping(relationshipEndObjectMapping))
            {
                return new List<IRRelationshipEnd>
                    (TotalExtractedConcepts.IRObjectsPerMappings[relationshipEndObjectMapping].GetObjects()
                        .Select(endObj => new IRRelationshipObjectBasedEnd(endObj)));
            }

            IRObjectsCollection endRelatedObjects = null;
            bool isObjMappingNonResolvable = IsNonResolvableObjectMapping(relationshipEndObjectMapping);
            if (isObjMappingNonResolvable)
            {
                RecordIRObjectPerFullIgnorableObjectMappings.TryGetValue(relationshipEndObjectMapping, out endRelatedObjects);
            }
            else
            {
                RecordMMPropertiesPerNonIgnorableObjectMappings.TryGetValue(relationshipEndObjectMapping, out endRelatedObjects);
            }
            if (endRelatedObjects != null)
            {
                return endRelatedObjects.GetRelationshipEnds();
            }
            else
            {
                //SaveLog("No extracted object matched to relationship end; Probably must-match property value(s) are not valid");
                return new List<IRRelationshipEnd>();
            }
        }

        private bool IsMustMatchPropertyValueMinimallyValid(string dataFieldValue, bool isGeoTimeSerializedValue = false)
        {
            if (string.IsNullOrWhiteSpace(dataFieldValue))
                return false;
            if (dataFieldValue.ToLower(CultureInfo.InvariantCulture).Equals("null"))
                return false;
            return true;
        }

        private bool IsNonResolvableObjectMapping(ObjectMapping objectMapping)
        {
            return ObjectsMappingNonResolvablityDictionary[objectMapping];
        }

        private bool IsFullyConstObjectMapping(ObjectMapping objectMapping)
        {
            return FullyConstValueObjectMappings.Contains(objectMapping);
        }

        private void ObjectMappingInnerInternalResolution
            (IRObject iRObjectToResolve, ObjectMapping objectMapping)
        {
            IRObjectsCollection resolvedIRObjectsForMapping = TotalExtractedConcepts.IRObjectsPerMappings[objectMapping];

            IRObject condidateIRObjectForResolve;
            if (resolvedIRObjectsForMapping.TryGetSameMustMatchObject(iRObjectToResolve, out condidateIRObjectForResolve))
                ResolveSameMMPropertiesIRObjects(ref condidateIRObjectForResolve, iRObjectToResolve);
            else
                resolvedIRObjectsForMapping.Add(iRObjectToResolve);
        }

        private void ResolveSameMMPropertiesIRObjects(ref IRObject resolvedIRObject, IRObject iRObjectToResolve)
        {
            ResolveProbablyConflictInDisplayName(ref resolvedIRObject.DisplayName, iRObjectToResolve.DisplayName);
            foreach (var currentIRIPropertyToResolve in iRObjectToResolve.IgnorableProperties)
            {
                ResolveProbablyConflictInIRIProperties(ref resolvedIRObject.IgnorableProperties, currentIRIPropertyToResolve);
            }
        }

        private void AddNewMustMatchProperty(ref List<IRMustMatchProperty> objectFindMatchProperties, string propertyTypeURI, string propertyValue)
        {
            var newIRMMProperty = new IRMustMatchProperty(propertyTypeURI, propertyValue);
            bool newIRMMPropertyMatchesToOneOfExistingProperties = false;
            // TODO: Replace with sortedList
            foreach (var item in objectFindMatchProperties)
            {
                if (item.Equals(newIRMMProperty))
                {
                    newIRMMPropertyMatchesToOneOfExistingProperties = true;
                    break;
                }
            }
            if (!newIRMMPropertyMatchesToOneOfExistingProperties)
                objectFindMatchProperties.Add(newIRMMProperty);
        }

        private void AddNewIgnorableProperty(ref List<IRIgnorableProperty> objectIgnorableProperties, string propertyTypeURI, string propertyValue)
        {
            var newIRIProperty = new IRIgnorableProperty()
            {
                TypeURI = propertyTypeURI,
                Values = new List<string>()
            };
            newIRIProperty.Values.Add(propertyValue);
            ResolveProbablyConflictInIRIProperties(ref objectIgnorableProperties, newIRIProperty);
        }

        private Dictionary<RelationshipMapping, HashSet<IRRelationship>> GenerateIRRelationshipEmptyListsByMapping()
        {
            var result = new Dictionary<RelationshipMapping, HashSet<IRRelationship>>();
            for (int i = 0; i < RelationshipsMappingArray.Length; i++)
            {
                RelationshipMapping item = RelationshipsMappingArray[i];
                result.Add(item, new HashSet<IRRelationship>());
            }
            return result;
        }

        private Dictionary<ObjectMapping, IRObjectsCollection> GenerateIRObjectEmptyListsByMapping()
        {
            var result = new Dictionary<ObjectMapping, IRObjectsCollection>();
            for (int i = 0; i < ObjectsMappingArray.Length; i++)
            {
                ObjectMapping item = ObjectsMappingArray[i];
                result.Add(item, new IRObjectsCollection());
            }
            return result;
        }
    }
}