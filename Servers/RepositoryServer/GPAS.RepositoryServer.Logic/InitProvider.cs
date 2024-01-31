using GPAS.Logger;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.RepositoryServer.Logic
{
    public class InitProvider
    {
        public static void Init()
        {
            ExceptionHandler.Init();
            (new RepositoryProvider()).Init();
        }
    }
}
