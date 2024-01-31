using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace GPAS.Dispatch.AdminTools.Model
{
    public class HorizonIndexModel : BaseModel
    {
        public HorizonIndexModel()
        {
            Properties = new ObservableCollection<HorizonIndexModel>();
        }

        private int idToShow ;

        public int IdToShow
        {
            get => idToShow;
            set
            {
                idToShow = value;
                OnPropertyChanged();
            }
        }

        private BitmapSource icon = null;

        public BitmapSource Icon
        {
            get => icon;
            set
            {
                icon = value;
                OnPropertyChanged();
            }
        }

        private string typeUriToShow;

        public string TypeUriToShow
        {
            get => typeUriToShow;
            set
            {
                typeUriToShow = value;
                OnPropertyChanged();
            }
        }

        private string typeUri;

        public string TypeUri
        {
            get => typeUri;
            set
            {
                typeUri = value;
                OnPropertyChanged();

                if (!string.IsNullOrEmpty(typeUri))
                {
                    TypeUriToShow = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(typeUri);
                    Icon = new BitmapImage(OntologyProvider.GetTypeIconPath(typeUri));
                }                
            }
        }

        private ObservableCollection<HorizonIndexModel> properties;

        public ObservableCollection<HorizonIndexModel> Properties
        {
            get => properties;
            set
            {
                properties = value;
                OnPropertyChanged();
            }
        }

        public void Reset()
        {
            Icon = null;
            TypeUri = string.Empty;
            TypeUriToShow = string.Empty;

            if (Properties.Count > 0)
                Properties.Clear();
        }
    }
}
