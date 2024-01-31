using System.Data;

namespace GPAS.LoadTest.Presenter.Model
{
    public class LoadTestResultModel : BaseModel
    {
        private string testTitle;
        public string TestTitle
        {
            get => testTitle;
            set
            {
                testTitle = value;
                OnPropertyChanged();
            }
        }

        private DataView resultTest;
        public DataView ResultTest
        {
            get => resultTest;
            set
            {
                resultTest = value;
                OnPropertyChanged();
            }
        }
    }
}
