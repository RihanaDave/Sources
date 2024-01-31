using System;

namespace GPAS.Ontology
{
    public class PropertyNode : OntologyNode
    {
        #region Properties

        private BaseDataTypes baseDataType = BaseDataTypes.None;
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

        #region Methodes

        public bool IsEmpty()
        {
            return BaseDataType == BaseDataTypes.None || string.IsNullOrEmpty(TypeUri);
        }

        #endregion

        #region Events

        public event EventHandler BaseDataTypeChanged;
        protected void OnBaseDataTypeChanged()
        {
            BaseDataTypeChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
