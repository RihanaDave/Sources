using GPAS.AccessControl;
using GPAS.SearchServer.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Logic
{
   public class GraphManager
    {
        public SearchGraphArrangement SaveNew(SearchGraphArrangement dbGraphArrangement)
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            return accessClient.AddSearchGraph(dbGraphArrangement);
        }

        public List<SearchGraphArrangement> GetGraphArrangements(AuthorizationParametters authorizationParametters)
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            return accessClient.GetGraphArrangements(authorizationParametters);
        }

        public byte[] GetGraphImage(int dbGrapharagmentID, AuthorizationParametters authorizationParametters)
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            return accessClient.GetGraphImage(dbGrapharagmentID, authorizationParametters);
        }

        public byte[] GetGraphArrangementXML(int dbGraphArrangementID, AuthorizationParametters authorizationParametters)
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            return accessClient.GetGraphArrangementXML(dbGraphArrangementID, authorizationParametters);
        }

        public bool DeleteGraph(int id)
        {
            Access.SearchEngine.ApacheSolr.AccessClient accessClient = new Access.SearchEngine.ApacheSolr.AccessClient();
            return accessClient.DeleteGraph(id);
        }
    }
}
