using System;
using System.Collections.Generic;

namespace GPAS.Workspace.Entities.Investigation
{
    [Serializable]
    public class BrowserApplicationStatus
    {

        public List<long> BrowsedObjectIds { get; set; }

        public long OpenedObjectId { get; set; }
    }
}