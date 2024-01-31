using GPAS.AccessControl;
using GPAS.Dispatch.Entities.Concepts.Geo;
using GPAS.SearchServer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using GPAS.DataImport.Transformation;
using System.Data;
using System.IO;
using GPAS.StatisticalQuery.Formula.DrillDown.PropertyValueBased;
using GPAS.StatisticalQuery.Formula;
using GPAS.StatisticalQuery.Formula.DrillDown.TypeBased;
using GPAS.FilterSearch;
using GPAS.Logger;
using GPAS.LoadTest.Core;
using System.Text;
using System.Collections.ObjectModel;

namespace GPAS.Search.Server.LoadTests
{
    public class BaseLoadTest
    {
        protected Service serviceClient = new Service();
        protected Random Random = new Random();
        protected int DataSourceId = 300;
        protected Ontology.Ontology ontology = new Ontology.Ontology();
        protected ACL currentACL;
        DataTable dtCSV = new DataTable();
        protected Dictionary<string, Ontology.BaseDataTypes> allProperties = new Dictionary<string, Ontology.BaseDataTypes>()
        {
            ["label"] = Ontology.BaseDataTypes.String,
            ["نام"] = Ontology.BaseDataTypes.String,
            ["نام_خانوادگی"] = Ontology.BaseDataTypes.String,
            ["توضیحات"] = Ontology.BaseDataTypes.String,
            ["سن"] = Ontology.BaseDataTypes.Int,
            ["قد"] = Ontology.BaseDataTypes.Double,
            ["شماره_گزارش"] = Ontology.BaseDataTypes.Long,
            ["تأهل"] = Ontology.BaseDataTypes.Boolean,
           // ["آدرس"] = Ontology.BaseDataTypes.String,
           // ["ملیت"] = Ontology.BaseDataTypes.String,
            ["تاریخ_تولد"] = Ontology.BaseDataTypes.DateTime,
            ["زمان_و_موقعیت_جغرافیایی"] = Ontology.BaseDataTypes.GeoTime,
        };
        protected List<string> allObjects = new List<string>() { "شخص", "مخاطب", "سوژه" };
        protected List<string> allRelations = new List<string>() { "حضور_در", "ارتباط_با", "تماس" };
        protected static long BatchItems = 100;
        protected static long BatchCount = 10;
        protected static long QueryCount = 10;

        private static ProcessLogger logger;

        public string LogMessage = "";
        public static string ClearAllDataTimeString = "";
        public static string TotalPublishTimeString = "";
        public static string AveragePublishTimeString = "";
        public static string TotalRetriveTimeString = "";
        public static string AverageRetriveTimeString = "";
        public string TestTime = "";
        public string TotalTimeFormat = @"hh\:mm\:ss\.fff";
        public string AverageTimeFormat = @"hh\:mm\:ss\.fff";
        protected long startStore = 10;
        protected long endStore = 10000;
        protected long startRetrieve = 10;
        protected long endRetrieve = 10000;

        private const int MaxPropertyValuesLength = 25; //بیش از 5 در سرویس GetTypeBasedResolutionCandidates به مشکل می خورد.

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

            ReadCSV(Directory.GetCurrentDirectory() + @"\wikihowAll.csv");
        }

