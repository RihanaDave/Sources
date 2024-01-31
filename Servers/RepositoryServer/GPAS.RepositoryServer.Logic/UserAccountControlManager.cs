using GPAS.AccessControl;
using GPAS.RepositoryServer.Data.DatabaseTables;
using Npgsql;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using GPAS.RepositoryServer.Entities;
using System;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition;

namespace GPAS.RepositoryServer.Logic
{
    public class UserAccountControlManager
    {
        private static string PluginPath = null;
        private const string StoragePluginName = "PluginRelativePath";

        private CompositionContainer compositionContainer;

        [Import(typeof(IAccessClient))]
        public IAccessClient StorageClient;

        public UserAccountControlManager()
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

        public void RegisterNewDataSource(long dsId, string name, DataSourceType type, ACL acl, string description, string createBy, string createdTime)
        {
            StorageClient.RegisterNewDataSource(dsId, name, type, acl, description, createBy, createdTime);
        }

        public long[] GetSubsetOfConceptsByPermission(ConceptType conceptType, long[] ids, string[] groupNames, Permission minimumPermission)
        {
            return StorageClient.GetSubsetOfConceptsByPermission(conceptType, ids, groupNames, minimumPermission);
        }

        //----------------------------------------------------------------------------------------------
        private static string ConnctionString = ConnectionStringManager.GetRepositoryDBConnectionString();
        private static readonly int RequestsTimeout = 10800;

        //private void AddNewDataSource(long dsId, string name, DataSourceType type, ACL acl, string description, string createBy, string createdTime, NpgsqlConnection connection, NpgsqlTransaction transaction)
        //{
        //    string nameParameterVariable = "na";
        //    string descriptionParameterVariable = "ds";
        //    string ceatedByParameterVariable = "cb";
        //    string createdTimeParameterVariable = "ct";
        //    string classificationParameterVariable = "cl";

        //    string query
        //        = $"INSERT INTO {DataSource.tableName}"
        //        + $" ({DataSource.id},{DataSource.dsname},{DataSource.description},{DataSource.classification},{DataSource.sourceType},{DataSource.createdBy},{DataSource.createdTime}) VALUES"
        //        + $" ({dsId},:{nameParameterVariable},:{descriptionParameterVariable},:{classificationParameterVariable},{(byte)type}, :{ceatedByParameterVariable},:{createdTimeParameterVariable});";

        //    NpgsqlCommand command = new NpgsqlCommand(query, connection, transaction);
        //    command.Parameters.AddWithValue(nameParameterVariable, name); // Parameters are used to avoid strings invalid characters bad effects on query
        //    command.Parameters.AddWithValue(descriptionParameterVariable, description);
        //    command.Parameters.AddWithValue(ceatedByParameterVariable, createBy);
        //    command.Parameters.AddWithValue(createdTimeParameterVariable, createdTime);
        //    command.Parameters.AddWithValue(classificationParameterVariable, acl.Classification);
        //    command.ExecuteNonQuery();
        //}

        //private void AddDataSourceAcis(long dsId, List<ACI> acis, NpgsqlConnection connection, NpgsqlTransaction transaction)
        //{
        //    if (acis.Count < 1)
        //        return;

        //    StringBuilder queryBuilder = new StringBuilder();
        //    queryBuilder.Append
        //        ($"INSERT INTO {DataSourceACI.tableName}"
        //        + $" ({DataSourceACI.dsid},{DataSourceACI.groupname},{DataSourceACI.permission}) VALUES ");

        //    var parameters = new List<NpgsqlParameter>(); // Parameters are used to avoid strings invalid characters bad effects on query
        //    for (int i = 0; i < acis.Count; i++)
        //    {
        //        ACI aci = acis[i];

        //        string groupnameParameterVariable = "g" + i.ToString();
        //        string permissionParameterVariable = "p" + i.ToString();

