using Orient.Client;
using Orient.Client.API.Query;
using OrientDB_Net.binary.Innov8tive.API;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Horizon.GraphRepositoryPlugins.OrientNet
{
    public class Connection
    {
        private ODatabase _instanceODatabase = null;
        private OServer _instanceOServer = null;

        internal static readonly string GraphRepositoryIP = ConfigurationManager.AppSettings["GPAS.Horizon.GraphRepositoryPlugins.OrientNet.RepositoryIP"];
        internal static readonly string UserName = ConfigurationManager.AppSettings["GPAS.Horizon.GraphRepositoryPlugins.OrientNet.RepositoryUser"];
        internal static readonly string Password = ConfigurationManager.AppSettings["GPAS.Horizon.GraphRepositoryPlugins.OrientNet.RepositoryPassword"];
        internal static readonly string DatabaseName = ConfigurationManager.AppSettings["GPAS.Horizon.GraphRepositoryPlugins.OrientNet.DatabaseName"];
        internal static readonly int Port = 2424;

        public Connection()
        {
        }

        public OServer OpenServerConnection()
        {
            _instanceOServer = new OServer(GraphRepositoryIP, Port, UserName, Password);
            return _instanceOServer;
        }

        public void CloseServerConnection()
        {
            _instanceOServer.Close();
        }

        public ODatabase OpenConnection()
        {
            _instanceOServer = new OServer(GraphRepositoryIP, Port, UserName, Password);

            _instanceODatabase = new ODatabase(
                new ConnectionOptions()
                {
                    HostName = GraphRepositoryIP,
                    UserName = UserName,
                    Password = Password,
                    Port = Port,
                    DatabaseName = DatabaseName,
                    DatabaseType = ODatabaseType.Graph,
                    PoolAlias = "ConnectionPool"
                }
            );

            short[] clusterIds = new short[] { };
            OClusterQuery clusterQuery = _instanceODatabase.Clusters(clusterIds);
            return _instanceODatabase;
        }
        public void CloseConnection()
        {
            _instanceOServer.Close();
            _instanceODatabase.Close();
        }
    }
}
