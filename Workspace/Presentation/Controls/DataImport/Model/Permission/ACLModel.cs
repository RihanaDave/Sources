using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Xml.Serialization;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.Permission
{
    [Serializable]
    public class ACLModel : BaseModel
    {
        #region Properties

        private ClassificationModel classification;
        public ClassificationModel Classification
        {
            get => classification;
            set
            {
                if (SetValue(ref classification, value))
                {
                    if (Classification != null)
                        Classification.OwnerACL = this;

                    OnClassificationChanged();
                }
            }
        }

        private ObservableCollection<ACIModel> permissions = new ObservableCollection<ACIModel>();
        public ObservableCollection<ACIModel> Permissions
        {
            get => permissions;
            set
            {
                ObservableCollection<ACIModel> oldVal = Permissions;
                if (SetValue(ref permissions, value))
                {
                    if (oldVal != null)
                    {
                        oldVal.CollectionChanged -= PermissionCollection_CollectionChanged;
                    }

                    if (Permissions == null)
                    {
                        PermissionCollection_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        Permissions.CollectionChanged -= PermissionCollection_CollectionChanged;
                        permissions.CollectionChanged += PermissionCollection_CollectionChanged;

                        if (oldVal == null)
                            PermissionCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Permissions));
                        else
                            PermissionCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, permissions, oldVal));
                    }
                }
            }
        }

        IDataSource ownerDataSource;
        [XmlIgnore]
        public IDataSource OwnerDataSource
        {
            get => ownerDataSource;
            set => SetValue(ref ownerDataSource, value);
        }

        #endregion

        #region Method

        public ACLModel()
        {
            Permissions = new ObservableCollection<ACIModel>();
        }

        public void Copy(MemoryStream memoryStream)
        {
            Utility.Utility.Serialize(memoryStream, this);
            memoryStream.Position = 0;
        }

        private void PermissionCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (ACIModel item in e.OldItems)
                {
                    item.AccessLevelChanged -= Item_AccessLevelChanged;
                }
            }

            if (e.NewItems != null)
            {
                foreach (ACIModel item in e.NewItems)
                {
                    item.OwnerACL = this;
                    item.AccessLevelChanged -= Item_AccessLevelChanged;
                    item.AccessLevelChanged += Item_AccessLevelChanged;
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Reset)
            {

            }

            OnPermissionsChanged(e);
        }

        private void Item_AccessLevelChanged(object sender, EventArgs e)
        {
            OnGroupAccessLevelChanged();
        }

        #endregion

        #region Event

        public event EventHandler ClassificationChanged;
        protected void OnClassificationChanged()
        {
            ClassificationChanged?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler GroupAccessLevelChanged;
        protected void OnGroupAccessLevelChanged()
        {
            GroupAccessLevelChanged?.Invoke(this, EventArgs.Empty);
        }

        public event NotifyCollectionChangedEventHandler PermissionsChanged;
        protected void OnPermissionsChanged(NotifyCollectionChangedEventArgs e)
        {
            PermissionsChanged?.Invoke(this, e);
        }

        #endregion
    }
}
