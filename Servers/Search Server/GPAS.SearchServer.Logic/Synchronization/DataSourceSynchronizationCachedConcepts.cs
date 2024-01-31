using GPAS.AccessControl;
using GPAS.DataSynchronization;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.SearchServer.Logic.Synchronization
{
    internal class DataSourceSynchronizationCachedConcepts : CachedConcepts
    {
        internal List<DataSourceInfo> CachedDataSources = null;

        public DataSourceSynchronizationCachedConcepts(List<DataSourceInfo> dataSourceInfos)
        {
            CachedDataSources = dataSourceInfos;
        }

        public override IEnumerable<long> GetCachedConceptIDs()
        {
            return CachedDataSources.Select(ds => ds.Id);
        }
    }
}
