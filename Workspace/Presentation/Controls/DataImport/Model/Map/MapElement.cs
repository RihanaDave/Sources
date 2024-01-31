using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Xml.Serialization;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.Map
{
    public abstract class MapElement : BaseModel
    {
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
                    {
                        AfterSelected();
                        OnSelected();
                    }
                    else
                    {
                        OnDeselected();
                    }
                }
            }
        }

        ObservableCollection<MapWarningModel> warningCollection = new ObservableCollection<MapWarningModel>();
        [XmlIgnore]
        public ObservableCollection<MapWarningModel> WarningCollection
        {
            get => warningCollection;
            set
            {
                ObservableCollection<MapWarningModel> oldVal = WarningCollection;
                if (SetValue(ref warningCollection, value))
                {
                    if (oldVal != null)
                    {
                        oldVal.CollectionChanged -= WarningCollectionOnCollectionChanged;
                    }
                    if (WarningCollection == null)
                    {
                        WarningCollectionOnCollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        WarningCollection.CollectionChanged -= WarningCollectionOnCollectionChanged;
                        WarningCollection.CollectionChanged += WarningCollectionOnCollectionChanged;

                        if (oldVal == null)
                        {
                            WarningCollectionOnCollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, WarningCollection));
                        }
                        else
                        {
                            WarningCollectionOnCollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, WarningCollection, oldVal));
                        }
                    }
                }
            }
        }

        protected MapElement()
        {
            WarningCollection = new ObservableCollection<MapWarningModel>();
        }

        private void WarningCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnWarningCollectionChanged(e);
        }

        protected virtual void AfterSelected()
        {

        }

        #region Event

        public event NotifyCollectionChangedEventHandler WarningCollectionChanged;
        protected void OnWarningCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            WarningCollectionChanged?.Invoke(this, e);
        }

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

        public event EventHandler ScenarioChanged;
        protected void OnScenarioChanged()
        {
            ScenarioChanged?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
