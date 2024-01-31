using GMap.NET;
using GMap.NET.WindowsPresentation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace GPAS.MapViewer
{
    public class HeatMapTile 
    {
        public HeatMapTile(IEnumerable<HeatPointLatLng> points, PointLatLng firstPoint, PointLatLng lastPoint, double minIntensity, double maxIntensity, MapViewer mapViewer) :
            this(points, firstPoint, lastPoint, minIntensity, maxIntensity, mapViewer, new GSize(256, 256), 10)
        {

        }

        public HeatMapTile(IEnumerable<HeatPointLatLng> points, PointLatLng firstPoint, PointLatLng lastPoint, double minIntensity, double maxIntensity, MapViewer mapViewer, GSize tileSize, double heatRadius) 
        {
            Points = points.ToList();
            FirstPoint = firstPoint;
            LastPoint = lastPoint;
            MaxIntensity = maxIntensity;
            MinIntensity = minIntensity;
            MapViewer = mapViewer;
            TileSize = tileSize;
            HeatRadius = heatRadius;

            localPoints = new Dictionary<Point, double>(Points.Count);
        }

        public List<HeatPointLatLng> Points;
        RenderTargetBitmap bitmapSource;
        GSize tileSize;
        double heatRadius;
        PointLatLng lastPoint;
        PointLatLng firstPoint;
        double maxIntensity;
        double minIntensity;
        MapViewer MapViewer;

        public double HeatRadius
        {
            get
            {
                return heatRadius;
            }

            set
            {
                heatRadius = value;
            }
        }

        public GSize TileSize
        {
            get
            {
                return tileSize;
            }

            set
            {
                tileSize = value;
            }
        }

        public PointLatLng LastPoint
        {
            get
            {
                return lastPoint;
            }
            set
            {
                lastPoint = value;
            }
        }

        public PointLatLng FirstPoint
        {
            get
            {
                return firstPoint;
            }

            set
            {
                firstPoint = value;
            }
        }

        public double MaxIntensity
        {
            get
            {
                return maxIntensity;
            }

            set
            {
                maxIntensity = value;
            }
        }

        public RenderTargetBitmap BitmapSource
        {
            get
            {
                return bitmapSource;
            }
        }

        public double MinIntensity
        {
            get
            {
                return minIntensity;
            }

            set
            {
                minIntensity = value;
            }
        }

        public CancellationTokenSource RenderCTS = new CancellationTokenSource();
        Dictionary<Point, double> localPoints = null;

        public virtual void Render(int zoom)
        {
            if (Points?.Count > 0)
            {
                RenderCTS.Cancel();
                RenderCTS = new CancellationTokenSource();

                GetLocalPoints(RenderCTS, zoom);
                if (localPointIndex == Points.Count)
                {
                    RenderCTS.Cancel();
                    RenderCTS = new CancellationTokenSource();

                    RenderIntensityMap(RenderCTS, localPoints);
                }
            }
            else
            {
               bitmapSource = ClearIntensityMap();
            }
        }

        private RenderTargetBitmap ClearIntensityMap()
        {
            RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)TileSize.Width, (int)TileSize.Height, 96, 96, PixelFormats.Pbgra32);
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext ctx = dv.RenderOpen())
            {
                ctx.DrawRectangle(Brushes.Transparent, null, new Rect(0, 0, renderTarget.PixelWidth, renderTarget.PixelHeight));
            }

            renderTarget.Render(dv);
            renderTarget.Freeze();

            return renderTarget;
        }

        int localPointIndex = 0;
        private void GetLocalPoints(CancellationTokenSource cts, int zoom)
        {
            GPoint offset = GeoMath.FromLatLngToPixel(FirstPoint, zoom);

            for (; localPointIndex < Points.Count; localPointIndex++)
            {
                try
                {
                    var point = Points[localPointIndex];
                    if (cts.IsCancellationRequested)
                        return;

                    GPoint gp = GeoMath.FromLatLngToPixel(point.Point, zoom);
                    var p = GeoMath.GPointToPoint(offset, gp);

                    double intensity = point.Intensity;


                    if (localPoints.ContainsKey(p))
                    {
                        double lastInensity = localPoints[p];
                        intensity = (lastInensity + intensity > MaxIntensity) ? MaxIntensity : lastInensity + intensity;
                    }

                    localPoints[p] = intensity;
                }
                catch
                {
                    continue;
                }
            }
        }

        private void RenderIntensityMap(CancellationTokenSource cts, Dictionary<Point, double> localPoints)
        {
            RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)TileSize.Width, (int)TileSize.Height, 96, 96, PixelFormats.Pbgra32);
            RadialGradientBrush radialBrush = new RadialGradientBrush();

            double multiple = 100 / (MaxIntensity);
            if (double.IsInfinity(multiple))
            {
                ClearIntensityMap();
                return;
            }

            float epsilon = (float)(MinIntensity * multiple) / 101 > .009999f ? .009999f : 0f;

            try
            {
                foreach (var point in localPoints)
                {
                    if (cts.IsCancellationRequested)
                        return;

                    DrawingVisual dv = new DrawingVisual();
                    using (DrawingContext ctx = dv.RenderOpen())
                    {
                        radialBrush.GradientStops.Clear();
                        float intensity = (float)(point.Value * multiple) / 100;

                        if (intensity <= epsilon)
                        {
                            continue;
                        }

                        radialBrush.GradientStops.Add(new GradientStop(Color.FromScRgb(intensity, 0xFF, 0xFF, 0xFF), 0));
                        radialBrush.GradientStops.Add(new GradientStop(Color.FromScRgb(epsilon, 0xFF, 0xFF, 0xFF), 1));

                        double r = HeatRadius;
                        ctx.DrawEllipse(radialBrush, null, new Point(point.Key.X, point.Key.Y), r, r);
                        //ctx.DrawRectangle(radialBrush, null, new Rect(new Point(point.Key.X - r, point.Key.Y - r), new Point(point.Key.X + r, point.Key.Y + r)));
                    }

                    renderTarget.Render(dv);
                }

                renderTarget.Freeze();
                bitmapSource = renderTarget;
            }
            catch
            {
                RenderCTS.Cancel();
                RenderCTS = new CancellationTokenSource();

                RenderIntensityMap(RenderCTS, localPoints);
            }
        }
    }
}
