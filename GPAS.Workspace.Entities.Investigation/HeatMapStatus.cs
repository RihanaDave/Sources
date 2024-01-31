using System;

namespace GPAS.Workspace.Entities.Investigation
{
    [Serializable]
    public class HeatMapStatus
    {
        public static readonly string ShowByCount = "Count";

        public bool IsHide { get; set; }

        public string ShowByCountOrValue { get; set; }

        public int DensityRedius { get; set; }

        public double ArealUnits { get; set; }

        public bool IsScaleOnMap { get; set; }

        public double Opacity { get; set; }

        public bool IsShowMapPoint { get; set; }

        public bool IsAllDataPoint { get; set; }
    }
}