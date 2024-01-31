using GPAS.LoadTest.Core;

namespace GPAS.LoadTest.Presenter.Model
{
    public class AvailableLoadTestsModel : BaseModel
    {
        private string loadTestName;
        public string LoadTestName
        {
            get => loadTestName;
            set
            {
                loadTestName = value;
                OnPropertyChanged();
            }
        }

        private TestClassType tag;
        public TestClassType Tag
        {
            get => tag;
            set
            {
                tag = value;
                OnPropertyChanged();
            }
        }

    }
}
