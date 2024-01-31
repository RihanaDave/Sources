using GPAS.Ontology;
using GPAS.Workspace.Entities.KWLinks;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Controls.DataImport;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.DataSourceField;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.Map;
using Shell32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport
{
    public class MappingViewModel : BaseViewModel
    {
        #region Properties

        private readonly Dictionary<string, int> counterObjectType = new Dictionary<string, int>();

        private MapModel map;
        public MapModel Map
        {
            get => map;
            set
            {
                if (SetValue(ref map, value))
                {
                    if (Map != null)
                    {
                        RegenerateCategoryFieldCollection();
                        Map.DataSourceFieldCollectionChanged -= Map_DataSourceFieldCollectionChanged;
                        Map.DataSourceFieldCollectionChanged += Map_DataSourceFieldCollectionChanged;
                        Map.ObjectCollectionChanged -= Map_ObjectCollectionChanged;
                        Map.ObjectCollectionChanged += Map_ObjectCollectionChanged;
                    }
                }
            }
        }        

        private OntologyNode newObjectNode;
        public OntologyNode NewObjectNode
        {
            get => newObjectNode;
            set => SetValue(ref newObjectNode, value);
        }

        private RelationshipMapModel newMapLink;
        public RelationshipMapModel NewMapLink
        {
            get => newMapLink;
            set => SetValue(ref newMapLink, value);
        }

        private MappingControlType currentMappingControl = MappingControlType.TabularMappingFirstStep;
        public MappingControlType CurrentMappingControl
        {
            get => currentMappingControl;
            set => SetValue(ref currentMappingControl, value);
        }

        ObservableCollection<MapWarningModel> warningCollection = new ObservableCollection<MapWarningModel>();
        public ObservableCollection<MapWarningModel> WarningCollection
        {
            get => warningCollection;
            set => SetValue(ref warningCollection, value);
        }

        ObservableCollection<SavedMapModel> recentMapCollection = new ObservableCollection<SavedMapModel>();
        public ObservableCollection<SavedMapModel> RecentMapCollection
        {
            get => recentMapCollection;
            set => SetValue(ref recentMapCollection, value);
        }

        ObservableCollection<DataSourceFieldCategoryModel> categoryFieldCollection = new ObservableCollection<DataSourceFieldCategoryModel>();
        public ObservableCollection<DataSourceFieldCategoryModel> CategoryFieldCollection
        {
            get => categoryFieldCollection;
            set => SetValue(ref categoryFieldCollection, value);
        }

        List<DataSourceFieldModel> selectedFields = new List<DataSourceFieldModel>();
        public List<DataSourceFieldModel> SelectedFields
        {
            get => selectedFields;
            set => SetValue(ref selectedFields, value);
        }

        #endregion

        #region Methodes

        public MappingViewModel(MapModel map)
        {
            Map = map;
            NewMapLink = new RelationshipMapModel();
            NewObjectNode = new OntologyNode();

            PrepareCommands();
            PrepareCounterObjectTypeDictionary();
        }

        private void Map_ObjectCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems?.Count > 0 && e.NewItems[e.NewItems.Count - 1] is DocumentMapModel)
            {

            }
        }

        private void Map_DataSourceFieldCollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RegenerateCategoryFieldCollection();
        }

        private void RegenerateCategoryFieldCollection()
        {
            CategoryFieldCollection.Clear();

            foreach (var field in Map.OwnerDataSource.FieldCollection)
            {
                AddFieldToCategoryFieldCollection(field);
            }
        }

        private void AddFieldToCategoryFieldCollection(DataSourceFieldModel field)
        {
            FieldType fieldType = GetFieldType(field);
            DataSourceFieldCategoryModel category = CategoryFieldCollection.FirstOrDefault(cf => cf.Category.Equals(fieldType));
            if (category == null)
            {
                category = new DataSourceFieldCategoryModel { Category = fieldType };
                CategoryFieldCollection.Add(category);
            }

            field.Selected -= Field_Selected;
            field.Selected += Field_Selected;

            field.Deselected -= Field_Deselected;
            field.Deselected += Field_Deselected;

            category.DataSourceCollection.Add(field);
        }

        private void Field_Deselected(object sender, EventArgs e)
        {
            SelectedFields = GetSelectedFields().ToList();
        }

        private void Field_Selected(object sender, EventArgs e)
        {
            SelectedFields = GetSelectedFields().ToList();
        }

        private IEnumerable<DataSourceFieldModel> GetSelectedFields()
        {
            return Map.OwnerDataSource.FieldCollection.Where(f => f.IsSelected);
        }

        private FieldType GetFieldType(DataSourceFieldModel field)
        {
            if (field is ConstFieldModel)
                return FieldType.Const;
            else if (field is TableFieldModel)
                return FieldType.Tabular;
            else if (field is PathPartFieldModel)
                return FieldType.Path;
            else if (field is MetaDataFieldModel)
                return FieldType.MetaData;
            else
            {
                throw new Exception("Field type not recognized.");
            }
        }

        private void PrepareCommands()
        {
            SaveCommand = new RelayCommand(SaveCommandMethod);
            LoadCommand = new RelayCommand(LoadCommandMethod);
            AddObjectCommand = new RelayCommand(AddObjectCommandMethod);
            AddLinkCommand = new RelayCommand(AddLinkCommandMethod);
            RemoveSelectedItemCommand = new RelayCommand(RemoveSelectedItemCommandMethod);
            ClearAllCommand = new RelayCommand(ClearAllCommandMethod);

            AddPropertyCommand = new RelayCommand(AddPropertyCommandMethod);
            RemovePropertyCommand = new RelayCommand(RemovePropertyCommandMethod);
            ClearAllPropertyCommand = new RelayCommand(ClearAllPropertyCommandMethod);
            PropertySetAsDisplayNameCommand = new RelayCommand(PropertySetAsDisplayNameCommandMethod);
            SetInternalResolutionCommand = new RelayCommand(SetInternalResolutionCommandMethod);

            AddValueCommand = new RelayCommand(AddValueCommandMethod);
            InsertValueCommand = new RelayCommand(InsertValueCommandMethod);
            RemoveValueCommand = new RelayCommand(RemoveValueCommandMethod);
            ClearAllValueCommand = new RelayCommand(ClearAllValueCommandMethod);
            MoveValueCommand = new RelayCommand(MoveValueCommandMethod);
            AddRegularExpressionCommand = new RelayCommand(AddRegularExpressionCommandMethod);
            RemoveRegularExpressionCommand = new RelayCommand(RemoveRegularExpressionCommandMethod);
        }

        public void FillRecentMapCollection()
        {
            if (RecentMapCollection.Count != 0)
                RecentMapCollection.Clear();

            string savedMapPath = string.Format(Properties.Settings.Default.SavedMapFolder,
                System.Windows.Forms.Application.StartupPath);

            if (Directory.Exists(savedMapPath))
            {
                string[] savedMapFiles = Directory.GetFiles(savedMapPath);

                ObservableCollection<SavedMapModel> recentMaps = new ObservableCollection<SavedMapModel>();

                foreach (string item in savedMapFiles)
                {
                    string targetFile = GetShortcutTargetFile(item);

                    if (string.IsNullOrEmpty(targetFile))
                        continue;

                    recentMaps.Add(new SavedMapModel
                    {
                        Name = Path.GetFileNameWithoutExtension(targetFile),
                        TargetPath = targetFile,
                        ShortcutPath = item,
                        LastAccessTime = File.GetLastAccessTime(item)
                    });

                    RecentMapCollection = new ObservableCollection<SavedMapModel>(recentMaps.OrderByDescending(x => x.LastAccessTime).Take(10));
                }
            }
        }

        private string GetShortcutTargetFile(string shortcutFilename)
        {
            string pathOnly = Path.GetDirectoryName(shortcutFilename);
            string filenameOnly = Path.GetFileName(shortcutFilename);

            Shell shell = new Shell();
            Folder folder = shell.NameSpace(pathOnly);
            FolderItem folderItem = folder.ParseName(filenameOnly);
            if (folderItem != null)
            {
                try
                {
                    ShellLinkObject link = (ShellLinkObject)folderItem.GetLink;
                    return link.Path;
                }
                catch (Exception)
                {
                    return string.Empty;
                }
            }

            return string.Empty;
        }

        public void SaveMap(string filePath)
        {
            Map.SaveToFile(filePath);
        }

        public void LoadMap(string filePath)
        {
            var mapModel = Utility.Utility.DeSerializeFromFile<MapModel>(filePath);
            var ontologyMatchResult = DataImportUtility.IsMapMatchWithOntology(mapModel);

            if (!ontologyMatchResult.IsOk)
                throw new Exception(ontologyMatchResult.ErrorMessage);

            var loadResult = DataImportUtility.LoadMap(Map.OwnerDataSource, mapModel);

            if (!loadResult.IsOk)
                throw new Exception(loadResult.ErrorMessage);

            Map.ClearAllItems();
            Map.OwnerDataSource.Map = mapModel;
            Map = mapModel;

            PrepareCounterObjectTypeDictionary();
        }

        public void DeleteSavedMap(string filePath)
        {
            if (File.Exists(filePath))
                File.Delete(filePath);

            FillRecentMapCollection();
        }

        public void PrepareCounterObjectTypeDictionary()
        {
            if (counterObjectType.Count != 0)
                counterObjectType.Clear();

            foreach (var objectMapModel in Map.ObjectCollection)
            {
                if (!counterObjectType.ContainsKey(objectMapModel.TypeUri))
                {
                    counterObjectType.Add(objectMapModel.TypeUri, 1);
                }
                else
                {
                    counterObjectType[objectMapModel.TypeUri]++;
                }
            }
        }

        public void AddObject(string typeUri)
        {
            AddObject(CreateObject(typeUri));
        }

        public static void AddObject(MapModel map, string typeUri)
        {
            AddObject(map, CreateObject(map, typeUri));
        }

        private void AddObject(ObjectMapModel obj)
        {
            Map.DeselectAllObjects();

            Map.ObjectCollection.Add(obj);
            obj.IsSelected = true;
        }

        private static void AddObject(MapModel map, ObjectMapModel obj)
        {
            map.ObjectCollection.Add(obj);
            obj.IsSelected = true;
        }

        public ObjectMapModel CreateObject(string typeUri)
        {
            if (!counterObjectType.ContainsKey(typeUri))
            {
                counterObjectType.Add(typeUri, 1);
            }
            else
            {
                counterObjectType[typeUri]++;
            }

            if (OntologyProvider.GetOntology().IsDocument(typeUri))
            {
                return new DocumentMapModel
                {
                    TypeUri = typeUri,
                    Title = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(typeUri) + counterObjectType[typeUri],
                    IconPath = OntologyIconProvider.GetTypeIconPath(typeUri).ToString(),
                    OwnerMap = Map,
                    IsResolvable = !OntologyProvider.GetOntology().IsDocument(typeUri),
                    DisplayNameChangeable = false
                };
            }
            else
            {
                return new ObjectMapModel
                {
                    TypeUri = typeUri,
                    Title = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(typeUri) + counterObjectType[typeUri],
                    IconPath = OntologyIconProvider.GetTypeIconPath(typeUri).ToString(),
                    OwnerMap = Map,
                    IsResolvable = !OntologyProvider.GetOntology().IsDocument(typeUri),
                };
            }
        }

        private static ObjectMapModel CreateObject(MapModel map, string typeUri)
        {
            if (OntologyProvider.GetOntology().IsDocument(typeUri))
            {
                return new DocumentMapModel
                {
                    TypeUri = typeUri,
                    Title = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(typeUri),
                    IconPath = OntologyIconProvider.GetTypeIconPath(typeUri).ToString(),
                    OwnerMap = map,
                    IsResolvable = !OntologyProvider.GetOntology().IsDocument(typeUri),
                    DisplayNameChangeable = false
                };
            }
            else
            {
                return new ObjectMapModel
                {
                    TypeUri = typeUri,
                    Title = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(typeUri),
                    IconPath = OntologyIconProvider.GetTypeIconPath(typeUri).ToString(),
                    OwnerMap = map,
                    IsResolvable = !OntologyProvider.GetOntology().IsDocument(typeUri),
                };
            }
        }

        /// <summary>
        /// افزودن موجودیت جدید به نگاشت از قسمت گراف
        /// </summary>
        public void AddNewObject()
        {
            AddObject(NewObjectNode.TypeUri);
        }

        private bool RemoveObjects(ObjectMapModel obj)
        {
            // با حذف موجودیت انتخاب شده روابط بین آنها نیز باید حذف شود
            List<RelationshipMapModel> relatedRelationships = GetRelatedRelationships(obj).ToList();
            foreach (var relationship in relatedRelationships)
            {
                RemoveRelationship(relationship);
            }

            return Map.ObjectCollection.Remove(obj);
        }

        public void AddLink(string linkTypeUri, string linkDescription, LinkDirection linkDirection, ObjectMapModel source,
            ObjectMapModel target)
        {
            if (source == null || target == null)
                return;

            if (OntologyProvider.GetOntology().IsEvent(linkTypeUri))
            {
                AddEventLink(linkTypeUri, linkDescription, linkDirection, source, target);
            }
            else
            {
                AddRelationShip(CreateRelationshipLink(linkTypeUri, linkDescription, linkDirection, source, target));
            }
        }

        private void AddRelationShip(RelationshipMapModel relationship)
        {
            Map.RelationshipCollection.Add(relationship);
        }

        private void AddEventLink(string linkTypeUri, string linkDescription, LinkDirection linkDirection, ObjectMapModel source,
            ObjectMapModel target)
        {
            ObjectMapModel eventObject = CreateObject(linkTypeUri);
            string relationshipTypeUri = OntologyProvider.GetOntology().GetDefaultRelationshipTypeForEventBasedLink(source.TypeUri, linkTypeUri, target.TypeUri);
            RelationshipMapModel relationship1 = CreateRelationshipLink(relationshipTypeUri, linkDescription, linkDirection, source, eventObject);
            RelationshipMapModel relationship2 = CreateRelationshipLink(relationshipTypeUri, linkDescription, linkDirection, eventObject, target);

            AddObject(eventObject);
            AddRelationShip(relationship1);
            AddRelationShip(relationship2);
        }

        public RelationshipMapModel CreateRelationshipLink(string linkTypeUri, string linkDescription, LinkDirection linkDirection,
            ObjectMapModel source, ObjectMapModel target)
        {
            return new RelationshipMapModel
            {
                TypeUri = linkTypeUri,
                Title = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(linkTypeUri),
                IconPath = OntologyIconProvider.GetTypeIconPath(linkTypeUri).ToString(),
                Description = linkDescription,
                Direction = linkDirection,
                Source = source,
                Target = target,
                OwnerMap = Map
            };
        }

        /// <summary>
        /// افزودن ارتباط جدید به نگاشت از قسمت گراف
        /// </summary>
        public void AddNewLink()
        {
            AddLink(NewMapLink.TypeUri, NewMapLink.Description, NewMapLink.Direction, NewMapLink.Source, NewMapLink.Target);
            NewMapLink = new RelationshipMapModel();
        }

        /// <summary>
        /// تنظیمات اولیه برای ایجاد ارتباط بین دو موجودیت
        /// </summary>
        /// <param name="sourceObject">موجودیت اول</param>
        /// <param name="targetObject">موجودیت دوم</param>
        public void PrepareCreateLink(ObjectMapModel sourceObject, ObjectMapModel targetObject)
        {
            if (sourceObject == null)
                throw new ArgumentNullException(nameof(sourceObject));

            if (targetObject == null)
                throw new ArgumentNullException(nameof(targetObject));

            NewMapLink.Source = sourceObject;
            NewMapLink.Target = targetObject;
        }

        private IEnumerable<RelationshipMapModel> GetRelatedRelationships(ObjectMapModel obj)
        {
            return Map.RelationshipCollection.Where(r => obj.Equals(r.Source) || obj.Equals(r.Target));
        }

        /// <summary>
        /// لینک ها و اشیاء انتخاب شده حذف می گردند. 
        /// </summary>
        public void RemoveSelectedItems(IEnumerable<ObjectMapModel> objects, IEnumerable<RelationshipMapModel> relations)
        {
            if (objects != null)
            {
                foreach (ObjectMapModel objectMapModel in objects)
                {
                    RemoveObjects(objectMapModel);
                }
            }

            if (relations != null)
            {
                foreach (RelationshipMapModel relation in relations)
                {
                    RemoveRelationship(relation);
                }
            }
        }

        private bool RemoveRelationship(RelationshipMapModel relationship)
        {
            return Map.RelationshipCollection.Remove(relationship);
        }

        /// <summary>
        /// حذف تمام نگاشت
        /// </summary>
        public void ClearAllItems()
        {
            if (Map.ObjectCollection.Count != 0)
                Map.ObjectCollection.Clear();

            if (Map.RelationshipCollection.Count != 0)
                Map.RelationshipCollection.Clear();

            if (counterObjectType.Count != 0)
                counterObjectType.Clear();
        }

        public PropertyMapModel CreateProperty(string typeUri, ObjectMapModel ownerObject)
        {
            BaseDataTypes dataType = OntologyProvider.GetBaseDataTypeOfProperty(typeUri);
            string title = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(typeUri);
            string iconPath = OntologyIconProvider.GetPropertyTypeIconPath(typeUri).ToString();

            if (dataType == BaseDataTypes.None)
            {
                throw new Exception("Selected property is invalid.");
            }
            else if (dataType == BaseDataTypes.DateTime)
            {
                return new DateTimePropertyMapModel() {TypeUri = typeUri, DataType = dataType, Title = title, IconPath = iconPath, OwnerObject = ownerObject};
            }
            else if (dataType == BaseDataTypes.GeoPoint)
            {
                return new GeoPointPropertyMapModel() { TypeUri = typeUri, DataType = dataType, Title = title, IconPath = iconPath, OwnerObject = ownerObject };
            }
            else if (dataType == BaseDataTypes.GeoTime)
            {
                return new GeoTimePropertyMapModel() { TypeUri = typeUri, DataType = dataType, Title = title, IconPath = iconPath, OwnerObject = ownerObject };
            }
            else
            {
                return new SinglePropertyMapModel() { TypeUri = typeUri, DataType = dataType, Title = title, IconPath = iconPath, OwnerObject = ownerObject };
            }
        }

        public void AddPropertyToObject(ObjectMapModel obj, PropertyMapModel property)
        {
            obj.Properties.Add(property);
            property.IsSelected = true;

            if (obj.DisplayNameProperty == null)
                property.IsDisplayName = true;
        }

        public void AddPropertyToObject(ObjectMapModel obj, string typeUri)
        {
            AddPropertyToObject(obj, CreateProperty(typeUri, obj));
        }

        public void AddPropertyToSelectedObject(string typeUri)
        {
            AddPropertyToObject(Map.SelectedObject, typeUri);
        }

        public bool RemovePropertyFromObject(ObjectMapModel obj, PropertyMapModel property)
        {
            return obj.Properties.Remove(property);
        }

        public bool RemovePropertyFromSelectedObject(PropertyMapModel property)
        {
            return RemovePropertyFromObject(Map.SelectedObject, property);
        }

        public void ClearAllPropertyFromObject(ObjectMapModel obj)
        {
            var removableProperties = obj.Properties.Where(x => x.Editable).ToList();
            foreach (var property in removableProperties)
            {
                obj.Properties.Remove(property);
            }
        }

        public void ClearAllPropertyFromSelectedObject()
        {
            ClearAllPropertyFromObject(Map.SelectedObject);
        }

        public void SetDisplayNamePropertyForObject(ObjectMapModel obj, PropertyMapModel property)
        {
            property.IsDisplayName = true;
        }

        public void SetInternalResolution(PropertyMapModel property, bool role)
        {
            if (property.IsResolvable)
                property.HasResolution = role;
            else
                property.HasResolution = false;
        }

        public void AddValueToProperty(SinglePropertyMapModel property, DataSourceFieldModel field)
        {
            AddValueToProperty(property, new ValueMapModel { Field = field });
        }

        public void AddValueToSelectedProperty(DataSourceFieldModel field)
        {
            if (Map.SelectedObject.SelectedProperty is SinglePropertyMapModel singleProperty)
            {
                AddValueToProperty(singleProperty as SinglePropertyMapModel, field);
            }
            else if (Map.SelectedObject.SelectedProperty is MultiPropertyMapModel multiProperty)
            {
                var selectedProperty = GetSelectedSinglePropertyFromMultiProperty(multiProperty);
                AddValueToProperty(selectedProperty, field);
            }
        }

        private void AddValueToProperty(SinglePropertyMapModel property, ValueMapModel value)
        {
            value.OwnerProperty = property;
            property.ValueCollection.Add(value);
        }

        public void InsertValueToProperty(SinglePropertyMapModel property, DataSourceFieldModel field, int index)
        {
            InsertValueToProperty(property, new ValueMapModel { Field = field }, index);
        }

        public void InsertValueToProperty(SinglePropertyMapModel property, ValueMapModel value, int index)
        {
            value.OwnerProperty = property;
            if (index < 0 || index > property.ValueCollection.Count)
                index = property.ValueCollection.Count;

            property.ValueCollection.Insert(index, value);
        }

        public bool RemoveValueFromProperty(SinglePropertyMapModel property, ValueMapModel value)
        {
            return property.ValueCollection.Remove(value);
        }

        public bool RemoveValueFromSelectedProperty(ValueMapModel value)
        {
            bool result = false;
            var selectedProperty = GetSelectedSinglePropertyFromMultiProperty(Map.SelectedObject.SelectedProperty);

            if (selectedProperty is SinglePropertyMapModel selectedSingleProperty)
            {
                result = RemoveValueFromProperty(selectedSingleProperty, value);
            }

            return result;
        }

        public SinglePropertyMapModel GetSelectedSinglePropertyFromMultiProperty(PropertyMapModel selectedProperty)
        {
            while (selectedProperty is MultiPropertyMapModel)
            {
                selectedProperty = (selectedProperty as MultiPropertyMapModel).SelectedInnerProperty;
            }
            return (SinglePropertyMapModel)selectedProperty;
        }

        public void ClearAllValueFromProperty(PropertyMapModel property)
        {
            if (property is SinglePropertyMapModel singlePropertyMapModel)
            {
                if (singlePropertyMapModel.ValueCollection != null && singlePropertyMapModel.ValueCollection.Count != 0)
                {
                    singlePropertyMapModel.ValueCollection.Clear();
                }
            }
            else if (property is MultiPropertyMapModel multiPropertyMapModel)
            {
                foreach (PropertyMapModel innerProperty in multiPropertyMapModel.InnerProperties)
                {
                    ClearAllValueFromProperty(innerProperty);
                }
            }
        }

        public void ClearAllValueFromSelectedProperty()
        {
            ClearAllValueFromProperty(Map.SelectedObject.SelectedProperty);
        }

        public void MoveValueInProperty(SinglePropertyMapModel property, ValueMapModel source, ValueMapModel target)
        {
            MoveValueInProperty(property, property.ValueCollection.IndexOf(source), property.ValueCollection.IndexOf(target));
        }

        public void MoveValueInProperty(SinglePropertyMapModel property, int sourceIndex, int targetIndex)
        {
            if (sourceIndex < 0 || sourceIndex >= property.ValueCollection.Count)
                return;

            if (targetIndex < 0 || targetIndex >= property.ValueCollection.Count)
                targetIndex = property.ValueCollection.Count - 1;

            ValueMapModel source = property.ValueCollection[sourceIndex];
            property.ValueCollection.RemoveAt(sourceIndex);
            if (targetIndex > sourceIndex)
                property.ValueCollection.Insert(targetIndex - 1, source);
            else
                property.ValueCollection.Insert(targetIndex, source);
        }

        public void AddRegularExpression(ValueMapModel value, string pattern)
        {
            value.RegularExpressionPattern = pattern;
            value.HasRegularExpression = true;
        }

        public void RemoveRegularExpression(ValueMapModel value)
        {
            value.HasRegularExpression = false;
        }       

        #endregion

        #region Command

        public RelayCommand SaveCommand { get; protected set; }

        private void SaveCommandMethod(object obj)
        {
            if (obj != null)
                SaveMap(obj.ToString());
        }

        public RelayCommand LoadCommand { get; protected set; }

        private void LoadCommandMethod(object obj)
        {
            if (obj != null)
                LoadMap(obj.ToString());
        }

        public RelayCommand AddObjectCommand { get; protected set; }

        private void AddObjectCommandMethod(object obj)
        {
            if (obj != null)
                AddObject(obj.ToString());
        }

        public RelayCommand AddLinkCommand { get; protected set; }

        private void AddLinkCommandMethod(object obj)
        {
            object[] parameters = null;

            if (obj is object[] objects)
                parameters = objects;

            if (!IsValidCreateLinkCommandParameters(parameters))
                return;

            string linkTypeUri = parameters[0].ToString();
            string linkDescription = parameters[1].ToString();
            LinkDirection linkDirection = (LinkDirection)parameters[2];
            ObjectMapModel source = (ObjectMapModel)parameters[3];
            ObjectMapModel target = (ObjectMapModel)parameters[4];

            AddLink(linkTypeUri, linkDescription, linkDirection, source, target);
        }

        private bool IsValidCreateLinkCommandParameters(object[] parameters)
        {
            return parameters != null &&
                parameters.Length == 5 &&
                parameters[0] != null &&
                parameters[1] != null &&
                parameters[2] is LinkDirection &&
                parameters[3] is ObjectMapModel &&
                parameters[4] is ObjectMapModel;
        }

        public RelayCommand RemoveSelectedItemCommand { get; protected set; }

        private void RemoveSelectedItemCommandMethod(object obj)
        {

        }

        public RelayCommand ClearAllCommand { get; protected set; }

        private void ClearAllCommandMethod(object obj)
        {
            ClearAllItems();
        }

        public RelayCommand AddPropertyCommand { get; set; }

        private void AddPropertyCommandMethod(object obj)
        {
            object[] parameters = null;

            if (obj is object[] objects)
                parameters = objects;

            if (parameters.Length >= 2 && parameters[0] is ObjectMapModel && parameters[1] != null)
                AddPropertyToObject((ObjectMapModel)parameters[0], parameters[1].ToString());
        }

        public RelayCommand RemovePropertyCommand { get; set; }

        private void RemovePropertyCommandMethod(object obj)
        {
            object[] parameters = null;

            if (obj is object[])
                parameters = (object[])obj;

            if (parameters.Length >= 2 && parameters[0] is ObjectMapModel && parameters[1] is PropertyMapModel)
                RemovePropertyFromObject((ObjectMapModel)parameters[0], (PropertyMapModel)parameters[1]);
        }

        public RelayCommand ClearAllPropertyCommand { get; set; }

        private void ClearAllPropertyCommandMethod(object obj)
        {
            if (obj is ObjectMapModel)
                ClearAllPropertyFromObject((ObjectMapModel)obj);
        }

        public RelayCommand PropertySetAsDisplayNameCommand { get; set; }

        private void PropertySetAsDisplayNameCommandMethod(object obj)
        {
            object[] parameters = null;

            if (obj is object[])
                parameters = (object[])obj;

            if (parameters.Length >= 2 && parameters[0] is ObjectMapModel && parameters[1] is PropertyMapModel)
                SetDisplayNamePropertyForObject((ObjectMapModel)parameters[0], (PropertyMapModel)parameters[1]);
        }

        public RelayCommand SetInternalResolutionCommand { get; set; }

        private void SetInternalResolutionCommandMethod(object obj)
        {
            object[] parameters = null;

            if (obj is object[])
                parameters = (object[])obj;

            if (parameters.Length >= 2 && parameters[0] is PropertyMapModel && parameters[1] is bool resolutionRole)
                SetInternalResolution((PropertyMapModel)parameters[0], resolutionRole);
        }

        public RelayCommand AddValueCommand { get; set; }

        private void AddValueCommandMethod(object obj)
        {
            object[] parameters = null;

            if (obj is object[])
                parameters = (object[])obj;

            if (parameters.Length >= 2 && parameters[0] is SinglePropertyMapModel && parameters[1] is DataSourceFieldModel)
            {
                AddValueToProperty((SinglePropertyMapModel)parameters[0], (DataSourceFieldModel)parameters[1]);
            }
        }

        public RelayCommand InsertValueCommand { get; set; }

        private void InsertValueCommandMethod(object obj)
        {
            object[] parameters = null;

            if (obj is object[])
                parameters = (object[])obj;

            if (parameters.Length >= 3 && parameters[0] is SinglePropertyMapModel &&
                parameters[1] is DataSourceFieldModel &&
                int.TryParse(parameters[2].ToString(), out int index))
            {
                InsertValueToProperty((SinglePropertyMapModel)parameters[0], (DataSourceFieldModel)parameters[1], index);
            }
        }

        public RelayCommand RemoveValueCommand { get; set; }

        private void RemoveValueCommandMethod(object obj)
        {
            object[] parameters = null;

            if (obj is object[])
                parameters = (object[])obj;

            if (parameters.Length >= 2 && parameters[0] is SinglePropertyMapModel && parameters[1] is ValueMapModel)
                RemoveValueFromProperty((SinglePropertyMapModel)parameters[0], (ValueMapModel)parameters[1]);
        }

        public RelayCommand ClearAllValueCommand { get; set; }

        private void ClearAllValueCommandMethod(object obj)
        {
            if (obj is PropertyMapModel)
                ClearAllValueFromProperty((PropertyMapModel)obj);
        }

        public RelayCommand MoveValueCommand { get; set; }

        private void MoveValueCommandMethod(object obj)
        {
            object[] parameters = null;

            if (obj is object[])
                parameters = (object[])obj;

            if (parameters.Length >= 3 && parameters[0] is SinglePropertyMapModel &&
                int.TryParse(parameters[1].ToString(), out int source) &&
                int.TryParse(parameters[2].ToString(), out int target))
            {
                MoveValueInProperty((SinglePropertyMapModel)parameters[0], source, target);
            }
        }

        public RelayCommand AddRegularExpressionCommand { get; set; }

        private void AddRegularExpressionCommandMethod(object obj)
        {
            object[] parameters = null;

            if (obj is object[])
                parameters = (object[])obj;

            if (parameters.Length >= 2 && parameters[0] is ValueMapModel && parameters[1] != null)
                AddRegularExpression((ValueMapModel)parameters[0], parameters[1].ToString());
        }

        public RelayCommand RemoveRegularExpressionCommand { get; set; }

        private void RemoveRegularExpressionCommandMethod(object obj)
        {
            if (obj is ValueMapModel)
                RemoveRegularExpression((ValueMapModel)obj);
        }

        #endregion
    }
}
