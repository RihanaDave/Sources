using GPAS.AccessControl;
using GPAS.RepositoryServer.Data.DatabaseTables;
using GPAS.RepositoryServer.Entities;
using GPAS.Utility;
using Npgsql;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.RepositoryServer.Logic
{
    public class DataSourceManager
    {
        private static string PluginPath = null;
        private const string StoragePluginName = "PluginRelativePath";

        private CompositionContainer compositionContainer;

        [Import(typeof(IAccessClient))]
        public IAccessClient StorageClient;

        public DataSourceManager()
        {
            if (PluginPath == null)
            {
                string pluginRelativePath = ConfigurationManager.AppSettings[StoragePluginName];
                if (pluginRelativePath == null)
                    throw new ConfigurationErrorsException($"Unable to read '{StoragePluginName}' App-Setting");
                PluginPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, pluginRelativePath);
            }

            // https://docs.microsoft.com/en-us/dotnet/framework/mef/index

            //An aggregate catalog that combines multiple catalogs
            var catalog = new AggregateCatalog();

            catalog.Catalogs.Add(new DirectoryCatalog(PluginPath));
            //Create the CompositionContainer with the parts in the catalog
            compositionContainer = new CompositionContainer(catalog);
            //Fill the imports of this object
            this.compositionContainer.ComposeParts(this);
        }

        public List<DBDataSourceACL> RetrieveDataSourceACLs(long[] DataSourceIDs)
        {
            return StorageClient.RetrieveDataSourceACLs(DataSourceIDs);
        }

        public List<DBDataSourceACL> RetrieveTopNDataSourceACLs(long topN)
        {
            return StorageClient.RetrieveTopNDataSourceACLs(topN);
        }

        public List<DataSourceInfo> RetriveDataSourcesSequentialIDByIDRange(long firstID, long lastID)
        {
            return StorageClient.RetriveDataSourcesSequentialIDByIDRange(firstID, lastID);
        }

        public List<DataSourceInfo> GetDataSourcesByIDs(List<long> ids)
        {
            return StorageClient.GetDataSourcesByIDs(ids);
        }

        //--------------------------------------------------------------------------------------------------------
        //private static readonly string ConnctionString = ConnectionStringManager.GetRepositoryDBConnectionString();
        //private static readonly int RequestsTimeout = 10800;

        //#region private functions
        //public List<DBDataSourceACL> GetDBDataSourceACLs(List<long> dataSourceIDs, Dictionary<long, List<ACI>> acisDic)
        //{
        //    if (!dataSourceIDs.Any())
        //        return new List<DBDataSourceACL>();
        //    List<DBDataSourceACL> dataSources = new List<DBDataSourceACL>();
        //    StringUtility stringUtil = new StringUtility();
        //    string query = string.Format("SELECT * FROM {0} WHERE {1} in ({2})"
        //        , DataSource.tableName, DataSource.id, stringUtil.SeperateIDsByComma(dataSourceIDs));
        //    using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
        //    {
        //        conn.Open();
        //        NpgsqlCommand command = new NpgsqlCommand(query, conn);
        //        command.CommandTimeout = RequestsTimeout;
        //        command.AllResultTypesAreUnknown = true;
        //        var reader = command.ExecuteReader();
        //        dataSources = NpgsqlDataReaderToDataSource(reader, acisDic);
        //    }
        //    return dataSources;
        //}

        //private List<DBDataSourceACL> NpgsqlDataReaderToDataSource(NpgsqlDataReader reader, Dictionary<long, List<ACI>> acisDic)
        //{
        //    List<DBDataSourceACL> dataSources = new List<DBDataSourceACL>();
        //    while (reader.Read())
        //    {
        //        long id = long.Parse(reader[DataSource.id].ToString());
        //        string classification = reader[DataSource.classification].ToString();
        //        //long sourcetype = long.Parse(reader[DataSource.sourceType].ToString());
        //        //string description = reader[DataSource.description].ToString();
        //        //string datasourceName = reader[DataSource.dsname].ToString();
        //        dataSources.Add(
        //            new DBDataSourceACL()
        //            {
        //                Id = id,
        //                Acl = new ACL()
        //                {
        //                    Classification = classification,
        //                    Permissions = acisDic[id]
        //                }
        //            }
        //            );
        //    }
        //    return dataSources;
        //}

        //public Dictionary<long, List<ACI>> GetACIs(List<long> dataSourceIDs)
        //{
        //    if (!dataSourceIDs.Any())
        //        return new Dictionary<long, List<ACI>>();
        //    Dictionary<long, List<ACI>> acisDic = new Dictionary<long, List<ACI>>();
        //    StringUtility stringUtil = new StringUtility();
        //    string query = string.Format("SELECT * FROM {0} WHERE {1} in ({2})"
        //        , DataSourceACI.tableName, DataSourceACI.dsid, stringUtil.SeperateIDsByComma(dataSourceIDs));
        //    using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
        //    {
        //        conn.Open();
        //        NpgsqlCommand command = new NpgsqlCommand(query, conn);
        //        command.CommandTimeout = RequestsTimeout;
        //        command.AllResultTypesAreUnknown = true;
        //        var reader = command.ExecuteReader();
        //        acisDic = NpgsqlDataReaderToDataSourceACI(reader);
        //    }
        //    return acisDic;
        //}

        //private Dictionary<long, List<ACI>> NpgsqlDataReaderToDataSourceACI(NpgsqlDataReader reader)
        //{
        //    Dictionary<long, List<ACI>> acisDic = new Dictionary<long, List<ACI>>();
        //    while (reader.Read())
        //    {
        //        long dsid = long.Parse(reader[DataSourceACI.dsid].ToString());
        //        string groupname = reader[DataSourceACI.groupname].ToString();
        //        long permission = long.Parse(reader[DataSourceACI.permission].ToString());
        //        if (!acisDic.ContainsKey(dsid))
        //        {
        //            List<ACI> aciTempList = new List<ACI>();
        //            aciTempList.Add(new ACI()
        //            {
        //                GroupName = groupname,
        //                AccessLevel = (Permission)permission
        //            });
        //            acisDic.Add(dsid, aciTempList);
        //        }
        //        else
        //        {
        //            acisDic[dsid].Add(new ACI()
        //            {
        //                GroupName = groupname,
        //                AccessLevel = (Permission)permission
        //            });
        //        }

        //    }
        //    return acisDic;
        //}

        //private List<long> GetTopAcis(long topN)
        //{
        //    if (topN <= 0)
        //        return new List<long>();
        //    List<long> dataSourceIds = new List<long>();
        //    string query = string.Format("SELECT {0} FROM {1} LIMIT({2})"
        //        , DataSource.id, DataSource.tableName, topN);
        //    using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
        //    {
        //        conn.Open();
        //        NpgsqlCommand command = new NpgsqlCommand(query, conn);
        //        command.CommandTimeout = RequestsTimeout;
        //        command.AllResultTypesAreUnknown = true;
        //        var reader = command.ExecuteReader();
        //        dataSourceIds = NpgsqlDataReaderToDataSourceId(reader);
        //    }
        //    return dataSourceIds;
        //    throw new NotImplementedException();
        //}

        //private List<long> NpgsqlDataReaderToDataSourceId(NpgsqlDataReader reader)
        //{
        //    List<long> dataSourceIds = new List<long>();
        //    while (reader.Read())
        //    {
        //        long id = long.Parse(reader[DataSource.id].ToString());
        //        dataSourceIds.Add(id);
        //    }
        //    return dataSourceIds;
        //}

        //private List<DataSourceInfo> NpgsqlDataReaderToDataSourceInfo(NpgsqlDataReader reader)
        //{
        //    List<DataSourceInfo> dataSourcesInformation = new List<DataSourceInfo>();
        //    while (reader.Read())
        //    {
        //        List<long> ids = new List<long>() { long.Parse(reader[DataSource.id].ToString()) };
        //        DataSourceInfo dataSource = new DataSourceInfo()
        //        {
        //            Id = long.Parse(reader[DataSource.id].ToString()),
        //            Description = reader[DataSource.description].ToString(),
        //            Name = reader[DataSource.dsname].ToString(),
        //            Type = long.Parse(reader[DataSource.sourceType].ToString()),
        //            CreatedBy = reader[DataSource.createdBy].ToString(),
        //            CreatedTime= reader[DataSource.createdTime].ToString(),
        //            Acl = new ACL()
        //            {
        //                Classification = reader[DataSource.classification].ToString(),
        //                Permissions = GetACIs(ids)[ids.FirstOrDefault()]
        //            }
        //        };
        //        dataSourcesInformation.Add(dataSource);
        //    }
        //    return dataSourcesInformation;
        //}

        //private List<DataSourceInfo> GetDataSourcesInformationByIDRange(long firstID, long lastID)
        //{
        //    List<DataSourceInfo> dataSourcesInformation;
        //    string query = $"SELECT * FROM {DataSource.tableName} WHERE ( {DataSource.id} >= {firstID} and {DataSource.id} <= {lastID} )";
        //    using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
        //    {
        //        conn.Open();
        //        NpgsqlCommand command = new NpgsqlCommand(query, conn);
        //        command.CommandTimeout = RequestsTimeout;
        //        command.AllResultTypesAreUnknown = true;
        //        var reader = command.ExecuteReader();
        //        dataSourcesInformation = NpgsqlDataReaderToDataSourceInfo(reader);
        //    }

        //    return dataSourcesInformation;
        //}


        //private List<DataSourceInfo> GetDataSourcesInformationByIDs(List<long> ids)
        //{
        //    List<DataSourceInfo> dataSourcesInformation;
        //    StringUtility stringUtil = new StringUtility();
        //    string query = $"SELECT * FROM {DataSource.tableName} WHERE  {DataSource.id} IN ( {stringUtil.SeperateIDsByComma(ids)} )";
        //    using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
        //    {
        //        conn.Open();
        //        NpgsqlCommand command = new NpgsqlCommand(query, conn);
        //        command.CommandTimeout = RequestsTimeout;
        //        command.AllResultTypesAreUnknown = true;
        //        var reader = command.ExecuteReader();
        //        dataSourcesInformation = NpgsqlDataReaderToDataSourceInfo(reader);
        //    }

        //    return dataSourcesInformation;
        //}
        //#endregion

        //#region public functions
        //public List<DBDataSourceACL> RetrieveDataSourceACLs(long[] dataSourceIDs)
        //{
        //    Dictionary<long, List<ACI>> acisDic = GetACIs(dataSourceIDs.ToList());
        //    return GetDBDataSourceACLs(dataSourceIDs.ToList(), acisDic);
        //}

        //public List<DBDataSourceACL> RetrieveTopNDataSourceACLs(long topN)
        //{
        //    List<long> topAcisList = GetTopAcis(topN);
        //    Dictionary<long, List<ACI>> acisDic = GetACIs(topAcisList.ToList());
        //    return GetDBDataSourceACLs(topAcisList.ToList(), acisDic);
        //}

        //public List<DataSourceInfo> RetriveDataSourcesSequentialIDByIDRange(long firstID, long lastID)
        //{
        //    if (firstID > lastID)
        //    {
        //        throw new InvalidOperationException("Wrong Range");
        //    }
        //    return GetDataSourcesInformationByIDRange(firstID, lastID);
        //}

        //public List<DataSourceInfo> GetDataSourcesByIDs(List<long> ids)
        //{
        //    if (!ids.Any())
        //    {
        //        throw new InvalidOperationException("id list is empty");
        //    }
        //    return GetDataSourcesInformationByIDs(ids);
        //}
        //#endregion
    }
}
