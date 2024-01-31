using GPAS.Dispatch.Entities.Concepts.Geo;
using GPAS.Ontology;
using GPAS.PropertiesValidation;
using GPAS.Workspace.Logic;
using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Xml.Serialization;

namespace GPAS.Workspace.Presentation.Controls.CustomSearchAround.Model
{
    public class CSAProperty : BaseModel, ICSAElement, ISelectable
    {
        #region Properties

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
            set
            {
                if (SetValue(ref dataType, value))
                {
                    CriteriaValue = GetDefaultCriteriaValueByBaseDataType(DataType);
                    SetValidation();
                    PrepareDefections();
                    OnScenarioChanged();
                }
            }
        }

        string criteriaValue = string.Empty;
        public string CriteriaValue
        {
            get => criteriaValue;
            set
            {
                if (SetValue(ref criteriaValue, value))
                {
                    SetValidation();
                    PrepareDefections();
                    OnScenarioChanged();
                }
            }
        }

        RelationalOperator relationalOperator = RelationalOperator.Equals;
        public RelationalOperator RelationalOperator
        {
            get => relationalOperator;
            set
            {
                if (SetValue(ref relationalOperator, value))
                {
                    OnScenarioChanged();
                }
            }
        }

        bool isValid = true;
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

        ObservableCollection<WarningModel> defections = null;

        [XmlIgnore]
        public ObservableCollection<WarningModel> Defections
        {
            get => defections;
            set
            {
                ObservableCollection<WarningModel> oldValue = null;
                if (SetValue(ref defections, value))
                {
                    if (oldValue != null)
                    {
                        oldValue.CollectionChanged -= Defections_CollectionChanged;
                    }
                    if (Defections == null)
                    {
                        Defections_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        Defections.CollectionChanged -= Defections_CollectionChanged;
                        Defections.CollectionChanged += Defections_CollectionChanged;

                        if (oldValue == null)
                        {
                            Defections_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Defections));
                        }
                        else
                        {
                            Defections_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, Defections, oldValue));
                        }
                    }
                }
            }
        }

        bool isSelected = false;

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

        CSAObject ownerObject;

        [XmlIgnore]
        public CSAObject OwnerObject
        {
            get => ownerObject;
            set
            {
                if (SetValue(ref ownerObject, value))
                {
                    OnScenarioChanged();
                }
            }
        }

        #endregion

        #region Methods

        public CSAProperty()
        {
            Defections = new ObservableCollection<WarningModel>();
        }

        private void Defections_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnDefectionsChanged(e);
        }

        protected virtual void SetValidation()
        {
            IsValid = PropertyManager.IsPropertyValid(DataType, CriteriaValue, CultureInfo.CurrentCulture).Status != ValidationStatus.Invalid;
        }

        protected virtual void PrepareDefections()
        {
            Defections = new ObservableCollection<WarningModel>();
            ValidationProperty validationProperty = PropertyManager.IsPropertyValid(DataType, CriteriaValue, CultureInfo.CurrentCulture);

            switch (validationProperty.Status)
            {
                case ValidationStatus.Invalid:
                    Defections.Add(new WarningModel()
                    {
                        Icon = MaterialDesignThemes.Wpf.PackIconKind.Dangerous,
                        RelatedElement = this,
                        WarningType = WarningType.ValueNotMachWithDataType,
                        Message = $"The {Title} property value does not match the type"
                    });
                    break;
                case ValidationStatus.Warning:
                    break;
                case ValidationStatus.Valid:
                default:
                    break;
            }
        }

        private string GetDefaultCriteriaValueByBaseDataType(BaseDataTypes dataType)
        {
            if (dataType == BaseDataTypes.GeoPoint)
            {
                string Latitude = "0";
                string Longitude = "0";
                string Radius = "5000";

                return JsonConvert.SerializeObject(new { Latitude, Longitude, Radius });
            }
            else if (dataType == BaseDataTypes.GeoTime)
            {
                GeoTimeEntityRawData geoTimeEntityRawData = new GeoTimeEntityRawData()
                {
                    Latitude = "0",
                    Longitude = "0",
                    TimeBegin = DateTime.Now.ToString(),
                    TimeEnd = DateTime.Now.ToString()
                };

                return GeoTime.GetGeoTimeStringValue(geoTimeEntityRawData);
            }
            else if (dataType == BaseDataTypes.Boolean)
            {
                return false.ToString();
            }
            else if (dataType == BaseDataTypes.Int || dataType == BaseDataTypes.Long || dataType == BaseDataTypes.Double)
            {
                return (0).ToString();
            }
            else if (dataType == BaseDataTypes.DateTime)
            {
                return new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).ToString(CultureInfo.CurrentCulture);
            }
            else if (dataType == BaseDataTypes.HdfsURI || dataType == BaseDataTypes.String)
            {
                return string.Empty;
            }

            return string.Empty;
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

        public event EventHandler IsValidChanged;
        protected void OnIsValidChanged()
        {
            IsValidChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler TypeUriChanged;
        protected void OnTypeUriChanged()
        {
            TypeUriChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler ScenarioChanged;
        protected void OnScenarioChanged()
        {
            ScenarioChanged?.Invoke(this, EventArgs.Empty);
        }

        public event NotifyCollectionChangedEventHandler DefectionsChanged;
        protected void OnDefectionsChanged(NotifyCollectionChangedEventArgs e)
        {
            DefectionsChanged?.Invoke(this, e);
        }

        #endregion
    }
}
