using GMap.NET;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.MapViewer.Heatmap
{
    public class ArealUnit
    {
        public ArealUnit(List<HeatPointLatLng> points, double areaInSquareMeters, PointLatLng topLeftPoint)
        {
            this.areaInSquareMeters = areaInSquareMeters;
            this.topLeftPoint = topLeftPoint;

            CalcWidthInMeters();
            CalcWidthInDegree();

            CalculateBoundPoint();

            AddRangeHeatPoint(points.ToList());
        }

        private List<HeatPointLatLng> heatPoints = new List<HeatPointLatLng>();
        private double areaInSquareMeters;
        private double widthInMeters;
        private double widthInDegree;
        private double intensityCount;
        private double intensityWeight;
        private PointLatLng topLeftPoint;
        private PointLatLng rightBottomPoint;
        private PointLatLng centerPoint;
        private PointLatLng centerDensityPoints;

        public IReadOnlyCollection<HeatPointLatLng> HeatPoints
        {
            get
            {
                return heatPoints.AsReadOnly();
            }
        }

        public double AreaInSquareMeters
        {
            get
            {
                return areaInSquareMeters;
            }

            set
            {
                areaInSquareMeters = value;

                CalcWidthInMeters();
                CalcWidthInDegree();

                CalculateBoundPoint();
            }
        }

        public PointLatLng TopLeftPoint
        {
            get
            {
                return topLeftPoint;
            }

            set
            {
                topLeftPoint = value;
                CalculateBoundPoint();
            }
        }

        public PointLatLng RightBottomPoint
        {
            get
            {
                return rightBottomPoint;
            }
        }

        public double WidthInMeters
        {
            get
            {
                return widthInMeters;
            }
        }

        public PointLatLng CenterPoint
        {
            get
            {
                return centerPoint;
            }
        }

        public PointLatLng CenterDensityPoints
        {
            get
            {
                return centerDensityPoints;
            }
        }

        public double IntensityCount
        {
            get
            {
                return intensityCount;
            }
        }

        public double IntensityWeight
        {
            get
            {
                return intensityWeight;
            }
        }

        private void CalcWidthInMeters()
        {
            widthInMeters = Math.Sqrt(areaInSquareMeters);
        }

        private void CalcWidthInDegree()
        {
            var p = GeoMath.RadialPoint(new PointLatLng(0, 0), WidthInMeters, 0);
            widthInDegree = p.Lat;
        }

        private void CalculateBoundPoint()
        {
            rightBottomPoint = new PointLatLng(TopLeftPoint.Lat - widthInDegree, TopLeftPoint.Lng + widthInDegree);
            centerPoint = new PointLatLng(TopLeftPoint.Lat - (widthInDegree / 2), TopLeftPoint.Lng + (widthInDegree / 2));
        }

        private void CalculateCenterDensityPoints()
        {
            if (HeatPoints?.Count > 0)
            {
                double maxLat, maxLng, minLat, minLng;

                var lats = from h in heatPoints select h.Point.Lat;
                maxLat = lats.Max();
                minLat = lats.Min();

                var lngs = from h in heatPoints select h.Point.Lng;
                maxLng = lngs.Max();
                minLng = lngs.Min();

                centerDensityPoints = new PointLatLng((minLat + maxLat) / 2, (minLng + maxLng) / 2);
            }
            else
            {
                centerDensityPoints = CenterPoint;
            }
        }

        public void AddHeatPoint(HeatPointLatLng point)
        {
            if (IsValidHeatPoint(point))
            {
                heatPoints.Add(point);

                intensityCount++;
                intensityWeight += point.Intensity;

                CalculateCenterDensityPoints();
            }
        }

        public void AddRangeHeatPoint(List<HeatPointLatLng> points)
        {
            points.RemoveAll(p => !IsValidHeatPoint(p)); //حذف نقاط نامعتبر

            heatPoints.AddRange(points);

            intensityCount += points.Count;
            intensityWeight += (from p in points select p.Intensity).Sum();

            CalculateCenterDensityPoints();
        }

        public void RemoveHeatPoint(HeatPointLatLng point)
        {
            heatPoints.Remove(point);

            intensityCount--;
            intensityWeight -= point.Intensity;

            CalculateCenterDensityPoints();
        }

        public void RemoveRangeHeatPoints(List<HeatPointLatLng> points)
        {
            heatPoints = heatPoints.Except(points).ToList();

            intensityCount -= points.Count;
            intensityCount -= (from p in points select p.Intensity).Sum();

            CalculateCenterDensityPoints();
        }

        public void ClearHeatPoints()
        {
            heatPoints.Clear();

            intensityCount = 0;
            intensityWeight = 0;

            CalculateCenterDensityPoints();
        }

        private bool IsValidHeatPoint(HeatPointLatLng point)
        {
            if (point.Point.Lat > TopLeftPoint.Lat || point.Point.Lat < RightBottomPoint.Lat ||
                        point.Point.Lng < TopLeftPoint.Lng || point.Point.Lng > RightBottomPoint.Lng)
            { //نقطه نا معتبر و خارج بازه Areal Unit است.

                return false;
            }

            return true;
        }
    }
}
