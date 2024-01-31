using GPAS.HistogramViewer;
using GPAS.Workspace.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Controls.Histograms
{
    public class ObjectPropertiesHistogramValueCountPair : IHistogramFillingValueCountPair
    {
        public ObjectPropertiesHistogramValueCountPair(string valueIndicator, List<KWProperty> propertiesWithTheTypeAndValue)
        {
            ValueIndicator = valueIndicator;
            PropertiesWithTheTypeAndValue = propertiesWithTheTypeAndValue;
        }

        public string ValueIndicator
        {
            get;
            private set;
        }

        public IEnumerable<KWProperty> PropertiesWithTheTypeAndValue
        {
            get;
            private set;
        }

        public IEnumerable<KWObject> ObjectsWithPerpertiesOfTypeAndValue
        {
            get
            {
                return PropertiesWithTheTypeAndValue.Select(p => p.Owner).Distinct();
            }
        }

        public int HistogramCountForValue
        {
            get { return ObjectsWithPerpertiesOfTypeAndValue.Count(); }
        }

        public string HistogramValue
        {
            get { return ValueIndicator; }
        }

        public bool IsTopValueCountPairInHistogramCategory
        {
            get { return false; }
        }
    }
}
