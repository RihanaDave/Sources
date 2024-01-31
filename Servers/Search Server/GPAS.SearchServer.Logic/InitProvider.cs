using GPAS.Logger;
using System;

namespace GPAS.SearchServer.Logic
{
    public class InitProvider
    {
        private static void WriteErrorLog(Exception ex)
        {
            ExceptionHandler exceptionHandler = new ExceptionHandler();
            exceptionHandler.WriteErrorLog(ex);
        }
        public static void Init()
        {
            try
            {
                ExceptionHandler.Init();
                OntologyProvider.Init();
                SearchEngineProvider.Init();
                AdministrativeEventReporter.Init();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
    }
}
