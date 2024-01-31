using GPAS.AccessControl;
using GPAS.RepositoryServer.Data.DatabaseTables;
using GPAS.RepositoryServer.Entities;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace GPAS.RepositoryServer.Logic
{
    /// <summary>
    /// این کلاس مدیریت ایجاد و بازیابی چینش گراف را در پایگاه داده انجام می دهد.
    /// </summary>
    public class GraphManager
    {
        private static readonly string ConnctionString = ConnectionStringManager.GetRepositoryDBConnectionString();

        /// <summary>
        /// این تابع چینش گراف ها را دریاقت کرده و در پایگاه داده ذخیره می کند.
        /// </summary>
        /// <param name="dbGraphArrangement">   یک شی از DBGraphArrangement را دریافت می کند.    </param>
        /// <returns>   یک شی از DBGraphArrangement را بر می گرداند.    </returns>
        public DBGraphArrangement SaveNew(DBGraphArrangement dbGraphArrangement)
        {

            string query = string.Format("INSERT INTO DBGraph ({0},{1},{2},{3},{4},{5},{6},{7}) VALUES "
                , GraphTable.id, GraphTable.title, GraphTable.description, GraphTable.timecreated
                , GraphTable.graphimage, GraphTable.grapharrangement, GraphTable.nodescount, GraphTable.dsid);
            query += string.Format("({0},'{1}','{2}','{3}','{4}','{5}',{6} ,{7})",
                dbGraphArrangement.Id, dbGraphArrangement.Title, dbGraphArrangement.Description, dbGraphArrangement.TimeCreated,
                Convert.ToBase64String(dbGraphArrangement.GraphImage), Convert.ToBase64String(dbGraphArrangement.GraphArrangementXML), dbGraphArrangement.NodesCount
                , dbGraphArrangement.DataSourceID
                );

            using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                command.ExecuteNonQuery();
            }
            return dbGraphArrangement;

        }

        /// <summary>
        /// این تابع همه چینش گراف های موجود در پایگاه داده را بر می گرداند.    
        /// </summary>
        /// <returns>   لیستی از DBGraphArrangement را بر می گرداند.    </returns>
        public List<DBGraphArrangement> GetGraphArrangements(AuthorizationParametters authParams)
        {

            List<DBGraphArrangement> dbGraphArrangements = new List<DBGraphArrangement>();
            string query = string.Format("SELECT * FROM DBGraph");
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                command.AllResultTypesAreUnknown = true;
                var reader = command.ExecuteReader();
                dbGraphArrangements = NpgsqlDataReaderToDBGraphArrangement(reader);
                dbGraphArrangements = GetReadableSubset(dbGraphArrangements, authParams);
            }
            return dbGraphArrangements;
        }

        private List<DBGraphArrangement> NpgsqlDataReaderToDBGraphArrangement(NpgsqlDataReader reader)
        {
            List<DBGraphArrangement> dbGraphArrangements = new List<DBGraphArrangement>();
            while (reader.Read())
            {
                long id = long.Parse(reader[GraphTable.id].ToString());
                string title = reader[GraphTable.title].ToString();
                string description = reader[GraphTable.description].ToString();
                string timeCreated = reader[GraphTable.timecreated].ToString();
                string graphImage = reader[GraphTable.graphimage].ToString();
                string graphArrangementXML = reader[GraphTable.grapharrangement].ToString();
                int nodesCount = int.Parse(reader[GraphTable.nodescount].ToString());
                long dataSourceID = long.Parse(reader[GraphTable.dsid].ToString());
                dbGraphArrangements.Add(
                    new DBGraphArrangement()
                    {
                        Id = id,
                        Title = title,
                        Description = description,
                        TimeCreated = timeCreated,
                        GraphImage = Convert.FromBase64String(graphImage),
                        GraphArrangementXML = Convert.FromBase64String(graphArrangementXML),
                        NodesCount = nodesCount,
                        DataSourceID = dataSourceID
                    }
                    );
            }
            return dbGraphArrangements;
        }

        /// <summary>
        /// این تابع تصویر متناسب با آیدی ورودی از کاربر را بر می گرداند.
        /// </summary>
        /// <param name="dbGraphArrangementID">  آیدی چینش گراف مورد نظر را از کاربر دریافت میکند.   </param>
        /// <returns>    تصویر را به صورت byte[] برمیگرداند.   </returns>
        public byte[] GetGraphImage(int dbGrapharagmentID, AuthorizationParametters authParams)
        {
            if (dbGrapharagmentID < 0)
                throw new ArgumentNullException("Grapharagment ID is invalid.");

            byte[] graphImage = null;
            string query = string.Format("SELECT graphImage FROM DBGraph where {0} = {1}"
                , GraphTable.id, dbGrapharagmentID);
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                command.AllResultTypesAreUnknown = true;
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    graphImage = Convert.FromBase64String(reader[0].ToString());
                }
            }

            return graphImage;
        }

        /// <summary>
        /// این تابع چینش XML متناسب با آیدی ورودی از کاربر را بر می گرداند.
        /// </summary>
        /// <param name="dbGraphArrangementID">  آیدی چینش گراف مورد نظر را از کاربر دریافت میکند.   </param>
        /// <returns>    چینش XML را به صورت byte[] برمیگرداند.   </returns>
        public byte[] GetGraphArrangementXML(int dbGrapharagmentID, AuthorizationParametters authParams)
        {

            if (dbGrapharagmentID < 0)
                throw new ArgumentNullException("Grapharagment ID is invalid.");

            byte[] graphArrangement = null;
            string query = string.Format("SELECT graphArrangement FROM DBGraph where {0} = {1}"
                , GraphTable.id, dbGrapharagmentID);
            using (NpgsqlConnection conn = new NpgsqlConnection(ConnctionString))
            {
                conn.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                command.AllResultTypesAreUnknown = true;
                var reader = command.ExecuteReader();
                while (reader.Read())
                {
                    graphArrangement = Convert.FromBase64String(reader[0].ToString());
                }
            }

            return graphArrangement;
        }

        public bool DeleteGraph(int id)
        {
            if (id == 0)
                throw new ArgumentNullException("Graph ID is invalid.");

            string query = string.Format("DELETE FROM DBGraph WHERE {0} = ({1});"
                , GraphTable.id, id);
            using (NpgsqlConnection connection = new NpgsqlConnection(ConnctionString))
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand(query, connection);
                command.ExecuteNonQuery();
            }
            return true;
        }
        private List<DBGraphArrangement> GetReadableSubset(List<DBGraphArrangement> dbGraphArrangement, AuthorizationParametters authParams)
        {
            long[] dataSourceIDs = dbGraphArrangement.Select(p => p.DataSourceID).ToArray();
            var uacMan = new UserAccountControlManager();
            HashSet<long> readableDataSourceIDs = uacMan.GetReadableDataSourceIDsByAuthParams(dataSourceIDs, authParams);
            return dbGraphArrangement.Where(p => readableDataSourceIDs.Contains(p.DataSourceID)).ToList();
        }
    }
}
