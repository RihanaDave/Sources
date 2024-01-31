namespace GPAS.Dispatch.AdminTools.Model
{
    public class PropertiesModel : BaseModel
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

        private string baseType;
        public string BaseType
        {
            get => baseType;
            set
            {
                baseType = value;
                OnPropertyChanged();
            }
        }

        private string value;
        public string Value
        {
            get => value;
            set
            {
                this.value = value;
                OnPropertyChanged();
            }
        }

        //private BaseDataType _baseType;
        //public BaseDataType BaseType
        //{
        //    get => _baseType;
        //    set
        //    {
        //        _baseType = value;
        //        OnPropertyChanged();
        //    }
        //}      

        private long dataSourceId;
        public long DataSourceId
        {
            get => dataSourceId;
            set
            {
                dataSourceId = value;
                OnPropertyChanged();
            }
        }

        private bool searchIndexed;
        public bool SearchIndexed
        {
            get => searchIndexed;
            set
            {
                searchIndexed = value;
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
    }
}
