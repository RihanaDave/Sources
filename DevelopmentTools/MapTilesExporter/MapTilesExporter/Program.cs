using MapTilesExporter.Logic;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MapTilesExporter
{
    class Program
    {
        static void Main(string[] args)
        {
            ImageCacheManeger imageCacheManeger = new ImageCacheManeger(
                ConfigurationManager.AppSettings["PathSqliteForExtract"],
                ConfigurationManager.AppSettings["PathForSaveImages"]);
            imageCacheManeger.GetImageFromCache();

        }
    }
}
