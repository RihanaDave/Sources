using GPAS.Workspace.Presentation.Controls.DataImport.Model.MetaData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model
{
    public interface IGroupDataSource : IDataSource
    {
        ObservableCollection<IDataSource> DataSourceCollection { get; set; }
        ObservableCollection<IDataSource> SelectedDataSources { get; set; }
        IDataSource SelectedDataSource { get; set; }
        MetaDataItemModel DataSourcesCountMetaDataItem { get; }

        void AddRangeDataSources(IEnumerable<IDataSource> dataSources);
        void RemoveRangeDataSources(IEnumerable<IDataSource> dataSources);
        void UngroupDataSources(IEnumerable<IDataSource> dataSources);
        void MoveDataSources(IEnumerable<IDataSource> dataSources, GroupDataSourceModel target);
        long GetAllDataSourcesFileSize();

        event NotifyCollectionChangedEventHandler DataSourceCollectionChanged;
        event EventHandler<UngroupDataSourceEventArgs> UngroupRequested;
    }
}
