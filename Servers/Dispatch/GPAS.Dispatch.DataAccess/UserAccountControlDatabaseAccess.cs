using GPAS.AccessControl.Users;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;

namespace GPAS.Dispatch.DataAccess
{
    public class UserAccountControlDatabaseAccess
    {
        private static string UserAccountDatabaseUserName = ConfigurationManager.AppSettings["UserAccountDatabaseUserName"];
        private static string UserAccountDatabasePassword = ConfigurationManager.AppSettings["UserAccountDatabasePassword"];
        private static string UserAccountDataBaseName = "UserAccountControlDB";
        private static string UserAccountTableName = "Users";

        #region private functions
        private string CreationTableQuery
        {
            get
            {
                return string.Format(" if not exists(SELECT * from sys.tables where name = \'{0}\')  CREATE TABLE {0} (Id int IDENTITY(1,1) PRIMARY KEY,UserName NVARCHAR(250), Password NVARCHAR(250), FirstName NVARCHAR(250), LastName NVARCHAR(250), Email VARCHAR(250), CreatedTime NVARCHAR(250));", UserAccountTableName);
            }
        }

        private string CreationDatabaseQuery
        {
            get
            {
                return string.Format(" if not exists(select * from sys.databases where name = \'{0}\') create database {0}", UserAccountDataBaseName);
            }
        }

        private string InsertionAdminUserQuery
        {
            get
            {
                return string.Format("insert into {0} (UserName, Password , FirstName, LastName ,Email ,CreatedTime) values ('{1}', '{2}', '{1}', '{1}', '{1}@Email.com' , '{3}' );"
                    , UserAccountTableName, NativeUser.Admin.ToString(), Base64Encode(GetHash("admin")), DateTime.Now.ToString());
            }
        }

        private string CheckAdminUserQuery
        {
            get
            {
                return string.Format("SELECT * FROM {0} WHERE UserName='{1}';"
                    , UserAccountTableName, NativeUser.Admin.ToString());
            }

        }

        private string CreationIndexQuery
        {
            get
            {
                return string.Format("IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'UserNameIndex') CREATE INDEX UserNameIndex ON {0}(UserName);", UserAccountTableName);
            }

        }

        private string GetHash(string inputString)
        {
            HashAlgorithm algorithm = MD5.Create();
            return Encoding.UTF8.GetString(algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString)));
        }

        private string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
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

        private string GenerateUserNameExistsQuery(string userName)
        {
            return string.Format("SELECT * FROM {0} WHERE UserName='{1}'", UserAccountTableName, userName);
        }

        private string GenerateNewAccountQuery(string userName, string password, string firstName, string lastName, string email)
        {
            return string.Format("insert into {0} (UserName, Password , FirstName, LastName ,Email ,CreatedTime) values ('{1}', '{2}', '{3}', '{4}', '{5}' , '{6}' );", UserAccountTableName, userName, password, firstName, lastName, email, DateTime.Now.ToString());
        }

        private string GenerateDeleteAccountQuery(int id)
        {
            return string.Format("DELETE FROM {0} WHERE Id={1}", UserAccountTableName, id);
        }

        private string GenerateUpdatePasswordQuery(string userName, string newPassword)
        {
            return string.Format("UPDATE {0} SET Password='{1}' WHERE UserName='{2}'", UserAccountTableName, newPassword, userName);
        }

        private string GenerateUpdateProfileQuery(
            string userName,
            string newFirstName, string newLastName, string newEmail
            )
        {
            return string.Format("UPDATE {0} SET FirstName='{1}', LastName='{2}', Email ='{3}' WHERE UserName='{4}';", UserAccountTableName, newFirstName, newLastName, newEmail, userName);
        }

        private string GenerateGetUserAccountsQuery()
        {

            return string.Format("SELECT * FROM {0} ;", UserAccountTableName);
        }

        private List<UserInfo> GetAllUserAccount(string Query)
        {
            List<UserInfo> userAccounts = new List<UserInfo>();
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(Query, connection))
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

        public bool CheckAdminUserAccount()
        {
            bool flag = false;
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(CheckAdminUserQuery, connection))
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

        public void CreateIndex()
        {
            ExecuteQuery(CreationIndexQuery);
        }

        public void InsertAdminUser()
        {
            ExecuteQuery(InsertionAdminUserQuery);
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

        public bool CheckUserAccount(string userName, string password)
        {
            string query = string.Format("SELECT * FROM {0} WHERE UserName = '{1}' AND Password = '{2}'", UserAccountTableName, userName, password);

            bool flag = false;
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                SqlCommand command = new SqlCommand(query, connection);
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    flag = true;
                    break;
                }
            }
            return flag;
        }

        public void CreateNewAccount(string userName, string password, string firstName, string lastName, string email)
        {
            string query = GenerateNewAccountQuery(userName, password, firstName, lastName, email);
            ExecuteQuery(query);
        }

        public void DeleteAccount(int id)
        {
            string query = GenerateDeleteAccountQuery(id);
            ExecuteQuery(query);
        }

        public bool CheckUserAccountExists(string userName)
        {
            bool flag = false;
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                string UserNameExistsQuery = GenerateUserNameExistsQuery(userName);
                using (SqlCommand command = new SqlCommand(UserNameExistsQuery, connection))
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

        public void ChangeUserAccountProfile(
            string userName,
            string newFirstName, string newLastName, string newEmail
            )
        {
            string UpdateProfileQuery = GenerateUpdateProfileQuery(userName, newFirstName, newLastName, newEmail);
            ExecuteQuery(UpdateProfileQuery);
        }

        public void ChangePassword(string userName, string newPassword)
        {
            string updatePasswordQuery = GenerateUpdatePasswordQuery(userName, newPassword);
            ExecuteQuery(updatePasswordQuery);
        }

        public List<UserInfo> GetUserAccounts()
        {
            string getUserAccountsQuery = GenerateGetUserAccountsQuery();
            List<UserInfo> userAccounts = GetAllUserAccount(getUserAccountsQuery);
            return userAccounts;
        }

        #endregion
    }
}
