using System.IO;
using GMap.NET;

namespace GPAS.MapViewer
{
    public class MapTileImageNeededEventArgs
    {
        public byte[] TileImage { get; set; }
        public GPoint TilePosition { get; internal set; }
        public int ZoomLevel { get; internal set; }
    }
}