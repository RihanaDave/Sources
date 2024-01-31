namespace GPAS.LoadTest.Presenter.Model
{
    public class ProgressBarModel : BaseModel
    {
        private string stepsProgressTitle;
        public string StepsProgressTitle
        {
            get => stepsProgressTitle;
            set
            {
                stepsProgressTitle = value;
                OnPropertyChanged();
            }
        }

        private string stepsProgressNumber;
        public string StepsProgressNumber
        {
            get => stepsProgressNumber;
            set
            {
                stepsProgressNumber = value;
                OnPropertyChanged();
            }
        }

        private double stepsProgressMaximum;
        public double StepsProgressMaximum
        {
            get => stepsProgressMaximum;
            set
            {
                stepsProgressMaximum = value;
                OnPropertyChanged();
            }
        }

        private double stepsProgressValue;
        public double StepsProgressValue
        {
            get => stepsProgressValue;
            set
            {
                stepsProgressValue = value;
                OnPropertyChanged();
            }
        }

        private string currentStepNumber;
        public string CurrentStepNumber
        {
            get => currentStepNumber;
            set
            {
                currentStepNumber = value;
                OnPropertyChanged();
            }
        }

        private double currentStepMaximum;
        public double CurrentStepMaximum
        {
            get => currentStepMaximum;
            set
            {
                currentStepMaximum = value;
                OnPropertyChanged();
            }
        }

        private double currentStepValue;
        public double CurrentStepValue
        {
            get => currentStepValue;
            set
            {
                currentStepValue = value;
                OnPropertyChanged();
            }
        }
    }
}
