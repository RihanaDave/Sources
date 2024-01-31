using GPAS.Dispatch.Entities.Concepts.Geo;
using GPAS.FilterSearch;
using GPAS.Ontology;
using GPAS.PropertiesValidation.Geo;
using GPAS.SearchAround;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Controls.CustomSearchAround.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Threading;

namespace GPAS.Workspace.Presentation.Applications.ApplicationViewModel.Graph
{
    public class CustomSearchAroundViewModel : BaseViewModel
    {
        #region Properties

        Ontology.Ontology ontology = null;

        CustomSearchAroundModel customSearchAroundModel = null;
        public CustomSearchAroundModel CustomSearchAroundModel
        {
            get => customSearchAroundModel;
            set
            {
                if (SetValue(ref customSearchAroundModel, value))
                {
                    if (CustomSearchAroundModel != null)
                    {
                        CustomSearchAroundModel.ScenarioChanged -= CustomSearchAroundModel_ScenarioChanged;
                        CustomSearchAroundModel.ScenarioChanged += CustomSearchAroundModel_ScenarioChanged;
                    }

                    OnCustomSearchAroundModelChanged();
                    PrepareDefections();
                }
            }
        }

        ObservableCollection<KWObject> sourceObjectCollection = null;
        public ObservableCollection<KWObject> SourceObjectCollection
        {
            get => sourceObjectCollection;
            set
            {
                ObservableCollection<KWObject> oldValue = SourceObjectCollection;
                if (SetValue(ref sourceObjectCollection, value))
                {
                    if (oldValue != null)
                    {
                        oldValue.CollectionChanged -= SourceObjectCollection_CollectionChanged;
                    }
                    if (SourceObjectCollection == null)
                    {
                        SourceObjectCollection_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        SourceObjectCollection.CollectionChanged -= SourceObjectCollection_CollectionChanged;
                        SourceObjectCollection.CollectionChanged += SourceObjectCollection_CollectionChanged;

                        if (oldValue == null)
                        {
                            SourceObjectCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, SourceObjectCollection));
                        }
                        else
                        {
                            SourceObjectCollection_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, SourceObjectCollection, oldValue));
                        }
                    }
                }
            }
        }

        ObservableCollection<WarningModel> defections = null;
        public ObservableCollection<WarningModel> Defections
        {
            get => defections;
            set
            {
                ObservableCollection<WarningModel> oldValue = Defections;
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
                            Defections_CollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Defections));
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

        WarningModel selectedDefection = null;
        public WarningModel SelectedDefection
        {
            get => selectedDefection;
            set
            {
                PreSelectedDefectionChanged(value);

                SetValue(ref selectedDefection, value);
            }
        }

        RecentLoadedFilesManager recentLoadedFilesManager = null;
        public RecentLoadedFilesManager RecentLoadedFilesManager
        {
            get => recentLoadedFilesManager;
            set => SetValue(ref recentLoadedFilesManager, value);
        }

        #endregion

        #region Methods

        public CustomSearchAroundViewModel()
        {
            ontology = OntologyProvider.GetOntology();
            Defections = new ObservableCollection<WarningModel>();
            CustomSearchAroundModel = new CustomSearchAroundModel();
            SourceObjectCollection = new ObservableCollection<KWObject>();
            RecentLoadedFilesManager = new RecentLoadedFilesManager();
            RecentLoadedFilesManager.DestinationSerializedFile =
                string.Format(Properties.Settings.Default.CustomSearchAroundRecentLoadedFilePath,
                AppDomain.CurrentDomain.BaseDirectory);
        }

        private void SourceObjectCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnSourceObjectCollectionChanged(e);
        }

        private void CustomSearchAroundModel_ScenarioChanged(object sender, EventArgs e)
        {
            PrepareDefections();
        }

        private void PrepareDefections()
        {
            IEnumerable<WarningModel> objectDefections = customSearchAroundModel.ObjectCollection.SelectMany(o => o.Defections);
            IEnumerable<WarningModel> linkDefections = customSearchAroundModel.ObjectCollection.Where(o => !o.IsEvent).SelectMany(o => o.RelatedLink.Defections);
            IEnumerable<WarningModel> propertyDefections = customSearchAroundModel.ObjectCollection.SelectMany(o => o.Properties).SelectMany(p => p.Defections);

            Defections = new ObservableCollection<WarningModel>(
                CustomSearchAroundModel.Defections.Concat(objectDefections.Concat(linkDefections.Concat(propertyDefections))));
        }

        private void PreSelectedDefectionChanged(WarningModel selectedDefection)
        {
            foreach (WarningModel defection in Defections)
            {
                if (!defection.Equals(selectedDefection))
                    defection.IsSelected = false;
            }
        }

        private void Defections_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems?.Count > 0)
            {
                foreach (WarningModel defection in e.OldItems)
                {
                    defection.Selected -= Defection_Selected;
                    defection.Deselected -= Defection_Deselected;
                }
            }

            if (e.NewItems?.Count > 0)
            {
                foreach (WarningModel defection in e.NewItems)
                {
                    defection.Selected -= Defection_Selected;
                    defection.Selected += Defection_Selected;

                    defection.Deselected -= Defection_Deselected;
                    defection.Deselected += Defection_Deselected;
                }
            }
        }

        private void Defection_Deselected(object sender, EventArgs e)
        {
            SelectedDefection = Defections?.FirstOrDefault(d => d.IsSelected);
        }

        private void Defection_Selected(object sender, EventArgs e)
        {
            SelectedDefection = (WarningModel)sender;
        }

        public void RemoveItemFromRecentLoadedList(LoadedFileModel loadedFile)
        {
            RecentLoadedFilesManager?.AllItems?.Remove(loadedFile);
        }

        public void AddObjectAndLink(string objectTypeUri, string linkTypeUri)
        {
            if (CustomSearchAroundModel.ObjectCollection == null)
                CustomSearchAroundModel.ObjectCollection = new ObservableCollection<CSAObject>();

            CSAObject newObject = null;

            if (ontology.IsEvent(linkTypeUri))
            {
                newObject = CreateObjectWithEventBaseLink(objectTypeUri, linkTypeUri);

                Application.Current.MainWindow.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                    new ParameterizedThreadStart(AddItem), newObject);

                Application.Current.MainWindow.Dispatcher.BeginInvoke(DispatcherPriority.Background,
                    new ParameterizedThreadStart(AddItem), ((CSAEventBaseLink)newObject.RelatedLink).EventObject);
            }
            else
            {
                newObject = CreateObjectWithLink(objectTypeUri, linkTypeUri);
                CustomSearchAroundModel.ObjectCollection.Add(newObject);
            }

            CustomSearchAroundModel.LinkCollection.Add(newObject.RelatedLink);
        }

        private void AddItem(object item)
        {
            CustomSearchAroundModel.ObjectCollection.Add(item as CSAObject);
        }

        private CSAObject CreateObjectWithLink(string objectTypeUri, string linkTypeUri)
        {
            return new CSAObject()
            {
                TypeUri = objectTypeUri,
                Title = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(objectTypeUri),
                IconPath = OntologyIconProvider.GetTypeIconPath(objectTypeUri).ToString(),
                RelatedLink = new CSALink()
                {
                    TypeUri = linkTypeUri,
                    Title = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(linkTypeUri),
                    IconPath = OntologyIconProvider.GetTypeIconPath(linkTypeUri).ToString(),
                }
            };
        }

        private CSAObject CreateObjectWithEventBaseLink(string objectTypeUri, string linkTypeUri)
        {
            CSAObject newObject = new CSAObject()
            {
                TypeUri = objectTypeUri,
                Title = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(objectTypeUri),
                IconPath = OntologyIconProvider.GetTypeIconPath(objectTypeUri).ToString(),
                RelatedLink = new CSAEventBaseLink()
                {
                    TypeUri = linkTypeUri,
                    Title = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(linkTypeUri),
                    IconPath = OntologyIconProvider.GetTypeIconPath(linkTypeUri).ToString(),
                    EventObject = new CSAObject()
                    {
                        TypeUri = linkTypeUri,
                        Title = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(linkTypeUri),
                        IconPath = OntologyIconProvider.GetTypeIconPath(linkTypeUri).ToString(),
                        IsEvent = true
                    }
                }
            };

            (newObject.RelatedLink as CSAEventBaseLink).EventObject.RelatedLink = newObject.RelatedLink;

            return newObject;
        }

        public void RemoveObject(CSAObject cSAObject)
        {
            if (cSAObject.RelatedLink is CSAEventBaseLink eventBaseLink)
            {
                CustomSearchAroundModel.ObjectCollection?.Remove(eventBaseLink.RelatedObject);
                CustomSearchAroundModel.ObjectCollection?.Remove(eventBaseLink.EventObject);
            }
            else
            {
                CustomSearchAroundModel.ObjectCollection?.Remove(cSAObject);
            }    

            if (CustomSearchAroundModel.LinkCollection?.Contains(cSAObject.RelatedLink) == true)
            {
                CustomSearchAroundModel.LinkCollection?.Remove(cSAObject.RelatedLink);
            }
        }

        public void RemoveLink(CSALink cSALink)
        {
            CSAObject cSAObject = CustomSearchAroundModel.ObjectCollection?.FirstOrDefault(o => o.RelatedLink == cSALink);

            if (cSAObject != null)
                RemoveObject(cSAObject);
        }

        public void ClearAll()
        {
            CustomSearchAroundModel.ObjectCollection?.Clear();
            CustomSearchAroundModel.LinkCollection?.Clear();
        }

        public void AddPropertyToObject(string propertyTypeUri, CSAObject cSAObject)
        {
            cSAObject.Properties.Add(CreateProperty(propertyTypeUri));
        }

        private CSAProperty CreateProperty(string propertyTypeUri)
        {
            return new CSAProperty()
            {
                TypeUri = propertyTypeUri,
                Title = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(propertyTypeUri),
                IconPath = OntologyIconProvider.GetTypeIconPath(propertyTypeUri).ToString(),
                RelationalOperator = RelationalOperator.Equals,
                DataType = OntologyProvider.GetBaseDataTypeOfProperty(propertyTypeUri),
            };
        }

        public void RemovePropertyFromObject(CSAProperty cSAProperty, CSAObject cSAObject)
        {
            cSAObject?.Properties?.Remove(cSAProperty);
        }

        public void RemoveAllPropertyFromObject(CSAObject cSAObject)
        {
            cSAObject?.Properties?.Clear();
        }

        public KWCustomSearchAroundResult Search()
        {
            if (!CustomSearchAroundModel.IsValid || SourceObjectCollection == null || SourceObjectCollection.Count == 0)
                return null;

            CustomSearchAroundCriteria customSearchAroundCriteria = new CustomSearchAroundCriteria()
            {
                SourceSetObjectTypes = SourceObjectCollection.Select(o => o.TypeURI).ToArray(),
                LinksFromSearchSet = GetSearchAroundSteps(),
            };

            KWCustomSearchAroundResult customSearchAroundResult =
                Logic.Search.SearchAround.PerformCustomSearchAround(SourceObjectCollection.ToArray(), customSearchAroundCriteria).Result;

            return customSearchAroundResult;
        }

        private SearchAroundStep[] GetSearchAroundSteps()
        {
            List<SearchAroundStep> searchAroundSteps = new List<SearchAroundStep>();

            if (customSearchAroundModel?.ObjectCollection != null)
            {
                foreach (CSAObject cSAObject in CustomSearchAroundModel.ObjectCollection.Where(o => !o.IsEvent))
                {
                    searchAroundSteps.Add(ConvertCSAObjectToSearchAroundStep(cSAObject));
                }
            }

            return searchAroundSteps.ToArray();
        }

        private SearchAroundStep ConvertCSAObjectToSearchAroundStep(CSAObject cSAObject)
        {
            SearchAroundStep searchAroundStep = new SearchAroundStep();

            if (ontology.IsEvent(cSAObject.RelatedLink.TypeUri))
            {
                searchAroundStep.LinkTypeUri = ontology.GetAllChilds(cSAObject.RelatedLink.TypeUri).ToArray();
                searchAroundStep.IsEvent = true;

                List<PropertyValueCriteria> eventPropertyValueCriterias = new List<PropertyValueCriteria>();
                foreach (CSAProperty cSAProperty in ((CSAEventBaseLink)cSAObject.RelatedLink).EventObject.Properties)
                {
                    eventPropertyValueCriterias.Add(ConvertCSAPropertyToPropertyValueCriteria(cSAProperty));
                }

                searchAroundStep.EventObjectPropertyCriterias = eventPropertyValueCriterias.ToArray();
            }
            else
            {
                searchAroundStep.LinkTypeUri = ontology.GetAllRelationshipChilds(cSAObject.RelatedLink.TypeUri).ToArray();
            }

            searchAroundStep.TargetObjectTypeUri = ontology.GetAllChilds(cSAObject.TypeUri).ToArray();

            List<PropertyValueCriteria> propertyValueCriterias = new List<PropertyValueCriteria>();
            foreach (CSAProperty cSAProperty in cSAObject.Properties)
            {
                propertyValueCriterias.Add(ConvertCSAPropertyToPropertyValueCriteria(cSAProperty));
            }

            searchAroundStep.TargetObjectPropertyCriterias = propertyValueCriterias.ToArray();

            return searchAroundStep;
        }

        private PropertyValueCriteria ConvertCSAPropertyToPropertyValueCriteria(CSAProperty cSAProperty)
        {
            PropertyValueCriteria propertyValueCriteria = new PropertyValueCriteria();
            propertyValueCriteria.PropertyTypeUri = cSAProperty.TypeUri;

            switch (cSAProperty.DataType)
            {
                case BaseDataTypes.String:
                case BaseDataTypes.HdfsURI:
                    propertyValueCriteria.OperatorValuePair = new StringPropertyCriteriaOperatorValuePair()
                    {
                        CriteriaValue = cSAProperty.CriteriaValue,
                        CriteriaOperator = (StringPropertyCriteriaOperatorValuePair.RelationalOperator)(int)cSAProperty.RelationalOperator
                    };
                    break;
                case BaseDataTypes.Double:
                    propertyValueCriteria.OperatorValuePair = new FloatPropertyCriteriaOperatorValuePair()
                    {
                        CriteriaValue = float.Parse(cSAProperty.CriteriaValue),
                        CriteriaOperator = (FloatPropertyCriteriaOperatorValuePair.RelationalOperator)(int)cSAProperty.RelationalOperator
                    };
                    break;
                case BaseDataTypes.Int:
                case BaseDataTypes.Long:
                    propertyValueCriteria.OperatorValuePair = new LongPropertyCriteriaOperatorValuePair()
                    {
                        CriteriaValue = long.Parse(cSAProperty.CriteriaValue),
                        CriteriaOperator = (LongPropertyCriteriaOperatorValuePair.RelationalOperator)(int)cSAProperty.RelationalOperator
                    };
                    break;
                case BaseDataTypes.DateTime:
                    propertyValueCriteria.OperatorValuePair = new DateTimePropertyCriteriaOperatorValuePair()
                    {
                        CriteriaValue = DateTime.Parse(cSAProperty.CriteriaValue),
                        CriteriaOperator = (DateTimePropertyCriteriaOperatorValuePair.RelationalOperator)(int)cSAProperty.RelationalOperator
                    };
                    break;
                case BaseDataTypes.Boolean:
                    propertyValueCriteria.OperatorValuePair = new BooleanPropertyCriteriaOperatorValuePair()
                    {
                        CriteriaValue = bool.Parse(cSAProperty.CriteriaValue),
                        CriteriaOperator = (BooleanPropertyCriteriaOperatorValuePair.RelationalOperator)(int)cSAProperty.RelationalOperator
                    };
                    break;
                case BaseDataTypes.GeoPoint:
                    propertyValueCriteria.OperatorValuePair = new GeoPointPropertyCriteriaOperatorValuePair()
                    {
                        CriteriaValue = GeoCircle.GeoCircleEntityRawData(cSAProperty.CriteriaValue),
                        CriteriaOperator = (GeoPointPropertyCriteriaOperatorValuePair.RelationalOperator)(int)cSAProperty.RelationalOperator
                    };
                    break;
                case BaseDataTypes.None:
                case BaseDataTypes.GeoTime:
                default:
                    break;
            }

            return propertyValueCriteria;
        }

        public void Save(string filePath)
        {
            Utility.Utility.SerializeToFile(filePath, CustomSearchAroundModel);
            RecentLoadedFilesManager?.SafeAddItem(filePath, DateTime.Now);
        }

        public LoadedResult Load(string filePath)
        {
            LoadedResult loadedResult = new LoadedResult();
            CustomSearchAroundModel loaded = Utility.Utility.DeSerializeFromFile<CustomSearchAroundModel>(filePath);

            if (IsMatchCSAModelWithSourceObjectCollection(loaded))
            {
                foreach (var link in loaded.LinkCollection)
                {
                    link.RelatedObject.RelatedLink = link;

                    if (link is CSAEventBaseLink eventLink)
                    {
                        eventLink.EventObject.RelatedLink = eventLink;
                        loaded.ObjectCollection.Add(eventLink.EventObject);
                    }

                    loaded.ObjectCollection.Add(link.RelatedObject);
                }

                CustomSearchAroundModel = loaded;
                loadedResult.Success = true;
                RecentLoadedFilesManager?.SafeAddItem(filePath, DateTime.Now);
            }
            else
            {
                loadedResult.Messages = GenerateNoMatchMessages(loaded);
                loadedResult.Success = false;
            }

            return loadedResult;
        }

        private List<string> GenerateNoMatchMessages(CustomSearchAroundModel loaded)
        {
            List<string> messages = new List<string>();
            List<string> objectsUri = SourceObjectCollection.Select(so => so.TypeURI).ToList();
            OntologyNode relationshipNode = ontology.GetAllValidRelationshipsHierarchyForDomainSet(objectsUri);
            OntologyNode eventNode = ontology.GetOntologyEventsHierarchy();
            List<CSALink> loadedLinks = loaded.LinkCollection.ToList();

            foreach (CSALink cSALink in loadedLinks.Where(l => !ontology.IsEvent(l.TypeUri)))
            {
                if (relationshipNode.Find(cSALink.TypeUri) == null && eventNode.Find(cSALink.TypeUri) == null)
                    messages.Add($"It is not possible to use link `{cSALink.Title}` for the selected set of objects.");

                List<OntologyNode> permittedDestinationObjects = GetPermittedObjectsForLink(cSALink, objectsUri);
                if (permittedDestinationObjects.FirstOrDefault(pdo => pdo.TypeUri == cSALink.RelatedObject.TypeUri) == null)
                    messages.Add($"It is not possible to establish a type `{cSALink.Title}` link between the selected set of objects and a type `{cSALink.RelatedObject.Title}` object.");

                if (cSALink.RelatedObject.Properties?.Count > 0)
                {
                    List<PropertyNode> permittedPropertis = GetPermittedPropertisForObject(cSALink.RelatedObject);
                    foreach (CSAProperty cSAProperty in cSALink.RelatedObject.Properties)
                    {
                        PropertyNode findedProperty = permittedPropertis.FirstOrDefault(pp => pp.TypeUri == cSAProperty.TypeUri);
                        if (findedProperty == null)
                        {
                            messages.Add($"Property `{cSAProperty.Title}` does not exist for object type `{cSALink.RelatedObject.Title}`.");
                        }
                        else
                        {
                            if (findedProperty.BaseDataType != cSAProperty.DataType)
                                messages.Add($"The data type of the `{cSAProperty.Title}` loaded property does not match the data type of the same property in the ontology.");
                        }
                    }
                }
            }

            return messages;
        }

        private bool IsMatchCSAModelWithSourceObjectCollection(CustomSearchAroundModel loaded)
        {
            List<string> objectsUri = SourceObjectCollection.Select(so => so.TypeURI).ToList();
            OntologyNode relationshipNode = ontology.GetAllValidRelationshipsHierarchyForDomainSet(objectsUri);
            OntologyNode eventNode = ontology.GetOntologyEventsHierarchy();
            List<CSALink> loadedLinks = loaded.LinkCollection.ToList();

            foreach (CSALink cSALink in loadedLinks)
            {
                if (relationshipNode.Find(cSALink.TypeUri) == null && eventNode.Find(cSALink.TypeUri) == null)
                    return false;

                if (ontology.IsRelationship(cSALink.TypeUri))
                {
                    List<OntologyNode> permittedDestinationObjects = GetPermittedObjectsForLink(cSALink, objectsUri);
                    if (permittedDestinationObjects.FirstOrDefault(pdo => pdo.TypeUri == cSALink.RelatedObject.TypeUri) == null)
                        return false;
                }

                if (cSALink.RelatedObject.Properties?.Count > 0)
                {
                    List<PropertyNode> permittedPropertis = GetPermittedPropertisForObject(cSALink.RelatedObject);
                    if (!cSALink.RelatedObject.Properties.All(p => IsMatchCSAPropertyWithPermittedProperties(p, permittedPropertis)))
                        return false;
                }
            }

            return true;
        }

        public static List<OntologyNode> GetPermittedObjectsForLink(CSALink cSALink, List<string> domainNames)
        {
            return GetPermittedObjectsForLink(cSALink?.TypeUri, domainNames);
        }

        public static List<OntologyNode> GetPermittedObjectsForLink(string linkTypeUri, List<string> domainNames)
        {
            List<OntologyNode> permittedDestinationObjects = new List<OntologyNode>();
            foreach (OntologyNode nodeObject in OntologyProvider.GetOntology().GetEntitiesTreeForASpecificDomainAndLink(domainNames, linkTypeUri))
            {
                permittedDestinationObjects.Add(nodeObject);
                permittedDestinationObjects.AddRange(nodeObject.GetAllChildren());
            }

            return permittedDestinationObjects;
        }

        private List<PropertyNode> GetPermittedPropertisForObject(CSAObject cSAObject)
        {
            List<PropertyNode> permittedPropertis = new List<PropertyNode>();
            foreach (PropertyNode propertyNode in ontology.GetHierarchyPropertiesOfObjects(new List<string>() { cSAObject.TypeUri }))
            {
                if (PropertyNodeIsSelectable(propertyNode))
                    permittedPropertis.Add(propertyNode);

                permittedPropertis.AddRange(propertyNode.GetAllChildren().OfType<PropertyNode>().
                    Where(p => PropertyNodeIsSelectable(p)));
            }

            return permittedPropertis;
        }

        private bool IsMatchCSAPropertyWithPermittedProperties(CSAProperty cSAProperty, List<PropertyNode> permittedPropertis)
        {
            return permittedPropertis.FirstOrDefault(pp => pp.TypeUri == cSAProperty.TypeUri && pp.BaseDataType == cSAProperty.DataType) != null;
        }

        private bool PropertyNodeIsSelectable(PropertyNode node)
        {
            return node.IsLeaf &&
                node.BaseDataType != BaseDataTypes.GeoTime &&
                node.BaseDataType != BaseDataTypes.None;
        }

        #endregion

        #region Events

        public event EventHandler CustomSearchAroundModelChanged;
        protected void OnCustomSearchAroundModelChanged()
        {
            CustomSearchAroundModelChanged?.Invoke(this, EventArgs.Empty);
        }

        public event NotifyCollectionChangedEventHandler SourceObjectCollectionChanged;
        protected void OnSourceObjectCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            SourceObjectCollectionChanged?.Invoke(this, e);
        }

        #endregion
    }
}
