using GPAS.Logger;
using System;

namespace GPAS.Horizon.Logic
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
                AdministrativeEventReporter.Init();
                OntologyProvider.Init();
                (new GraphRepositoryProvider()).Init();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }            
        }
    }
}
