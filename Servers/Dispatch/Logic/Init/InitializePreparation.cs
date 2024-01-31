using System.Configuration;
using GPAS.Logger;

namespace GPAS.Dispatch.Logic.Init
{
    public class InitializePreparation
    {
        public static bool IsInitializing;
        public static void InitializeDispatch()
        {
            if (!IsInitializing)
            {
                IsInitializing = true;
                try
                {
                    ExceptionHandler.Init();
                    AdministrativeEventReporter.Init();
                    new InvestigationManagement().Init();
                    new GroupManagement().Init();
                    new UserAccountManagement().Init();
                    new GroupMembershipManagement().Init();
                    new SearchIndexesSynchronization().Init();
                    GeographicalStaticLocationProvider.Init();
                    IdGenerators.InitializeIdGenerators();
                    MapProvider.Init();
                    OntologyLoader.OntologyLoader.Init
                    (
                        new DispatchOntologyDownLoader(),
                        ConfigurationManager.AppSettings["OntologyLoaderFolderPath"],
                        ConfigurationManager.AppSettings["OntologyIconsLoaderFolderPath"]
                    );
                }
                finally
                {
                    IsInitializing = false;
                }
            }
        }
    }
}
