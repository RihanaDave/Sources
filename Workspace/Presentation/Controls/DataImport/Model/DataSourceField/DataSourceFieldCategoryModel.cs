using System.Collections.ObjectModel;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.DataSourceField
{
    public class DataSourceFieldCategoryModel : BaseModel
    {
        #region Properties

        FieldType category = FieldType.None;
        public FieldType Category
        {
            get => category;
            set => SetValue(ref category, value);
        }

        ObservableCollection<DataSourceFieldModel> dataSourceCollection = new ObservableCollection<DataSourceFieldModel>();
        public ObservableCollection<DataSourceFieldModel> DataSourceCollection
        {
            get => dataSourceCollection;
            set => SetValue(ref dataSourceCollection, value);
        }       

        #endregion
    }
}
