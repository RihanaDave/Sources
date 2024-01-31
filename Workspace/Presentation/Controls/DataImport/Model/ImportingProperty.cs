using GPAS.Ontology;
using System;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model
{
    public class ImportingProperty : BaseModel
    {
        #region Properties

        string typeUri = string.Empty;
        public string TypeUri
        {
            get => typeUri;
            set => SetValue(ref typeUri, value);
        }

        string title = string.Empty;
        public string Title
        {
            get => title;
            set => SetValue(ref title, value);
        }

        string iconPath = null;
        public string IconPath
        {
            get => iconPath;
            set => SetValue(ref iconPath, value);
        }

        BaseDataTypes dataType = BaseDataTypes.None;
        public BaseDataTypes DataType
        {
            get => dataType;
            set => SetValue(ref dataType, value);
        }

        string _value = null;
        public string Value
        {
            get => _value;
            set => SetValue(ref _value, value);
        }

        bool isSelected = false;
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

        ImportingObject ownerObject;
        public ImportingObject OwnerObject
        {
            get => ownerObject;
            set => SetValue(ref ownerObject, value);
        }

        #endregion

        #region Methods

        public ImportingProperty()
        {

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

        #endregion
    }
}
