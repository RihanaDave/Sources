using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace GPAS.TimelineViewer
{
    public class SuperCategory : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event NotifyCollectionChangedEventHandler SubCategotiesChanged;
        protected void OnSubCategotiesChanged(NotifyCollectionChangedEventArgs e)
        {
            SubCategotiesChanged?.Invoke(this, e);
        }

        public SuperCategory()
        {
            SubCategories.CollectionChanged -= SubCategories_CollectionChanged;
            SubCategories.CollectionChanged += SubCategories_CollectionChanged;
        }

        private void SubCategories_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if(e.NewItems != null)
            {
                foreach (Category item in e.NewItems)
                {
                    item.Parent = this;
                }
            }

            OnSubCategotiesChanged(e);
        }

        private string title; 
        public string Title
        {
            get
            {
                return title;
            }

            set
            {
                if (value != title)
                {
                    title = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private ObservableCollection<Category> subCategories = new ObservableCollection<Category>();
        public ObservableCollection<Category> SubCategories
        {
            get
            {
                return subCategories;
            }
            set
            {
                if (value != subCategories)
                {
                    subCategories = value;
                    NotifyPropertyChanged();

                    SubCategories.CollectionChanged -= SubCategories_CollectionChanged;
                    SubCategories.CollectionChanged += SubCategories_CollectionChanged;
                }
            }
        }

        private bool? isChecked = true;
        public bool? IsChecked
        {
            get
            {
                return isChecked;
            }
            set
            {
                if (value != isChecked)
                {
                    isChecked = value;
                    NotifyPropertyChanged();

                    if (SubCategories != null && value != null)
                    {
                        foreach (var subCat in SubCategories)
                        {
                            subCat.IsChecked = (bool)value;
                        }
                    }
                }
            }
        }

        private object tag;
        public object Tag
        {
            get
            {
                return tag;
            }
            set
            {
                if (value != tag)
                {
                    tag = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool ExistsSubCategory(object identifier)
        {
            return SubCategories.Select(sub => sub.Identifier).Contains(identifier);
        }

        public Category FindSubCategory(object identifier)
        {
            return SubCategories.FirstOrDefault(sub => sub.Identifier.Equals(identifier));
        }

        public bool HasChild()
        {
            return SubCategories != null && SubCategories.Count > 0;
        }
    }
}
