using GPAS.RepositoryServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GPAS.RepositoryServer.IISHost.App_Code
{
    public class Initializer
    {
        public static void AppInitialize()
        {
            InitProvider.Init();
        }
    }
}