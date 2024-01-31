using GPAS.Dispatch.Entities.Map;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;

namespace GPAS.Dispatch.Logic
{
    public class MapProvider
    {
        private static bool isInitialized = false;
        public static void Init()
        {
            if (isInitialized)
                return;
            MapTileSources = new Dictionary<string, MapTileSource>();

            var configTileSources = (Entities.ConfigElements.MapTile.MapTileSources)ConfigurationManager.GetSection(nameof(MapTileSources));
            if (configTileSources == null)
            {
                throw new ConfigurationErrorsException("Unable to read \"MapTileSources\" configuration section");
            }
            foreach (Entities.ConfigElements.MapTile.MapTileSource source in configTileSources.Sources)
            {
                if (source.Type == Entities.ConfigElements.MapTile.MapTileSourceType.LocallyStored)
                {
                    LocallySharedMapTileSource locallySharedMapTileSource = new LocallySharedMapTileSource()
                    {
                        UniqueName = source.UniqueName,
                        TitlePathPattern = source.AccessPattern
                    };
                    MapTileSources.Add(locallySharedMapTileSource.UniqueName, locallySharedMapTileSource);
                }
                else if (source.Type == Entities.ConfigElements.MapTile.MapTileSourceType.RestfullService)
                {
                    RestfullServiceMapTileSource restfullServiceMapTileSource = new RestfullServiceMapTileSource()
                    {
                        UniqueName = source.UniqueName,
                        ServiceUrlPattern = source.AccessPattern
                    };
                    MapTileSources.Add(restfullServiceMapTileSource.UniqueName, restfullServiceMapTileSource);
                }
                else
                {
                    throw new NotSupportedException("Unknown Map Tile Source Type");
                }
            }
            isInitialized = true;
        }

        public static Dictionary<string, MapTileSource> MapTileSources { get; private set; }

        public string[] GetMapTileSources()
        {
            List<string> mapTileSources = new List<string>(MapTileSources.Count);
            foreach (var item in MapTileSources.Keys)
            {
                mapTileSources.Add(MapTileSources[item].UniqueName);
            }
            return mapTileSources.ToArray();
        }

        public byte[] GetMapTileImage(string tileSource, int zoomLevel, long x, long y)
        {
            if (!isInitialized)
            {
                throw new InvalidOperationException("Class is not initialized");
            }
            if (string.IsNullOrEmpty(tileSource))
            {
                throw new Exception("TileSource  Is Null Or Empty.");
            }
            if (!MapTileSources.ContainsKey(tileSource))
            {
                throw new Exception("TileSource Is not Exist in Dictionary");
            }

            MapTileSource mapTileSource = MapTileSources[tileSource];
            if (mapTileSource is LocallySharedMapTileSource)
            {
                return (mapTileSource as LocallySharedMapTileSource).GetTileImage(zoomLevel, x, y);
            }
            else if (mapTileSource is RestfullServiceMapTileSource)
            {
                return (mapTileSource as RestfullServiceMapTileSource).GetTileImage(zoomLevel, x, y);
            }
            else
            {
                throw new Exception("MapTileSource Dont Supported.");
            }
        }
    }
}
