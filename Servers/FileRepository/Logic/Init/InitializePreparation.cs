using GPAS.Logger;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace GPAS.FileRepository.Logic.Init
{
    public class InitializePreparation
    {
        public static bool IsInitializing = false;
        public static void InitializeHadoop()
        {
            if (!IsInitializing)
            {
                IsInitializing = true;
                try
                {
                    ExceptionHandler.Init();
                    (new DataSourceAndDocumentManager()).Init();
                    //(new MediaManager()).Init();
                }
                finally
                {
                    IsInitializing = false;
                }
            }
        }
    }
}
