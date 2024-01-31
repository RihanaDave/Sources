using GPAS.Horizon.GraphRepository;
using GPAS.Horizon.Logic;

namespace GPAS.Horizon.LogicTests.DefaultGraphRepositoryProviderTests
{
    internal class AccessClientFactory
    {
        private static bool IsOntologyInitialized = false;
        internal static IAccessClient GetNewInstanceOfDefaultGraphRepositoryAccessClient()
        {
            GraphRepositoryProvider graphRepositoryProvider = new GraphRepositoryProvider();
            graphRepositoryProvider.Init();
            IAccessClient accessClient = graphRepositoryProvider.GetNewSearchEngineClient();
            accessClient.OpenConnetion();
            if (!IsOntologyInitialized)
            {
                accessClient.Init(OntologyMaterial.GetOntologyMaterial(OntologyProvider.GetOntology()));
                IsOntologyInitialized = true;
            }
            return accessClient;
        }
    }
}
