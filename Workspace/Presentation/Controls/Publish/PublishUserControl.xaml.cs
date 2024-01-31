using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Logic.Publish;
using GPAS.Workspace.Presentation.Windows;
using GPAS.Workspace.ViewModel.Publish;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using UnpublishedObject = GPAS.Workspace.Presentation.Windows.UnpublishedObject;

namespace GPAS.Workspace.Presentation.Controls.Publish
{
    /// <summary>
    /// Interaction logic for PublishUserControl.xaml
    /// </summary>
    public partial class PublishUserControl : INotifyPropertyChanged
    {
        #region متغیرهای سراسری
        internal Dictionary<KWRelationship, UnpublishedComponentValue> kwrelationToComponentValueDictionary =
            new Dictionary<KWRelationship, UnpublishedComponentValue>();

        private int changedEntitiesCount;

        public int ChangedEntitiesCount
        {
            get { return changedEntitiesCount; }
            set
            {
                if (changedEntitiesCount != value)
                {
                    changedEntitiesCount = value;
                    NotifyPropertyChanged("ChangedEntitiesCount");
                }
            }
        }

        private int changedEventsCount;
        public int ChangedEventsCount
        {
            get { return this.changedEventsCount; }
            set
            {
                if (this.changedEventsCount != value)
                {
                    this.changedEventsCount = value;
                    this.NotifyPropertyChanged("ChangedEventsCount");
                }
            }
        }

        private int changedMediasCount;
        public int ChangedMediasCount
        {
            get { return this.changedMediasCount; }
            set
            {
                if (this.changedMediasCount != value)
                {
                    this.changedMediasCount = value;
                    this.NotifyPropertyChanged("ChangedMediasCount");
                }
            }
        }

        private int addedEntitiesCount;
        public int AddedEntitiesCount
        {
            get { return this.addedEntitiesCount; }
            set
            {
                if (this.addedEntitiesCount != value)
                {
                    this.addedEntitiesCount = value;
                    this.NotifyPropertyChanged("AddedEntitiesCount");
                }
            }
        }

        private int addedEventsCount;
        public int AddedEventsCount
        {
            get { return this.addedEventsCount; }
            set
            {
                if (this.addedEventsCount != value)
                {
                    this.addedEventsCount = value;
                    this.NotifyPropertyChanged("AddedEventsCount");
                }
            }
        }

        private int addedMediasCount;
        public int AddedMediasCount
        {
            get { return this.addedMediasCount; }
            set
            {
                if (this.addedMediasCount != value)
                {
                    this.addedMediasCount = value;
                    this.NotifyPropertyChanged("AddedMediasCount");
                }
            }
        }

        public ObservableCollection<UnpublishConcept> AddedUnpublishConcept { get; set; }
        public ObservableCollection<UnpublishConcept> ChangedUnpublishConcept { get; set; }

        #endregion

        #region توابع

        public PublishUserControl()
        {
            InitializeComponent();
            DataContext = this;
            AddedUnpublishConcept = new ObservableCollection<UnpublishConcept>();
            ChangedUnpublishConcept = new ObservableCollection<UnpublishConcept>();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Init();
        }

        public async void Init()
        {
            try
            {
                OnBeginOfAccessLevelChecking();
                await ChangeStateOfUpperWriteAccessNotification();
            }
            finally
            {
                OnEndOfAccessLevelChecking();
            }
        }

        public void ShowUnpublishedObjects(Tuple<List<ViewModel.Publish.UnpublishedObject>,
            List<ViewModel.Publish.UnpublishedObject>, List<ViewModel.Publish.UnpublishedObject>> seperatedUnpublishedObjects)
        {
            if (seperatedUnpublishedObjects == null)
            {
                throw new ArgumentNullException("seperatedUnpublishedObjects");
            }

            AddedUnpublishConcept.Clear();
            ChangedUnpublishConcept.Clear();

            List<ViewModel.Publish.UnpublishedObject> changedUnpublishedObject = seperatedUnpublishedObjects.Item1;
            List<ViewModel.Publish.UnpublishedObject> addedUnpublishedObject = seperatedUnpublishedObjects.Item2;
            List<ViewModel.Publish.UnpublishedObject> deletedUnpublishedObject = seperatedUnpublishedObjects.Item3;

            AddedUnpublishConcept = GenerateUnpublishConcept(addedUnpublishedObject, UnpublishConceptChangeType.Added);
            AddedConceptsTreeview.ItemsSource = AddedUnpublishConcept;

            ChangedUnpublishConcept = GenerateUnpublishConcept(changedUnpublishedObject, UnpublishConceptChangeType.Changed);
            ChangedConceptsTreeview.ItemsSource = ChangedUnpublishConcept;
        }

