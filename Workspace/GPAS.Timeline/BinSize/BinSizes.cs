using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.TimelineViewer.BinSize
{
    internal static class BinSizes
    {
        private static List<BinSize> binSizesList = new List<BinSize>();
        static BinSizes()
        {
            binSizesList.Add(new BinSize(BinScaleLevel.Year, 10, BinScaleLevel.Year, "10 Year", 2));
            binSizesList.Add(new BinSize(BinScaleLevel.Year, 2, BinScaleLevel.Year, "2 Year", 1));
            binSizesList.Add(DefaultBinSize = new BinSize(BinScaleLevel.Year, 1, BinScaleLevel.Year, "1 Year", 1));
            binSizesList.Add(new BinSize(BinScaleLevel.Month, 6, BinScaleLevel.Year, "6 Month"));
            binSizesList.Add(new BinSize(BinScaleLevel.Month, 4, BinScaleLevel.Year, "4 Month"));
            binSizesList.Add(new BinSize(BinScaleLevel.Month, 3, BinScaleLevel.Year, "3 Month"));
            binSizesList.Add(new BinSize(BinScaleLevel.Month, 1, BinScaleLevel.Year, "1 Month"));
            binSizesList.Add(new BinSize(BinScaleLevel.Day, 15, BinScaleLevel.Month, "15 Day", 1));
            binSizesList.Add(new BinSize(BinScaleLevel.Day, 7, BinScaleLevel.Month, "7 Day"));
            binSizesList.Add(new BinSize(BinScaleLevel.Day, 2, BinScaleLevel.Month, "2 Day"));
            binSizesList.Add(new BinSize(BinScaleLevel.Day, 1, BinScaleLevel.Month, "1 Day"));
            binSizesList.Add(new BinSize(BinScaleLevel.Hour, 12, BinScaleLevel.Day, "12 Hour"));
            binSizesList.Add(new BinSize(BinScaleLevel.Hour, 6, BinScaleLevel.Day, "6 Hour"));
            binSizesList.Add(new BinSize(BinScaleLevel.Hour, 4, BinScaleLevel.Day, "4 Hour"));
            binSizesList.Add(new BinSize(BinScaleLevel.Hour, 2, BinScaleLevel.Day, "2 Hour"));
            binSizesList.Add(new BinSize(BinScaleLevel.Hour, 1, BinScaleLevel.Day, "1 Hour"));
        }

        internal static BinSize DefaultBinSize;

        internal static List<BinSize> BinSizesList
        {
            get
            {
                return binSizesList;
            }
        }
    }
}
