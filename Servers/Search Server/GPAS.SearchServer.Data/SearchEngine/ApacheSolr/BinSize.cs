using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Access.SearchEngine.ApacheSolr
{
    internal class BinSize
    {
        public BinSize(BinScaleLevel scaleLevel, int scaleFactor, BinScaleLevel timeAxisLabelsScale, string title, int gapBetweenLabels = 0)
        {
            if (gapBetweenLabels < 0)
                throw new ArgumentOutOfRangeException("gapBetweenLabels");

            ScaleLevel = scaleLevel;
            ScaleFactor = scaleFactor;
            TimeAxisLabelsScale = timeAxisLabelsScale;
            GapBetweenLabels = gapBetweenLabels;
            Title = title;
        }

        internal BinScaleLevel ScaleLevel
        {
            private set;
            get;
        }
        internal int ScaleFactor
        {
            private set;
            get;
        }

        internal string Title
        {
            private set;
            get;
        }
        internal int GapBetweenLabels
        {
            private set;
            get;
        }

        internal BinScaleLevel TimeAxisLabelsScale
        {
            private set;
            get;
        }

        public override string ToString()
        {
            return Title;
        }
    }
}
