using GPAS.AccessControl.Users;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;

namespace GPAS.Dispatch.DataAccess
{
    public class GroupMembershipDatabaseAccess
    {
        private static string UserAccountDatabaseUserName = ConfigurationManager.AppSettings["UserAccountDatabaseUserName"];
        private static string UserAccountDatabasePassword = ConfigurationManager.AppSettings["UserAccountDatabasePassword"];
        private static string UserAccountDataBaseName = "UserAccountControlDB";
        private static string GroupTableName = "GroupMembership";

        private string CreationTableQuery
        {
            get
            {
                return string.Format(" if not exists(SELECT * from sys.tables where name = \'{0}\')  CREATE TABLE {0} (Id int IDENTITY(1,1) PRIMARY KEY,GroupName nvarchar(250) NOT NULL, UserName nvarchar(250) NOT NULL);", GroupTableName);
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

        private List<UserInfo> GetUserAccountInfo(string query)
        {
            List<UserInfo> userAccounts = new List<UserInfo>();
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            UserInfo userAccount = new UserInfo()
                            {
                                UserName = reader["UserName"].ToString(),
                                FirstName = reader["FirstName"].ToString(),
                                LastName = reader["LastName"].ToString(),
                                Email = reader["Email"].ToString(),
                                CreatedTime = reader["CreatedTime"].ToString(),
                                Id = Int32.Parse(reader["Id"].ToString())
                            };
                            userAccounts.Add(userAccount);
                        }
                    }
                }
            }
            return userAccounts;
        }

     

        public List<UserInfo> GetMembershipUsers(string groupName)
        {
            string query = string.Format("SELECT * FROM Users WHERE UserName in ( SELECT UserName FROM {0} WHERE GroupName='{1}');", GroupTableName, groupName);

            return GetUserAccountInfo(query);
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

        private string GenerateNewGroupQuery(int groupId, int userId)
        {
            return $"if not exists (SELECT * FROM GroupMembership where GroupName in (select GroupName from Groups where id = \'{groupId}\') and UserName in (select UserName from Users where id = \'{userId}\')) insert into GroupMembership (GroupName , UserName) values ((select GroupName from Groups where id = \'{groupId}\'), ( select UserName from Users where id = \'{userId}\'));";
        }

        private string GenerateNewGroupQuery(string groupName, string userName)
        {
            return $"if not exists (SELECT * FROM GroupMembership where GroupName=\'{groupName}\' and UserName=\'{userName}\') insert into GroupMembership (GroupName , UserName) values (\'{groupName}\' , \'{userName}\')";

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

        public void CreateNewMembership(int groupId, int userId)
        {
            string query = GenerateNewGroupQuery(groupId, userId);
            ExecuteQuery(query);
        }

        public void CreateNewMembership(string groupName, string userName)
        {
            string query = GenerateNewGroupQuery(groupName, userName);
            ExecuteQuery(query);
        }
        public string[] GetGroupsOfUser(string username)
        {
            string query = GenerateGroupsOfUserQuery(username);
            List<string> groups = new List<string>();
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while(reader.Read())
                        {
                            groups.Add(reader["GroupName"].ToString());
                        }
                    }
                }
                connection.Close();
            }
            return groups.ToArray();
        }
        private string GenerateGroupsOfUserQuery(string username)
        {
            return $"SELECT GroupName FROM {GroupTableName} where UserName = \'{username} \';";
        }
        private string RevokeMembershipOfUserQuery(string userName, string groupName)
        {
            return $"DELETE from GroupMembership where GroupName = N\'{groupName}\' and UserName = N\'{userName}\'";
        }

        private string RemoveMembershipQuery(string groupName)
        {
            return $"DELETE from GroupMembership where GroupName = N\'{groupName}\'";
        }
        public void RevokeMembership(string userName, string groupName)
        {
            string query = RevokeMembershipOfUserQuery(userName, groupName);
            ExecuteQuery(query);
        }

        public void RemoveMembership(string groupName)
        {
            string query = RemoveMembershipQuery(groupName);
            ExecuteQuery(query);
        }
    }
}
