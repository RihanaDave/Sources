using System.Collections.ObjectModel;

namespace GPAS.FilterSearch
{
    public class ObjectTypeCriteria : CriteriaBase
    {
        private ObservableCollection<string> objectsTypeUri = new ObservableCollection<string>();
        public ObservableCollection<string> ObjectsTypeUri
        {
            get { return objectsTypeUri; }
            set { SetValue(ref objectsTypeUri, value); }
        }
    }
}
