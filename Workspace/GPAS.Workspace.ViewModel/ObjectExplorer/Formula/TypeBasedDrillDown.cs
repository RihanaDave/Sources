using GPAS.Workspace.ViewModel.ObjectExplorer.Statistics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace GPAS.Workspace.ViewModel.ObjectExplorer.Formula
{
    public class TypeBasedDrillDown : FormulaBase
    {
        public event EventHandler<EventArgs> FilterByChanged;

        private List<PreviewStatistic> filterBy = new List<PreviewStatistic>();

        public ReadOnlyCollection<PreviewStatistic> FilteredBy
        {
            get { return filterBy.AsReadOnly(); }
        }

        public void AddItemsToFilterByProperty(IEnumerable<PreviewStatistic> items)
        {
            filterBy.AddRange(items);
            foreach (var item in items)
            {
                item.TitleChanged += FilterByItem_TitleChanged;
                item.SuperCategoryChanged += FilterByItem_SuperCategoryChanged;
            }

            OnFilterByChanged();
        }

        private void OnFilterByChanged()
        {
            FilterByChanged?.Invoke(this, new EventArgs());
        }

        private void FilterByItem_SuperCategoryChanged(object sender, TextChangedEventArgs e)
        {
            OnFilterByChanged();
        }

        private void FilterByItem_TitleChanged(object sender, TextChangedEventArgs e)
        {
            OnFilterByChanged();
        }
    }
}
