using System;

namespace GPAS.Ontology
{
    public class DataType : BaseModel
    {
        #region Properties

        private string typeName = string.Empty;
        public string TypeName
        {
            get { return typeName; }
            set
            {
                if (SetValue(ref typeName, value))
                {
                    OnTypeUriChanged();
                }
            }
        }

        private BaseDataTypes baseDataType;
        public BaseDataTypes BaseDataType
        {
            get { return baseDataType; }
            set
            {
                if (SetValue(ref baseDataType, value))
                {
                    OnBaseDataTypeChanged(); 
                }
            }
        }

        #endregion
        public DataType() { }
        #region Events

        public event EventHandler TypeUriChanged;
        protected void OnTypeUriChanged()
        {
            TypeUriChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler BaseDataTypeChanged;
        protected void OnBaseDataTypeChanged()
        {
            BaseDataTypeChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    };
}