        public async Task<bool> Publish()
        {
            UnpublishedConcepts unpublishedConcepts = GetSelectedUnpublishedConcepts();
            if (unpublishedConcepts.unpublishedObjectChanges.Count > 0 ||
                unpublishedConcepts.unpublishedPropertyChanges.Count > 0 ||
                unpublishedConcepts.unpublishedRelationshipChanges.Count > 0 ||
                unpublishedConcepts.unpublishedMediaChanges.Count > 0)
            {

                PublishResultMetadata publishResult = await PublishManager.PublishSpecifiedManuallyEnteredConcepts
                    (unpublishedConcepts.unpublishedObjectChanges,
                    unpublishedConcepts.unpublishedPropertyChanges,
                    unpublishedConcepts.unpublishedMediaChanges,
                    unpublishedConcepts.unpublishedRelationshipChanges);

                KWMessageBox.Show(publishResult.GetMessage(), MessageBoxButton.OK, MessageBoxImage.Information);

                return true;
            }
            else
            {
                KWMessageBox.Show(string.Format("{0}",
                        Properties.Resources.There_Is_No_Items_To_Publish_),
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                return false;
            }
        }

        private ObservableCollection<UnpublishConcept> GenerateUnpublishConcept(List<ViewModel.Publish.UnpublishedObject> unpublishedObjects, UnpublishConceptChangeType changeType)
        {
            if (unpublishedObjects == null)
            {
                throw new ArgumentNullException("unpublishedObjects");
            }
            ObservableCollection<UnpublishedObject> unpublishedObjectsToShow = GenerateUnpublishedObject(unpublishedObjects);
            ObservableCollection<UnpublishConcept> unpublishConcepts = GenerateUnpublishConcepts(unpublishedObjectsToShow, changeType);
            return unpublishConcepts;
        }

        private ObservableCollection<UnpublishConcept> GenerateUnpublishConcepts(ObservableCollection<UnpublishedObject> unpublishedObjects, UnpublishConceptChangeType changeType)
        {
            if (unpublishedObjects == null)
            {
                throw new ArgumentNullException("unpublishedObjects");
            }

            ObservableCollection<UnpublishConcept> result = new ObservableCollection<UnpublishConcept>();
            UnpublishConcept EntityUnpublishedObjects = new UnpublishConcept()
            {
                IsExpanded = true,
                ConceptType = ConceptType.Entities,
                UnpublishedObjects = SeparateEntityObjects(unpublishedObjects)
            };

            if (EntityUnpublishedObjects.UnpublishedObjects.Count > 0)
            {
                result.Add(EntityUnpublishedObjects);
            }

            if (changeType == UnpublishConceptChangeType.Added)
            {
                AddedEntitiesCount = EntityUnpublishedObjects.UnpublishedObjects.Count;
            }
            else if (changeType == UnpublishConceptChangeType.Changed)
            {
                ChangedEntitiesCount = EntityUnpublishedObjects.UnpublishedObjects.Count;
            }


            UnpublishConcept EventUnpublishedObjects = new UnpublishConcept()
            {
                IsExpanded = true,
                ConceptType = ConceptType.Events,
                UnpublishedObjects = SeparateEventObjects(unpublishedObjects)
            };

            if (EventUnpublishedObjects.UnpublishedObjects.Count > 0)
            {
                result.Add(EventUnpublishedObjects);
            }

            if (changeType == UnpublishConceptChangeType.Added)
            {
                AddedEventsCount = EventUnpublishedObjects.UnpublishedObjects.Count;
            }
            else if (changeType == UnpublishConceptChangeType.Changed)
            {
                ChangedEventsCount = EventUnpublishedObjects.UnpublishedObjects.Count;
            }

            UnpublishConcept DocumentUnpublishedObjects = new UnpublishConcept()
            {
                IsExpanded = true,
                ConceptType = ConceptType.Documents,
                UnpublishedObjects = SeparateDocumentObjects(unpublishedObjects)
            };
            if (DocumentUnpublishedObjects.UnpublishedObjects.Count > 0)
            {
                result.Add(DocumentUnpublishedObjects);
            }

            if (changeType == UnpublishConceptChangeType.Added)
            {
                AddedMediasCount = DocumentUnpublishedObjects.UnpublishedObjects.Count;
            }
            else if (changeType == UnpublishConceptChangeType.Changed)
            {
                ChangedMediasCount = DocumentUnpublishedObjects.UnpublishedObjects.Count;
            }
            return result;
        }

        private ObservableCollection<UnpublishedObject> SeparateDocumentObjects(ObservableCollection<UnpublishedObject> unpublishedObjects)
        {
            if (unpublishedObjects == null)
            {
                throw new ArgumentNullException("unpublishedObjects");
            }

            ObservableCollection<UnpublishedObject> result = new ObservableCollection<UnpublishedObject>();
            foreach (var currentunpublishedObject in unpublishedObjects)
            {
                if (OntologyProvider.GetOntology().IsDocument(currentunpublishedObject.UnpublishedObjectTypeURI))
                {
                    result.Add(currentunpublishedObject);
                }
            }
            return result;
        }

        private ObservableCollection<UnpublishedObject> SeparateEventObjects(ObservableCollection<UnpublishedObject> unpublishedObjects)
        {
            if (unpublishedObjects == null)
            {
                throw new ArgumentNullException("unpublishedObjects");
            }

            ObservableCollection<UnpublishedObject> result = new ObservableCollection<UnpublishedObject>();
            foreach (var currentunpublishedObject in unpublishedObjects)
            {
                if (OntologyProvider.GetOntology().IsEvent(currentunpublishedObject.UnpublishedObjectTypeURI))
                {
                    result.Add(currentunpublishedObject);
                }
            }
            return result;
        }

        private ObservableCollection<UnpublishedObject> SeparateEntityObjects(ObservableCollection<UnpublishedObject> unpublishedObjects)
        {
            if (unpublishedObjects == null)
            {
                throw new ArgumentNullException("unpublishedObjects");
            }

            ObservableCollection<UnpublishedObject> result = new ObservableCollection<UnpublishedObject>();
            foreach (var currentunpublishedObject in unpublishedObjects)
            {
                if (OntologyProvider.GetOntology().IsEntity(currentunpublishedObject.UnpublishedObjectTypeURI))
                {
                    result.Add(currentunpublishedObject);
                }
            }
            return result;
        }

        private ObservableCollection<UnpublishedObject> GenerateUnpublishedObject(List<ViewModel.Publish.UnpublishedObject> unpublishedObjects)
        {
            if (unpublishedObjects == null)
            {
                throw new ArgumentNullException("unpublishedObjects");
            }

            ObservableCollection<UnpublishedObject> result = new ObservableCollection<UnpublishedObject>();

            foreach (var currentUnpublishedObject in unpublishedObjects)
            {
                UnpublishedObject unpublishedObject = new UnpublishedObject();
                if (currentUnpublishedObject.ChangeType == UnpublishConceptChangeType.Added)
                {
                    unpublishedObject.ChangeType = ChangeType.Added;
                }
                else if (currentUnpublishedObject.ChangeType == UnpublishConceptChangeType.Changed)
                {
                    unpublishedObject.ChangeType = ChangeType.Changed;
                }
                unpublishedObject.IsUnpublishedObjectSelected = true;
                unpublishedObject.UnpublishedObjectDisplayName = currentUnpublishedObject.DisplayName;
                unpublishedObject.UnpublishedObjectTypeURI = currentUnpublishedObject.TypeURI;
                unpublishedObject.UnpublishedObjectIcon = new BitmapImage(OntologyIconProvider.GetTypeIconPath(currentUnpublishedObject.TypeURI));
                unpublishedObject.EnableSelection = true;
                unpublishedObject.relatedViewModel = currentUnpublishedObject;
                unpublishedObject.relatedKWObject = currentUnpublishedObject.RelatedKWObject;
                unpublishedObject.UnpublishObjectComponents = GenerateUnpublishObjectComponents(currentUnpublishedObject, unpublishedObject);
                result.Add(unpublishedObject);
            }
            return result;
        }

        private ObservableCollection<UnpublishObjectComponent> GenerateUnpublishObjectComponents(
            ViewModel.Publish.UnpublishedObject unpublishedObjects, UnpublishedObject parent)
        {
            if (unpublishedObjects == null)
            {
                throw new ArgumentNullException("unpublishedObjects");
            }
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }

            ObservableCollection<UnpublishObjectComponent> result = new ObservableCollection<UnpublishObjectComponent>();

            if (unpublishedObjects.UnpublishedProperties.Count > 0)
            {
                result.Add(new UnpublishObjectComponent()
                {
                    ComponentType = ComponentType.Property,
                    ComponentValues = GeneratePropertiesComponentValue(unpublishedObjects.UnpublishedProperties, parent)
                });
            }

            if (unpublishedObjects.UnpublishedLinks.Count > 0)
            {
                result.Add(new UnpublishObjectComponent()
                {
                    ComponentType = ComponentType.Link,
                    ComponentValues = GenerateLinksComponentValue(unpublishedObjects.UnpublishedLinks, parent)
                });
            }

            if (unpublishedObjects.UnpublishedMedias.Count > 0)
            {
                result.Add(new UnpublishObjectComponent()
                {
                    ComponentType = ComponentType.Media,
                    ComponentValues = GenerateMediasComponentValue(unpublishedObjects.UnpublishedMedias, parent)
                });
            }

            return result;
        }

