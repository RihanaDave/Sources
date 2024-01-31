using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Xml.Serialization;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model
{
    public class ImportingObject : BaseModel
    {
        #region Properties

        long id = 0;
        public long Id
        {
            get => id;
            set => SetValue(ref id, value);
        }

        string typeUri = string.Empty;
        public string TypeUri
        {
            get => typeUri;
            set => SetValue(ref typeUri, value);
        }

        string userFriendlyName = string.Empty;
        public string UserFriendlyName
        {
            get => userFriendlyName;
            set => SetValue(ref userFriendlyName, value);
        }

        string displayName = string.Empty;
        public string DisplayName
        {
            get => displayName;
            set => SetValue(ref displayName, value);
        }

        string iconPath = string.Empty;
        public string IconPath
        {
            get => iconPath;
            set => SetValue(ref iconPath, value);
        }

        ObservableCollection<ImportingProperty> properties = new ObservableCollection<ImportingProperty>();
        public virtual ObservableCollection<ImportingProperty> Properties
        {
            get => properties;
            set
            {
                ObservableCollection<ImportingProperty> oldVal = Properties;
                if (SetValue(ref properties, value))
                {
                    if (oldVal != null)
                    {
                        oldVal.CollectionChanged -= Properties_CollectionChanged;
                    }

                    if (Properties == null)
                    {
                        Properties_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        Properties.CollectionChanged -= Properties_CollectionChanged;
                        Properties.CollectionChanged += Properties_CollectionChanged;

                        if(oldVal == null)
                        {
                            Properties_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Properties));
                        }
                        else
                        {
                            Properties_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, Properties, oldVal));
                        }
                    }
                }
            }
        }

        ImportingProperty selectedProperty;
        public ImportingProperty SelectedProperty
        {
            get => selectedProperty;
            set
            {
                PreSelectedPropertyChanged(value);

                SetValue(ref selectedProperty, value);
            }
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

        bool? isChecked = false;
        public bool? IsChecked
        {
            get => isChecked;
            set
            {
                if (SetValue(ref isChecked, value))
                {
                    AfterIsCheckedChanged(value);

                    OnIsCheckedChanged();
                }
            }
        }

        bool isResolvable = true;
        public bool IsResolvable
        {
            get => isResolvable;
            set => SetValue(ref isResolvable, value);
        }

        GPAS.DataImport.ConceptsToGenerate.ImportingObject relatedDataImportLibImpoertingObject;

        [XmlIgnore]
        public GPAS.DataImport.ConceptsToGenerate.ImportingObject RelatedDataImportLibImpoertingObject
        {
            get => relatedDataImportLibImpoertingObject;
            set => SetValue(ref relatedDataImportLibImpoertingObject, value);
        }

        IDataSource ownerDataSource;

        [XmlIgnore]
        public IDataSource OwnerDataSource
        {
            get => ownerDataSource;
            set => SetValue(ref ownerDataSource, value);
        }

        #endregion

        #region Methods

        public ImportingObject()
        {
            Id = Guid.NewGuid().GetHashCode();
            Properties.CollectionChanged += Properties_CollectionChanged;
        }

        private void PreSelectedPropertyChanged(ImportingProperty selectedProperty)
        {
            foreach (ImportingProperty property in Properties)
            {
                if (!property.Equals(selectedProperty))
                    property.IsSelected = false;
            }
        }

        private void Properties_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems?.Count > 0)
            {
                foreach (ImportingProperty item in e.NewItems)
                {
                    item.OwnerObject = this;

                    item.Selected -= Property_Selected;
                    item.Selected += Property_Selected;

                    item.Deselected -= Property_Deselected;
                    item.Deselected += Property_Deselected;
                }
            }

            if (e.OldItems?.Count > 0)
            {
                foreach (ImportingProperty item in e.OldItems)
                {
                    item.IsSelected = false;

                    item.Selected -= Property_Selected;
                    item.Deselected -= Property_Deselected;
                }
            }

            if (Properties == null || Properties.Count == 0)
            {
                SelectedProperty = null;
            }
        }

        private void Property_Deselected(object sender, EventArgs e)
        {
            SelectedProperty = Properties.FirstOrDefault(p => p.IsSelected);
        }

        private void Property_Selected(object sender, EventArgs e)
        {
            SelectedProperty = (ImportingProperty)sender;
        }

        protected virtual void AfterIsCheckedChanged(bool? isChecked)
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

        public event EventHandler IsCheckedChanged;
        protected void OnIsCheckedChanged()
        {
            IsCheckedChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
