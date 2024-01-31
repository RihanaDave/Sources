using GPAS.HistogramViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Controls.Histograms
{
    public class ObjectPropertiesHistogramGroup : IHistogramFillingPropertiesGroup
    {
        public ObjectPropertiesHistogramGroup(string title)
        {
            Title = title;
        }

        private List<ObjectPropertiesHistogramCategory> subPropertyCategories = new List<ObjectPropertiesHistogramCategory>();

        public event EventHandler<NewGroupSubItemAddedEventArgs> NewSubItemAdded;
        protected void OnNewSubItemAdded(IEnumerable<ObjectPropertiesHistogramCategory> addedCategories)
        {
            if (NewSubItemAdded != null)
                NewSubItemAdded(this, new NewGroupSubItemAddedEventArgs(addedCategories));
        }

        public List<IHistogramFillingProperty> HistogramSubItems
        {
            get
            {
                List<IHistogramFillingProperty> result = new List<IHistogramFillingProperty>();
                foreach (ObjectPropertiesHistogramCategory item in subPropertyCategories)
                {
                    result.Add(item);
                }
                return result;
            }
        }
        public void AddSubCategory(ObjectPropertiesHistogramCategory categoryToAdd)
        {
            subPropertyCategories.Add(categoryToAdd);
            OnNewSubItemAdded(new ObjectPropertiesHistogramCategory[] { categoryToAdd});
        }

        public string Title
        {
            get;
            private set;
        }
        public string HistogramTitle
        {
            get { return Title; }
        }
    }
}
