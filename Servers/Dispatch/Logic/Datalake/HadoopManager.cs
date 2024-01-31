using GPAS.Dispatch.ServiceAccess.DataLakeService;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;

namespace GPAS.Dispatch.Logic.Datalake
{
    public class HadoopManager
    {
        public List<string> GetListDirectory(string path)
        {
            DatalakeOperationClient proxy = null;
            try
            {
                proxy = new DatalakeOperationClient();                
                return proxy.GetDatalakeCategories(path).ToList();
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }            
        }
    }
}
