using GPAS.Dispatch.Entities.Jobs;

namespace GPAS.Dispatch.AdminTools.Model
{
    public class JobModel : BaseModel
    {
        private int id;
        public int Id
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged();
            }
        }

        private string registerTime;
        public string RegisterTime
        {
            get => registerTime;
            set
            {
                registerTime = value;
                OnPropertyChanged();
            }
        }

        private string beginTime;
        public string BeginTime
        {
            get => beginTime;
            set
            {
                beginTime = value;
                OnPropertyChanged();
            }
        }

        private string endTime;
        public string EndTime
        {
            get => endTime;
            set
            {
                endTime = value;
                OnPropertyChanged();
            }
        }

        private JobRequestStatus state;
        public JobRequestStatus State
        {
            get => state;
            set
            {
                state = value;
                OnPropertyChanged();
            }
        }

        private JobRequestType type;
        public JobRequestType Type
        {
            get => type;
            set
            {
                type = value;
                OnPropertyChanged();
            }
        }

        private string statusMessage;
        public string StatusMessage
        {
            get => statusMessage;
            set
            {
                statusMessage = value;
                OnPropertyChanged();
            }
        }

        private string lastPublishedObjectIndex;
        public string LastPublishedObjectIndex
        {
            get => lastPublishedObjectIndex;
            set
            {
                lastPublishedObjectIndex = value;
                OnPropertyChanged();
            }
        }
        
        private string lastPublishedRelationIndex;
        public string LastPublishedRelationIndex
        {
            get => lastPublishedRelationIndex;
            set
            {
                lastPublishedRelationIndex = value;
                OnPropertyChanged();
            }
        }
        private double percentageRateDone = 0;
        public double PercentageRateDone
        {
            get => percentageRateDone;
            set
            {
                percentageRateDone = value;
                OnPropertyChanged();
            }
        }
    }
}
