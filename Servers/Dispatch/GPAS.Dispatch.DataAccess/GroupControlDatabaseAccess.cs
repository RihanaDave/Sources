using GPAS.AccessControl.Groups;
using GPAS.AccessControl.Users;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace GPAS.Dispatch.DataAccess
{
    public  class GroupControlDatabaseAccess
    {
        private static string UserAccountDatabaseUserName = ConfigurationManager.AppSettings["UserAccountDatabaseUserName"];
        private static string UserAccountDatabasePassword = ConfigurationManager.AppSettings["UserAccountDatabasePassword"];
        private static string UserAccountDataBaseName = "UserAccountControlDB";
        private static string GroupTableName = "Groups";

        #region private functions
        private string CreationTableQuery
        {
            get
            {
                return string.Format(" if not exists(SELECT * from sys.tables where name = \'{0}\')  CREATE TABLE {0} (Id int IDENTITY(1,1) PRIMARY KEY,GroupName NVARCHAR(250), Description NVARCHAR(450), CreatedBy NVARCHAR(250), CreatedTime NVARCHAR(250));", GroupTableName);
            }
        }

        private string CreationDatabaseQuery
        {
            get
            {
                return string.Format(" if not exists(select * from sys.databases where name = \'{0}\') create database {0}", UserAccountDataBaseName);
            }
        }
        
        private string CreationIndexQuery
        {
            get
            {
                return string.Format("IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'GroupNameIndex') CREATE INDEX GroupNameIndex ON {0}(GroupName);", GroupTableName);
            }

        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(GetConnectionString());
        }

        private string GetConnectionString()
        {
            return string.Format("Data Source=localhost\\SqlExpress; Initial catalog={0};User id={1}; Password={2};"
                                     , UserAccountDataBaseName, UserAccountDatabaseUserName, UserAccountDatabasePassword);
        }

        private string GenerateGroupNameExistsQuery(string groupName)
        {
            return string.Format("SELECT * FROM {0} WHERE GroupName='{1}'", GroupTableName, groupName);
        }

        private string GenerateNewGroupQuery(string groupName, string description, string createdBy)
        {
            return string.Format("insert into {0} (GroupName, Description , CreatedBy, CreatedTime) values ('{1}', '{2}', '{3}', '{4}');", GroupTableName, groupName, description, createdBy, DateTime.Now.ToString());
        }

        private void ExecuteQuery(string query)
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private string GenerateGetUserAccountsQuery()
        {
            return string.Format("SELECT * FROM {0} ;", GroupTableName);
        }

        private List<GroupInfo> GetAllGroup(string Query)
        {
            List<GroupInfo> groups = new List<GroupInfo>();
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(Query, connection))
                {

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            GroupInfo userAccount = new GroupInfo()
                            {
                                GroupName = reader["GroupName"].ToString(),
                                Description = reader["Description"].ToString(),
                                CreatedBy = reader["CreatedBy"].ToString(),
                                CreatedTime = reader["CreatedTime"].ToString(),
                                Id = Int32.Parse(reader["Id"].ToString())
                            };
                            groups.Add(userAccount);
                        }
                    }
                }
            }
            return groups;
        }

        #endregion

        #region public functions
        public void CreateDataBase()
        {
            using (SqlConnection connection = new SqlConnection(
                string.Format("server=(local)\\SQLEXPRESS;Initial Catalog= master;User id={0}; Password={1};",
                UserAccountDatabaseUserName, UserAccountDatabasePassword))
                )
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(CreationDatabaseQuery, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void CreateTable()
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(CreationTableQuery, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }
        
        public void CreateIndex()
        {
            ExecuteQuery(CreationIndexQuery);
        }

        public void CreateNewGroup(string groupName, string description, string createdBy)
        {
            string query = GenerateNewGroupQuery(groupName, description, createdBy);
            ExecuteQuery(query);
        }

        public bool CheckGroupExists(string groupName)
        {
            bool flag = false;
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                string groupNameExistsQuery = GenerateGroupNameExistsQuery(groupName);
                using (SqlCommand command = new SqlCommand(groupNameExistsQuery, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            flag = true;
                            break;
                        }
                    }
                }
            }
            return flag;
        }

        public List<GroupInfo> GetGroups()
        {
            string getUserAccountsQuery = GenerateGetUserAccountsQuery();
            List<GroupInfo> userAccounts = GetAllGroup(getUserAccountsQuery);
            return userAccounts;
        }


        public void DeleteGroup(int id)
        {
            string query = GenerateDeleteGroupQuery(id);
            ExecuteQuery(query);
        }

        private string GenerateDeleteGroupQuery(int id)
        {
            return string.Format("DELETE FROM {0} WHERE Id={1}",GroupTableName , id);
        }

        #endregion

    }
}
