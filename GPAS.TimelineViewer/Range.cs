using GPAS.TimelineViewer.EventArguments;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace GPAS.TimelineViewer
{
    public class Range : INotifyPropertyChanged
    {
        public event EventHandler<RangeChangedEventArgs> RangeChanged;
        protected void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public void OnRangeChanged(RangeChangedEventArgs args)
        {
            RangeChanged?.Invoke(this, args);
        }

        private DateTime from = new DateTime();
        public DateTime From
        {
            get { return from; }
            set
            {
                if (value != from)
                {
                    OnRangeChanged(new RangeChangedEventArgs(value, To, from, To));
                    from = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private DateTime to = new DateTime();
        public DateTime To
        {
            get { return to; }
            set
            {
                if(value!= to)
                {
                    OnRangeChanged(new RangeChangedEventArgs(From, value, From, to));
                    to = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public TimeSpan Duration
        {
            get
            {
                return To - From;
            }
        }

        public List<Range> Subtract(Range otherRange)
        {
            List<Range> subtractRanges = new List<Range>();
            Range intersect = Intersect(otherRange);
            if (intersect == null)
            {
                subtractRanges.Add(this);
            }
            else
            {
                Range left = new Range()
                {
                    From = this.from,
                    To = intersect.From.AddTicks(-1),
                };
                Range right = new Range()
                {
                    From = intersect.To.AddTicks(1),
                    To = this.To,
                };

                var d = left.Duration.Ticks + right.Duration.Ticks;

                if (this.From == intersect.From && this.To == intersect.To)
                {

                }
                else if (this.From == intersect.From)
                {
                    subtractRanges.Add(right);
                }
                else if (this.To == intersect.To)
                {
                    subtractRanges.Add(left);
                }
                else
                {
                    subtractRanges.Add(left);
                    subtractRanges.Add(right);
                }
            }

            return subtractRanges;
        }

        private object tag = null;
        public object Tag
        {
            get { return tag; }
            set
            {
                if (value != tag)
                {
                    tag = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Range FitToBoundBinSizeRange(BinSize binSize)
        {
            return new Range()
            {
                From = Utility.Floor(From, binSize),
                To = Utility.Ceiling(To, binSize),
            };
        }

        public Range Intersect(Range otherRange)
        {
            return Range.Intersect(this, otherRange);
        }

        public static Range Intersect(Range range1, Range range2)
        {
            if (range1.To < range2.From || range1.From > range2.To)
                return null;

            Range intersectRange = new Range();
            intersectRange.From = (range1.From > range2.From) ? range1.From : range2.From;
            intersectRange.To = (range1.To < range2.To) ? range1.To : range2.To;
            return intersectRange;
        }

        internal IEnumerable<Range> GetRelatedBars(BinSize binSize)
        {
            List<Range> bars = new List<Range>();
            Range boundRange = FitToBoundBinSizeRange(binSize);
            boundRange.To = boundRange.To.AddTicks(-1);

            while (boundRange.Duration.Ticks > 0)
            {
                Range bar = new Range()
                {
                    From = boundRange.From,
                    To = binSize.BinAddToDate(boundRange.From, 1)
                };

                boundRange.From = boundRange.From.Add(bar.Duration);
                bar.To = bar.To.AddTicks(-1);

                bars.Add(bar);
            }

            return bars;
        }

        public FilterRange ConvertToFilterRange()
        {
            return new FilterRange()
            {
                From = From,
                To = To,
            };
        }
    }
}
