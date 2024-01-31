using System;
using System.IO;

namespace GPAS.Dispatch.Entities.Map
{
    public class LocallySharedMapTileSource : MapTileSource
    {
        public string TitlePathPattern { get; set; }

        public override byte[] GetTileImage(int zoonLevel, long x, long y)
        {
            try
            {
                byte[] imageTile = File.ReadAllBytes(TitlePathPattern.Replace("{z}"
                       , zoonLevel.ToString())
                       .Replace("{x}", x.ToString())
                       .Replace("{y}", y.ToString()));
                return imageTile;
            }
            catch
            {
                return null;
            }
        }
    }
}
