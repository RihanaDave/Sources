using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GPAS.Dispatch.IISHost.App_Code
{
    public class Run
    {
        public class Initializer
        {
            public static void AppInitialize()
            {
                Logic.Init.InitializePreparation.InitializeDispatch();
            }
        }
    }
}