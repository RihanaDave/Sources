using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Xml.Serialization;

namespace GPAS.Workspace.Presentation.Controls.CustomSearchAround.Model
{
    [XmlInclude(typeof(CSAEventBaseLink))]
    public class CSALink : BaseModel, ICSAElement, ISelectable
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

        CSAObject relatedObject = null;
        
        public CSAObject RelatedObject
        {
            get => relatedObject;
            set
            {
                if (SetValue(ref relatedObject, value))
                {
                    if (RelatedObject != null)
                    {
                        RelatedObject.RelatedLink = this;
                    }

                    OnScenarioChanged();
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

        #endregion

        #region Methods

        public CSALink()
        {
            Defections = new ObservableCollection<WarningModel>();
        }

        private void Defections_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnDefectionsChanged(e);
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
