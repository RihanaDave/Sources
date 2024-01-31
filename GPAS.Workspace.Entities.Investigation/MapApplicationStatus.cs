using System;
using System.Collections.Generic;

namespace GPAS.Workspace.Entities.Investigation
{
    [Serializable]
    public class MapApplicationStatus
    {
        public List<long> ShowingObjectIds { get; set; }

        public List<long> SelectedObjectIds { get; set; }

        public HeatMapStatus HeatMapStatus { get; set; }
    }
}