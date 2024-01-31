using GPAS.HistogramViewer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.Workspace.Presentation.Controls.Histograms
{
    public class ObjectTypesHistogramCategory : IHistogramFillingProperty
    {
        public ObjectTypesHistogramCategory(string title)
        {
            this.title = title;
            OrderByChanged += ObjectTypesHistogramCategory_OrderByChanged;
            HistogramValueCountsAdded += ObjectTypesHistogramCategory_HistogramValueCountsAdded;
        }

        private void ObjectTypesHistogramCategory_HistogramValueCountsAdded(object sender, HistogramCategoryValueCountAddedEventArgs e)
        {
            HistogramValueCountsOrderBy(OrderBy);
        }

        private void ObjectTypesHistogramCategory_OrderByChanged(object source, HistogramCategoryOrderByChangedEventArgs e)
        {
            HistogramValueCountsOrderBy(e.NewValue);
        }

        public string title;
        public string HistogramTitle
        {
            get { return title; }
        }

        public event EventHandler<HistogramCategoryValueCountAddedEventArgs> HistogramValueCountsAdded;

        public event HistogramCategoryOrderByChangedHandler OrderByChanged;

        private HistogramPropertyNodeOrderBy orderBy;

        public HistogramPropertyNodeOrderBy OrderBy
        {
            get
            {
                return orderBy;
            }
            set
            {
                if(orderBy!=value)
                {
                    OrderByChanged.Invoke(this, new HistogramCategoryOrderByChangedEventArgs(orderBy, value));
                }

                orderBy = value;
            }
        }

        private List<ObjectTypesHistogramTypeCountPair> histgramValueCounts = new List<ObjectTypesHistogramTypeCountPair>();
        public List<IHistogramFillingValueCountPair> HistgramValueCounts
        {
            get
            {
                List<IHistogramFillingValueCountPair> result = new List<IHistogramFillingValueCountPair>();
                foreach (ObjectTypesHistogramTypeCountPair item in histgramValueCounts)
                {
                    result.Add(item);
                }
                return result;
            }
        }

        public void AddObjectTypeCollection(List<ObjectTypesHistogramTypeCountPair> objectTypeList)
        {
            histgramValueCounts.AddRange(objectTypeList);
            HistogramValueCountsAdded.Invoke(this, new HistogramCategoryValueCountAddedEventArgs(objectTypeList));
        }

        public void HistogramValueCountsOrderBy(HistogramPropertyNodeOrderBy orderBy)
        {
            if (orderBy == HistogramPropertyNodeOrderBy.Count)
            {
                histgramValueCounts = histgramValueCounts.OrderByDescending(hvc => hvc.IsTopValueCountPairInHistogramCategory).
                    ThenByDescending(hvc => hvc.HistogramCountForValue).ThenBy(hvc => hvc.HistogramValue).ToList();
            }
            else if (orderBy == HistogramPropertyNodeOrderBy.Title)
            {
                histgramValueCounts = histgramValueCounts.OrderByDescending(hvc => hvc.IsTopValueCountPairInHistogramCategory).
                    ThenBy(hvc => hvc.HistogramValue).ThenByDescending(hvc => hvc.HistogramCountForValue).ToList();
            }
            else { }
        }
    }
}
