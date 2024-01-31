using System;
using System.Xml.Serialization;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.DataSourceField
{
    [XmlInclude(typeof(ConstFieldModel))]
    [XmlInclude(typeof(MetaDataFieldModel))]
    [XmlInclude(typeof(PathPartFieldModel))]
    [XmlInclude(typeof(TableFieldModel))]
    [Serializable]
    public abstract class DataSourceFieldModel : BaseModel
    {
        #region Properties

        private string id;
        public string Id
        {
            get => id;
            set => SetValue(ref id, value);
        }

        private string title = string.Empty;
        public string Title
        {
            get => title;
            set => SetValue(ref title, value);
        }

        private string sampleValue = string.Empty;
        public string SampleValue
        {
            get => sampleValue;
            set
            {
                if (SetValue(ref sampleValue, value))
                {
                    OnSampleValueChanged();
                }
            }
        }

        private FieldType type = FieldType.None;
        public FieldType Type
        {
            get => type;
            set => SetValue(ref type, value);
        }

        private bool isSelected;

        [XmlIgnore]
        public bool IsSelected
        {
            get => isSelected;
            set
            {
                if (SetValue(ref isSelected, value))
                {
                    if (IsSelected)
                        OnSelected();
                    else
                        OnDeselected();
                }
            }
        }

        IDataSource ownerDataSource;
        [XmlIgnore]
        public IDataSource OwnerDataSource
        {
            get => ownerDataSource;
            set
            {
                if (SetValue(ref ownerDataSource, value))
                {
                }
            }
        }

        #endregion

        #region Methods

        protected DataSourceFieldModel()
        {
            Id = Guid.NewGuid().ToString();
        }

        public bool Equals(DataSourceFieldModel otherField)
        {
            return Id.Equals(otherField.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        #endregion

        #region Events

        public event EventHandler Selected;
        protected void OnSelected()
        {
            Selected?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Deselected;
        protected void OnDeselected()
        {
            Deselected?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler SampleValueChanged;
        protected void OnSampleValueChanged()
        {
            SampleValueChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
