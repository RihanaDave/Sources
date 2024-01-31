using GPAS.SearchServer.Logic;
using System;
using System.Collections.Generic;
using System.Web;

namespace GPAS.SearchServer.IISHost.App_Code
{
    public class Initializer
    {
        public static void AppInitialize()
        {
            InitProvider.Init();
        }
    }
}