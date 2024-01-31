using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using System;
using System.Xml.Serialization;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.MetaData
{
    public class MetaDataItemModel : BaseModel
    {
        #region Properties

        string id;
        public string Id
        {
            get => id;
            set => SetValue(ref id, value);
        }

        string title = string.Empty;
        public string Title
        {
            get => title;
            set => SetValue(ref title, value);
        }

        object _value = null;
        public object Value
        {
            get => _value;
            set
            {
                if (SetValue(ref _value, value))
                {
                    OnValueChanged();
                }
            }
        }

        MetaDataType type = MetaDataType.File;
        public MetaDataType Type
        {
            get => type;
            set => SetValue(ref type, value);
        }

        IDataSource ownerDataSource;
        [XmlIgnore]
        public IDataSource OwnerDataSource
        {
            get => ownerDataSource;
            set => SetValue(ref ownerDataSource, value);
        }

        IShellProperty shellMetaData;

        [XmlIgnore]
        public IShellProperty ShellMetaData
        {
            get => shellMetaData;
            set => SetValue(ref shellMetaData, value);
        }

        bool needsRecalculation = false;
        public bool NeedsRecalculation
        {
            get => needsRecalculation;
            set => SetValue(ref needsRecalculation, value);
        }

        #endregion

        #region Methods

        public MetaDataItemModel()
        {
            Id = Guid.NewGuid().ToString();
        }

        #endregion

        #region Events

        public event EventHandler ValueChanged;
        protected void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
