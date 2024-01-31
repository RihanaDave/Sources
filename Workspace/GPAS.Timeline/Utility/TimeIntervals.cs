using GPAS.TimelineViewer.BinSize;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.TimelineViewer.Utility
{
    internal static class TimeIntervals
    {
        internal static List<DateTime> GetIntervalsStartTime(DateTime lowerBound, DateTime upperBound, BinSize.BinSize bin)
        {
            if (lowerBound > upperBound)
                throw new ArgumentException("Lower-Bound > Upper-Bound !");

            DateTime firstIntervalStartTime = GetIntervalStartTimeCoversTime(lowerBound, bin);
            DateTime lastIntervalStartTime = GetIntervalStartTimeCoversTime(upperBound, bin);

            List<DateTime> result = new List<DateTime>();

            DateTime checkingIntervalStartTime = firstIntervalStartTime;
            do
            {
                result.Add(checkingIntervalStartTime);
                IncreaseTimeOneStep(ref checkingIntervalStartTime, bin);
            } while (checkingIntervalStartTime <= lastIntervalStartTime);
            return result;
        }
        internal static void IncreaseTimeOneStep(ref DateTime checkingIntervalStartTime, BinSize.BinSize bin)
        {
            switch (bin.ScaleLevel)
            {
                case BinScaleLevel.Year:
                    checkingIntervalStartTime = checkingIntervalStartTime.AddYears(bin.ScaleFactor);
                    break;
                case BinScaleLevel.Month:
                    checkingIntervalStartTime = checkingIntervalStartTime.AddMonths(bin.ScaleFactor);
                    break;
                case BinScaleLevel.Day:
                    DateTime increasedDate = checkingIntervalStartTime.AddDays(bin.ScaleFactor);
                    if (bin.TimeAxisLabelsScale == BinScaleLevel.Month
                        && new DateTime(increasedDate.Year, increasedDate.Month, 1) > new DateTime(checkingIntervalStartTime.Year, checkingIntervalStartTime.Month, 1))
                    {
                        increasedDate = new DateTime(increasedDate.Year, increasedDate.Month, 1);
                    }
                    checkingIntervalStartTime = increasedDate;
                    break;
                case BinScaleLevel.Hour:
                    checkingIntervalStartTime = checkingIntervalStartTime.AddHours(bin.ScaleFactor);
                    break;
                case BinScaleLevel.Minute:
                    checkingIntervalStartTime = checkingIntervalStartTime.AddMonths(bin.ScaleFactor);
                    break;
                case BinScaleLevel.Second:
                    checkingIntervalStartTime = checkingIntervalStartTime.AddSeconds(bin.ScaleFactor);
                    break;
                default:
                    throw new NotSupportedException("Bin Scale Level not supported");
            }
        }
        internal static void DecreaseTimeOneStep(ref DateTime checkingIntervalStartTime, BinSize.BinSize bin)
        {
            switch (bin.ScaleLevel)
            {
                case BinScaleLevel.Year:
                    checkingIntervalStartTime = checkingIntervalStartTime.AddYears(-bin.ScaleFactor);
                    break;
                case BinScaleLevel.Month:
                    checkingIntervalStartTime = checkingIntervalStartTime.AddMonths(-bin.ScaleFactor);
                    break;
                case BinScaleLevel.Day:
                    checkingIntervalStartTime = checkingIntervalStartTime.AddDays(-bin.ScaleFactor);
                    break;
                case BinScaleLevel.Hour:
                    checkingIntervalStartTime = checkingIntervalStartTime.AddHours(-bin.ScaleFactor);
                    break;
                case BinScaleLevel.Minute:
                    checkingIntervalStartTime = checkingIntervalStartTime.AddMonths(-bin.ScaleFactor);
                    break;
                case BinScaleLevel.Second:
                    checkingIntervalStartTime = checkingIntervalStartTime.AddSeconds(-bin.ScaleFactor);
                    break;
                default:
                    throw new NotSupportedException("Bin Scale Level not supported");
            }
        }
        internal static DateTime GetIntervalStartTimeCoversTime(DateTime timeToCover, BinSize.BinSize bin)
        {
            DateTime StartingTimeForLowerBound;
            switch (bin.ScaleLevel)
            {
                case BinScaleLevel.Year:
                    StartingTimeForLowerBound = new DateTime(timeToCover.Year - (timeToCover.Year % bin.ScaleFactor), 1, 1, 0, 0, 0);
                    break;
                case BinScaleLevel.Month:
                    StartingTimeForLowerBound = new DateTime(timeToCover.Year, timeToCover.Month - ((timeToCover.Month - 1) % bin.ScaleFactor), 1, 0, 0, 0);
                    break;
                case BinScaleLevel.Day:
                    StartingTimeForLowerBound = new DateTime(timeToCover.Year, timeToCover.Month, timeToCover.Day - ((timeToCover.Day - 1) % bin.ScaleFactor), 0, 0, 0);
                    break;
                case BinScaleLevel.Hour:
                    StartingTimeForLowerBound = new DateTime(timeToCover.Year, timeToCover.Month, timeToCover.Day, timeToCover.Hour - (timeToCover.Hour % bin.ScaleFactor), 0, 0);
                    break;
                case BinScaleLevel.Minute:
                    StartingTimeForLowerBound = new DateTime(timeToCover.Year, timeToCover.Month, timeToCover.Day, timeToCover.Hour, timeToCover.Minute - (timeToCover.Minute % bin.ScaleFactor), 0);
                    break;
                case BinScaleLevel.Second:
                    StartingTimeForLowerBound = new DateTime(timeToCover.Year, timeToCover.Month, timeToCover.Day, timeToCover.Hour, timeToCover.Minute, timeToCover.Second - (timeToCover.Second % bin.ScaleFactor));
                    break;
                default:
                    throw new NotSupportedException("Bin Scale Level not supported");
            }
            return StartingTimeForLowerBound;
        }

        internal static int GetBinsCountBetweenIntervalsStartTime(DateTime smallerIntervalStartTime, DateTime largerIntervalStartTime, BinSize.BinSize bin)
        {
            if (smallerIntervalStartTime > largerIntervalStartTime)
                throw new ArgumentException();

            int count = -1;
            do
            {
                IncreaseTimeOneStep(ref smallerIntervalStartTime, bin);
                count++;
            } while (smallerIntervalStartTime < largerIntervalStartTime);
            return count;
        }

        internal static string GetTimeAxisLabelForStartingTime(DateTime startingTime, BinSize.BinSize bin)
        {
            if (IsIntervalTimeOnLabelScaleBorder(startingTime, BinScaleLevel.Year))
                return startingTime.ToString("yyyy");
            if (IsIntervalTimeOnLabelScaleBorder(startingTime, BinScaleLevel.Month))
                return startingTime.ToString("MM");
            if (IsIntervalTimeOnLabelScaleBorder(startingTime, BinScaleLevel.Day))
                return startingTime.ToString("dd");
            if (IsIntervalTimeOnLabelScaleBorder(startingTime, BinScaleLevel.Hour))
                return startingTime.ToString("HH");
            if (IsIntervalTimeOnLabelScaleBorder(startingTime, BinScaleLevel.Minute))
                return startingTime.ToString("mm");
            if (IsIntervalTimeOnLabelScaleBorder(startingTime, BinScaleLevel.Second))
                return startingTime.ToString("ss");
            return startingTime.ToString();
        }

        internal static bool IsIntervalTimeOnLabelScaleBorder(DateTime dateTime, BinScaleLevel timeAxisLabelsScale)
        {
            switch (timeAxisLabelsScale)
            {
                case BinScaleLevel.Year:
                    return dateTime.Millisecond == 0
                        && dateTime.Second == 0
                        && dateTime.Minute == 0
                        && dateTime.Hour == 0
                        && dateTime.Day == 1
                        && dateTime.Month == 1;
                case BinScaleLevel.Month:
                    return dateTime.Millisecond == 0
                        && dateTime.Second == 0
                        && dateTime.Minute == 0
                        && dateTime.Hour == 0
                        && dateTime.Day == 1;
                case BinScaleLevel.Day:
                    return dateTime.Millisecond == 0
                        && dateTime.Second == 0
                        && dateTime.Minute == 0
                        && dateTime.Hour == 0;
                case BinScaleLevel.Hour:
                    return dateTime.Millisecond == 0
                        && dateTime.Second == 0
                        && dateTime.Minute == 0;
                case BinScaleLevel.Minute:
                    return dateTime.Millisecond == 0
                        && dateTime.Second == 0;
                case BinScaleLevel.Second:
                    return dateTime.Millisecond == 0;
                default:
                    return false;
            }
        }

        internal static object GetTimeAxisLabelForStartingTime(DateTime t, BinScaleLevel timeAxisLabelsScale)
        {
            switch (timeAxisLabelsScale)
            {
                case BinScaleLevel.Year:
                    return GetYearLabel(t);
                case BinScaleLevel.Month:
                    return GetMonthLabel(t);
                case BinScaleLevel.Day:
                    return GetDayLabel(t);
                case BinScaleLevel.Hour:
                    return GetHourLabel(t);
                case BinScaleLevel.Minute:
                    return GetMniuteLabel(t);
                case BinScaleLevel.Second:
                    return GetSecondLabel(t);
                default:
                    throw new NotSupportedException();
            }
        }

        private static object GetSecondLabel(DateTime t)
        {
            if (t.Second != 0)
            {
                return t.ToString("ss");
            }
            else
            {
                return GetMniuteLabel(t);
            }
        }

        private static object GetMniuteLabel(DateTime t)
        {
            if (t.Minute != 0)
            {
                return t.ToString("mm");
            }
            else
            {
                return GetHourLabel(t);
            }
        }

        private static object GetHourLabel(DateTime t)
        {
            if (t.Hour != 0)
            {
                return t.ToString("HH");
            }
            else
            {
                return GetDayLabel(t);
            }
        }

        private static object GetDayLabel(DateTime t)
        {
            if (t.Day != 1)
            {
                return t.ToString("dd");
            }
            else
            {
                return GetMonthLabel(t);
            }
        }

        private static object GetMonthLabel(DateTime t)
        {
            if (t.Month != 1)
            {
                return t.ToString("MMM");
            }
            else
            {
                return GetYearLabel(t);
            }
        }

        private static object GetYearLabel(DateTime t)
        {
            return t.ToString("yyyy");
        }
    }
}