        //        queryBuilder.Append($"({dsId},:{groupnameParameterVariable},{(byte)aci.AccessLevel}),");
        //        parameters.Add(new NpgsqlParameter(groupnameParameterVariable, aci.GroupName));
        //    }
        //    queryBuilder.Remove(queryBuilder.Length - 1, 1);
        //    queryBuilder.Append(';');

        //    NpgsqlCommand command = new NpgsqlCommand(queryBuilder.ToString(), connection, transaction);
        //    command.Parameters.AddRange(parameters.ToArray());
        //    command.ExecuteNonQuery();
        //}

        //public void RegisterNewDataSource(long dsId, string name, DataSourceType type, ACL acl, string description, string createBy, string createdTime)
        //{
        //    NpgsqlConnection connection = new NpgsqlConnection(ConnctionString);
        //    try
        //    {
        //        connection.Open();
        //        using (NpgsqlTransaction transaction = connection.BeginTransaction())
        //        {
        //            AddNewDataSource(dsId, name, type, acl, description, createBy, createdTime ,connection, transaction);
        //            AddDataSourceAcis(dsId, acl.Permissions, connection, transaction);
        //            transaction.Commit();
        //        }
        //    }
        //    finally
        //    {
        //        if (connection != null)
        //            connection.Close();
        //    }
        //}

        //public long[] GetSubsetOfConceptsByPermission(ConceptType conceptType, long[] ids, string[] groupNames, Permission minimumPermission)
        //{
        //    switch (conceptType)
        //    {
        //        case ConceptType.Media:
        //            {
        //                return GetSubsetOfTableByPermission(MediaTable.tableName, ids, groupNames, minimumPermission);
        //            }
        //        case ConceptType.Property:
        //            {
        //                return GetSubsetOfTableByPermission(PropertyTable.tableName, ids, groupNames, minimumPermission);
        //            }
        //        case ConceptType.Relationship:
        //            {
        //                return GetSubsetOfTableByPermission(RelationshipTable.tableName, ids, groupNames, minimumPermission);
        //            }
        //        case ConceptType.Object:
        //            {
        //                return GetSubsetOfObjectConceptByPermission(ids, groupNames, minimumPermission);
        //            }

        //        default:
        //            throw new NotSupportedException();
        //    }
        //}

        //private long[] GetSubsetOfTableByPermission(string tableName, long[] ids, string[] groupNames, Permission minimumPermission)
        //{
        //    List<long> permittedIds = new List<long>();
        //    if (!ids.Any())
        //        return permittedIds.ToArray();
        //    if (groupNames.Length == 0)
        //    {
        //        throw new Exception("groupNames is empty.");
        //    }
        //    string groupNamesPart = CreateGroupNamePart(groupNames, minimumPermission);
        //    string groupNamesCommaString = CreateCommaSeprateFromArray(groupNames);
        //    string idCommaString = CreateCommaSeprateFromArray(ids);
        //    string query = $"SELECT id FROM {tableName} where id in ({idCommaString}) and dsid in ({groupNamesPart})";
        //    NpgsqlConnection connection = null;
        //    try
        //    {
        //        using (connection = new NpgsqlConnection(ConnctionString))
        //        {
        //            connection.Open();
        //            NpgsqlCommand command = new NpgsqlCommand(query, connection);
        //            command.CommandTimeout = RequestsTimeout;
        //            command.AllResultTypesAreUnknown = true;
        //            var reader = command.ExecuteReader();
        //            permittedIds = GetIDsFromResult(reader);
        //        }
        //    }
        //    finally
        //    {
        //        if (connection != null)
        //            connection.Close();
        //    }
        //    return permittedIds.ToArray();
        //}

        //private string CreateGroupNamePart(string[] groupNames, Permission minimumPermission)
        //{
        //    string groupNamePart = string.Empty;
        //    groupNamePart = string.Format("select dsid from {0} WHERE {1} >= {2} and (", DataSourceACI.tableName, DataSourceACI.permission, (byte)minimumPermission);
        //    groupNamePart += string.Format("dsid in (select dsid from {0} WHERE ", DataSourceACI.tableName);
        //    foreach (var currentGroup in groupNames)
        //    {
        //        groupNamePart += string.Format("{0} = '{1}' or ", DataSourceACI.groupname, currentGroup);
        //    }
        //    groupNamePart = groupNamePart.Remove(groupNamePart.Length - 4, 4);
        //    groupNamePart += "))";
        //    return groupNamePart;
        //}

