using System.Collections.ObjectModel;

namespace GPAS.FilterSearch
{
    public class CriteriaSet : BaseModel
    {
        public CriteriaSet()
        {
        }

        private ObservableCollection<CriteriaBase> criterias = new ObservableCollection<CriteriaBase>();
        public ObservableCollection<CriteriaBase> Criterias
        {
            get { return criterias; }
            set { SetValue(ref criterias, value); }
        }

        private BooleanOperator setOperator = BooleanOperator.All;
        public BooleanOperator SetOperator
        {
            get { return setOperator; }
            set { SetValue(ref setOperator, value); }
        }

        public bool IsEmpty()
        {
            return Criterias == null || Criterias.Count <= 0;
        }
    }
}
