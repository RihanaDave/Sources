using GPAS.Horizon.Logic;
using GPAS.Horizon.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace GPAS.IISHost.App_Code
{
    public class Run
    {
        public class Initializer
        {
            public static void AppInitialize()
            {
                InitProvider.Init();
            }
        }
    }
}