using GPAS.AccessControl;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace GPAS.Dispatch.DataAccess
{
    public class ClassificationBasedGroupPermissionsDataAccess
    {
        private static string UserAccountDatabaseUserName = ConfigurationManager.AppSettings["UserAccountDatabaseUserName"];
        private static string UserAccountDatabasePassword = ConfigurationManager.AppSettings["UserAccountDatabasePassword"];
        private static string UserAccountDataBaseName = "UserAccountControlDB";
        private static string ClassificationGroupTableName = "ClassificationBasedGroupPermissions";
        private List<string> transactionCommands;
        #region private functions
        private SqlConnection GetConnection()
        {
            return new SqlConnection(GetConnectionString());
        }
        private string GetConnectionString()
        {
            return string.Format("Data Source=localhost\\SqlExpress; Initial catalog={0};User id={1}; Password={2};"
                                     , UserAccountDataBaseName, UserAccountDatabaseUserName, UserAccountDatabasePassword);
        }
        private string CreationTableQuery
        {
            get
            {
                return $" if not exists(SELECT * from sys.tables where name = \'{ClassificationGroupTableName}\')  CREATE TABLE {ClassificationGroupTableName} (GroupName NVARCHAR(250),ClassificationIdentifier NVARCHAR(250), Permission VARCHAR(2)  , PRIMARY Key(GroupName,ClassificationIdentifier)) ";
            }
        }
        private string CreationDatabaseQuery
        {
            get
            {
                return $" if not exists(select * from sys.databases where name = \'{UserAccountDataBaseName}\') create database {UserAccountDataBaseName}";
            }
        }
        private string GetGroupPermissionQuery(string groupName)
        {
            return $"SELECT ClassificationIdentifier,Permission FROM {ClassificationGroupTableName} WHERE GroupName = '{groupName}'";
        }
        private string InsertGroupPermissionQuery(string groupName, string classificationIdentifier, Permission permission)
        {

            return $"insert into {ClassificationGroupTableName} (GroupName, ClassificationIdentifier , Permission) values (N'{groupName}', N'{classificationIdentifier}', '{(byte)permission}');";
        }
        private string DeleteGroupPermissions(string groupName)
        {
            return $"delete from {ClassificationGroupTableName} WHERE GroupName ='{groupName}'";
        }
        private AuthorizationParametters GetUserAuthorizationParamettersByQuery(string query)
        {
            AuthorizationParametters result = new AuthorizationParametters();
            result.permittedGroupNames = new List<string>();
            result.readableClassifications = new List<string>();
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string groupName = reader["GroupName"].ToString();
                            if (!result.permittedGroupNames.Contains(groupName))
                            {
                                result.permittedGroupNames.Add(groupName);
                            }
                            string classificationID = reader["ClassificationIdentifier"].ToString();
                            if (!result.readableClassifications.Contains(classificationID))
                            {
                                result.readableClassifications.Add(classificationID);

                                ClassificationEntry eqivalentEntry = Classification.GetClassificationEntryByIdentifier(classificationID);
                                List<ClassificationEntry> lowerEtries = Classification.GetClassificationsWithLowerPriorityThan(eqivalentEntry);
                                foreach (var entry in lowerEtries)
                                {
                                    if (!result.readableClassifications.Contains(entry.IdentifierString))
                                    {
                                        result.readableClassifications.Add(entry.IdentifierString);
                                    }
                                }
                            }
                        }
                    }
                }
                connection.Close();
            }
            return result;
        }

        private string GenerateUserAuthorizationParamettersQuery(string userName)
        {
            return string.Format("select b.GroupName, d.ClassificationIdentifier from [dbo].[GroupMembership] as b" +
                " join [dbo].[ClassificationBasedGroupPermissions] as d on b.GroupName = d.GroupName where b.UserName = N'{0}' and d.Permission >= 2 ", userName);
        }

        private List<ClassificationBasedPermission> GetAllPermissionByQuery(string query)
        {
            List<ClassificationBasedPermission> classificationPermissionList = new List<ClassificationBasedPermission>();
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string classification = reader["ClassificationIdentifier"].ToString();
                            Permission permission = (Permission)long.Parse(reader["Permission"].ToString());
                            ClassificationBasedPermission classificationPermission = new ClassificationBasedPermission {
                                AccessLevel = permission,
                                Classification = Classification.GetClassificationEntryByIdentifier(classification)
                                } ;
                            classificationPermissionList.Add(classificationPermission);
                        }
                    }
                }
                connection.Close();
            }

            return classificationPermissionList;
        }
        #endregion
        #region public functions
        public void CreateDataBase()
        {
            using (SqlConnection connection = new SqlConnection(
                string.Format("server=(local)\\SQLEXPRESS;Initial Catalog= master;User id={0}; Password={1};",
                UserAccountDatabaseUserName, UserAccountDatabasePassword)))
            {
                connection.Open();
                using (SqlCommand comand = new SqlCommand(CreationDatabaseQuery, connection))
                {
                    comand.ExecuteNonQuery();
                }
            }
        }
        public void CreateTable()
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(CreationTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
                connection.Close();
            }
        }
        public void ApplyChanges()
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                SqlCommand command = connection.CreateCommand();
                SqlTransaction transaction = connection.BeginTransaction("ClassificationPermission"); ;
                command.Connection = connection;
                command.Transaction = transaction;
                try
                {
                    if (transactionCommands.Count != 0)
                    {
                        foreach (string query in transactionCommands)
                        {
                            command.CommandText = query;
                            command.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
                connection.Close();
            }

        }
        public void UpdateGroupClassificationBasedPermissions(string groupName, List<KeyValuePair<string, Permission>> permissionPerClassification)
        {
            transactionCommands = new List<string>();
            transactionCommands.Add(DeleteGroupPermissions(groupName));
            foreach (KeyValuePair<string, Permission> permissionClassfication in permissionPerClassification)
            {
                transactionCommands.Add(InsertGroupPermissionQuery(groupName, permissionClassfication.Key, permissionClassfication.Value));
            }
        }
        public List<ClassificationBasedPermission> GetPermissionsForGroup(string groupName)
        {
            List<ClassificationBasedPermission> permissions = GetAllPermissionByQuery(GetGroupPermissionQuery(groupName));
            return permissions;
        }

        public AuthorizationParametters GetUserAuthorizationParametters(string userName)
        {
            return GetUserAuthorizationParamettersByQuery(GenerateUserAuthorizationParamettersQuery(userName));
        }
        #endregion
    }
}
