using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch.Entities.Jobs
{
    public enum JobRequestType
    {
        Unknown,
        ImportFromCsvFile,
        ImportFromExcelSheet,
        ImportFromAttachedDatabaseTableOrView,
        ImportFromEmlDirectory,
        ImportFromAccessTable
    }
}
