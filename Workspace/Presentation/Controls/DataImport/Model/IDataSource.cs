using GPAS.Workspace.Presentation.Controls.DataImport.Model.DataSourceField;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.Map;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.MetaData;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.Permission;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model
{
    public interface IDataSource : IPreviewableDataSource, ISelectable, ICheckable, IProgressive
    {
        long Id { get; set; }
        string Title { get; set; }
        string Type { get; set; }
        BitmapSource LargeIcon { get; set; }
        BitmapSource SmallIcon { get; set; }
        DateTime AddedTime { get; set; }
        bool IsValid { get; set; }
        MapModel Map { get; set; }
        ObservableCollection<DataSourceFieldModel> FieldCollection { get; set; }
        ObservableCollection<MetaDataItemModel> MetaDataCollection { get; set; }
        DataSourceImportStatus ImportingObjectCollectionGenerationStatus { get; set; }
        ObservableCollection<ImportingObject> ImportingObjectCollection { get; set; }
        ObservableCollection<ImportingRelationship> ImportingRelationshipCollection { get; set; }
        ObservableCollection<DefectionModel> DefectionMessageCollection { get; set; }
        ACLModel Acl { get; set; }
        DataSourceImportStatus ImportStatus { get; set; }
        GroupDataSourceModel ParentDataSource { get; set; }
        ObservableCollection<long> ImportedObjectsId { get; set; }

        Task RecalculateMetaDataItemAsync(MetaDataItemModel metaDataItem);
        bool CanImportWorkSpaceSide();

        event EventHandler MapChanged;
        event EventHandler AclChanged;
        event NotifyCollectionChangedEventHandler FieldCollectionChanged;
        event NotifyCollectionChangedEventHandler MetaDataCollectionChanged;
        event NotifyCollectionChangedEventHandler ImportingObjectCollectionChanged;
        event EventHandler ImportStatusChanged;
        event EventHandler IsCompletedGenerationImportingObjectCollectionChanged;
        event EventHandler IsValidChanged;
    }
}
