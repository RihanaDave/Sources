using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GPAS.FileRepository.IISHost.App_Code
{
    public class Run
    {
        public static void AppInitialize()
        {
            FileRepository.Logic.Init.InitializePreparation.InitializeHadoop();
        }
    }
}