using GPAS.HistogramViewer;
using System.Collections.Generic;
using System.Linq;
using System;

namespace GPAS.Workspace.Presentation.Controls.Histograms
{
    public class ObjectTypesHistogramGroup : IHistogramFillingPropertiesGroup
    {
        public ObjectTypesHistogramGroup()
        {
        }
        List<ObjectTypesHistogramCategory> categories = new List<ObjectTypesHistogramCategory>();
        List<IHistogramFillingProperty> IHistogramFillingPropertiesGroup.HistogramSubItems
        {
            get
            {
                List<IHistogramFillingProperty> result = new List<IHistogramFillingProperty>();
                foreach (ObjectTypesHistogramCategory item in categories)
                {
                    result.Add(item);
                }
                return result;
            }
        }
        public string HistogramTitle
        {
            get { return Properties.Resources.Object_Types; }
        }

        public void AddSubCategories(IEnumerable<ObjectTypesHistogramCategory> categoriesToAdd)
        {
            categories.AddRange(categoriesToAdd);
            OnNewSubItemAdded(categoriesToAdd);
        }

        public static ObjectTypesHistogramGroup Empty = new ObjectTypesHistogramGroup();

        public event EventHandler<NewGroupSubItemAddedEventArgs> NewSubItemAdded;
        protected void OnNewSubItemAdded(IEnumerable<ObjectTypesHistogramCategory> addedCategories)
        {
            if (NewSubItemAdded != null)
                NewSubItemAdded(this, new NewGroupSubItemAddedEventArgs(addedCategories));
        }
    }
}