using GMap.NET;
using GMap.NET.WindowsPresentation;
using GPAS.MapViewer.Heatmap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace GPAS.MapViewer
{
    /// <summary>
    /// مراحل ترسیم هیت مپ:
    /// 1. نقاط ورودی را درون ArealUnit های مربوط به خود قرارداده.
    /// 2. ArealUnitih موثر بر تایل های موجود در تایل های BoundingBox را یافته و به تایل مربوطه اختصاص می دهد.
    /// 3. تصویر مربوط به تایل های ViewingBox را بصورت تایل های مجزا ترسیم می کند.
    /// 4. تایل ها را به هم می چسباند و یک تصویر واحد ایجاد می شود.
    /// </summary>
    public class HeatmapMarker: GMapMarker , IShapable, IMapShape
    {
        public HeatmapMarker(List<HeatPointLatLng> points) : base(new PointLatLng(0, 0))
        {
            tileSize = GeoMath.GetTileSize();
            generateProgress = new Progress<double>(percent => { CalcProgressPercent(percent); });
            Points = points;
        }

        public List<HeatPointLatLng> Points = new List<HeatPointLatLng>();
        Dictionary<GPoint, ArealUnit> ArealUnitsXY;

        Dictionary<GPoint, HeatMapTile> Tiles = new Dictionary <GPoint, HeatMapTile>();

        GPoint tileViewMinXY, tileViewMaxXY, tileBoundMinXY, tileBoundMaxXY;
        int tileBoundNumber = 1;
        GSize tileSize = new GSize(256, 256);
        int zoom = 0;
        double radius;
        double opacity;
        LayerShowState status = LayerShowState.Hidden;
        private Brush gradient;
        double maxIntensityCount;
        double minIntensityCount;
        double maxIntensityWeight;
        double minIntensityWeight;
        bool warningArealUnitNotMatchRadius = false;
        RenderTargetBitmap bitmapSource = null;
        double progressPercent = 0;
        IProgress<double> generateProgress;
        byte[] pixels;

        private MapViewer map;
        private double arealUnitAreaInSquareMeters = 1000000;

        public event MouseEventHandler MouseEnter;
        public event MouseEventHandler MouseLeave;
        public event MouseButtonEventHandler MouseDown;
        public event MouseEventHandler MouseMove;
        public event MouseButtonEventHandler MouseUp;

#pragma warning disable CS0108 // 'HeatmapMarker.Map' hides inherited member 'GMapMarker.Map'. Use the new keyword if hiding was intended.
        public MapViewer Map
#pragma warning restore CS0108 // 'HeatmapMarker.Map' hides inherited member 'GMapMarker.Map'. Use the new keyword if hiding was intended.
        {
            get { return map; }
            set
            {
                map = value;
                if (map != null)
                {
                    map.gmapControl.MouseMove += GmapControl_MouseMove;
                    Opacity = map.HeatmapOpacity;
                    Gradient = map.HeatmapColorSpectrum;
                }
            }
        }

        public GPoint TileViewMinXY
        {
            get
            {
                return tileViewMinXY;
            }
        }

        public GPoint TileViewMaxXY
        {
            get
            {
                return tileViewMaxXY;
            }
        }

        public GSize TileSize
        {
            get
            {
                return tileSize;
            }
        }

        public double Radius
        {
            get
            {
                return radius;
            }

            set
            {
                radius = value;
                double min = MinimumAllowedForRadius();
                if (radius < min)
                {
                    warningArealUnitNotMatchRadius = true;
                }
                else
                {
                    warningArealUnitNotMatchRadius = false;
                }

                GenerateShape(false);
            }
        }

        public double Opacity
        {
            get
            {
                return opacity;
            }

            set
            {
                opacity = value;

                if (Shape is System.Windows.Controls.Image)
                {
                    (Shape as System.Windows.Controls.Image).Effect = GenerateEffect();
                }
            }
        }

        public LayerShowState Status
        {
            get
            {
                return status;
            }

            set
            {
                status = value;
                if (status == LayerShowState.Hidden)
                {
                    Erase();
                }
                else
                {
                    Draw();
                }
            }
        }

        public Brush Gradient
        {
            get
            {
                return gradient;
            }

            set
            {
                gradient = value;

                if(Shape is System.Windows.Controls.Image)
                {
                    (Shape as System.Windows.Controls.Image).Effect = GenerateEffect();
                }
            }
        }

        public double MaxIntensityCount
        {
            get
            {
                return maxIntensityCount;
            }
        }

        public double MinIntensityCount
        {
            get
            {
                return minIntensityCount;
            }
        }

        public double MaxIntensityWeight
        {
            get
            {
                return maxIntensityWeight;
            }
        }

        public double MinIntensityWeight
        {
            get
            {
                return minIntensityWeight;
            }
        }

        public double ArealUnitAreaInSquareMeters
        {
            get { return arealUnitAreaInSquareMeters; }
            set
            {
                arealUnitAreaInSquareMeters = value;
                double max = MaximumAllowedForArealUnitArea();
                if(arealUnitAreaInSquareMeters>max)
                {
                    warningArealUnitNotMatchRadius = true;
                }
                else
                {
                    warningArealUnitNotMatchRadius = false;
                }

                SetArealUnits();
            }
        }

        public bool WarningArealUnitNotMatchRadius
        {
            get
            {
                return warningArealUnitNotMatchRadius;
            }
        }

        public double ProgressPercent
        {
            get
            {
                return progressPercent;
            }
        }

        public PointsValueSourceType PointsSource { get; set; }

        private double MaximumAllowedForArealUnitArea()
        {
            return Math.Pow(Radius, 2);
        }

        private double MinimumAllowedForRadius()
        {
            return Math.Sqrt(ArealUnitAreaInSquareMeters);
        }

        public void CalcProgressPercent(double percent)
        {
            progressPercent = percent;
            if (Map != null)
            {
                if (Status == LayerShowState.Shown)
                    Map.HeatmapProgressPercent = progressPercent;
                else
                    Map.HeatmapProgressPercent = 0;
            }
        }

        public virtual void RegenerateShape(GMapControl map)
        {
            if (map != null && Map != null)
            {
                Map.gmapControl = map;
                
                bool changeZoom = zoom != (int)Map.gmapControl.Zoom;
                zoom = (int)Map.gmapControl.Zoom;

                GenerateShape(!changeZoom);
            }
        }

        Task SetArealUnitsTask;
        CancellationTokenSource SetArealUnitsCTS = new CancellationTokenSource();

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        private async void SetArealUnits()
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            GenerateImageCTS.Cancel();
            GenerateTilesCTS.Cancel();
            FindTilesMatchedWithArealUnitsCTS.Cancel();
            SetArealUnitsCTS.Cancel();

            SetArealUnitsCTS = new CancellationTokenSource();
            SetArealUnitsTask = Task.Run(() => SetArealUnitsAsync(SetArealUnitsCTS));
        }

        private void CalculateMinMaxIntensity(Dictionary<GPoint, ArealUnit> arealUnitsXY)
        {
            if (arealUnitsXY == null || arealUnitsXY.Values == null || arealUnitsXY.Values.Count == 0)
                return;

            maxIntensityCount = (from au in arealUnitsXY.Values select au.IntensityCount).Max();
            minIntensityCount = (from au in arealUnitsXY.Values select au.IntensityCount).Min();

            maxIntensityWeight = (from au in arealUnitsXY.Values select au.IntensityWeight).Max();
            minIntensityWeight = (from au in arealUnitsXY.Values select au.IntensityWeight).Min();
        }

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        private async Task SetArealUnitsAsync(CancellationTokenSource cts)
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            var arealUnitsXYLocal = new Dictionary<GPoint, ArealUnit>(Points.Count);
            double basePrg = 0, prg = 0, totalPrg = 20;

            if (Points?.Count > 0)
            {
                double widthArealUnitInMeters = Math.Sqrt(ArealUnitAreaInSquareMeters);
                var s = new PointLatLng(0, 0);
                var p1 = GeoMath.RadialPoint(s, widthArealUnitInMeters, 0);
                double widthArealUnitInDegree = p1.Lat;

                double prgStep = totalPrg / Points.Count;

                foreach (var point in Points)
                {
                    if (cts.IsCancellationRequested)
                    {
                        return;
                    }

                    GPoint gp = new GPoint((long)Math.Floor(point.Point.Lng / widthArealUnitInDegree), (long)Math.Ceiling(point.Point.Lat / widthArealUnitInDegree));

                    if (!arealUnitsXYLocal.ContainsKey(gp))
                    {
                        arealUnitsXYLocal[gp] = new ArealUnit(new List<HeatPointLatLng>() { point }, ArealUnitAreaInSquareMeters, new PointLatLng(gp.Y * widthArealUnitInDegree, gp.X * widthArealUnitInDegree));
                    }
                    else
                    {
                        arealUnitsXYLocal[gp].AddHeatPoint(point);
                    }

                    prg += prgStep;
                    generateProgress.Report(basePrg + prg);
                }

                CalculateMinMaxIntensity(arealUnitsXYLocal);

                generateProgress.Report(basePrg + totalPrg);
            }

            ArealUnitsXY = arealUnitsXYLocal;
            Map?.Dispatcher.BeginInvoke(new Action(() => { GenerateShape(false); }), DispatcherPriority.Background);
        }

        public void GenerateShape(bool cache)
        {
            SetTilesView();
            RefreshTileBound(false);
            if (Map != null)
            {
                Map.HeatmapPointsCount = Points.Count;
            }

            if (SetArealUnitsTask == null || !SetArealUnitsTask.IsCompleted)
                return;

            if (ArealUnitsXY == null || ArealUnitsXY.Count == 0)
                return;

            if (Map != null)
            {
                if (PointsSource == PointsValueSourceType.MarkersCount)
                {
                    Map.HeatmapLeastDensity = MinIntensityCount;
                    Map.HeatmapMostDensity = MaxIntensityCount;
                }
                else
                {
                    Map.HeatmapLeastDensity = MinIntensityWeight;
                    Map.HeatmapMostDensity = MaxIntensityWeight;
                }
            }

            GenerateImageCTS.Cancel();
            GenerateTilesCTS.Cancel();
            FindTilesMatchedWithArealUnitsCTS.Cancel();

            if (Shape is UIElement)
                Shape.Visibility = Visibility.Hidden;

            int maxTilesNumbers = (Math.Pow(4, zoom) > ArealUnitsXY?.Count) ? ArealUnitsXY.Count : (int)Math.Pow(4, zoom);
            Tiles = new Dictionary<GPoint, HeatMapTile>(maxTilesNumbers);

            FindTilesMatchedWithArealUnitsCTS = new CancellationTokenSource();
            FindTilesMatchedWithArealUnitsTask = Task.Run(() => FindTilesMatchedWithArealUnitsAsync(FindTilesMatchedWithArealUnitsCTS));
        }

        private void RefreshTileBound(bool removerOuterTiles)
        {
            tileBoundMinXY = new GPoint(TileViewMinXY.X - tileBoundNumber, TileViewMinXY.Y - tileBoundNumber);
            tileBoundMaxXY = new GPoint(TileViewMaxXY.X + tileBoundNumber, TileViewMaxXY.Y + tileBoundNumber);

            if (removerOuterTiles)
            {
                RemoveTilesOuterBound();
            }
        }

        private void RemoveTilesOuterBound()
        {
            var tiles = new Dictionary<GPoint, HeatMapTile>();
            foreach (var tile in Tiles)
            {
                if (IsBoundTile(tile.Key))
                {
                    tiles[tile.Key] = tile.Value;
                }
            }

            Tiles = tiles;
        }

        private bool IsBoundTile(GPoint tile)
        {
            if (tile.X < tileBoundMinXY.X || tile.Y < tileBoundMinXY.Y || tile.X > tileBoundMaxXY.X || tile.Y > tileBoundMaxXY.Y)
            {
                return false;
            }

            return true;
        }

        public Color GetPixel(Point p)
        {
            if(Shape is System.Windows.Controls.Image)
            {
                System.Windows.Controls.Image img = Shape as System.Windows.Controls.Image;

                if (img.Visibility == Visibility.Visible)
                {
                    try
                    {
                        int stride = (img.Source as BitmapSource).PixelWidth * 4;

                        int index = (int)p.Y * stride + 4 * (int)p.X;
                        byte red = pixels[index];
                        byte green = pixels[index + 1];
                        byte blue = pixels[index + 2];
                        byte alpha = pixels[index + 3];

                        return Color.FromArgb(alpha, red, green, blue);
                    }
                    catch
                    {
                        return Color.FromArgb(0, 0, 0, 0);
                    }
                }
            }

            return Color.FromArgb(0, 0, 0, 0);
        }

        private void SetTilesView()
        {
            tileViewMinXY = GeoMath.FromLatLngToTileXY(Map.gmapControl.ViewArea.LocationTopLeft, zoom);
            
            tileViewMaxXY = GeoMath.FromLatLngToTileXY(Map.gmapControl.ViewArea.LocationRightBottom, zoom);
        }

        Task FindTilesMatchedWithArealUnitsTask;
        CancellationTokenSource FindTilesMatchedWithArealUnitsCTS = new CancellationTokenSource();

        private void FindTilesMatchedWithArealUnitsAsync(CancellationTokenSource cts)
        {
            double pxRadius = ConvertMeterToPixel(radius);
            var rDeg = GeoMath.RadialPoint(new PointLatLng(0, 0), radius, 0).Lat;

            PointLatLng pllBoundTopLeft = GeoMath.FromTileXYToLatLng(tileBoundMinXY, TileSize, zoom);
            PointLatLng pllBoundRightBottom = GeoMath.FromTileXYToLatLng(new GPoint(tileBoundMaxXY.X + 1, tileBoundMaxXY.Y + 1), TileSize, zoom);

            var rectBound = new RectLatLng(pllBoundTopLeft.Lat + rDeg, pllBoundTopLeft.Lng - rDeg, (pllBoundRightBottom.Lng - pllBoundTopLeft.Lng + (2 * rDeg)), (pllBoundTopLeft.Lat - pllBoundRightBottom.Lat + (2 * rDeg)));
            var arealsInBound = (from aud in ArealUnitsXY.Values
                                where GeoMath.IsPointLatLngInsideArea(aud.CenterDensityPoints, rectBound)
                                select aud);

            long minX = tileBoundMinXY.X, maxX = tileBoundMaxXY.X, minY = tileBoundMinXY.Y, maxY = tileBoundMaxXY.Y;

            double basePrg = 20, prg = 0, totalPrg = 40, prgStep = totalPrg / ((maxX - minX + 1) * (maxY - minY + 1));

            for (long x = minX; x <= maxX; x++)
            {
                for (long y = minY; y <= maxY; y++)
                {
                    if (cts.IsCancellationRequested)
                        return;

                    var tile = new GPoint(x, y);

                    if (!Tiles.ContainsKey(tile))
                    {
                        PointLatLng pllTileTopLeft = GeoMath.FromTileXYToLatLng(tile, TileSize, zoom);
                        PointLatLng pllTileBottomRight = GeoMath.FromTileXYToLatLng(new GPoint(tile.X + 1, tile.Y + 1), TileSize, zoom);
                        var rect = new RectLatLng(pllTileTopLeft.Lat + rDeg, 
                            pllTileTopLeft.Lng - rDeg,
                            (pllTileBottomRight.Lng - pllTileTopLeft.Lng + (2 * rDeg)),
                            (pllTileTopLeft.Lat - pllTileBottomRight.Lat + (2 * rDeg)));

                        IEnumerable<HeatPointLatLng> arealsInTile = null;
                        HeatMapTile heatMapTile = null;

                        if (PointsSource == PointsValueSourceType.MarkersCount)
                        {
                            arealsInTile = (from aud in arealsInBound
                                                where GeoMath.IsPointLatLngInsideArea(aud.CenterDensityPoints, rect)
                                                select new HeatPointLatLng(aud.CenterDensityPoints, aud.IntensityCount));

                            heatMapTile = new HeatMapTile(arealsInTile, pllTileTopLeft, pllTileBottomRight, MinIntensityCount, MaxIntensityCount, Map) { HeatRadius = pxRadius };
                        }
                        else
                        {
                            arealsInTile = (from aud in arealsInBound
                                            where GeoMath.IsPointLatLngInsideArea(aud.CenterDensityPoints, rect)
                                            select new HeatPointLatLng(aud.CenterDensityPoints, aud.IntensityWeight));

                            heatMapTile = new HeatMapTile(arealsInTile, pllTileTopLeft, pllTileBottomRight, MinIntensityWeight, MaxIntensityWeight, Map) { HeatRadius = pxRadius };
                        }

                        try
                        {
                            Tiles.Add(tile, heatMapTile);
                        }
                        catch
                        {
                            continue;
                        }

                        if (cts.IsCancellationRequested)
                            return;
                    }

                    prg += prgStep;
                    generateProgress.Report(basePrg + prg);
                }
            }

            generateProgress.Report(basePrg + totalPrg);
            Map?.Dispatcher.BeginInvoke(new Action(() => GenerateTiles()), DispatcherPriority.Background);
        }

        private bool IsBoundPoint(PointLatLng point)
        {
            var pllTopLeftBound = GeoMath.FromTileXYToLatLng(tileBoundMinXY, TileSize, zoom);
            var pllRightBottomBound = GeoMath.FromTileXYToLatLng(new GPoint(tileBoundMaxXY.X + 1, tileBoundMaxXY.Y + 1), TileSize, zoom);

            if (point.Lat < pllRightBottomBound.Lat || point.Lat > pllTopLeftBound.Lat || point.Lng < pllTopLeftBound.Lng || point.Lng > pllRightBottomBound.Lng)
                return false;

            return true;
        }

        private double ConvertMeterToPixel(double meter)
        {
            var rp = GeoMath.RadialPoint(new PointLatLng(0, 0), meter, 0);
            double pixel = GeoMath.FromLatLngToPixel(new PointLatLng(0, 0), zoom).Y - GeoMath.FromLatLngToPixel(rp, zoom).Y;
            return pixel > 0 ? pixel : 1;
        }

        Task GenerateTilesTask;
        CancellationTokenSource GenerateTilesCTS = new CancellationTokenSource();

        public void GenerateTiles()
        {
            if (status == LayerShowState.Hidden)
                return;

            GenerateImageCTS.Cancel();
            GenerateTilesCTS.Cancel();

            GenerateTilesCTS = new CancellationTokenSource();
            GenerateTilesTask = Task.Run(() => GenerateTilesAsync(GenerateTilesCTS));
        }

        private async Task GenerateTilesAsync(CancellationTokenSource cts)
        {
            double basePrg = 60, prg = 0, totalPrg = 40, prgStep = totalPrg / ((TileViewMaxXY.X - TileViewMinXY.X + 1) * (TileViewMaxXY.Y - TileViewMinXY.Y + 1));

            for (long x = TileViewMinXY.X; x <= TileViewMaxXY.X; x++)
            {
                for (long y = TileViewMinXY.Y; y <= TileViewMaxXY.Y; y++)
                {
                    if (cts.IsCancellationRequested)
                    {
                        return;
                    }

                    var tileXY = new GPoint(x, y);
                    if (Tiles.ContainsKey(tileXY))
                    {
                        var heatMapTile = Tiles[tileXY];

                        if (heatMapTile.BitmapSource == null)
                        {
                            await Task.Run(() => heatMapTile.Render(zoom));
                        }
                    }

                    prg += prgStep;
                    generateProgress.Report(basePrg + prg);
                }
            }

            RemoveTilesOuterBound();

            generateProgress.Report(basePrg + totalPrg);

            Map?.Dispatcher.BeginInvoke(new Action(() => GenerateImage()), DispatcherPriority.Background);
        }

        Task GenerateImageTask;
        CancellationTokenSource GenerateImageCTS = new CancellationTokenSource();
        private void GenerateImage()
        {
            GenerateImageCTS.Cancel();
            GenerateImageCTS = new CancellationTokenSource();
            GenerateImageTask = Task.Run(() => GenerateImageAsync(GenerateImageCTS));
        }

        private void GenerateImageAsync(CancellationTokenSource cts)
        {
            RenderTargetBitmap renderTarget = new RenderTargetBitmap((int)((TileViewMaxXY.X - TileViewMinXY.X + 1) * TileSize.Width),
                    (int)((TileViewMaxXY.Y - TileViewMinXY.Y + 1) * TileSize.Height), 96, 96, PixelFormats.Pbgra32);

            for (long x = TileViewMinXY.X; x <= TileViewMaxXY.X; x++)
            {
                for (long y = TileViewMinXY.Y; y <= TileViewMaxXY.Y; y++)
                {
                    if (cts.IsCancellationRequested)
                    {
                        return;
                    }

                    DrawingVisual dv = new DrawingVisual();

                    var tileXY = new GPoint(x, y);
                    if (Tiles.ContainsKey(tileXY))
                    {
                        var heatMapTile = Tiles[tileXY];
                        DrawingContext dc = dv.RenderOpen();
                        dc.DrawImage(heatMapTile.BitmapSource, new Rect((x - TileViewMinXY.X) * TileSize.Width, (y - TileViewMinXY.Y) * TileSize.Height, TileSize.Width, TileSize.Height));
                        dc.Close();

                        renderTarget.Render(dv);
                    }
                }
            }
            renderTarget.Freeze();
            bitmapSource = renderTarget;

            Map?.Dispatcher.BeginInvoke(new Action(() =>
            {
                try
                {
                    GPoint TileFirstGPoint = new GPoint(TileViewMinXY.X * TileSize.Width, TileViewMinXY.Y * TileSize.Height);
                    Position = GeoMath.FromPixelToLatLng(TileFirstGPoint, zoom);

                    System.Windows.Controls.Image image = new System.Windows.Controls.Image();
                    image.Effect = GenerateEffect();

                    image.Source = bitmapSource;

                    int stride = bitmapSource.PixelWidth * 4;
                    int size = bitmapSource.PixelHeight * stride;
                    pixels = new byte[size];
                    bitmapSource.CopyPixels(pixels, stride, 0);

                    Shape = image;

                    Shape.IsHitTestVisible = false;
                    Shape.Visibility = Visibility.Visible;

                    Shape.MouseEnter += Shape_MouseEnter;
                    Shape.MouseLeave += Shape_MouseLeave;
                    Shape.MouseDown += Shape_MouseDown;
                    Shape.MouseMove += Shape_MouseMove;
                    Shape.MouseUp += Shape_MouseUp;
                }
                catch
                {

                }
            }), DispatcherPriority.Background);
        }

        private System.Windows.Media.Effects.Effect GenerateEffect()
        {
            Effects.HeatColorizer effect = new Effects.HeatColorizer();
            var pallete = new VisualBrush();
            if (Gradient == null)
                Gradient = Map.HeatmapColorSpectrum;

            var rect = new System.Windows.Shapes.Rectangle() { Width = 256, Height = 1, Fill = ConvertToGradientForHeatmap(Gradient), Opacity = Opacity };

            pallete.Visual = rect;
            effect.Palette = pallete;

            return effect;
        }

        private Brush ConvertToGradientForHeatmap(Brush gradientColor)
        {
            if (gradientColor as LinearGradientBrush != null)
            {
                LinearGradientBrush linearGradient = new LinearGradientBrush();
                bool addTransparentStop = true;

                for (int i = 0; i < (gradientColor as LinearGradientBrush).GradientStops.Count; i++)
                {
                    GradientStop stop = (gradientColor as LinearGradientBrush).GradientStops[i];
                    linearGradient.GradientStops.Add(new GradientStop(stop.Color, stop.Offset));
                    stop = linearGradient.GradientStops[i];

                    if(stop.Offset == 0) //اگر آفست 0 وجود داشت
                    {
                        addTransparentStop = false; //اگر آفست 0 وجود داشت و شفاف بود
                        if (stop.Color.A != 0) //اگر آفست 0 شفاف نبود
                        {
                            stop.Offset = .008;
                            addTransparentStop = true;
                        }
                    }
                }

                if (addTransparentStop)
                {
                    linearGradient.GradientStops.Add(new GradientStop(Colors.Transparent, 0));
                }

                return linearGradient;
            }
            else
            {
                return gradientColor;
            }
        }

        private void GmapControl_MouseMove(object sender, MouseEventArgs e)
        {
            zoom = (int)Map.gmapControl.Zoom;

            var tileTopLeft = GeoMath.FromLatLngToTileXY(Map.gmapControl.ViewArea.LocationTopLeft, zoom);
            var tileBottomRight = GeoMath.FromLatLngToTileXY(Map.gmapControl.ViewArea.LocationRightBottom, zoom);

            if (IsTilesViewChanged(tileTopLeft, tileBottomRight))
            {
               ChangeMapView();
            }
        }

        public virtual void ChangeMapView()
        {
            SetTilesView();
            RefreshTileBound(false);

            if (SetArealUnitsTask == null || !SetArealUnitsTask.IsCompleted)
                return;

            if (ArealUnitsXY == null || ArealUnitsXY.Count == 0)
                return;

            GenerateImageCTS.Cancel();
            GenerateTilesCTS.Cancel();
            FindTilesMatchedWithArealUnitsCTS.Cancel();

            FindTilesMatchedWithArealUnitsCTS = new CancellationTokenSource();
            FindTilesMatchedWithArealUnitsTask = Task.Run(() => FindTilesMatchedWithArealUnitsAsync(FindTilesMatchedWithArealUnitsCTS));
        }

        private void Shape_MouseUp(object sender, MouseButtonEventArgs e)
        {
            MouseUp?.Invoke(this, e);
        }

        private void Shape_MouseMove(object sender, MouseEventArgs e)
        {
            MouseMove?.Invoke(this, e);
        }

        private void Shape_MouseDown(object sender, MouseButtonEventArgs e)
        {
            MouseDown?.Invoke(this, e);
        }

        private void Shape_MouseLeave(object sender, MouseEventArgs e)
        {
            MouseLeave?.Invoke(this, e);
        }

        private void Shape_MouseEnter(object sender, MouseEventArgs e)
        {
            MouseEnter?.Invoke(this, e);
        }

        private bool IsTilesViewChanged(GPoint tileTopLeft, GPoint tileBottomRight)
        {
            return !(tileTopLeft.X == TileViewMinXY.X && tileTopLeft.Y == TileViewMinXY.Y && tileBottomRight.X == TileViewMaxXY.X && tileBottomRight.Y == TileViewMaxXY.Y);
        }

        /// <summary>
        /// شکل را حذف می کند
        /// </summary>
        public void Delete()
        {
            Erase();
        }

        /// <summary>
        /// هیت مپ را روی نقشه نشان می دهد..
        /// </summary>
        public void Draw()
        {
            Erase();

            if (Map != null && Map.gmapControl != null && !Map.gmapControl.Markers.Contains(this))
                Map?.gmapControl?.Markers.Add(this);
        }

        /// <summary>
        /// هیت مپ را از روی نقشه پاک می کند.
        /// </summary>
        public void Erase()
        {
           Application.Current.Dispatcher.Invoke(() =>
            {
                Map?.gmapControl?.Markers.Remove(this);
            });
            
        }
    }
}
