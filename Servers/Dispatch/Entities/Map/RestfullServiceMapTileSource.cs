using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch.Entities.Map
{
    public class RestfullServiceMapTileSource : MapTileSource
    {
        public string ServiceUrlPattern { get; set; }

        public override byte[] GetTileImage(int zoonLevel, long x, long y)
        {
            try
            {
                WebClient webclient = new WebClient();
                byte[] imageTile = webclient.DownloadData(ServiceUrlPattern.Replace("{z}"
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
