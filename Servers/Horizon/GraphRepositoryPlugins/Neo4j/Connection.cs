using Neo4j.Driver;
using System.Configuration;
using System.Threading.Tasks;

namespace GPAS.Horizon.GraphRepositoryPlugins.Neo4j
{
    public class Connection
    {
        private IDriver driver;
        private IAsyncSession instanceDatabaseSession;
        private IAsyncSession instanceServerSession;

        internal static readonly string graphRepositoryIp = ConfigurationManager.AppSettings["GPAS.Horizon.GraphRepositoryPlugins.Neo4j.RepositoryIP"];
        internal static readonly string username = ConfigurationManager.AppSettings["GPAS.Horizon.GraphRepositoryPlugins.Neo4j.RepositoryUser"];
        internal static readonly string password = ConfigurationManager.AppSettings["GPAS.Horizon.GraphRepositoryPlugins.Neo4j.RepositoryPassword"];
        internal static readonly string databaseName = ConfigurationManager.AppSettings["GPAS.Horizon.GraphRepositoryPlugins.Neo4j.DatabaseName"];
        internal static readonly int port = 7687;

        private string MakeGraphRepositoryURI()
        {
            return "neo4j://" + graphRepositoryIp + ":" + port;
        }

        public IAsyncSession OpenServerSession()
        {
            driver = GraphDatabase.Driver(MakeGraphRepositoryURI(), AuthTokens.Basic(username, password));
            instanceServerSession = driver.AsyncSession();
            return instanceServerSession;
        }

        public IAsyncSession OpenDatabaseSession()
        {
            driver = GraphDatabase.Driver(MakeGraphRepositoryURI(), AuthTokens.Basic(username, password));
            instanceDatabaseSession = driver?.AsyncSession(SessionConfigBuilder.ForDatabase(databaseName));
            return instanceDatabaseSession;
        }

        public async Task CloseDatabaseSession()
        {
            await instanceDatabaseSession.CloseAsync();
        }

        public async Task CloseServerSession()
        {
            await instanceServerSession.CloseAsync();
            driver?.Dispose();
        }
    }
}
