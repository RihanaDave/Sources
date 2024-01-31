using System;

namespace GPAS.TimelineViewer
{
    public sealed class BinSizes
    {
        private BinSizes()
        {

        }

        public static BinSizesEnum FindEnumFromBinSize(BinSize binSize)
        {
            if (binSize.Equals(OneHour))
                return OneHour.RelatedBinSizesEnum;
            else if (binSize.Equals(TwoHours))
                return TwoHours.RelatedBinSizesEnum;
            else if (binSize.Equals(FourHours))
                return FourHours.RelatedBinSizesEnum;
            else if (binSize.Equals(SixHours))
                return SixHours.RelatedBinSizesEnum;
            else if (binSize.Equals(TwelveHours))
                return TwelveHours.RelatedBinSizesEnum;
            else if (binSize.Equals(OneDay))
                return OneDay.RelatedBinSizesEnum;
            else if (binSize.Equals(TwoDays))
                return TwoDays.RelatedBinSizesEnum;
            else if (binSize.Equals(SevenDays))
                return SevenDays.RelatedBinSizesEnum;
            else if (binSize.Equals(FifteenDays))
                return FifteenDays.RelatedBinSizesEnum;
            else if (binSize.Equals(OneMonth))
                return OneMonth.RelatedBinSizesEnum;
            else if (binSize.Equals(ThreeMonths))
                return ThreeMonths.RelatedBinSizesEnum;
            else if (binSize.Equals(FourMonths))
                return FourMonths.RelatedBinSizesEnum;
            else if (binSize.Equals(SixMonths))
                return SixMonths.RelatedBinSizesEnum;
            else if (binSize.Equals(OneYear))
                return OneYear.RelatedBinSizesEnum;
            else if (binSize.Equals(TwoYears))
                return TwoYears.RelatedBinSizesEnum;
            else if (binSize.Equals(TenYears))
                return TenYears.RelatedBinSizesEnum;
            else
                return BinSizesEnum.Custom;
        }

        public static BinSize FindBinSizeFromEnum(BinSizesEnum binSizesEnum, DateTime beginTime, DateTime endTime, int barCount)
        {
            BinSize binSize = new BinSize();
            switch (binSizesEnum)
            {
                case BinSizesEnum.OneHour:
                    binSize = BinSizes.OneHour;
                    break;
                case BinSizesEnum.TwoHours:
                    binSize = BinSizes.TwoHours;
                    break;
                case BinSizesEnum.FourHours:
                    binSize = BinSizes.FourHours;
                    break;
                case BinSizesEnum.SixHours:
                    binSize = BinSizes.SixHours;
                    break;
                case BinSizesEnum.TwelveHours:
                    binSize = BinSizes.TwelveHours;
                    break;
                case BinSizesEnum.OneDay:
                    binSize = BinSizes.OneDay;
                    break;
                case BinSizesEnum.TwoDays:
                    binSize = BinSizes.TwoDays;
                    break;
                case BinSizesEnum.SevenDays:
                    binSize = BinSizes.SevenDays;
                    break;
                case BinSizesEnum.FifteenDays:
                    binSize = BinSizes.FifteenDays;
                    break;
                case BinSizesEnum.OneMonth:
                    binSize = BinSizes.OneMonth;
                    break;
                case BinSizesEnum.ThreeMonths:
                    binSize = BinSizes.ThreeMonths;
                    break;
                case BinSizesEnum.FourMonths:
                    binSize = BinSizes.FourMonths;
                    break;
                case BinSizesEnum.SixMonths:
                    binSize = BinSizes.SixMonths;
                    break;
                case BinSizesEnum.OneYear:
                    binSize = BinSizes.OneYear;
                    break;
                case BinSizesEnum.TwoYears:
                    binSize = BinSizes.TwoYears;
                    break;
                case BinSizesEnum.TenYears:
                    binSize = BinSizes.TenYears;
                    break;
                case BinSizesEnum.Custom:
                    binSize = BinSize.GenerateCustomBinSize(beginTime, endTime, barCount);
                    break;
                case BinSizesEnum.Unknown:
                default:
                    throw new NotSupportedException();
            }

            return binSize;
        }

        public static BinSize Default => OneDay;

        public static BinSize OneHour => new BinSize() {
            BinFactor = 1,
            BinScale = BinScaleLevel.Hour,
            TimeAxisLabelsScale = BinScaleLevel.Day,
            RelatedBinSizesEnum = BinSizesEnum.OneHour
        };

