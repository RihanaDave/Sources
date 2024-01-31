using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Logic
{
    public class Optimizer
    {
        public void Optimize()
        {
            SearchEngineProvider.GetNewDefaultSearchEngineClient().Optimize();
        }
    }
}
