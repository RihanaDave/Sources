using System;
using System.Collections.ObjectModel;

namespace GPAS.Dispatch.AdminTools.Model
{
    public class ObjectModel : BaseModel
    {
        private long id;
        public long Id
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged();
            }
        }

        private string type;
        public string Type
        {
            get => type;
            set
            {
                type = value;
                OnPropertyChanged();
            }
        }

        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private object resolveTo;
        public object ResolveTo
        {
            get => resolveTo;
            set
            {
                resolveTo = value;
                OnPropertyChanged();
            }
        }

        private bool? faceDetected;
        public bool? FaceDetected
        {
            get => faceDetected;
            set
            {
                faceDetected = value;
                OnPropertyChanged();
            }
        }

        private bool objectIndex;
        public bool ObjectIndex
        {
            get => objectIndex;
            set
            {
                objectIndex = value;
                OnPropertyChanged();
            }
        }

        private bool? documentIndex;
        public bool? DocumentIndex
        {
            get => documentIndex;
            set
            {
                documentIndex = value;
                OnPropertyChanged();
            }
        }

        private bool? imageIndex;
        public bool? ImageIndex
        {
            get => imageIndex;
            set
            {
                imageIndex = value;
                OnPropertyChanged();
            }
        }

        private string imageIndexCount;
        public string ImageIndexCount
        {
            get => imageIndexCount;
            set
            {
                imageIndexCount = value;
                OnPropertyChanged();
            }
        }

        private bool horizonIndexed;
        public bool HorizonIndexed
        {
            get => horizonIndexed;
            set
            {
                horizonIndexed = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<PropertiesModel> properties;
        public ObservableCollection<PropertiesModel> Properties
        {
            get => properties;
            set
            {
                properties = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<RelationModel> relatedEntities;
        public ObservableCollection<RelationModel> RelatedEntities
        {
            get => relatedEntities;
            set
            {
                relatedEntities = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<RelationModel> relatedEvents;
        public ObservableCollection<RelationModel> RelatedEvents
        {
            get => relatedEvents;
            set
            {
                relatedEvents = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<RelationModel> relatedDocuments;
        public ObservableCollection<RelationModel> RelatedDocuments
        {
            get => relatedDocuments;
            set
            {
                relatedDocuments = value;
                OnPropertyChanged();
            }
        }

        public void Reset()
        {
            Id = 0;
            Type = String.Empty;
            Name = String.Empty;
            ResolveTo = null;
            FaceDetected = false;

            try
            {
                if (Properties.Count != 0)
                {
                    Properties.Clear();
                }

                if (RelatedEntities.Count != 0)
                {
                    RelatedEntities.Clear();
                }

                if (RelatedEvents.Count != 0)
                {
                    RelatedEvents.Clear();
                }

                if (RelatedDocuments.Count != 0)
                {
                    RelatedDocuments.Clear();
                }
            }
            catch
            {
                //Do nothing
            }
        }
    }
}