        public static BinSize TwoHours => new BinSize()
        {
            BinFactor = 2,
            BinScale = BinScaleLevel.Hour,
            TimeAxisLabelsScale = BinScaleLevel.Day,
            RelatedBinSizesEnum = BinSizesEnum.TwoHours
        };

        public static BinSize FourHours => new BinSize()
        {
            BinFactor = 4,
            BinScale = BinScaleLevel.Hour,
            TimeAxisLabelsScale = BinScaleLevel.Day,
            RelatedBinSizesEnum = BinSizesEnum.FourHours
        };

        public static BinSize SixHours => new BinSize()
        {
            BinFactor = 6,
            BinScale = BinScaleLevel.Hour,
            TimeAxisLabelsScale = BinScaleLevel.Day,
            RelatedBinSizesEnum = BinSizesEnum.SixHours
        };

        public static BinSize TwelveHours => new BinSize()
        {
            BinFactor = 12,
            BinScale = BinScaleLevel.Hour,
            TimeAxisLabelsScale = BinScaleLevel.Day,
            RelatedBinSizesEnum = BinSizesEnum.TwelveHours
        };

        public static BinSize OneDay => new BinSize()
        {
            BinFactor = 1,
            BinScale = BinScaleLevel.Day,
            TimeAxisLabelsScale = BinScaleLevel.Month,
            RelatedBinSizesEnum = BinSizesEnum.OneDay
        };

        public static BinSize TwoDays => new BinSize()
        {
            BinFactor = 2,
            BinScale = BinScaleLevel.Day,
            TimeAxisLabelsScale = BinScaleLevel.Month,
            RelatedBinSizesEnum = BinSizesEnum.TwoDays
        };

        public static BinSize SevenDays => new BinSize()
        {
            BinFactor = 7,
            BinScale = BinScaleLevel.Day,
            TimeAxisLabelsScale = BinScaleLevel.Month,
            RelatedBinSizesEnum = BinSizesEnum.SevenDays
        };

        public static BinSize FifteenDays => new BinSize()
        {
            BinFactor = 15,
            BinScale = BinScaleLevel.Day,
            TimeAxisLabelsScale = BinScaleLevel.Month,
            RelatedBinSizesEnum = BinSizesEnum.FifteenDays
        };

        public static BinSize OneMonth => new BinSize()
        {
            BinFactor = 1,
            BinScale = BinScaleLevel.Month,
            TimeAxisLabelsScale = BinScaleLevel.Year,
            RelatedBinSizesEnum = BinSizesEnum.OneMonth
        };

        public static BinSize ThreeMonths => new BinSize()
        {
            BinFactor = 3,
            BinScale = BinScaleLevel.Month,
            TimeAxisLabelsScale = BinScaleLevel.Year,
            RelatedBinSizesEnum = BinSizesEnum.ThreeMonths
        };

        public static BinSize FourMonths => new BinSize()
        {
            BinFactor = 4,
            BinScale = BinScaleLevel.Month,
            TimeAxisLabelsScale = BinScaleLevel.Year,
            RelatedBinSizesEnum = BinSizesEnum.FourMonths
        };

        public static BinSize SixMonths => new BinSize()
        {
            BinFactor = 6,
            BinScale = BinScaleLevel.Month,
            TimeAxisLabelsScale = BinScaleLevel.Year,
            RelatedBinSizesEnum = BinSizesEnum.SixMonths
        };

        public static BinSize OneYear => new BinSize()
        {
            BinFactor = 1,
            BinScale = BinScaleLevel.Year,
            TimeAxisLabelsScale = BinScaleLevel.Year,
            RelatedBinSizesEnum = BinSizesEnum.OneYear
        };

        public static BinSize TwoYears => new BinSize()
        {
            BinFactor = 2,
            BinScale = BinScaleLevel.Year,
            TimeAxisLabelsScale = BinScaleLevel.Year,
            RelatedBinSizesEnum = BinSizesEnum.TwoYears
        };

        public static BinSize TenYears => new BinSize()
        {
            BinFactor = 10,
            BinScale = BinScaleLevel.Year,
            TimeAxisLabelsScale = BinScaleLevel.Year,
            RelatedBinSizesEnum = BinSizesEnum.TenYears
        };
    }
}
