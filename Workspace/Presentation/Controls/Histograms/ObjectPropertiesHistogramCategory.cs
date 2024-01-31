using GPAS.HistogramViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Controls.Histograms
{
    public class ObjectPropertiesHistogramCategory : IHistogramFillingProperty
    {
        public ObjectPropertiesHistogramCategory(string propertyTypeUri)
        {
            typeUri = propertyTypeUri;
            OrderByChanged += ObjectPropertiesHistogramCategory_OrderByChanged;
            HistogramValueCountsAdded += ObjectPropertiesHistogramCategory_HistogramValueCountsAdded;
        }

        private void ObjectPropertiesHistogramCategory_HistogramValueCountsAdded(object sender, HistogramCategoryValueCountAddedEventArgs e)
        {
            HistogramValueCountsOrderBy(OrderBy);
        }

        private void ObjectPropertiesHistogramCategory_OrderByChanged(object source, HistogramCategoryOrderByChangedEventArgs e)
        {
            HistogramValueCountsOrderBy(e.NewValue);
        }

        public void AddValueCountPairCollection(IEnumerable<ObjectPropertiesHistogramValueCountPair> pairToAddList)
        {
            subValueCountPairs.AddRange(pairToAddList);
            HistogramValueCountsAdded.Invoke(this, new HistogramCategoryValueCountAddedEventArgs(pairToAddList));
        }

        public event HistogramCategoryOrderByChangedHandler OrderByChanged;

        public event EventHandler<HistogramCategoryValueCountAddedEventArgs> HistogramValueCountsAdded;

        private HistogramPropertyNodeOrderBy orderBy;

        public HistogramPropertyNodeOrderBy OrderBy
        {
            get
            {
                return orderBy;
            }
            set
            {
                if (orderBy != value)
                {
                    OrderByChanged.Invoke(this, new HistogramCategoryOrderByChangedEventArgs(orderBy, value));
                }

                orderBy = value;
            }
        }

        private string typeUri;

        private List<ObjectPropertiesHistogramValueCountPair> subValueCountPairs = new List<ObjectPropertiesHistogramValueCountPair>();
        public List<IHistogramFillingValueCountPair> HistgramValueCounts
        {
            get
            {
                List<IHistogramFillingValueCountPair> result = new List<IHistogramFillingValueCountPair>();
                foreach (ObjectPropertiesHistogramValueCountPair item in subValueCountPairs)
                {
                    result.Add(item);
                }
                return result;
            }
        }

        public string HistogramTitle
        {
            get { return Logic.OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(typeUri); }
        }

        public void HistogramValueCountsOrderBy(HistogramPropertyNodeOrderBy orderBy)
        {
            if (orderBy == HistogramPropertyNodeOrderBy.Count)
            {
                subValueCountPairs = subValueCountPairs.OrderByDescending(svp => svp.IsTopValueCountPairInHistogramCategory).
                    ThenByDescending(svp => svp.HistogramCountForValue).ThenBy(svp => svp.HistogramValue).ToList();
            }
            else if (orderBy == HistogramPropertyNodeOrderBy.Title)
            {
                subValueCountPairs = subValueCountPairs.OrderByDescending(svp => svp.IsTopValueCountPairInHistogramCategory).
                    ThenBy(svp => svp.HistogramValue).ThenByDescending(svp => svp.HistogramCountForValue).ToList();
            }
            else { }
        }
    }
}
