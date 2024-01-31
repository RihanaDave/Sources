using System.ComponentModel;

namespace GPAS.Dispatch.Entities.ConfigElements.MapTile
{
    public enum MapTileSourceType
    {
        [Description("LocallyStored")]
        LocallyStored = 1,
        [Description("RestfullService")]
        RestfullService = 2
    }
}