        Stream stream = null;
        private void ReadCSV(string fileName)
        {
            try
            {
                stream = File.Open(fileName, FileMode.Open);
                int maxNumberOfRow = 10000000;
                int numberOfRow = (int)(BatchCount * BatchItems);
                if (numberOfRow > maxNumberOfRow)
                    numberOfRow = maxNumberOfRow;

                dtCSV = GenerateDataTableFromCsvContentStream(stream, ',', numberOfRow);
                stream.Close();
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch(Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                if (stream != null)
                    stream.Close();

                ReadCSV(fileName);
            }
        }

        public DataTable GenerateDataTableFromCsvContentStream(Stream csvStream, char separator, int numberOfRows)
        {
            if (csvStream == null)
                throw new ArgumentNullException("csvStream");
            if (numberOfRows <= 0)
                throw new ArgumentOutOfRangeException("numberOfRows");

            string[][] dataSourceFields = GetParsableFieldsFromCsvContentStream(csvStream, separator, null, true, numberOfRows);

            return GenerateDataTableFromStringArray(dataSourceFields);
        }

        /// <summary>Get fields of parsable rows from CSV content stream for transformation process</summary>
        /// <param name="csvStream">Source CSV stream</param>
        /// <param name="separator">Separator character for CSV content</param>
        /// <param name="logger">Report process and exceptions and avoid parse break; If equals 'null' no exception may thrown</param>
        /// <param name="readLimitedNumberOfRows">If 'True' only read the specified number of rows, otherwise read total rows and avoid rows count limit</param>
        /// <param name="parsableRowsCountLimit">In case of readLimitedNumberOfRows equals 'True', indicates number of rows that may read form start of the stream (Header row is discounted from the limitatiom)</param>
        /// <returns>Matrix of fields; Rows are the data records from CSV lines</returns>
        internal string[][] GetParsableFieldsFromCsvContentStream(Stream csvStream, char separator, ProcessLogger logger = null, bool readLimitedNumberOfRows = false, int parsableRowsCountLimit = 10)
        {
            List<string[]> totalFields = new List<string[]>(parsableRowsCountLimit);

            using (StreamReader reader = new StreamReader(csvStream))
            {
                using (CsvHelper.CsvReader csvReader = new CsvHelper.CsvReader(reader))
                {
                    csvReader.Configuration.Delimiter = separator.ToString();
                    csvReader.Configuration.BadDataFound = (readerContext) =>
                    {
                        if (logger != null)
                            logger.WriteLog($"Bad data found at row #{readerContext.Row}");
                    };
                    string[] record;
                    if (readLimitedNumberOfRows)
                    {
                        while (parsableRowsCountLimit > (totalFields.Count - 1))
                        {
                            bool endFile = false;
                            try
                            {
                                endFile = !csvReader.Read();
                                if (endFile)
                                    break;
                                record = csvReader.Context.Record;
                                totalFields.Add(record);
                            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
                            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
                            {

                            }
                        }
                    }
                    else
                    {
                        while (true)
                        {
                            bool endFile = false;
                            try
                            {
                                endFile = !csvReader.Read();
                                if (endFile)
                                    break;
                                record = csvReader.Context.Record;
                                totalFields.Add(record);
                            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
                            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
                            {

                            }
                        }
                    }
                }
            }

            return totalFields.ToArray();
        }

        public DataTable GenerateDataTableFromStringArray(string[][] dataSourceFields)
        {
            DataTable result = new DataTable();

            if (dataSourceFields.Length > 0)
            {

                foreach (var currentColumnHeader in dataSourceFields[0])
                {
                    result.Columns.Add(new DataColumn(currentColumnHeader.Trim()));
                }
                for (int i = 1; i < dataSourceFields.Length; i++)
                {
                    DataRow row = result.NewRow();
                    row.ItemArray = dataSourceFields[i].Take(result.Columns.Count).ToArray();
                    result.Rows.Add(row);
                }
            }

            return result;
        }

        protected void SetOntology(List<string> properties)
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

        protected object GenerateValueProperty(string propertyName, bool readStringPropertiesFromCSVFile = false, long csvRowIndex = 0)
        {
            int max = dtCSV.Rows.Count;

            int csvRow = (int)(csvRowIndex % max);
            //int csvRow = (int)csvRowIndex;
            switch (allProperties[propertyName])
            {
                case Ontology.BaseDataTypes.Int:
                    return Random.Next(0, 1000000);
                case Ontology.BaseDataTypes.Boolean:
                    return Random.NextDouble() > .5;
                case Ontology.BaseDataTypes.DateTime:
                    return RandomDateTime();
                case Ontology.BaseDataTypes.String:
                    if (readStringPropertiesFromCSVFile)
                    {
                        if (dtCSV.Rows?.Count > 0 && csvRow < dtCSV.Rows?.Count - 1)
                        {
                            string result = string.Empty;
                            if (propertyName.Equals("label"))
                            {
                                if (dtCSV.Columns.Count > 0)
                                    result = dtCSV.Rows[csvRow][0].ToString();
                                else
                                    result = RandomString();
                            }
                            else if (propertyName.Equals("name"))
                            {
                                if (dtCSV.Columns.Count > 1)
                                    result = dtCSV.Rows[csvRow][1].ToString();
                                else
                                    result = RandomString();
                            }
                            else if (propertyName.Equals("description"))
                            {
                                if (dtCSV.Columns.Count > 2)
                                    result = dtCSV.Rows[csvRow][2].ToString();
                                else
                                    result = RandomString();
                            }
                            else
                            {
                                result = RandomString();
                            }

                            result = result.Replace('"', ' ');
                            result = result.Replace(':', ' ');
                            result = result.Replace('(', ' ');
                            result = result.Replace(')', ' ');
                            result = result.Replace('{', ' ');
                            result = result.Replace('}', ' ');
                            result = result.Replace('[', ' ');
                            result = result.Replace(']', ' ');
                            result = result.Replace('^', ' ');
                            if (result.Length < MaxPropertyValuesLength)
                                return result;
                            else
                                return result.Substring(0, MaxPropertyValuesLength);
                        }
                        else
                        {
                            return RandomString();
                        }
                    }
                    else
                    {
                        return RandomString();
                    }
                case Ontology.BaseDataTypes.Double:
                    return Random.NextDouble();
                case Ontology.BaseDataTypes.Long:
                    return (long)Random.Next(0, 1000000);
                case Ontology.BaseDataTypes.GeoTime:
                    var gt = new GeoTimeEntityRawData()
                    {
                        Latitude = RandomDouble(-85, 85).ToString(),
                        Longitude = RandomDouble(-180, 180).ToString(),
                        TimeBegin = RandomDateTime().ToString(),
                        TimeEnd = RandomDateTime().ToString(),
                    };

                    var s = PropertiesValidation.GeoTime.GetGeoTimeStringValue(gt);
                    return s;
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

        protected PropertyValueBasedDrillDown GeneratePropertyValueBasedDrillDown(string pName = null, int repeat = 0)
        {
            PropertyValueBasedDrillDown propertyValueBasedDrillDown = new PropertyValueBasedDrillDown();
            propertyValueBasedDrillDown.Portions = new List<HasPropertyWithTypeAndValue>();

            if (repeat < 1)
                repeat = Random.Next(1, 4);

            for (int i = 0; i < repeat; i++)
            {
                string propName = pName;
                if (propName == null || propName == string.Empty || !allProperties.Keys.Contains(propName))
                {
                    List<string> properties = allProperties.Where(item => !(item.Value == Ontology.BaseDataTypes.GeoTime || item.Value == Ontology.BaseDataTypes.DateTime))
                                                             .ToDictionary(k => k.Key, v => v.Value).Keys.ToList();

                    int propIndex = Random.Next(0, properties.Count - 1);
                    propName = properties[propIndex];
                }

                propertyValueBasedDrillDown.Portions.Add(new HasPropertyWithTypeAndValue()
                {
                    PropertyTypeUri = propName,
                    PropertyValue = GenerateValueProperty(propName, true, Random.Next(0, int.MaxValue)).ToString(),
                });
            }

            return propertyValueBasedDrillDown;
        }

        protected PropertyValueRangeDrillDown GeneratePropertyValueRangeDrillDown()
        {
            PropertyValueRangeDrillDown propertyValueRangeDrillDown = new PropertyValueRangeDrillDown();
            propertyValueRangeDrillDown.DrillDownDetails = new PropertyValueRangeStatistics();

            List<string> properties = allProperties.Keys.ToList();
            int propIndex = Random.Next(4, 6);
            string pName = properties[propIndex];
            if (!(allProperties[pName] == Ontology.BaseDataTypes.Double ||
                allProperties[pName] == Ontology.BaseDataTypes.Int ||
                allProperties[pName] == Ontology.BaseDataTypes.Long))
            {
                return propertyValueRangeDrillDown;
            }

            propertyValueRangeDrillDown.DrillDownDetails.NumericPropertyTypeUri = pName;
            propertyValueRangeDrillDown.DrillDownDetails.MinValue = RandomDouble(-1000000, 1000000);
            propertyValueRangeDrillDown.DrillDownDetails.MaxValue = RandomDouble(propertyValueRangeDrillDown.DrillDownDetails.MinValue, 2000000);
            propertyValueRangeDrillDown.DrillDownDetails.BucketCount = Random.Next(2, 10);
            int step = (int)Math.Ceiling((propertyValueRangeDrillDown.DrillDownDetails.MaxValue - propertyValueRangeDrillDown.DrillDownDetails.MinValue)
                            / propertyValueRangeDrillDown.DrillDownDetails.BucketCount);

            propertyValueRangeDrillDown.DrillDownDetails.Bars = new List<PropertyValueRangeStatistic>();
            for (int i = (int)propertyValueRangeDrillDown.DrillDownDetails.MinValue; i < propertyValueRangeDrillDown.DrillDownDetails.MaxValue; i += step)
            {
                propertyValueRangeDrillDown.DrillDownDetails.Bars.Add(new PropertyValueRangeStatistic()
                {
                    Start = i,
                    End = i + step,
                });
            }

            return propertyValueRangeDrillDown;
        }

        protected LinkBasedDrillDown GenerateLinkBasedDrillDown()
        {
            LinkBasedDrillDown linkBasedDrillDown = new LinkBasedDrillDown();
            linkBasedDrillDown.Portions = new List<LinkBasedDrillDownPortionBase>();

            int repeat = Random.Next(1, 4);
            for (int i = 0; i < repeat; i++)
            {
                LinkBasedDrillDownPortionBase linkBasedDrillDownPortionBase = null;

                if (Random.NextDouble() > .5)
                {
                    linkBasedDrillDownPortionBase = new LinkedObjectTypeBasedDrillDown()
                    {
                        LinkedObjectTypeUri = allObjects[Random.Next(0, allObjects.Count - 1)],
                    };
                }
                else
                {
                    linkBasedDrillDownPortionBase = new LinkTypeBasedDrillDown()
                    {
                        LinkTypeUri = allRelations[Random.Next(0, allRelations.Count - 1)],
                    };
                }

                linkBasedDrillDown.Portions.Add(linkBasedDrillDownPortionBase);
            }

            return linkBasedDrillDown;
        }

        protected TypeBasedDrillDown GenerateTypeBasedDrillDown(bool onlyProperty = false)
        {
            TypeBasedDrillDown typeBasedDrillDown = new TypeBasedDrillDown();
            typeBasedDrillDown.Portions = new List<TypeBasedDrillDownPortionBase>();
            List<string> properties = allProperties.Keys.ToList();

            int repeat = Random.Next(1, 4);
            for (int i = 0; i < repeat; i++)
            {
                TypeBasedDrillDownPortionBase typeBasedDrillDownPortionBase = null;
                if (onlyProperty)
                {
                    typeBasedDrillDownPortionBase = new HasPropertyWithType()
                    {
                        PropertyTypeUri = properties[Random.Next(0, properties.Count - 1)],
                    };
                }
                else
                {
                    if (Random.NextDouble() > 0.5)
                    {
                        typeBasedDrillDownPortionBase = new HasPropertyWithType()
                        {
                            PropertyTypeUri = properties[Random.Next(0, properties.Count - 1)],
                        };
                    }
                    else
                    {
                        typeBasedDrillDownPortionBase = new OfObjectType()
                        {
                            ObjectTypeUri = allObjects[Random.Next(0, allObjects.Count - 1)],
                        };
                    }
                }

                typeBasedDrillDown.Portions.Add(typeBasedDrillDownPortionBase);
            }

            return typeBasedDrillDown;
        }

        protected double RandomDouble(double min, double max)
        {
            if (min > max)
                throw new ArgumentOutOfRangeException("min Larger than max");

            double x = Random.NextDouble();
            double d = max - min;
            return (d * x) + min;
        }

        protected List<T> GetRandomElements<T>(List<T> list, int maxElementCount = -1)
        {
            int elementsCount = Random.Next(1, list.Count);
            if (maxElementCount >= 0)
                elementsCount = elementsCount > maxElementCount ? maxElementCount : elementsCount;

            return list.OrderBy(arg => Guid.NewGuid()).Take(elementsCount).ToList();
        }

        protected ObjectTypeCriteria GenerateObjectTypeCriteria()
        {
            ObjectTypeCriteria objectTypeCriteria = new ObjectTypeCriteria();
            objectTypeCriteria.ObjectsTypeUri = new ObservableCollection<string>(GetRandomElements(allObjects));

            return objectTypeCriteria;
        }

        protected DateRangeCriteria GenerateDateRangeCriteria()
        {
            DateRangeCriteria dateRangeCriteria = new DateRangeCriteria();
            dateRangeCriteria.EndTime = RandomDateTime().ToString();
            dateRangeCriteria.StartTime = RandomDateTime().ToString();
            return dateRangeCriteria;
        }

        protected KeywordCriteria GenerateKeywordCriteria()
        {
            KeywordCriteria keywordCriteria = new KeywordCriteria();
            keywordCriteria.Keyword = GenerateValueProperty("label", true, Random.Next(0, int.MaxValue)).ToString();
            return keywordCriteria;
        }

        public void ClearAllPublishedData()
        {
            SyncPublishChangesLoadTests syncPublishChangesLoadTests = new SyncPublishChangesLoadTests();
            syncPublishChangesLoadTests.ClearAllData();
        }

        protected void WriteLog(string message)
        {
            if (!Directory.Exists(Properties.Resource.String_LogFilePath))
            {
                Directory.CreateDirectory(Properties.Resource.String_LogFilePath);
            }

            if (logger == null)
            {
                logger = new ProcessLogger();

                string fileName = Properties.Resource.String_LogFilePath + string.Format(Properties.Resource.String_LogFileName, DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss"));
                logger.Initialization(fileName);
            }

            Debug.WriteLine(message);
            logger.WriteLog(message);
        }

        /// <summary>
        /// ساخت فایل و نوشتن نتایج تست‌ها در قالب
        /// csv
        /// </summary>
        /// <param name="results">نتایحج تست‌ها</param>
        public void ExportTestCSV(List<LoadTestResult> results, DateTime startRunTestsTime)
        {
            try
            {
                string resultFolderPath = Properties.Resource.String_LogFilePath;

                if (!Directory.Exists(resultFolderPath))
                {
                    Directory.CreateDirectory(resultFolderPath);
                }

                foreach (var result in results)
                {
                    using (StreamWriter streamWriter = File.CreateText(Path.Combine(resultFolderPath, result.Title + " " + startRunTestsTime.ToString("yyyy-MM-dd HH-mm-ss") + ".csv")))
                    {
                        streamWriter.WriteLine(" ******** Test name: " + result.Title + " ******** ");
                        streamWriter.WriteLine(" Description: " + result.Description);
                        streamWriter.WriteLine("");

                        streamWriter.WriteLine(" ******** Publish Details ******** ");
                        streamWriter.WriteLine(" Total Time: " + result.TotalPublishTimeString);
                        streamWriter.WriteLine(" Average Time: " + result.AveragePublishTimeString);
                        streamWriter.WriteLine("");

                        StringBuilder stringBuilder = new StringBuilder();

                        IEnumerable<string> columnNames = result.Statistics.Columns.Cast<DataColumn>().
                            Select(column => column.ColumnName);
                        stringBuilder.AppendLine(string.Join(",", columnNames));

                        foreach (DataRow row in result.Statistics.Rows)
                        {
                            IEnumerable<string> fields = row.ItemArray.Select(field => field.ToString());
                            stringBuilder.AppendLine(string.Join(",", fields));
                        }

                        streamWriter.WriteLine(stringBuilder.ToString());

                        streamWriter.WriteLine(" ******** Clear Details ******** ");
                        streamWriter.WriteLine(" Total Time: " + result.ClearAllDataTimeString);
                        streamWriter.WriteLine("");
                    }
                }
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {

            }
        }

        public string QueryToString(GPAS.StatisticalQuery.Query query)
        {
            string queryString = string.Empty;
            foreach (var formula in query.FormulaSequence)
            {
                if (formula is PropertyValueBasedDrillDown)
                {
                    foreach (var protion in (formula as PropertyValueBasedDrillDown).Portions)
                    {
                        queryString += protion.ToString() + " | ";
                    }
                }
                else if (formula is PropertyValueRangeDrillDown)
                {
                    PropertyValueRangeStatistics details = (formula as PropertyValueRangeDrillDown).DrillDownDetails;
                    queryString += $"Value range between [{details.MinValue}, {details.MaxValue}] for '{details.NumericPropertyTypeUri}' property | ";
                }
                else if (formula is LinkBasedDrillDown)
                {
                    foreach (var protion in (formula as LinkBasedDrillDown).Portions)
                    {
                        if (protion is LinkedObjectTypeBasedDrillDown)
                        {
                            string linkedObjectTypeUri = (protion as LinkedObjectTypeBasedDrillDown).LinkedObjectTypeUri;
                            queryString += $"LinkedObject type for '{linkedObjectTypeUri}' | ";
                        }
                        else if (protion is LinkTypeBasedDrillDown)
                        {
                            string linkedObjectTypeUri = (protion as LinkTypeBasedDrillDown).LinkTypeUri;
                            queryString += $"Link type for '{linkedObjectTypeUri}' | ";
                        }
                        else
                        {

                        }
                    }
                }
                else if (formula is TypeBasedDrillDown)
                {
                    foreach (var protion in (formula as TypeBasedDrillDown).Portions)
                    {
                        if (protion is HasPropertyWithType)
                        {
                            queryString += (protion as HasPropertyWithType).ToString() + " | ";
                        }
                        else if (protion is OfObjectType)
                        {
                            queryString += (protion as OfObjectType).ToString() + " | ";
                        }
                        else
                        {

                        }
                    }
                }

                queryString += " ==> ";
            }

            return queryString;
        }
    }
}
