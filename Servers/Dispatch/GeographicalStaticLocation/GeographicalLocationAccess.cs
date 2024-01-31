using GPAS.Dispatch.Entities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch.GeographicalStaticLocation
{
    public class GeographicalLocationAccess
    {
        private static string GeographicalDatabaseUserName = ConfigurationManager.AppSettings["GeographicalDatabaseUserName"];
        private static string GeographicalDatabasePassword = ConfigurationManager.AppSettings["GeographicalDatabasePassword"];
        public GeographicalLocationAccess()
        { }

        private static string DatabaseName
        {
            get
            {
                return "GeographicalStaticlocationDB";
            }
        }

        private static string TableName
        {
            get
            {
                return "GeographicalLocationBaseOnIp";
            }
        }

        private static string CreationTableQuery
        {
            get
            {
                return string.Format(" if not exists(SELECT * from sys.tables where name = \'{0}\')  CREATE TABLE GeographicalLocationBaseOnIp (IP VARCHAR(50) PRIMARY KEY, Latitude float, Longitude float);", TableName);
            }
        }

        private static string CreationDatabaseQuery
        {
            get
            {
                return string.Format(" if not exists(select * from sys.databases where name = \'{0}\') create database {0}", DatabaseName);
            }
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(GetConnectionString());
        }

        private string GetConnectionString()
        {
            return string.Format("Data Source=localhost\\SqlExpress; Initial catalog={0};User id={1}; Password={2};"
                                    , DatabaseName, GeographicalDatabaseUserName, GeographicalDatabasePassword);
        }
        private List<GeographicalLocationModel> CastSqlDataReaderToGeographicalLocationModel(SqlDataReader reader)
        {
            List<GeographicalLocationModel> list = new List<GeographicalLocationModel>();
            while (reader.Read())
            {
                GeographicalLocationModel item = new GeographicalLocationModel();
                item.Ip = (reader["IP"].ToString());
                item.Latitude = (Convert.ToDouble(reader["Latitude"]));
                item.Longitude = (Convert.ToDouble(reader["Longitude"]));
                list.Add(item);
            }
            return list;
        }

        #region public functions
        public void CreateDatabase()
        {
            using (SqlConnection connection = new SqlConnection(string.Format("server=(local)\\SQLEXPRESS;Initial Catalog= master;User id={0}; Password={1};",
                GeographicalDatabaseUserName, GeographicalDatabasePassword))
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
      
        public GeographicalLocationModel GetGeoLocationBaseOnIP(string ip)
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                SqlDataReader reader = new SqlCommand("select * from GeographicalLocationBaseOnIp where Ip ='" + ip + "'", connection).ExecuteReader();
                return this.CastSqlDataReaderToGeographicalLocationModel(reader).FirstOrDefault<GeographicalLocationModel>();
            }
        }

        public bool DeleteEntireOfGeographicalLocationBaseOnIp(SqlConnection conn)
        {
            bool flag;
            try
            {
                string str = "delete from GeographicalLocationBaseOnIp";
                new SqlCommand(str, conn).ExecuteNonQuery();
                flag = true;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return flag;
        }

        private bool InsertGeoSpecialInformationBasedOnIP(SqlConnection conn, string ip, double latitude, double longitude)
        {
            bool flag;
            try
            {
                string query = string.Format("insert into {0} (IP, Latitude, Longitude) values ('{1}', {2}, {3})", TableName, ip, latitude , longitude);
                SqlCommand sqlCommand = new SqlCommand(query, conn);
                sqlCommand.ExecuteNonQuery();
                flag = true;
            }
            catch (Exception exception)
            {
                throw exception;
            }
            return flag;
        }

        public bool InsertGeoSpecialInformationBasedOnIP(string ip, double latitude, double longitude)
        {
            using (SqlConnection connection = GetConnection())
            {
                connection.Open();
                return InsertGeoSpecialInformationBasedOnIP(connection, ip, latitude, longitude);
            }
        }
        #endregion
    }
}
