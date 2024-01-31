namespace GPAS.LoadTest.Presenter.Model
{
    public class RunTestsInfoModel : BaseModel
    {
        public RunTestsInfoModel()
        {
            PublishStart = 10;
            PublishEnd = 10;
            RetrieveStart = 10;
            RetrieveEnd = 10;
        }

        private long publishStart;
        public long PublishStart
        {
            get => publishStart;
            set
            {
                publishStart = value;
                OnPropertyChanged();
            }
        }

        private long publishEnd;
        public long PublishEnd
        {
            get => publishEnd;
            set
            {
                publishEnd = value;
                OnPropertyChanged();
            }
        }

        private long retrieveStart;
        public long RetrieveStart
        {
            get => retrieveStart;
            set
            {
                retrieveStart = value;
                OnPropertyChanged();
            }
        }

        private long retrieveEnd;
        public long RetrieveEnd
        {
            get => retrieveEnd;
            set
            {
                retrieveEnd = value;
                OnPropertyChanged();
            }
        }
    }
}
