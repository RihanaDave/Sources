using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Configuration;
using GPAS.Dispatch.Entities;

namespace GPAS.Dispatch.DataAccess
{
    public class InvestigationDatabaseAccess
    {
        private static readonly string InvestigationDatabaseUserName = ConfigurationManager.AppSettings["UserAccountDatabaseUserName"];
        private static readonly string InvestigationDatabasePassword = ConfigurationManager.AppSettings["UserAccountDatabasePassword"];
        private static readonly string InvestigationDataBaseName = "InvestigationDB";

        #region private functions
        private string CreationTableQuery
        {
            get
            {
                return string.Format(" if not exists(SELECT * from sys.tables where name = \'{0}\')  CREATE TABLE {0} ({1} bigint PRIMARY KEY,{2} NVARCHAR(250), {3} NVARCHAR(450), {4} NVARCHAR(250), {5} NVARCHAR(250), {6} NVARCHAR(max), {7} NVARCHAR(max));"
                    , InvestigationTable.tableName, InvestigationTable.id, InvestigationTable.title, InvestigationTable.description, InvestigationTable.createdBy, InvestigationTable.createdTime, InvestigationTable.investigationStatus, InvestigationTable.investigationImage);
            }
        }

        private string CreationDatabaseQuery
        {
            get
            {
                return string.Format(" if not exists(select * from sys.databases where name = \'{0}\') create database {0}", InvestigationDataBaseName);
            }
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(GetConnectionString());
        }

        private string GetConnectionString()
        {
            return string.Format("Data Source=localhost\\SqlExpress; Initial catalog={0};User id={1}; Password={2};"
                                     , InvestigationDataBaseName, InvestigationDatabaseUserName, InvestigationDatabasePassword);
        }

        private string GenerateSaveInvestigationQuery(KInvestigation kInvestigation, string userName)
        {
            string query = $"insert into {InvestigationTable.tableName} " +
                $"({InvestigationTable.id}, {InvestigationTable.title} , {InvestigationTable.description}, {InvestigationTable.createdBy}, {InvestigationTable.createdTime}, {InvestigationTable.investigationStatus}, {InvestigationTable.investigationImage}) " +
                $"values ({kInvestigation.Id}, '{kInvestigation.Title}', '{kInvestigation.Description}', '{userName}', '{kInvestigation.CreatedTime}', '{Convert.ToBase64String(kInvestigation.InvestigationStatus)}', '{Convert.ToBase64String(kInvestigation.InvestigationImage)}');";
            return query;
        }

        private string GenerateGetInvestigationsQuery(string userName)
        {
            string query = $"SELECT {InvestigationTable.id}, {InvestigationTable.title}, {InvestigationTable.description}, {InvestigationTable.createdBy}, {InvestigationTable.createdTime}  FROM {InvestigationTable.tableName}" +
                $" where {InvestigationTable.createdBy}='{userName}';";
            return query;
        }

        private string GenerateGetInvestigationStatusQuery(long id, string userName)
        {
            string query = $"SELECT {InvestigationTable.investigationStatus}  FROM {InvestigationTable.tableName}" +
                $" where {InvestigationTable.id}={id} and {InvestigationTable.createdBy}='{userName}';";
            return query;
        }

        private string GenerateGetInvestigationImageQuery(long id, string userName)
        {
            string query = $"SELECT {InvestigationTable.investigationImage}  FROM {InvestigationTable.tableName}" +
                $" where {InvestigationTable.id}={id} and {InvestigationTable.createdBy}='{userName}';";
            return query;
        }

        #endregion

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

        public void TruncateInvestigationTable(string tableName)
        {
            using (SqlConnection connection = GetConnection())
            {
                string query = $"TRUNCATE TABLE {tableName}";
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(query, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void CreateDataBase()
        {
            using (SqlConnection connection = new SqlConnection(
                string.Format("server=(local)\\SQLEXPRESS;Initial Catalog= master;User id={0}; Password={1};",
                InvestigationDatabaseUserName, InvestigationDatabasePassword))
                )
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(CreationDatabaseQuery, connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void SaveInvestigation(KInvestigation kInvestigation, string userName)
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand cmd = new SqlCommand(GenerateSaveInvestigationQuery(kInvestigation, userName), connection))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<InvestigationInfo> GetSavedInvestigations(string userName)
        {
            List<InvestigationInfo> investigationInfos = new List<InvestigationInfo>();

            string query = GenerateGetInvestigationsQuery(userName);
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            InvestigationInfo investigationInfo = new InvestigationInfo()
                            {
                                Id = long.Parse(reader[InvestigationTable.id].ToString()),
                                Title = reader[InvestigationTable.title].ToString(),
                                Description = reader[InvestigationTable.description].ToString(),
                                CreatedBy = reader[InvestigationTable.createdBy].ToString(),
                                CreatedTime = reader[InvestigationTable.createdTime].ToString()
                            };
                            investigationInfos.Add(investigationInfo);
                        }
                    }
                }
            }
            return investigationInfos;
        }

        public byte[] GetSavedInvestigationStatus(long id, string userName)
        {
            byte[] investigationStatus = null;
            string query = GenerateGetInvestigationStatusQuery(id, userName);
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            investigationStatus = Convert.FromBase64String(reader[InvestigationTable.investigationStatus].ToString());
                        }
                    }
                }
            }
            return investigationStatus;
        }

        public byte[] GetSavedInvestigationImage(long id, string userName)
        {
            byte[] investigationImage = null;
            string query = GenerateGetInvestigationImageQuery(id, userName);
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            investigationImage = Convert.FromBase64String(reader[InvestigationTable.investigationImage].ToString());
                        }
                    }
                }
            }
            return investigationImage;
        }

    }
}
