using GPAS.Workspace.Presentation.Controls.DataImport.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport
{
    public class DatabaseViewModel : BaseViewModel
    {
        #region Properties

        ObservableCollection<DatabaseModel> databaseCollection;
        public ObservableCollection<DatabaseModel> DatabaseCollection
        {
            get => databaseCollection;
            set
            {
                ObservableCollection<DatabaseModel> oldValue = DatabaseCollection;
                if (SetValue(ref databaseCollection, value))
                {
                    if (oldValue != null)
                    {
                        oldValue.CollectionChanged -= DatabaseCollection_CollectionChanged;
                    }
                    if (DatabaseCollection == null)
                    {
                        DatabaseCollection_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        DatabaseCollection.CollectionChanged -= DatabaseCollection_CollectionChanged;
                        DatabaseCollection.CollectionChanged += DatabaseCollection_CollectionChanged;

                        if (oldValue == null)
                        {
                            DatabaseCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, DatabaseCollection));
                        }
                        else
                        {
                            DatabaseCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, DatabaseCollection, oldValue));
                        }
                    }
                }
            }
        }

        DatabaseModel selectedDatabase = null;
        public DatabaseModel SelectedDatabase
        {
            get => selectedDatabase;
            set => SetValue(ref selectedDatabase, value);
        }

        List<SQLServerDataSourceModel> checkedTablesAndViews = new List<SQLServerDataSourceModel>();
        public IReadOnlyCollection<SQLServerDataSourceModel> CheckedTablesAndViews
        {
            get => checkedTablesAndViews.AsReadOnly();
        }

        bool canAddAllChecked = false;
        public bool CanAddAllChecked
        {
            get => canAddAllChecked;
            set => SetValue(ref canAddAllChecked, value);
        }

        #endregion

        #region Methods

        public DatabaseViewModel()
        {
            DatabaseCollection = new ObservableCollection<DatabaseModel>();
        }

        private void DatabaseCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems?.Count > 0)
            {
                foreach (DatabaseModel database in e.OldItems)
                {
                    database.Selected -= Database_Selected;
                    database.Deselected -= Database_Deselected;
                    database.CheckedTablesAndViewsChanged -= Database_CheckedTablesAndViewsChanged;
                }
            }

            if (e.NewItems?.Count > 0)
            {
                foreach (DatabaseModel database in e.NewItems)
                {
                    database.Selected -= Database_Selected;
                    database.Selected += Database_Selected;

                    database.Deselected -= Database_Deselected;
                    database.Deselected += Database_Deselected;

                    database.CheckedTablesAndViewsChanged -= Database_CheckedTablesAndViewsChanged;
                    database.CheckedTablesAndViewsChanged += Database_CheckedTablesAndViewsChanged;
                }
            }

            if (DatabaseCollection == null || DatabaseCollection.Count == 0)
            {
                SelectedDatabase = null;
            }
        }

        private void Database_CheckedTablesAndViewsChanged(object sender, EventArgs e)
        {
            if (DatabaseCollection == null)
                checkedTablesAndViews = new List<SQLServerDataSourceModel>();
            else
                checkedTablesAndViews = new List<SQLServerDataSourceModel>(
                    DatabaseCollection.SelectMany(db => db.TablesAndViewsCollection.Where(tav => tav.IsChecked == true))
                );

            CanAddAllChecked = checkedTablesAndViews?.Count > 0 && checkedTablesAndViews.All(ctav => ctav.IsValid);
        }

        private void Database_Deselected(object sender, EventArgs e)
        {
            SelectedDatabase = DatabaseCollection.FirstOrDefault(db => db.IsSelected);
        }

        private void Database_Selected(object sender, EventArgs e)
        {
            SelectedDatabase = (DatabaseModel)sender;
        }

        public async Task ReloadTotalTablesAndViewsInDatabases()
        {
            List<DatabaseModel> databases = new List<DatabaseModel>();
            await Task.Run(() => databases = DataImportUtility.GetAllTablesAndViewsInDatabases().Result.ToList());

            DatabaseCollection = new ObservableCollection<DatabaseModel>(databases);
        }

        public void UncheckedAllCheckedTablesAndViews()
        {
            foreach (SQLServerDataSourceModel sQLServerDataSource in CheckedTablesAndViews)
            {
                sQLServerDataSource.IsChecked = false;
            }
        }

        #endregion
    }
}
