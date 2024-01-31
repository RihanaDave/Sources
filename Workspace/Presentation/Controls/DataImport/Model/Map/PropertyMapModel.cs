using GPAS.Ontology;
using GPAS.PropertiesValidation;
using GPAS.Workspace.Logic;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.Map
{
    [XmlInclude(typeof(SinglePropertyMapModel))]
    [XmlInclude(typeof(MultiPropertyMapModel))]
    [Serializable]
    public abstract class PropertyMapModel : MapElement
    {
        #region Properties

        string id;
        public string Id
        {
            get => id;
            set => SetValue(ref id, value);
        }

        string typeUri = string.Empty;
        public string TypeUri
        {
            get => typeUri;
            set
            {
                if (SetValue(ref typeUri, value))
                {
                    OnTypeUriChanged();
                    OnScenarioChanged();
                }
            }
        }

        string title = string.Empty;
        public string Title
        {
            get => title;
            set
            {
                if (SetValue(ref title, value))
                {
                    PrepareWarnings();
                    OnTitleChanged();
                }
            }
        }

        string sampleValue = string.Empty;
        public string SampleValue
        {
            get => sampleValue;
            set
            {
                if (SetValue(ref sampleValue, value))
                {
                    SetValidationForSampleValue();
                    AfterSampleValueChanged();
                    OnSampleValueChanged();
                }
            }
        }

        string iconPath = null;
        public string IconPath
        {
            get => iconPath;
            set => SetValue(ref iconPath, value);
        }

        bool isResolvable = true;
        public bool IsResolvable
        {
            get => isResolvable;
            set => SetValue(ref isResolvable, value);
        }

        bool isValid = false;
        public bool IsValid
        {
            get => isValid;
            set
            {
                if (SetValue(ref isValid, value))
                {
                    OnIsValidChanged();
                }
            }
        }

        bool isDisplayName = false;
        public bool IsDisplayName
        {
            get => isDisplayName;
            set
            {
                bool safeValue = value;
                if (!CanSetAsForDisplayName)
                    safeValue = false;

                if (SetValue(ref isDisplayName, safeValue))
                {
                    OnIsDisplayNameChanged();
                }
            }
        }

        bool canSetAsForDisplayName = true;
        public bool CanSetAsForDisplayName
        {
            get => canSetAsForDisplayName;
            set
            {
                if (SetValue(ref canSetAsForDisplayName, value))
                {
                    if (!CanSetAsForDisplayName)
                        IsDisplayName = false;
                }
            }
        }

        private bool editable = true;
        public bool Editable
        {
            get => editable;
            set => SetValue(ref editable, value);
        }

        ValidationProperty sampleValueValidationStatus;
        public ValidationProperty SampleValueValidationStatus
        {
            get => sampleValueValidationStatus;
            set
            {
                if (SetValue(ref sampleValueValidationStatus, value))
                {
                    OnSampleValueValidationStatusChanged();
                }
            }
        }

        BaseDataTypes dataType = BaseDataTypes.None;
        public BaseDataTypes DataType
        {
            get => dataType;
            set
            {
                if (SetValue(ref dataType, value))
                {
                    SetValidationForSampleValue();
                }
            }
        }

        MultiPropertyMapModel parentProperty = null;

        [XmlIgnore]
        public MultiPropertyMapModel ParentProperty
        {
            get => parentProperty;
            set
            {
                if (SetValue(ref parentProperty, value))
                {
                    OnScenarioChanged();
                }
            }
        }

        ObjectMapModel ownerObject = null;

        [XmlIgnore]
        public ObjectMapModel OwnerObject
        {
            get => ownerObject;
            set
            {
                if (SetValue(ref ownerObject, value))
                {
                    SetOwnerObjectForInnerProperties();
                    PrepareWarnings();
                    OnScenarioChanged();
                }
            }
        }

        bool hasResolution = false;
        public bool HasResolution
        {
            get => hasResolution;
            set
            {
                if (SetValue(ref hasResolution, value))
                {
                    OnHasResolutionChanged();
                    OnScenarioChanged();
                }
            }
        }

        #endregion

        #region Methods

        protected PropertyMapModel()
        {
            Id = Guid.NewGuid().ToString();
        }

        protected abstract void PrepareWarnings();

        protected abstract void SetOwnerObjectForInnerProperties();

        protected virtual void SetSampleValue()
        {
            throw new NotImplementedException();
        }

        public bool Equals(PropertyMapModel otherProperty)
        {
            return Id.Equals(otherProperty?.Id);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// اعتبار سنجی مقدار ویژگی
        /// </summary>
        protected virtual void SetValidationForSampleValue()
        {
            if (DataType == BaseDataTypes.None)
                return;
            SampleValueValidationStatus = PropertyManager.IsPropertyValid(DataType, SampleValue, CultureInfo.CurrentCulture);
        }

        /// <summary>
        /// اعتبار سنجی ویژگی
        /// </summary>
        protected virtual void SetValidation()
        {
            IsValid = false;
        }

        /// <summary>
        /// کلیه مقادیر منتصب به ویژگی و مقادیر منتصب به زیر ویژگی های آن را برمی گرداند 
        /// </summary>
        /// <returns>Values + AllValues in innerProperties</returns>
        public virtual IEnumerable<ValueMapModel> GetAllValues()
        {
            throw new NotImplementedException();
        }

        public MultiPropertyMapModel GetRootProperty()
        {
            MultiPropertyMapModel root = ParentProperty;
            while (root?.ParentProperty != null)
            {
                root = root.ParentProperty;
            }
            return root;
        }

        public List<MultiPropertyMapModel> GetAncestors()
        {
            List<MultiPropertyMapModel> ancestors = new List<MultiPropertyMapModel>();
            MultiPropertyMapModel parentProperty = ParentProperty;
            while (parentProperty != null)
            {
                ancestors.Insert(0, parentProperty);
                parentProperty = parentProperty.ParentProperty;
            }
            return ancestors;
        }

        public string GetAncestorsTitle()
        {
            List<string> ancestorsTitle = GetAncestors().Select(anc => anc.Title).ToList();

            return string.Join("->", ancestorsTitle);
        }

        protected virtual void AfterSampleValueChanged()
        {

        }

        #endregion

        #region Events

        public event EventHandler SampleValueChanged;
        protected void OnSampleValueChanged()
        {
            SampleValueChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler IsValidChanged;
        protected void OnIsValidChanged()
        {
            IsValidChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler IsDisplayNameChanged;
        protected void OnIsDisplayNameChanged()
        {
            IsDisplayNameChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler TypeUriChanged;
        protected void OnTypeUriChanged()
        {
            TypeUriChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler TitleChanged;
        protected void OnTitleChanged()
        {
            TitleChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler SampleValueValidationStatusChanged;
        protected void OnSampleValueValidationStatusChanged()
        {
            SampleValueValidationStatusChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler HasResolutionChanged;
        protected void OnHasResolutionChanged()
        {
            HasResolutionChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
