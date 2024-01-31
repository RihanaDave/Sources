using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.Projections;
using GMap.NET.WindowsPresentation;
using System;

namespace GPAS.MapViewer
{
    public class MapProvider : GMapProvider
    {
        // Map Provider
        // می‌بایست دارای یک شناسه‌ی ثابت برای استفاده توسط
        // GMap.net
        // باشد؛ شناسه‌ی نوع برای ذخیره‌سازی در بانک اطلاعاتی
        // از این شناسه‌ی یکتا استخراج می‌شود، در نتیجه این شناسه
        // هرگز نباید تغییر کند تا در عملکرد میانگیری محلی نقشه
        // اختلالی رخ ندهد

        //readonly Guid id = new Guid("1173a05f-bba7-4638-9563-1a686f2eef2b");
        public override Guid Id
        {
            get
            {
                return Guid.NewGuid();
            }
        }

        readonly string name = "GPAS Map Provider Service";
        public override string Name
        {
            get
            {
                return name;
            }
        }

        // براساس پیاده‌سازی
        // GoogleMap Map Provider
        // کد مبدا GMap.net
        GMapProvider[] overlays;
        public override GMapProvider[] Overlays
        {
            get
            {
                if (overlays == null)
                {
                    overlays = new GMapProvider[] { this };
                }
                return overlays;
            }
        }

        // براساس پیاده‌سازی
        // GoogleMap Map Provider
        // کد مبدا GMap.net
        public override PureProjection Projection
        {
            get
            {
                return MercatorProjection.Instance;
            }
        }

        private PureImageProxy TileImageProxy = GMapImageProxy.Instance;
        public override PureImage GetTileImage(GPoint pos, int zoom)
        {
            PureImage ret = null;
            
            var tileImageByteArray = GetTileImageByThrowingEvent(zoom, pos);
            if (tileImageByteArray != null && tileImageByteArray.Length > 0)
            {
                ret = TileImageProxy.FromArray(GetTileImageByThrowingEvent(zoom, pos));
            }
            return ret;
        }

        public event EventHandler<MapTileImageNeededEventArgs> MapTileImageNeeded;
        protected byte[] GetTileImageByThrowingEvent(int zoomLevel, GPoint tilePosition)
        {
            byte[] result = null;
            if (MapTileImageNeeded != null)
            {
                var args = new MapTileImageNeededEventArgs()
                {
                    ZoomLevel = zoomLevel,
                    TilePosition = tilePosition
                };
                MapTileImageNeeded(this, args);
                result = args.TileImage;
            }
            return result;
        }
    }
}