        private ObservableCollection<UnpublishedComponentValue> GenerateMediasComponentValue(List<UnpublishedMedia> unpublishedMedias, UnpublishedObject parent)
        {
            if (unpublishedMedias == null)
            {
                throw new ArgumentNullException("unpublishedMedias");
            }
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }

            ObservableCollection<UnpublishedComponentValue> result = new ObservableCollection<UnpublishedComponentValue>();
            foreach (var currentMedia in unpublishedMedias)
            {
                UnpublishedComponentValue unpublishedComponentValue = new UnpublishedComponentValue();
                if (currentMedia.ChangeType == UnpublishConceptChangeType.Added)
                {
                    unpublishedComponentValue.ChangeType = ChangeType.Added;
                }
                else if (currentMedia.ChangeType == UnpublishConceptChangeType.Changed)
                {
                    unpublishedComponentValue.ChangeType = ChangeType.Changed;
                }
                else if (currentMedia.ChangeType == UnpublishConceptChangeType.Deleted)
                {
                    unpublishedComponentValue.ChangeType = ChangeType.Deleted;
                }
                unpublishedComponentValue.IsUnpublishedComponentValueSelected = true;
                unpublishedComponentValue.Value = GenerateMediaUnpublishedComponentValue(currentMedia);
                unpublishedComponentValue.UnpublishedComponentType = ComponentType.Media;
                unpublishedComponentValue.Parents.Add(parent);
                unpublishedComponentValue.relatedMedia = currentMedia.relatedKWMedia;
                result.Add(unpublishedComponentValue);
            }
            return result;
        }

