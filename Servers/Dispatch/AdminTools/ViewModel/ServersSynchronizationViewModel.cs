using System.Threading.Tasks;
using GPAS.Dispatch.AdminTools.Model;
using GPAS.Dispatch.Logic;

namespace GPAS.Dispatch.AdminTools.ViewModel
{
    public class ServersSynchronizationViewModel : BaseViewModel
    {
        public ServersSynchronizationViewModel()
        {
            ServersSynchronization = new ServersSynchronizationModel();
        }

        public ServersSynchronizationModel ServersSynchronization { get; set; }

        public async Task GetServersStability()
        {
            bool searchStability = false;
            bool horizonStability = false;
            long horizonNonSyncObjects = 0;
            long horizonNonSyncRelations = 0;
            long searchNonSyncObjects = 0;

            await Task.Run(() =>
            {
                SearchIndexesSynchronization synchronization = new SearchIndexesSynchronization();

                searchStability = synchronization.IsHorizonDataIndicesStable().GetAwaiter().GetResult();
                horizonStability = synchronization.IsSearchDataIndicesStable().GetAwaiter().GetResult();

                horizonNonSyncObjects = synchronization.GetHorizonUnsyncObjectsCount();
                horizonNonSyncRelations = synchronization.GetHorizonUnsyncRelationshipsCount();
                searchNonSyncObjects = synchronization.GetSearchUnsyncObjectsCount();
            });

            ServersSynchronization.IsSearchDataIndicesStable = searchStability;
            ServersSynchronization.IsHorizonDataIndicesStable = horizonStability;
            ServersSynchronization.HorizonNonSyncObjects = horizonNonSyncObjects;
            ServersSynchronization.HorizonNonSyncRelations = horizonNonSyncRelations;
            ServersSynchronization.SearchNonSyncObjects = searchNonSyncObjects;

            ServersSynchronization.SearchNonSyncCount = searchNonSyncObjects;
            ServersSynchronization.HorizonNonSyncCount = horizonNonSyncObjects + horizonNonSyncRelations + searchNonSyncObjects;
        }
    }
}
