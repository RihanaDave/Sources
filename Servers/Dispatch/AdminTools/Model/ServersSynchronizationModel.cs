using System.Configuration;

namespace GPAS.Dispatch.AdminTools.Model
{
    public class ServersSynchronizationModel : BaseModel
    {
        public ServersSynchronizationModel()
        {
            MaxNonSyncConcepts = int.Parse(ConfigurationManager.AppSettings["MaximumAcceptableUnsynchronized"]);
        }

        private bool isHorizonDataIndicesStable;
        public bool IsHorizonDataIndicesStable
        {
            get => isHorizonDataIndicesStable;
            set
            {
                isHorizonDataIndicesStable = value;
                OnPropertyChanged();
            }
        }

        private bool isSearchDataIndicesStable;
        public bool IsSearchDataIndicesStable
        {
            get => isSearchDataIndicesStable;
            set
            {
                isSearchDataIndicesStable = value;
                OnPropertyChanged();
            }
        }

        private double maxNonSyncConcepts ;
        public double MaxNonSyncConcepts
        {
            get => maxNonSyncConcepts;
            set
            {
                maxNonSyncConcepts = value;
                OnPropertyChanged();
            }
        }

        private double searchNonSyncObjects;
        public double SearchNonSyncObjects
        {
            get => searchNonSyncObjects;
            set
            {
                searchNonSyncObjects = value;
                OnPropertyChanged();
            }
        }

        private double horizonNonSyncObjects;
        public double HorizonNonSyncObjects
        {
            get => horizonNonSyncObjects;
            set
            {
                horizonNonSyncObjects = value;
                OnPropertyChanged();
            }
        }

        private double horizonNonSyncRelations;
        public double HorizonNonSyncRelations
        {
            get => horizonNonSyncRelations;
            set
            {
                horizonNonSyncRelations = value;
                OnPropertyChanged();
            }
        }

        private double horizonNonSyncCount;
        public double HorizonNonSyncCount
        {
            get => horizonNonSyncCount;
            set
            {
                horizonNonSyncCount = value;
                OnPropertyChanged();
            }
        }

        private double searchNonSyncCount;
        public double SearchNonSyncCount
        {
            get => searchNonSyncCount;
            set
            {
                searchNonSyncCount = value;
                OnPropertyChanged();
            }
        }
    }
}