        private string GenerateMediaUnpublishedComponentValue(UnpublishedMedia media)
        {
            if (media == null)
            {
                throw new ArgumentNullException("media");
            }

            string result = "";
            result = string.Format("Media file path: {0}", media.UnpublishedMediaFilePath);
            return result;
        }

        private ObservableCollection<UnpublishedComponentValue> GenerateLinksComponentValue(List<UnpublishedLink> unpublishedLinks, UnpublishedObject parent)
        {
            if (unpublishedLinks == null)
            {
                throw new ArgumentNullException("unpublishedLinks");
            }
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }

            ObservableCollection<UnpublishedComponentValue> result = new ObservableCollection<UnpublishedComponentValue>();
            foreach (var currentLink in unpublishedLinks)
            {
                UnpublishedComponentValue unpublishedComponentValue = GetAppropriateUnpublishedComponentValue(currentLink, parent);
                result.Add(unpublishedComponentValue);
            }
            return result;
        }

        private UnpublishedComponentValue GetAppropriateUnpublishedComponentValue(UnpublishedLink link, UnpublishedObject parent)
        {
            if (link == null)
            {
                throw new ArgumentNullException("link");
            }
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }

            UnpublishedComponentValue result = null;
            if (kwrelationToComponentValueDictionary.ContainsKey(link.relatedKWRelationship))
            {
                result = kwrelationToComponentValueDictionary[link.relatedKWRelationship];
                result.Parents.Add(parent);
            }
            else
            {
                result = new UnpublishedComponentValue();
                if (link.ChangeType == UnpublishConceptChangeType.Added)
                {
                    result.ChangeType = ChangeType.Added;
                }
                else if (link.ChangeType == UnpublishConceptChangeType.Changed)
                {
                    result.ChangeType = ChangeType.Changed;
                }
                else if (link.ChangeType == UnpublishConceptChangeType.Deleted)
                {
                    result.ChangeType = ChangeType.Deleted;
                }
                result.IsUnpublishedComponentValueSelected = true;
                result.Value = GenerateLinkUnpublishedComponentValue(link);
                result.UnpublishedComponentType = ComponentType.Link;
                result.Parents.Add(parent);
                result.relatedRelationship = link.relatedKWRelationship;
                kwrelationToComponentValueDictionary.Add(link.relatedKWRelationship, result);
            }
            return result;

        }

        private string GenerateLinkUnpublishedComponentValue(UnpublishedLink link)
        {
            if (link == null)
            {
                throw new ArgumentNullException("link");
            }

            string result = "";
            result = string.Format("Type: {0}, Description: {1}", OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(link.TypeURI), link.Description);
            return result;
        }

        private ObservableCollection<UnpublishedComponentValue> GeneratePropertiesComponentValue(List<UnpublishedProperty> unpublishedProperties, UnpublishedObject parent)
        {
            if (unpublishedProperties == null)
            {
                throw new ArgumentNullException("unpublishedProperties");
            }
            if (parent == null)
            {
                throw new ArgumentNullException("parent");
            }

            ObservableCollection<UnpublishedComponentValue> result = new ObservableCollection<UnpublishedComponentValue>();
            foreach (var currentProperty in unpublishedProperties)
            {
                UnpublishedComponentValue unpublishedComponentValue = new UnpublishedComponentValue();
                if (currentProperty.ChangeType == UnpublishConceptChangeType.Added)
                {
                    unpublishedComponentValue.ChangeType = ChangeType.Added;
                }
                else if (currentProperty.ChangeType == UnpublishConceptChangeType.Changed)
                {
                    unpublishedComponentValue.ChangeType = ChangeType.Changed;
                }
                else if (currentProperty.ChangeType == UnpublishConceptChangeType.Deleted)
                {
                    unpublishedComponentValue.ChangeType = ChangeType.Deleted;
                }
                unpublishedComponentValue.IsUnpublishedComponentValueSelected = true;
                unpublishedComponentValue.Value = GeneratePropertyUnpublishedComponentValue(currentProperty);
                unpublishedComponentValue.UnpublishedComponentType = ComponentType.Property;
                unpublishedComponentValue.Parents.Add(parent);
                unpublishedComponentValue.relatedProperty = currentProperty.relatedKWProperty;
                result.Add(unpublishedComponentValue);
            }
            return result;
        }

        private string GeneratePropertyUnpublishedComponentValue(UnpublishedProperty property)
        {
            if (property == null)
            {
                throw new ArgumentNullException("property");
            }

            string result = "";
            result = string.Format("Type: {0}, Value: {1}", property.UnpublishedPropertyType, property.UnpublishedPropertyValue);
            return result;
        }

        private static void SelectAllSelectedUnpublishConcept(object checkBox)
        {
            if (checkBox == null)
                throw new ArgumentNullException(nameof(checkBox));

            UnpublishConcept selectedUnpublishConcept = (checkBox as CheckBox).DataContext as UnpublishConcept;
            foreach (var currentUnpublishedObject in selectedUnpublishConcept.UnpublishedObjects)
            {
                (currentUnpublishedObject as UnpublishedObject).IsUnpublishedObjectSelected = true;
                foreach (var currentUnpublishObjectComponent in currentUnpublishedObject.UnpublishObjectComponents)
                {
                    foreach (var currentComponentValue in currentUnpublishObjectComponent.ComponentValues)
                    {
                        currentComponentValue.IsUnpublishedComponentValueSelected = true;
                    }
                }
            }
        }

        private static void DeselectAllSelectedUnpublishConcept(object checkBox)
        {
            if (checkBox == null)
                throw new ArgumentNullException(nameof(checkBox));

            UnpublishConcept selectedUnpublishConcept = (checkBox as CheckBox).DataContext as UnpublishConcept;
            foreach (var currentUnpublishedObject in selectedUnpublishConcept.UnpublishedObjects)
            {
                (currentUnpublishedObject as UnpublishedObject).IsUnpublishedObjectSelected = false;
                foreach (var currentUnpublishObjectComponent in currentUnpublishedObject.UnpublishObjectComponents)
                {
                    foreach (var currentComponentValue in currentUnpublishObjectComponent.ComponentValues)
                    {
                        currentComponentValue.IsUnpublishedComponentValueSelected = false;
                    }
                }
            }
        }

        private UnpublishedConcepts GetSelectedUnpublishedConcepts()
        {
            HashSet<KWObject> selectedKWObjects = new HashSet<KWObject>();
            HashSet<KWProperty> selectedKWProperties = new HashSet<KWProperty>();
            HashSet<KWMedia> selectedKWMedias = new HashSet<KWMedia>();
            HashSet<KWRelationship> selectedKWRelationships = new HashSet<KWRelationship>();

            foreach (var currentUnpublishConcept in AddedUnpublishConcept)
            {
                foreach (var currentUnpublishedObject in currentUnpublishConcept.UnpublishedObjects)
                {
                    if (currentUnpublishedObject.IsUnpublishedObjectSelected
                        && !selectedKWObjects.Contains(currentUnpublishedObject.relatedKWObject))
                    {
                        selectedKWObjects.Add(currentUnpublishedObject.relatedKWObject);
                    }
                    foreach (var currentUnpublishObjectComponent in currentUnpublishedObject.UnpublishObjectComponents)
                    {
                        switch (currentUnpublishObjectComponent.ComponentType)
                        {
                            case ComponentType.Property:
                                foreach (var currentComponentValue in currentUnpublishObjectComponent.ComponentValues)
                                {
                                    if (currentComponentValue.IsUnpublishedComponentValueSelected
                                        && !selectedKWProperties.Contains(currentComponentValue.relatedProperty))
                                    {
                                        selectedKWProperties.Add(currentComponentValue.relatedProperty);
                                    }
                                }
                                break;
                            case ComponentType.Link:
                                foreach (var currentComponentValue in currentUnpublishObjectComponent.ComponentValues)
                                {
                                    if (currentComponentValue.IsUnpublishedComponentValueSelected
                                        && !selectedKWRelationships.Contains(currentComponentValue.relatedRelationship))
                                    {
                                        selectedKWRelationships.Add(currentComponentValue.relatedRelationship);
                                    }
                                }
                                break;
                            case ComponentType.Media:
                                foreach (var currentComponentValue in currentUnpublishObjectComponent.ComponentValues)
                                {
                                    if (currentComponentValue.IsUnpublishedComponentValueSelected
                                        && !selectedKWMedias.Contains(currentComponentValue.relatedMedia))
                                    {
                                        selectedKWMedias.Add(currentComponentValue.relatedMedia);
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            foreach (var currentUnpublishConcept in ChangedUnpublishConcept)
            {
                foreach (var currentUnpublishedObject in currentUnpublishConcept.UnpublishedObjects)
                {
                    if (currentUnpublishedObject.IsUnpublishedObjectSelected
                        && !selectedKWObjects.Contains(currentUnpublishedObject.relatedKWObject))
                    {
                        selectedKWObjects.Add(currentUnpublishedObject.relatedKWObject);
                    }
                    foreach (var currentUnpublishObjectComponent in currentUnpublishedObject.UnpublishObjectComponents)
                    {
                        switch (currentUnpublishObjectComponent.ComponentType)
                        {
                            case ComponentType.Property:
                                foreach (var currentComponentValue in currentUnpublishObjectComponent.ComponentValues)
                                {
                                    if (currentComponentValue.IsUnpublishedComponentValueSelected
                                        && !selectedKWProperties.Contains(currentComponentValue.relatedProperty))
                                    {
                                        selectedKWProperties.Add(currentComponentValue.relatedProperty);
                                    }
                                }
                                break;
                            case ComponentType.Link:
                                foreach (var currentComponentValue in currentUnpublishObjectComponent.ComponentValues)
                                {
                                    if (currentComponentValue.IsUnpublishedComponentValueSelected
                                        && !selectedKWRelationships.Contains(currentComponentValue.relatedRelationship))
                                    {
                                        selectedKWRelationships.Add(currentComponentValue.relatedRelationship);
                                    }
                                }
                                break;
                            case ComponentType.Media:
                                foreach (var currentComponentValue in currentUnpublishObjectComponent.ComponentValues)
                                {
                                    if (currentComponentValue.IsUnpublishedComponentValueSelected
                                        && !selectedKWMedias.Contains(currentComponentValue.relatedMedia))
                                    {
                                        selectedKWMedias.Add(currentComponentValue.relatedMedia);
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            UnpublishedConcepts result = new UnpublishedConcepts();
            result.unpublishedObjectChanges = selectedKWObjects.ToList();
            result.unpublishedPropertyChanges = selectedKWProperties.ToList();
            result.unpublishedRelationshipChanges = selectedKWRelationships.ToList();
            result.unpublishedMediaChanges = selectedKWMedias.ToList();
            return result;
        }

        private void SelectOtherUnpublishedConcepts(UnpublishedComponentValue selectedUnpublishedComponentValue)
        {
            if (selectedUnpublishedComponentValue == null)
            {
                throw new ArgumentNullException("selectedUnpublishedComponentValue");
            }

            if (selectedUnpublishedComponentValue.UnpublishedComponentType == ComponentType.Link)
            {
                foreach (var currentParrent in selectedUnpublishedComponentValue.Parents)
                {
                    UnpublishedObject selectedObject = currentParrent;
                    if (selectedObject.ChangeType == ChangeType.Added &&
                        !CheckUnpublishedObjectForSelectedComponent(selectedObject))
                    {
                        selectedObject.IsUnpublishedObjectSelected = true;
                    }
                    else if (selectedObject.ChangeType == ChangeType.Changed &&
                        !CheckUnpublishedObjectForSelectedComponent(selectedObject))
                    {
                        selectedObject.IsUnpublishedObjectSelected = true;
                        selectedObject.EnableSelection = true;
                    }
                }
            }
            else
            {
                UnpublishedObject selectedObject = selectedUnpublishedComponentValue.Parents.FirstOrDefault();
                if (selectedObject.ChangeType == ChangeType.Added &&
                    !CheckUnpublishedObjectForSelectedComponent(selectedObject))
                {
                    selectedObject.IsUnpublishedObjectSelected = true;
                }
                else if (selectedObject.ChangeType == ChangeType.Changed &&
                    !CheckUnpublishedObjectForSelectedComponent(selectedObject))
                {
                    selectedObject.IsUnpublishedObjectSelected = true;
                    selectedObject.EnableSelection = true;
                }
            }
        }

        private void DeselectOtherUnpublishedConcepts(UnpublishedComponentValue selectedUnpublishedComponentValue)
        {
            if (selectedUnpublishedComponentValue == null)
            {
                throw new ArgumentNullException("selectedUnpublishedComponentValue");
            }

            if (selectedUnpublishedComponentValue.UnpublishedComponentType == ComponentType.Link)
            {
                foreach (var currentParrent in selectedUnpublishedComponentValue.Parents)
                {
                    UnpublishedObject selectedObject = currentParrent;
                    if (selectedObject.ChangeType == ChangeType.Changed &&
                        (IsThereOnlyOneComponentInUnpublishedObject(selectedObject) || CheckUnpublishedObjectForSelectedComponent(selectedObject)))
                    {
                        selectedObject.IsUnpublishedObjectSelected = false;
                        selectedObject.EnableSelection = false;
                    }
                }
            }
            else
            {
                UnpublishedObject selectedObject = selectedUnpublishedComponentValue.Parents.FirstOrDefault();
                if (selectedObject.ChangeType == ChangeType.Changed &&
                    (IsThereOnlyOneComponentInUnpublishedObject(selectedObject) || (CheckUnpublishedObjectForSelectedComponent(selectedObject))))
                {
                    selectedObject.IsUnpublishedObjectSelected = false;
                    selectedObject.EnableSelection = false;
                }
            }

        }

        private bool CheckUnpublishedObjectForSelectedComponent(UnpublishedObject selectedObject)
        {
            if (selectedObject == null)
            {
                throw new ArgumentNullException("selectedObject");
            }

            bool result = false;
            int propertyComponentCount = GetSelectedPropertyCount(selectedObject);
            int mediaComponentCount = GetSelectedMediaCount(selectedObject);
            int linkComponentCount = GetSelectedLinkCount(selectedObject);
            if (propertyComponentCount == 0 && mediaComponentCount == 0 && linkComponentCount == 0)
            {
                result = true;
            }
            return result;
        }

        public bool IsThereOnlyOneComponentInUnpublishedObject(UnpublishedObject unpublishedObject)
        {
            if (unpublishedObject == null)
            {
                throw new ArgumentNullException("unpublishedObject");
            }

            bool result = false;
            int propertyCount = GetPropertyComponentCount(unpublishedObject);
            int mediaCount = GetMediaComponentCount(unpublishedObject);
            int linkCount = GetLinkComponentCount(unpublishedObject);

            if (((propertyCount == 1) && (mediaCount == 0) && (linkCount == 0)) ||
                ((propertyCount == 0) && (mediaCount == 1) && (linkCount == 0)) ||
                ((propertyCount == 0) && (mediaCount == 0) && (linkCount == 1)))
            {
                result = true;
            }
            return result;
        }

        private int GetLinkComponentCount(UnpublishedObject unpublishedObject)
        {
            if (unpublishedObject == null)
            {
                throw new ArgumentNullException("unpublishedObject");
            }

            int result = 0;
            foreach (var currentComponent in unpublishedObject.UnpublishObjectComponents)
            {
                if (currentComponent.ComponentType == ComponentType.Link)
                {
                    result = currentComponent.ComponentValues.Count;
                }
            }
            return result;
        }

        private int GetSelectedLinkCount(UnpublishedObject unpublishedObject)
        {
            if (unpublishedObject == null)
            {
                throw new ArgumentNullException("unpublishedObject");
            }

            int result = 0;
            foreach (var currentComponent in unpublishedObject.UnpublishObjectComponents)
            {
                if (currentComponent.ComponentType == ComponentType.Link)
                {
                    foreach (var currentComponentValue in currentComponent.ComponentValues)
                    {
                        if (currentComponentValue.IsUnpublishedComponentValueSelected)
                        {
                            result++;
                        }
                    }
                }
            }
            return result;
        }

        private int GetMediaComponentCount(UnpublishedObject unpublishedObject)
        {
            if (unpublishedObject == null)
            {
                throw new ArgumentNullException("unpublishedObject");
            }

            int result = 0;
            foreach (var currentComponent in unpublishedObject.UnpublishObjectComponents)
            {
                if (currentComponent.ComponentType == ComponentType.Media)
                {
                    result = currentComponent.ComponentValues.Count;
                }
            }
            return result;
        }

        private int GetSelectedMediaCount(UnpublishedObject unpublishedObject)
        {
            if (unpublishedObject == null)
            {
                throw new ArgumentNullException("unpublishedObject");
            }

            int result = 0;
            foreach (var currentComponent in unpublishedObject.UnpublishObjectComponents)
            {
                if (currentComponent.ComponentType == ComponentType.Media)
                {
                    foreach (var currentComponentValue in currentComponent.ComponentValues)
                    {
                        if (currentComponentValue.IsUnpublishedComponentValueSelected)
                        {
                            result++;
                        }
                    }
                }
            }
            return result;
        }

        private int GetPropertyComponentCount(UnpublishedObject unpublishedObject)
        {
            if (unpublishedObject == null)
            {
                throw new ArgumentNullException("unpublishedObject");
            }

            int result = 0;
            foreach (var currentComponent in unpublishedObject.UnpublishObjectComponents)
            {
                if (currentComponent.ComponentType == ComponentType.Property)
                {
                    result = currentComponent.ComponentValues.Count;
                }
            }
            return result;
        }

        private int GetSelectedPropertyCount(UnpublishedObject unpublishedObject)
        {
            if (unpublishedObject == null)
            {
                throw new ArgumentNullException("unpublishedObject");
            }

            int result = 0;
            foreach (var currentComponent in unpublishedObject.UnpublishObjectComponents)
            {
                if (currentComponent.ComponentType == ComponentType.Property)
                {
                    foreach (var currentComponentValue in currentComponent.ComponentValues)
                    {
                        if (currentComponentValue.IsUnpublishedComponentValueSelected)
                        {
                            result++;
                        }
                    }

                }
            }
            return result;
        }

        public async Task ChangeStateOfUpperWriteAccessNotification()
        {
            UserAccountControlProvider authentication = new UserAccountControlProvider();

            if (await authentication.HasLoggedInUserUpperWritePermission(UserAccountControlProvider.ManuallyEnteredDataACL))
            {
                HideNotificationGrid();
            }
            else
            {
                ShowNotificationGrid();
            }
        }

        private void ShowNotificationGrid()
        {
            NotificationGrid.Visibility = Visibility.Visible;
        }

        private void HideNotificationGrid()
        {
            NotificationGrid.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region مدیریت رخدادگردانها        

        private void UnpublishedComponentValue_Checked(object sender, RoutedEventArgs e)
        {
            UnpublishedComponentValue selectedUnpublishedComponentValue = ((sender as System.Windows.Controls.CheckBox).DataContext as UnpublishedComponentValue);
            SelectOtherUnpublishedConcepts(selectedUnpublishedComponentValue);
        }

        private void UnpublishedComponentValue_Unchecked(object sender, RoutedEventArgs e)
        {
            UnpublishedComponentValue selectedUnpublishedComponentValue = ((sender as System.Windows.Controls.CheckBox).DataContext as UnpublishedComponentValue);
            DeselectOtherUnpublishedConcepts(selectedUnpublishedComponentValue);
        }

        private void UnpublishedObject_Unchecked(object sender, RoutedEventArgs e)
        {
            UnpublishedObject selectedUnpublishedObject = (sender as CheckBox).DataContext as UnpublishedObject;
            foreach (var currentUnpublishObjectComponent in selectedUnpublishedObject.UnpublishObjectComponents)
            {
                foreach (var currentComponentValue in currentUnpublishObjectComponent.ComponentValues)
                {
                    currentComponentValue.IsUnpublishedComponentValueSelected = false;
                }
            }
        }

        private void UnpublishedObject_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void Level1CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            SelectAllSelectedUnpublishConcept(sender);
        }

        private void Level1CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            DeselectAllSelectedUnpublishConcept(sender);
        }

        public event EventHandler<EventArgs> BeginOfAccessLevelChecking;

        protected void OnBeginOfAccessLevelChecking()
        {
            BeginOfAccessLevelChecking?.Invoke(this, new EventArgs());
        }

        public event EventHandler<EventArgs> EndOfAccessLevelChecking;

        protected void OnEndOfAccessLevelChecking()
        {
            EndOfAccessLevelChecking?.Invoke(this, new EventArgs());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        #endregion

        private void AddedItemsScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }

        private void ChangedItemsScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            ScrollViewer scv = (ScrollViewer)sender;
            scv.ScrollToVerticalOffset(scv.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
