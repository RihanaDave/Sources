using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GPAS.HistogramViewer
{
    public interface IHistogramFillingValueCountPair
    {
        string HistogramValue { get; }
        int HistogramCountForValue { get; }
        bool IsTopValueCountPairInHistogramCategory{ get; }
    }
}