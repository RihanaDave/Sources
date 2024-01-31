using System;
using System.Collections.Generic;

namespace GPAS.DataImport.Material.SemiStructured
{
    [Serializable]
    public class AttachedDatabaseTableMaterial : MaterialBase
    {

        public AttachedDatabaseTableMaterial()
        { }
        
        public string TableName
        {
            get;
            set;
        }

        public string DatabaseUri
        {
            get;
            set;
        }
        
        public static string GetDatabaseUri(string serverKey, string databaseName)
        {
            return string.Format("[{0}].[{1}]", serverKey, databaseName);
        }

        public static string GetDatabaseNameFromDatabaseUri(string dbUri)
        {
            return dbUri.Substring(dbUri.IndexOf("].[") + 3, dbUri.Length - (dbUri.IndexOf("].[") + 4));
        }

        public static string GetServerKeyFromDatabaseUri(string dbUri)
        {
            return dbUri.Substring(1, dbUri.IndexOf(']') - 1);
        }
    }
}
