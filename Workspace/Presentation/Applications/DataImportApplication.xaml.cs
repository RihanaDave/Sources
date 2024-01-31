using GPAS.Workspace.Entities;
using GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport;
using GPAS.Workspace.Presentation.Controls.DataImport.EventArguments;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace GPAS.Workspace.Presentation.Applications
{
    /// <summary>
    /// Interaction logic for DataImportApplication.xaml
    /// </summary>
    public partial class DataImportApplication
    {
        public DataImportApplication()
        {
            InitializeComponent();
        }

        public void DocumentCreationRequest(string filePath)
        {
            AddFiles(new string[] { filePath });
        }

        public event EventHandler<SelectionChangedEventArgs> DataSourceListSelectionChanged;

        private void OnDataSourceListSelectionChanged(SelectionChangedEventArgs e)
        {
            DataSourceListSelectionChanged?.Invoke(this, e);
        }

        private void DataImportUserControlOnAddFiles(object sender, AddFilesEventArgs e)
        {
            AddFiles(e.FilesPath);
        }

        private void DataImportUserControlOnRemoveDataSources(object sender, EventArgs e)
        {
            RemoveDataSources();
        }

        private void DataImportUserControlOnDataSourceListSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            OnDataSourceListSelectionChanged(e);
        }

        private async void AddFiles(string[] filesPath)
        {
            try
            {
                DataImportUserControl.MainWaitingControl.Message = Properties.Resources.Please_Wait;
                DataImportUserControl.MainWaitingControl.TaskIncrement();
                await ((DataImportViewModel)DataContext).AddDataSource(filesPath);
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            finally
            {
                DataImportUserControl.MainWaitingControl.TaskDecrement();
            }
        }

        private void RemoveDataSources()
        {
            try
            {
                DataImportUserControl.MainWaitingControl.TaskIncrement();
                ((DataImportViewModel)DataContext).RemoveSelectedDataSources();
            }
            catch (Exception ex)
            {
                KWMessageBox.Show(ex.Message);
            }
            finally
            {
                DataImportUserControl.MainWaitingControl.TaskDecrement();
            }
        }

        private void DataImportUserControl_ShowOnGraphRequested(object sender, Controls.DataImport.ShowOnGraphRequestedEventArgs e)
        {
            OnShowOnGraphRequested(new ShowOnGraphRequestedEventArgs(e.ObjectRequestedToShowOnGraph));
        }

        public void Reset()
        {
            ((DataImportViewModel)DataContext).Reset();
            DataImportUserControl.Reset();
        }

        public event EventHandler<ShowOnGraphRequestedEventArgs> ShowOnGraphRequested;
        protected void OnShowOnGraphRequested(ShowOnGraphRequestedEventArgs e)
        {
            ShowOnGraphRequested?.Invoke(this, e);
        }

        public class ShowOnGraphRequestedEventArgs : EventArgs
        {
            public ShowOnGraphRequestedEventArgs(IEnumerable<KWObject> objectRequestedToShowOnGraph)
            {
                ObjectRequestedToShowOnGraph = objectRequestedToShowOnGraph;
            }
            public IEnumerable<KWObject> ObjectRequestedToShowOnGraph
            {
                get;
                private set;
            }
        }
    }
}
