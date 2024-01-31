using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace GPAS.Workspace.Presentation.Controls.CustomSearchAround.Model
{
    [Serializable]
    public class RecentLoadedFilesManager : BaseModel
    {
        #region Properties

        string destinationSerializedFile = string.Empty;

        [XmlIgnore]
        public string DestinationSerializedFile
        {
            get => destinationSerializedFile;
            set
            {
                if (SetValue(ref destinationSerializedFile, value))
                {
                    ReloadSerializedFile();
                }
            }
        }

        int maximumNumber = 1000;

        [XmlIgnore]
        public int MaximumNumber
        {
            get => maximumNumber;
            set => SetValue(ref maximumNumber, value);
        }

        int displayedNumber = 20;

        [XmlIgnore]
        public int DisplayedNumber
        {
            get => displayedNumber;
            set => SetValue(ref displayedNumber, value);
        }

        ObservableCollection<LoadedFileModel> allItems = null;
        public ObservableCollection<LoadedFileModel> AllItems
        {
            get => allItems;
            set
            {
                ObservableCollection<LoadedFileModel> oldValue = AllItems;
                if (SetValue(ref allItems, value))
                {
                    if (oldValue != null)
                    {
                        oldValue.CollectionChanged -= AllItems_CollectionChanged;
                    }
                    if (AllItems == null)
                    {
                        AllItems_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        AllItems.CollectionChanged -= AllItems_CollectionChanged;
                        AllItems.CollectionChanged += AllItems_CollectionChanged;

                        if (oldValue == null)
                        {
                            AllItems_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, AllItems));
                        }
                        else
                        {
                            AllItems_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, AllItems, oldValue));
                        }
                    }
                }
            }
        }

        ObservableCollection<LoadedFileModel> sortedItems = null;

        [XmlIgnore]
        public ObservableCollection<LoadedFileModel> SortedItems
        {
            get => sortedItems;
            set
            {
                ObservableCollection<LoadedFileModel> oldValue = SortedItems;
                if (SetValue(ref sortedItems, value))
                {
                    if (oldValue != null)
                    {
                        oldValue.CollectionChanged -= SortedItems_CollectionChanged;
                    }
                    if (SortedItems == null)
                    {
                        SortedItems_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        SortedItems.CollectionChanged -= SortedItems_CollectionChanged;
                        SortedItems.CollectionChanged += SortedItems_CollectionChanged;

                        if (oldValue == null)
                        {
                            SortedItems_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, SortedItems));
                        }
                        else
                        {
                            SortedItems_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, SortedItems, oldValue));
                        }
                    }
                }
            }
        }

        ObservableCollection<LoadedFileModel> displayedItems = null;

        [XmlIgnore]
        public ObservableCollection<LoadedFileModel> DisplayedItems
        {
            get => displayedItems;
            set => SetValue(ref displayedItems, value);
        }

        #endregion

        #region Methods

        public RecentLoadedFilesManager()
        {

        }

        private void AllItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (AllItems.Count != AllItems.Select(i => i.FullPath).Distinct().Count())
                throw new ApplicationException("There is duplicate file in the list.");

            if (AllItems.Count > MaximumNumber)
            {
                AllItems = new ObservableCollection<LoadedFileModel>(AllItems.Take(MaximumNumber));
                return;
            }

            if (e.OldItems?.Count > 0)
            {
                foreach (LoadedFileModel loadedFile in e.OldItems)
                {
                    loadedFile.PinnedChanged -= LoadedFile_PinnedChanged;
                    loadedFile.DateUsedChanged -= LoadedFile_DateUsedChanged;
                }
            }

            if (e.NewItems?.Count > 0)
            {
                foreach (LoadedFileModel loadedFile in e.NewItems)
                {
                    loadedFile.PinnedChanged -= LoadedFile_PinnedChanged;
                    loadedFile.PinnedChanged += LoadedFile_PinnedChanged;

                    loadedFile.DateUsedChanged -= LoadedFile_DateUsedChanged;
                    loadedFile.DateUsedChanged += LoadedFile_DateUsedChanged;
                }
            }

            if (AllItems == null || AllItems.Count == 0)
            {

            }

            SaveToSerializedFile();
            ReorderItems();
        }

        private void LoadedFile_DateUsedChanged(object sender, EventArgs e)
        {
            SaveToSerializedFile();
            ReorderItems();
        }

        private void LoadedFile_PinnedChanged(object sender, EventArgs e)
        {
            SaveToSerializedFile();
            ReorderItems();
        }

        private void ReorderItems()
        {
            if (AllItems == null)
            {
                SortedItems = null;
                return;
            }

            SortedItems = new ObservableCollection<LoadedFileModel>(
                AllItems.OrderByDescending(i => i.Pinned).ThenByDescending(i => i.DateUsed)
                );
        }

        private void SortedItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (SortedItems == null)
                DisplayedItems = null;
            else
            {
                List<LoadedFileModel> allPin = SortedItems.Where(si => si.Pinned).ToList();
                List<LoadedFileModel> recents = SortedItems.Skip(allPin.Count).Take(DisplayedNumber - allPin.Count).ToList();
                DisplayedItems = new ObservableCollection<LoadedFileModel>(allPin.Concat(recents));
            }
        }

        private void ReloadSerializedFile()
        {
            try
            {
                if (!File.Exists(DestinationSerializedFile))
                {
                    if (AllItems == null)
                        AllItems = new ObservableCollection<LoadedFileModel>();

                    SaveToSerializedFile();
                    return;
                }

                RecentLoadedFilesManager recentLoadedFilesManager =
                    Utility.Utility.DeSerializeFromFile<RecentLoadedFilesManager>(DestinationSerializedFile);
                AllItems = recentLoadedFilesManager.AllItems;
            }
            catch
            {
                AllItems = new ObservableCollection<LoadedFileModel>();
            }
        }

        private void SaveToSerializedFile()
        {
            if (!string.IsNullOrWhiteSpace(DestinationSerializedFile))
                Utility.Utility.SerializeToFile<RecentLoadedFilesManager>(DestinationSerializedFile, this);
        }

        public void SafeAddItem(string filePath, DateTime addedTime)
        {
            LoadedFileModel loadedFile = AllItems.FirstOrDefault(i => i.FullPath == filePath);
            if (loadedFile == null)
            {
                loadedFile = new LoadedFileModel()
                {
                    FullPath = filePath,
                    DateUsed = addedTime,
                };

                AllItems.Add(loadedFile);
            }
            else
            {
                loadedFile.DateUsed = addedTime;
            }
        }

        #endregion
    }
}
