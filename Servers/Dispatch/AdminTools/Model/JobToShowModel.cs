using GPAS.Dispatch.AdminTools.Model;
using GPAS.Dispatch.Entities.Jobs;
using System.Windows.Media.Imaging;

namespace GPAS.Dispatch.AdminTools.JobManager
{
    public class JobToShowModel : BaseModel
    {
        private BitmapImage jobStatusIcon;
        public BitmapImage JobStatusIcon
        {
            get => jobStatusIcon;
            set
            {
                jobStatusIcon = value;
                OnPropertyChanged();
            }
        }

        private string jobStatusTooltip;
        public string JobStatusTooltip
        {
            get => jobStatusTooltip;
            set
            {
                jobStatusTooltip = value;
                OnPropertyChanged();
            }
        }
        
        private bool isJobRestartable;
        public bool IsJobRestartable
        {
            get => isJobRestartable;
            set
            {
                isJobRestartable = value;
                OnPropertyChanged();
            }
        }

        private string registerTime;
        public string RegisterTime
        {
            get => registerTime;
            set
            {
                this.registerTime = value;
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

        private JobRequest jobRequest;
        public JobRequest JobRequest
        {
            get => jobRequest;
            set
            {
                jobRequest = value;
                OnPropertyChanged();
            }
        }
    }
}
