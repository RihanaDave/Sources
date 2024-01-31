using GPAS.AccessControl;
using GPAS.DataImport.Material.SemiStructured;
using GPAS.Utility;
using System;
using System.IO;

namespace GPAS.DataImport.Publish
{
    public class DataSourceMetadata
    {
        public byte[] Content { get; set; }
        public string Name { get; set; }
        public DataSourceType Type { get; set; }
        public ACL Acl { get; set; }
        public string Description { get; set; }

        public static byte[] GetNonFileBasedSemiStructuredDataSourceContent(MaterialBase material)
        {
            if (material is AttachedDatabaseTableMaterial
                || material is DataLakeSearchResultMaterial)
            {
                MaterialBaseSerializer serializer = new MaterialBaseSerializer();
                MemoryStream materialStream = null;
                try
                {
                    materialStream = new MemoryStream();
                    serializer.Serialize(materialStream, material);
                    StreamUtility util = new StreamUtility();
                    return util.ReadStreamAsBytesArray(materialStream);
                }
                finally
                {
                    if (materialStream != null)
                        materialStream.Close();
                }
            }
            else
            {
                throw new NotSupportedException("Unknown import material type");
            }
        }
        public static DataSourceType GetSemiStructuredDataSourceType(MaterialBase material)
        {
            if (material is CsvFileMaterial)
            {
                return DataSourceType.CsvFile;
            }
            else if (material is ExcelSheet)
            {
                return DataSourceType.ExcelSheet;
            }
            else if (material is AccessTable)
            {
                return DataSourceType.AccessTable;
            }
            else if (material is AttachedDatabaseTableMaterial)
            {
                return DataSourceType.AttachedDatabaseTable;
            }
            else if (material is DataLakeSearchResultMaterial)
            {
                return DataSourceType.DataLakeSearchResult;
            }
            else if (material is EmlDirectory)
            {
                return DataSourceType.CsvFile;
            }
            else
            {
                throw new NotSupportedException("Unknown import material type");
            }
        }
        public static string GetSemiStructuredDataSourceName(MaterialBase material)
        {
            if (material is CsvFileMaterial)
            {
                FileInfo fi = new FileInfo(((CsvFileMaterial)material).FileJobSharePath);
                return fi.Name;
            }
            else if (material is ExcelSheet)
            {
                return ((ExcelSheet)material).SheetName;
            }
            else if (material is AccessTable)
            {
                return ((AccessTable)material).TableName;
            }
            else if (material is AttachedDatabaseTableMaterial)
            {
                return $"Attached Database Table/View: {((AttachedDatabaseTableMaterial)material).TableName}";
            }
            else if (material is DataLakeSearchResultMaterial)
            {
                return "Data Lake search result";
            }
            else if (material is EmlDirectory)
            {
                return "E-mails CSV";
            }
            else
            {
                throw new NotSupportedException("Unknown import material type");
            }
        }
    }
}
