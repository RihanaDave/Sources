using GPAS.Dispatch.AdminTools.Model;
using GPAS.Dispatch.Entities;
using GPAS.Dispatch.Logic;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.Dispatch.AdminTools.ViewModel
{
    public class HorizonIndexManagerViewModel : BaseViewModel
    {
        private HorizonIndexModel newIndex = new HorizonIndexModel();
        public HorizonIndexModel NewIndex
        {
            get => newIndex;
            set
            {
                newIndex = value;
                OnPropertyChanged();
            }
        }

        private HorizonIndexModel editedIndex = new HorizonIndexModel();
        public HorizonIndexModel EditedIndex
        {
            get => editedIndex;
            set
            {
                editedIndex = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<HorizonIndexModel> AllIndexCollection { get; set; }

        public HorizonIndexManagerViewModel()
        {
            AllIndexCollection = new ObservableCollection<HorizonIndexModel>();
        }

        public async Task GetAllIndexes()
        {
            IEnumerable<HorizonIndex> indexList = null;

            await Task.Run(() =>
            {
                HorizonIndexManagement horizonIndexManagement = new HorizonIndexManagement();
                indexList = horizonIndexManagement.GetAllIndexes();
            });

            if (indexList != null)
                PrepareIndexCollectin(indexList);
        }

        public async Task CreateIndex()
        {
            if (NewIndex == null)
                return;

            HorizonIndex horizonIndex = new HorizonIndex()
            {
                NodeType = NewIndex.TypeUri,
                PropertiesType = NewIndex.Properties.Select(p => p.TypeUri).ToArray()
            };

            await Task.Run(() =>
            {
                HorizonIndexManagement horizonIndexManagement = new HorizonIndexManagement();
                horizonIndexManagement.CreateIndex(horizonIndex);
            });

            NewIndex.Reset();
        }

        public async Task DeleteIndex(HorizonIndexModel index)
        {
            if (index == null)
                return;

            HorizonIndex horizonIndex = new HorizonIndex()
            {
                NodeType = index.TypeUri,
                PropertiesType = index.Properties.Select(p => p.TypeUri).ToArray()
            };

            await Task.Run(() =>
            {
                HorizonIndexManagement horizonIndexManagement = new HorizonIndexManagement();
                horizonIndexManagement.DeleteIndex(horizonIndex);
            });
        }

        public async Task EditIndex(HorizonIndexModel oldIndex)
        {
            if (oldIndex == null)
                return;

            HorizonIndex oldHorizonIndex = new HorizonIndex()
            {
                NodeType = oldIndex.TypeUri,
                PropertiesType = oldIndex.Properties.Select(p => p.TypeUri).ToArray()
            };

            HorizonIndex newHorizonIndex = new HorizonIndex()
            {
                NodeType = EditedIndex.TypeUri,
                PropertiesType = EditedIndex.Properties.Select(p => p.TypeUri).ToArray()
            };

            await Task.Run(() =>
            {
                HorizonIndexManagement horizonIndexManagement = new HorizonIndexManagement();
                horizonIndexManagement.EditIndex(oldHorizonIndex, newHorizonIndex);
            });
        }

        public async Task DeleteAllIndexes()
        {
            await Task.Run(() =>
            {
                HorizonIndexManagement horizonIndexManagement = new HorizonIndexManagement();
                horizonIndexManagement.DeleteAllIndexes();
            });
        }

        private void PrepareIndexCollectin(IEnumerable<HorizonIndex> indexes)
        {
            if (AllIndexCollection.Count > 0)
                AllIndexCollection.Clear();

            int counter = 1;

            foreach (var index in indexes)
            {
                HorizonIndexModel indexModel = new HorizonIndexModel()
                {
                    IdToShow = counter,
                    TypeUri = index.NodeType
                };

                foreach (var property in index.PropertiesType)
                {
                    indexModel.Properties.Add(new HorizonIndexModel()
                    {
                        TypeUri = property
                    });
                }

                AllIndexCollection.Add(indexModel);
                counter++;
            }
        }
    }
}
