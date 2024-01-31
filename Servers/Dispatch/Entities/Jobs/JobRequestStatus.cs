using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch.Entities.Jobs
{
    public enum JobRequestStatus
    {
        Pending,
        Busy,
        Timeout,
        Terminated,
        Failed,
        Success,
        Pause,
        Resume,
    }
}