        private static string CreateCommaSeprateFromArray<T>(T[] array)
        {
            string commaSeprateString = string.Empty;
            for (int i = 0; i < array.Length; i++)
            {
                commaSeprateString += $"'{array[i]}'";
                if (i < array.Length - 1)
                    commaSeprateString += " , ";
            }
            return commaSeprateString;
        }

        //private long[] GetSubsetOfObjectConceptByPermission(long[] ids, string[] groupNames, Permission minimumPermission)
        //{
        //    ProperyManager propertyManager = new ProperyManager();
        //    List<DBProperty> propertiesOfObjectIDs = propertyManager.GetPropertiesOfObjectsWithoutAuthorizationParameters(ids);
        //    Dictionary<long, long> PropertyToObjectIdMapping = new Dictionary<long, long>();
        //    foreach (var currentProperty in propertiesOfObjectIDs)
        //    {
        //        PropertyToObjectIdMapping.Add(currentProperty.Id, currentProperty.Owner.Id);
        //    }
        //    long[] propertiesInPermission = GetSubsetOfTableByPermission(PropertyTable.tableName, PropertyToObjectIdMapping.Keys.ToArray(), groupNames, minimumPermission);
        //    HashSet<long> permittedObjectIds = new HashSet<long>();
        //    foreach (long currentPropertyId in propertiesInPermission)
        //    {
        //        long objectId;
        //        if (PropertyToObjectIdMapping.ContainsKey(currentPropertyId))
        //        {
        //            PropertyToObjectIdMapping.TryGetValue(currentPropertyId, out objectId);
        //            permittedObjectIds.Add(objectId);
        //        }

        //    }
        //    return permittedObjectIds.ToArray();
        //}

        internal HashSet<long> GetReadableDataSourceIDsByAuthParams(long[] dataSourceIDs, AuthorizationParametters authParams)
        {
            HashSet<long> readableIds = new HashSet<long>();
            if (!dataSourceIDs.Any())
                return readableIds;
            string commaSperatedClassification = CreateCommaSeprateFromArray(authParams.readableClassifications.ToArray());
            string commaSepratedDataSourceId = CreateCommaSeprateFromArray(dataSourceIDs);
            string commaSepratedGroupNames = CreateCommaSeprateFromArray(authParams.permittedGroupNames.ToArray());
            string query = $"SELECT {DataSource.id} FROM {DataSource.tableName} where ({DataSource.classification} in ({commaSperatedClassification})) and " +
                $"({DataSource.id} in (select {DataSourceACI.dsid} from {DataSourceACI.tableName} where ({DataSourceACI.dsid} in ({commaSepratedDataSourceId})) and ({DataSourceACI.permission} >= {(byte)Permission.Read}) and ({DataSourceACI.groupname} in ({commaSepratedGroupNames}))))";
            NpgsqlConnection connection = null;
            try
            {
                using (connection = new NpgsqlConnection(ConnctionString))
                {
                    connection.Open();
                    NpgsqlCommand command = new NpgsqlCommand(query, connection);
                    command.CommandTimeout = RequestsTimeout;
                    command.AllResultTypesAreUnknown = true;
                    var reader = command.ExecuteReader();
                    readableIds = new HashSet<long>(GetIDsFromResult(reader));
                }
            }
            finally
            {
                if (connection != null)
                    connection.Close();
            }
            return readableIds;
        }

        private List<long> GetIDsFromResult(NpgsqlDataReader reader)
        {
            List<long> ids = new List<long>();
            while (reader.Read())
            {
                ids.Add(long.Parse(reader["id"].ToString()));
            }
            return ids;
        }
    }
}
