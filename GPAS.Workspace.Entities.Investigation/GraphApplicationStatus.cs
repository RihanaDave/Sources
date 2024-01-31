using System;
using System.Collections.Generic;

namespace GPAS.Workspace.Entities.Investigation
{
    [Serializable]
    public class GraphApplicationStatus
    {
        public string GraphArrangement { get; set; }

        public List<long> SelectedObjectIds { get; set; }
    }
}