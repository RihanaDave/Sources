using GPAS.JobServer.Logic.SemiStructuredDataImport;
using GPAS.Logger;

namespace GPAS.JobServer.Logic
{
    public class InitProvider
    {
        public static void Init()
        {
            ExceptionHandler.Init();
            DatabaseImporter.Init();
        }
    }
}
