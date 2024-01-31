using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GPAS.JobsManagement
{
    [Serializable]
    public abstract class DataImportRequest : Request
    {
        protected DataImportRequest()
        {
        }
    }
}
