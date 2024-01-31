using GPAS.AccessControl;
using GPAS.Dispatch.Entities.Concepts.Geo;
using GPAS.Dispatch.Entities.NLP;
using GPAS.Dispatch.Entities.NLP.Summarization;
using GPAS.Ontology;
using GPAS.PropertiesValidation;
using GPAS.Utility;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Entities.SearchAroundResult;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Controls.Browser.EventsArgs;
using GPAS.Workspace.Presentation.Controls.PropertyValueTemplates;
using GPAS.Workspace.Presentation.Controls.TagCloud;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ExceptionHandler = GPAS.Logger.ExceptionHandler;

namespace GPAS.Workspace.Presentation.Controls.Browser
{
    public partial class BrowseControl : INotifyPropertyChanged
    {
        #region متغیرهای سراسری

        /// <summary>
        /// موجودیتی که می‌خواهیم به نمایش درآید
        /// </summary>
        public KWObject ObjectToBrowse
        {
            get => (KWObject)GetValue(ObjectToBrowseProperty);
            set => SetValue(ObjectToBrowseProperty, value);
        }

        // Using a DependencyProperty as the backing store for MinFlowPathWeight.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ObjectToBrowseProperty =
            DependencyProperty.Register(nameof(ObjectToBrowse), typeof(KWObject), typeof(BrowseControl),
                new PropertyMetadata(null, OnSetObjectToBrowseChanged));

        /// <summary>
        /// لیست ویژگی های شی
        /// </summary>
        public ObservableCollection<PropertyModel> ObjectProperties
        {
            get { return (ObservableCollection<PropertyModel>)GetValue(ObjectPropertiesProperty); }
            set { SetValue(ObjectPropertiesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ObjectProperties.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ObjectPropertiesProperty =
            DependencyProperty.Register(nameof(ObjectProperties), typeof(ObservableCollection<PropertyModel>), typeof(BrowseControl),
                new PropertyMetadata(null, OnSetObjectPropertiesChanged));

        private string objectDisplayName;
        public string ObjectDisplayName
        {
            get => objectDisplayName;
            set
            {
                if (objectDisplayName == value)
                    return;

                objectDisplayName = value;
                OnPropertyChanged();
            }
        }

        //رنگ‌هایی که وضعیت اعتبارسنجی ویژگی‌ها رو نمایش می‌دهند
        private readonly SolidColorBrush validBrush;
        private readonly SolidColorBrush inValidBrush;
        private readonly SolidColorBrush warningBrush;

        /// <summary>
        /// متغییری که نشان می‌دهد رویداد‌ها یکبار بارگیری شده‌اند
        /// </summary>
        private bool relatedEventsLoaded;

        /// <summary>
        /// متغییری که نشان می‌دهد موجودیت‌ها یکبار بارگیری شده‌اند
        /// </summary>
        private bool relatedEntitiesLoaded;

        /// <summary>
        /// متغییری که نشان می‌دهد سند‌ها یکبار بارگیری شده‌اند
        /// </summary>
        private bool relatedDocumentsLoaded;

        private bool isRefreshingContentAnalysisTab;
        private bool isRefreshingSummarizationTab;

        bool relatedDocumentsCountMoreThanThreshold;
        bool relatedEntitiesCountMoreThanThreshold;
        bool relatedEventsCountMoreThanThreshold;
        RelationshipBasedResultsPerSearchedObjects relatedDocumentsBrowsedObjResults;
        RelationshipBasedResultsPerSearchedObjects relatedEntitiesBrowsedObjResults;
        RelationshipBasedResultsPerSearchedObjects relatedEventsBrowsedObjResults;

        /// <summary>
        /// لیست آیتم‌های منوی گناری
        /// </summary>
        public ObservableCollection<BrowserMenuItemModel> MenuItems { get; set; }

        /// <summary>
        /// لیست رویداد‌های مرتبط با شی
        /// </summary>
        public ObservableCollection<RelatedEventForShowInBrowser> RelatedEvents { get; set; }

        /// <summary>
        /// لیست موجودیت‌های مرتبط با شی
        /// </summary>
        public ObservableCollection<RelatedEntityForShowInBrowser> RelatedEntities { get; set; }

        /// <summary>
        /// لیست سند‌های مرتبط با شی
        /// </summary>
        public ObservableCollection<RelatedDocumentForShowInBrowser> RelatedDocuments { get; set; }

        public List<GroupClassificationBasedPermission> ClassificationBasedPermissions { get; set; }

        public List<KWDataSourceACL> DataSourceAcls { get; set; }

        private ContentAnalysisInfo contentAnalysisInfo;

        SummarizationRateType summarizationRateType = Dispatch.Entities.NLP.Summarization.SummarizationRateType.Paragraph;
        double summarizationRateValue = 3;

        /// <summary>
        /// نشان می‌دهد که آیا کنترل‌های پردازش متن
        /// را می‌توان استفاده کرد یا نه
        /// </summary>
        private bool showNlpTools;
        public bool ShowNlpTools
        {
            get => showNlpTools;
            set
            {
                if (showNlpTools == value)
                    return;

                showNlpTools = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// نشان می‌دهد که آیا شی یک سند است یا خیر
        /// </summary>
        private bool isDocument;
        public bool IsDocument
        {
            get => isDocument;
            set
            {
                if (isDocument == value)
                    return;

                isDocument = value;

                if (isDocument)
                {
                    PropertyPicker.ExceptDataTypeCollection.Add(BaseDataTypes.GeoTime);
                }
                else
                {
                    PropertyPicker.ExceptDataTypeCollection.Remove(BaseDataTypes.GeoTime);
                }

                OnPropertyChanged();
            }
        }

        /// <summary>
        /// نشان می‌دهد که آیا کنترل‌های پردازش تصویر
        /// را می‌توان استفاده کرد یا نه
        /// </summary>
        private bool showImageAnalysisTools;
        public bool ShowImageAnalysisTools
        {
            get => showImageAnalysisTools;
            set
            {
                if (showImageAnalysisTools == value)
                    return;

                showImageAnalysisTools = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// برای مدیریت اینکه چه بخشی نمایش داده 
        /// شود از این متغیر استفاده می‌کنیم
        /// به این صورت که خاصیت 
        /// Visibility
        /// آن کنترل ها را به این متغییر بایند می‌کنیم
        /// و به هرکدام به عنوان پارامتر  متغییر مربوط 
        /// به خود را در لیست کنترل ها می‌دهیم
        /// سپس در مبدل این دو با هم چک می‌شوند اگر یکسان باشند آن کنترل را نشان می‌دهد
        /// در غیر این صورت آن کنترل مخفی می‌شود 
        /// </summary>
        private BrowseMenuItemTypes currentItem = BrowseMenuItemTypes.Properties;
        public BrowseMenuItemTypes CurrentItem
        {
            get => currentItem;
            set
            {
                currentItem = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void OnPropertyChanged([CallerMemberName] string caller = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(caller));
        }

        #endregion

        #region مدیریت رخدادها

        /// <summary>
        /// رخداد نمایش شی بر روی نقشه
        /// </summary>
        public event EventHandler<ShowOnMapRequestedEventArgs> ShowOnMapRequested;

        protected void OnShowOnMapRequested(KWObject objectRequestedToShowOnMap)
        {
            if (objectRequestedToShowOnMap == null)
                throw new ArgumentNullException(nameof(objectRequestedToShowOnMap));

            ShowOnMapRequested?.Invoke(this, new ShowOnMapRequestedEventArgs(objectRequestedToShowOnMap));
        }

        /// <summary>
        /// رخداد بارگذاری پردازش تصویر
        /// </summary>
        public event EventHandler<LoadInImageAnalysisRequestedEventArgs> LoadInImageAnalysisRequested;

        protected void OnLoadInImageAnalysisRequested(KWObject objectRequested)
        {
            if (objectRequested == null)
                throw new ArgumentNullException(nameof(objectRequested));

            LoadInImageAnalysisRequested?.Invoke(this, new LoadInImageAnalysisRequestedEventArgs(objectRequested));
        }

        /// <summary>
        /// رخداد نمایش رابطه در گراف
        /// </summary>
        public event EventHandler<AddRelationshipToGraphEventArgs> AddRelationshipToGraph;

        private void OnAddRelationshipToGraph(RelationshipBasedKWLink relationshipToShow)
        {
            if (relationshipToShow == null)
                throw new ArgumentNullException(nameof(relationshipToShow));

            AddRelationshipToGraph?.Invoke(this, new AddRelationshipToGraphEventArgs(relationshipToShow));
        }

        /// <summary>
        /// رخداد حذف یک ویژگی 
        /// </summary>
        public event NotifyCollectionChangedEventHandler PropertiesChanged;

        protected void OnPropertiesChanged(NotifyCollectionChangedEventArgs e)
        {
            PropertiesChanged?.Invoke(this, e);
        }

        public event EventHandler<KWObject> ShowInBrowser;

        private void OnShowInBrowser(KWObject objectToShow)
        {
            if (objectToShow == null)
                throw new ArgumentNullException(nameof(objectToShow));

            ShowInBrowser?.Invoke(this, objectToShow);
        }
        #endregion

        #region توابع        

        /// <summary>
        /// سازنده کلاس
        /// </summary>
        public BrowseControl()
        {
            InitializeComponent();
            IsDocument = false;
            ShowNlpTools = false;
            ObjectProperties = new ObservableCollection<PropertyModel>();
            MenuItems = new ObservableCollection<BrowserMenuItemModel>();
            RelatedEvents = new ObservableCollection<RelatedEventForShowInBrowser>();
            RelatedEntities = new ObservableCollection<RelatedEntityForShowInBrowser>();
            RelatedDocuments = new ObservableCollection<RelatedDocumentForShowInBrowser>();
            ClassificationBasedPermissions = new List<GroupClassificationBasedPermission>();
            DataSourceAcls = new List<KWDataSourceACL>();

            validBrush = new SolidColorBrush(Colors.Black);
            inValidBrush = new SolidColorBrush(Colors.Red);
            warningBrush = new SolidColorBrush(Colors.DarkOrange);
        }

        private static void OnSetObjectPropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BrowseControl)d).OnSetObjectPropertiesChanged(e);
        }

        private void OnSetObjectPropertiesChanged(DependencyPropertyChangedEventArgs e)
        {
            ObservableCollection<PropertyModel> oldVal = (ObservableCollection<PropertyModel>)e.OldValue;

            if (oldVal != null)
            {
                oldVal.CollectionChanged -= ObjectProperties_CollectionChanged;
            }
            if (ObjectProperties == null)
            {
                ObjectProperties_CollectionChanged(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }
            else
            {
                ObjectProperties.CollectionChanged -= ObjectProperties_CollectionChanged;
                ObjectProperties.CollectionChanged += ObjectProperties_CollectionChanged; ;

                if (oldVal == null)
                {
                    ObjectProperties_CollectionChanged(this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, ObjectProperties));
                }
                else
                {
                    ObjectProperties_CollectionChanged(this,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, ObjectProperties, oldVal));
                }
            }
        }

        private void ObjectProperties_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null)
            {
                foreach (PropertyModel property in e.NewItems)
                {

                }
            }

            if (e.OldItems != null)
            {
                foreach (PropertyModel property in e.OldItems)
                {

                }
            }

            if (ObjectProperties == null || ObjectProperties.Count == 0)
            {
                PropertyPicker.ExceptTypeUriCollection = new ObservableCollection<string>();
            }
            else
            {
                PropertyPicker.ExceptTypeUriCollection = new ObservableCollection<string>(ObjectProperties.Select(p=>p.TypeUri).Distinct());
            }
        }

        private void DisplayNameOnValueChanged(object sender, EventArgs e)
        {
            ObjectDisplayName = ObjectToBrowse.GetObjectLabel();
        }

        /// <summary>
        /// آماده‌سازی اولیه کنترل
        /// </summary>
        /// <param name="kwObject">شی ای که قرار است نمایش داده شود</param>
        /// <returns></returns>
        public async Task Init(KWObject kwObject = null)
        {
            if (kwObject != null)
            {
                ObjectToBrowse = kwObject;
            }

            contentAnalysisInfo = new ContentAnalysisInfo(ObjectToBrowse.ID);

            await UpdateDataSourceACL();
            await ShowObjectIcon();
            await GetObjectProperties();
            ShowNlpTools = await CanShowNlpTools();

            if (OntologyProvider.GetOntology().IsDocument(ObjectToBrowse.TypeURI))
            {
                IsDocument = true;
            }

            ShowImageAnalysisTools = await CanShowImageAnalysisTools();

            BrowseControlNotInitializedLabel.Visibility = Visibility.Hidden;
            BrowseControlMainGrid.Visibility = Visibility.Visible;

            SummarizationRateValue.Text = summarizationRateValue.ToString(CultureInfo.CurrentCulture);

            PrepareMenuItems();
        }

        #region Menu items

        //توابع مربوط به منوی کناری آیتم‌ها

        /// <summary>
        /// مدیریت رخداد انتخاب آیتم از منوی کناری
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuItem_OnSelected(object sender, RoutedEventArgs e)
        {
            PrepareControlToShow(((BrowserMenuItemModel)MenuItemsTreeView.SelectedItem).Type);
        }

        /// <summary>
        /// کنترل مرتبط با آیتم انتخاب شده از منوی کناری را به نمایش درمی‌آورد
        /// </summary>
        /// <param name="selectedItemType">آیتم انتخاب شده</param>
        private async void PrepareControlToShow(BrowseMenuItemTypes selectedItemType)
        {
            if (selectedItemType == BrowseMenuItemTypes.None)
                return;

            switch (selectedItemType)
            {
                case BrowseMenuItemTypes.RelatedEvents:
                    if (!relatedEventsLoaded)
                        await ShowRelatedEvents();
                    break;
                case BrowseMenuItemTypes.RelatedEntities:
                    if (!relatedEntitiesLoaded)
                        await ShowRelatedEntities();
                    break;
                case BrowseMenuItemTypes.RelatedDocuments:
                    if (!relatedDocumentsLoaded)
                        await ShowRelatedDocuments();
                    break;
                case BrowseMenuItemTypes.ContentAnalysis:
                    ContentAnalysisTabItem_GotFocus();
                    break;
                case BrowseMenuItemTypes.Summarization:
                    SummarizationTabItem_GotFocus();
                    break;
            }

            CurrentItem = selectedItemType;
        }

        /// <summary>
        /// آماده‌سازی منوی کناری
        /// </summary>
        private void PrepareMenuItems()
        {
            if (MenuItems.Count != 0)
            {
                MenuItems.Clear();
            }

            MenuItems.Add(new BrowserMenuItemModel
            {
                Text = "Properties",
                IsSelected = true,
                Icon = PackIconKind.FormatListBulleted,
                Type = BrowseMenuItemTypes.Properties,
                Items = null
            });

            MenuItems.Add(new BrowserMenuItemModel
            {
                Text = "Related",
                IsSelected = false,
                Icon = PackIconKind.LinkVariant,
                Type = BrowseMenuItemTypes.None,
                Items = new List<BrowserMenuItemModel>
                 {
                     new BrowserMenuItemModel
                     {
                         Text = "Events",
                         IsSelected = false,
                         Icon = PackIconKind.EventClock,
                         Type = BrowseMenuItemTypes.RelatedEvents,
                         Items = null
                     },
                     new BrowserMenuItemModel
                     {
                         Text = "Entities",
                         IsSelected = false,
                         Icon = PackIconKind.FileTree,
                         Type = BrowseMenuItemTypes.RelatedEntities,
                         Items = null
                     },
                     new BrowserMenuItemModel
                     {
                         Text = "Document",
                         IsSelected = false,
                         Icon = PackIconKind.FileDocument,
                         Type = BrowseMenuItemTypes.RelatedDocuments,
                         Items = null
                     }
                 }
            });

            if (showNlpTools)
            {
                MenuItems.Add(new BrowserMenuItemModel
                {
                    Text = "Content analysis",
                    IsSelected = false,
                    Icon = PackIconKind.FileDocumentBoxSearchOutline,
                    Type = BrowseMenuItemTypes.ContentAnalysis,
                    Items = null
                });

                MenuItems.Add(new BrowserMenuItemModel
                {
                    Text = "Summarization",
                    IsSelected = false,
                    Icon = PackIconKind.NoteTextOutline,
                    Type = BrowseMenuItemTypes.Summarization,
                    Items = null
                });
            }
        }

        private void OpenDocumentButton_OnClick(object sender, RoutedEventArgs e)
        {
            OpenDocument();
        }

        /// <summary>
        /// دانلود و نمایش سند مورد نظر
        /// </summary>
        private async void OpenDocument()
        {
            try
            {
                ButtonProgressAssist.SetIsIndicatorVisible(OpenDocumentButton, true);
                string localPath = await Logic.DataSourceProvider.DownloadDocumentAsync(ObjectToBrowse);
                FileUtility utility = new FileUtility();
                utility.OpenFileWithWindowsAppropriateApplication(localPath);

            }
            catch (FileNotFoundException)
            {
                KWMessageBox.Show(Properties.Resources.There_is_no_file_for_the_document,
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                ButtonProgressAssist.SetIsIndicatorVisible(OpenDocumentButton, false);
            }
        }

        private void ShowObjectOnMapButton_OnClick(object sender, RoutedEventArgs e)
        {
            OnShowOnMapRequested(ObjectToBrowse);
        }

        private void LoadOnImageAnalysisButton_OnClick(object sender, RoutedEventArgs e)
        {
            OnLoadInImageAnalysisRequested(ObjectToBrowse);
        }

        #endregion

        #region Properties section

        //توابع مربوط به بخش ویژگی‌ها


        public async Task GetObjectProperties()
        {
            PropertyPicker.ObjectTypeUriCollection = new ObservableCollection<string>
            {
                ObjectToBrowse.TypeURI
            };

            IEnumerable<KWProperty> objectProperties = null;
            IEnumerable<KWProperty> unpublishedProperties = null;

            try
            {
                PropertyActionsWaitingControl.TaskIncrement();
                objectProperties = await PropertyManager.GetPropertiesOfObjectAsync(ObjectToBrowse);
                unpublishedProperties = PropertyManager.GetUnpublishedPropertiesOfObject(ObjectToBrowse);
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
            }
            finally
            {
                PropertyActionsWaitingControl.TaskDecrement();
            }

            var unpublishedPropertiesList = unpublishedProperties.ToList();
            objectProperties = objectProperties.Except(unpublishedPropertiesList);
            var objectPropertiesList = objectProperties.ToList();
            objectPropertiesList.AddRange(unpublishedPropertiesList);

            PreparePropertiesToShow(objectPropertiesList);
        }

        private void PreparePropertiesToShow(List<KWProperty> propertiesList)
        {
            if (ObjectProperties.Count != 0)
            {
                ObjectProperties.Clear();
            }

            ObjectProperties.Add(new PropertyModel
            {
                Id = propertiesList.First().ID,
                TypeUri = propertiesList.First().TypeURI,
                UserFriendlyName = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(propertiesList.First().TypeURI),
                PropertyValue = PreparePropertyValue(propertiesList.First()),
                Background = CheckPropertyValidation(propertiesList.First()),
                IsUnpublished = false,
                Tag = propertiesList.First()
            });

            for (int i = 1; i < propertiesList.Count; i++)
            {
                ObjectProperties.Add(new PropertyModel
                {
                    Id = propertiesList[i].ID,
                    TypeUri = propertiesList[i].TypeURI,
                    UserFriendlyName = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(propertiesList[i].TypeURI),
                    PropertyValue = PreparePropertyValue(propertiesList[i]),
                    Background = CheckPropertyValidation(propertiesList[i]),
                    IsUnpublished = PropertyManager.IsUnpublishedProperty(propertiesList[i]),
                    Tag = propertiesList[i]
                });
            }

#if DEBUG
            ObjectIdTxtBlock.Text = $" [ID: {ObjectToBrowse.ID.ToString()}]";
#endif
        }

        private async Task AddProperty()
        {
            try
            {
                string newPropertyValue = NewPropertyValueTemplatesControl.GetPropertyValue();
                KWProperty newProperty = null;

                PropertyActionsWaitingControl.TaskIncrement();
                var objectToBrowse = ObjectToBrowse;
                var selectedPropertyTypeUri = PropertyPicker.SelectedItem.TypeUri;

                await Task.Run(() =>
                {
                    newProperty = PropertyManager.CreateNewPropertyForObject
                        (objectToBrowse, selectedPropertyTypeUri, newPropertyValue);
                });

                ObjectProperties.Add(new PropertyModel
                {
                    Id = newProperty.ID,
                    TypeUri = newProperty.TypeURI,
                    UserFriendlyName = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(newProperty.TypeURI),
                    PropertyValue = PreparePropertyValue(newProperty),
                    Background = Brushes.Black,
                    IsUnpublished = true,
                    Tag = newProperty
                });

                OnPropertiesChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, new List<KWProperty>() { newProperty }));
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show($"{Properties.Resources.Invalid_Server_Response}\n\n{ex.Message}");
            }
            finally
            {
                PropertyActionsWaitingControl.TaskDecrement();
            }
        }

        private void EditProperty()
        {
            Dictionary<KWProperty, KWProperty> propertiesEdited = new Dictionary<KWProperty, KWProperty>();//first new property. second old property
            foreach (var propertyModel in ObjectProperties)
            {
                if (propertyModel.Tag == EditPropertyValueTemplatesControl.Property)
                {
                    try
                    {
                        string stringValue = EditPropertyValueTemplatesControl.GetPropertyValue();

                        propertyModel.PropertyValue = stringValue;

                        switch (PropertyManager.IsPropertyValid(OntologyProvider.GetBaseDataTypeOfProperty(
                            EditPropertyValueTemplatesControl.Property.TypeURI), stringValue, CultureInfo.CurrentCulture).Status)
                        {
                            case ValidationStatus.Valid:
                                propertyModel.Background = validBrush;
                                break;
                            case ValidationStatus.Warning:
                                propertyModel.Background = warningBrush;
                                break;
                            case ValidationStatus.Invalid:
                                propertyModel.Background = inValidBrush;
                                break;
                        }

                        KWProperty oldProperty = GetCopyOfKWProperty(EditPropertyValueTemplatesControl.Property);
                        PropertyManager.EditPropertyOfObject(EditPropertyValueTemplatesControl.Property, stringValue);
                        propertiesEdited.Add(EditPropertyValueTemplatesControl.Property, oldProperty);
                    }
                    catch (Exception ex)
                    {
                        ExceptionHandler exceptionHandler = new ExceptionHandler();
                        exceptionHandler.WriteErrorLog(ex);
                        KWMessageBox.Show($"{Properties.Resources.Invalid_Server_Response}\n\n{ex.Message}");
                    }
                }
            }

            OnPropertiesChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, propertiesEdited.Keys.ToList(), propertiesEdited.Values.ToList()));
        }

        private KWProperty GetCopyOfKWProperty(KWProperty kWProperty)
        {
            return new KWProperty()
            {
                DataSourceId = kWProperty.DataSourceId,
                Deleted = kWProperty.Deleted,
                ID = kWProperty.ID,
                Owner = kWProperty.Owner,
                TypeURI = kWProperty.TypeURI,
                Value = kWProperty.Value,
            };
        }

        private void PropertyPicker_OnSelectedItemChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (PropertyPicker.SelectedItem == null)
            {
                NewPropertyValueTemplatesControl.ResetControl();
                NewPropertyValueTemplatesControl.IsEnabled = false;
                AcceptNewPropertyButton.IsEnabled = false;
                return;
            }

            NewPropertyValueTemplatesControl.PrepareControl(PropertyPicker.SelectedItem.BaseDataType);
            PropertiesDataGrid.UnselectAll();
        }

        private void PropertiesList_OnMouseUpHandler(object sender, MouseButtonEventArgs e)
        {
            if (PropertiesDataGrid.SelectedItem == null)
                return;

            var selectedProperty = ((PropertyModel)PropertiesDataGrid.SelectedItem).Tag;
            PreparePropertyToEdit((KWProperty)selectedProperty);
        }

        private void PreparePropertyToEdit(KWProperty property)
        {
            if (property.DataSourceId == null)
            {
                ShowEditPropertyControl(property, false);
            }
            else
            {
                ShowEditPropertyControl(property, !IsPropertyEditable(property));
            }
        }

        private void ShowEditPropertyControl(KWProperty property, bool readOnly)
        {
            EditPropertyValueTemplatesControl.SetPropertyValue(property, readOnly);
        }

        public bool IsPropertyEditable(KWProperty property)
        {
            bool result = false;

            PermissionControl permissionControl = new PermissionControl();

            if (property == null)
            {
                throw new ArgumentNullException(nameof(property));
            }

            if (ObjectManager.IsUnpublishObject(property.Owner))
            {
                result = true;
            }
            else
            {
                if (DataSourceAcls != null &&
                    ClassificationBasedPermissions != null &&
                    DataSourceAcls.Select(ds => ds.Id).Contains(property.DataSourceId.GetValueOrDefault()) &&
                    permissionControl.CanWriteWithPermissions(
                        DataSourceAcls.FirstOrDefault(ds => ds.Id == property.DataSourceId.GetValueOrDefault())
                            ?.Acl, ClassificationBasedPermissions))
                {
                    result = true;
                }
            }

            return result;
        }

        private string PreparePropertyValue(KWProperty property)
        {
            string propertyValue;

            if (property is GeoTimeKWProperty geoTimeKWProperty)
            {
                var result = ValueBaseValidation.TryParsePropertyValue(BaseDataTypes.GeoTime, geoTimeKWProperty.Value, out _, CultureInfo.CurrentCulture);

                if (result.Status == ValidationStatus.Invalid)
                {
                    propertyValue = property.Value;
                }
                else
                {
                    propertyValue = GeoTime.GetGeoTimeStringValue(geoTimeKWProperty.GeoTimeValue);
                }
            }
            else if (property is GeoPointKWProperty geoPointKWProperty)
            {
                var result = ValueBaseValidation.TryParsePropertyValue(BaseDataTypes.GeoPoint, geoPointKWProperty.Value, out _, CultureInfo.CurrentCulture);

                if (result.Status == ValidationStatus.Invalid)
                {
                    propertyValue = property.Value;
                }
                else
                {
                    propertyValue = GeoPoint.GetGeoPointStringValue(geoPointKWProperty.GeoLocationValue);
                }
            }
            else
            {
                var result = ValueBaseValidation.TryParsePropertyValue(
                    OntologyProvider.GetOntology().GetBaseDataTypeOfProperty(property.TypeURI), property.Value, out var parsedValue);

                propertyValue = result.Status == ValidationStatus.Invalid ? property.Value : string.Format(CultureInfo.CurrentCulture, "{0}", parsedValue);
            }

            return propertyValue;
        }

        private SolidColorBrush CheckPropertyValidation(KWProperty property)
        {
            switch (PropertyManager.IsPropertyValid(
                OntologyProvider.GetOntology().GetBaseDataTypeOfProperty(property.TypeURI), property.Value).Status)
            {
                case ValidationStatus.Valid:
                    return validBrush;
                case ValidationStatus.Warning:
                    return warningBrush;
                case ValidationStatus.Invalid:
                    return inValidBrush;
                default:
                    return inValidBrush;
            }
        }

        private void DeletePropertyButton_OnClick(object sender, RoutedEventArgs e)
        {
            DeleteUnpublishedProperty((KWProperty)((PropertyModel)PropertiesDataGrid.SelectedItem).Tag);
        }

        private void NewPropertyValueTemplatesControl_OnEnterKeyHit(object sender, EventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(true, null);
        }

        private void EditPropertyValueTemplatesControl_OnEnterKeyHit(object sender, EventArgs e)
        {
            DialogHost.CloseDialogCommand.Execute(true, null);
        }

        private async void AddNewPropertyOnDialogClosing(object sender, DialogClosingEventArgs eventArgs)
        {

            if (bool.TryParse(eventArgs.Parameter.ToString(), out bool result))
            {
                if (!result)
                    return;

                await AddProperty();
                NewPropertyValueTemplatesControl.ResetControl();
                PropertyPicker.RemoveSelectedItem();
                NewPropertyValueTemplatesControl.IsEnabled = false;
            }
            else
            {
                eventArgs.Cancel();
            }
        }

        private void EditPropertyOnDialogClosing(object sender, DialogClosingEventArgs eventArgs)
        {
            if (bool.TryParse(eventArgs.Parameter.ToString(), out bool result))
            {
                if (!result)
                    return;

                EditProperty();
            }
            else
            {
                eventArgs.Cancel();
            }
        }

        private void NewPropertyValueTemplatesControl_OnCheckValidation(object sender, ValidationResultEventArgs e)
        {
            AcceptNewPropertyButton.IsEnabled = e.ValidationResult == ValidationStatus.Valid;
        }

        private void EditPropertyValueTemplatesControl_OnCheckValidation(object sender, ValidationResultEventArgs e)
        {
            AcceptEditPropertyButton.IsEnabled = e.ValidationResult == ValidationStatus.Valid;
        }

        private void PropertiesDataGrid_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PropertiesDataGrid.SelectedIndex < 0)
            {
                EditPropertyButton.IsEnabled = false;
            }
            else
            {
                EditPropertyButton.IsEnabled = true;
            }
        }

        private void PropertiesDataGrid_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            DataGridsOnMouseDownHandler(e, PropertiesDataGrid);
            EditPropertyButton.IsEnabled = false;
        }

        #endregion

        #region Related events section

        //توابع مربوط به بخش رخداد‌های مرتبط

        private async Task ShowRelatedEvents()
        {
            try
            {
                if (RelatedEvents.Count != 0)
                    RelatedEvents.Clear();

                relatedEventsLoaded = true;

                RelatedEventsWaitingControl.TaskIncrement();

                var relationshipBasedResult = await Logic.Search.SearchAround.GetRelatedEvents(new[] { ObjectToBrowse });

                relatedEventsCountMoreThanThreshold = relationshipBasedResult.IsResultsCountMoreThanThreshold;

                relatedEventsBrowsedObjResults =
                    relationshipBasedResult.Results.FirstOrDefault(r => r.SearchedObject == ObjectToBrowse);

                if (relatedEventsBrowsedObjResults == null)
                {
                    ShowMoreEventsStackPanel.Visibility = Visibility.Collapsed;
                    return;
                }

                if (relatedEventsBrowsedObjResults.NotLoadedResults.Count == 0)
                {
                    ShowMoreEventsStackPanel.Visibility = Visibility.Collapsed;
                }
                else if (relatedEventsBrowsedObjResults.NotLoadedResults.Count > 0)
                {
                    ShowMoreEventsStackPanel.Visibility = Visibility.Visible;
                }

                List<RelationshipBasedKWLink> loadedRelationships =
                    await LinkManager.RetrieveRelationshipBaseLinksAsync(relatedEventsBrowsedObjResults.LoadedResults
                        .SelectMany(r => r.RelationshipIDs).ToList());

                foreach (var currentEvent in loadedRelationships)
                {
                    if (OntologyProvider.GetOntology().IsEvent(ObjectToBrowse.TypeURI))
                    {
                        if (currentEvent.Source.ID == ObjectToBrowse.ID)
                        {
                            if (OntologyProvider.GetOntology().IsEvent(currentEvent.Target.TypeURI))
                            {
                                BindDeletedChangeToLink(currentEvent);
                                RelatedEvents.Add(GenerateRelatedEventForShow(currentEvent));
                            }
                        }
                        else if (currentEvent.Target.ID == ObjectToBrowse.ID)
                        {
                            if (OntologyProvider.GetOntology().IsEvent(currentEvent.Source.TypeURI))
                            {
                                BindDeletedChangeToLink(currentEvent);
                                RelatedEvents.Add(GenerateRelatedEventForShow(currentEvent));
                            }
                        }
                    }
                    else
                    {
                        BindDeletedChangeToLink(currentEvent);
                        RelatedEvents.Add(GenerateRelatedEventForShow(currentEvent));
                    }
                }

                ListCollectionView collection = new ListCollectionView(RelatedEvents);
                collection.GroupDescriptions.Add(new PropertyGroupDescription("Relationship"));
                RelatedEventsDataGrid.ItemsSource = collection;
            }
            catch (Exception)
            {
                ShowMoreEventsStackPanel.Visibility = Visibility.Collapsed;
            }
            finally
            {
                RelatedEventsWaitingControl.TaskDecrement();
            }
        }

        private async Task ShowNotLoadedRelatedEventsAsync()
        {
            if (relatedEventsCountMoreThanThreshold)
                ShowMoreEventsStackPanel.Visibility = Visibility.Visible;

            if (relatedEventsBrowsedObjResults == null)
                return;

            List<RelationshipBasedKWLink> notLoadedRelationships =
                await LinkManager.RetrieveRelationshipBaseLinksAsync(relatedEventsBrowsedObjResults.NotLoadedResults
                    .SelectMany(r => r.RelationshipIDs).ToList());

            foreach (var currentEvent in notLoadedRelationships)
            {
                if (OntologyProvider.GetOntology().IsEvent(ObjectToBrowse.TypeURI))
                {
                    if (currentEvent.Source.ID == ObjectToBrowse.ID)
                    {
                        if (OntologyProvider.GetOntology().IsEvent(currentEvent.Target.TypeURI))
                        {
                            BindDeletedChangeToLink(currentEvent);
                            RelatedEvents.Add(GenerateRelatedEventForShow(currentEvent));
                        }
                    }
                    else if (currentEvent.Target.ID == ObjectToBrowse.ID)
                    {
                        if (OntologyProvider.GetOntology().IsEvent(currentEvent.Source.TypeURI))
                        {
                            BindDeletedChangeToLink(currentEvent);
                            RelatedEvents.Add(GenerateRelatedEventForShow(currentEvent));
                        }
                    }
                }
                else
                {
                    BindDeletedChangeToLink(currentEvent);
                    RelatedEvents.Add(GenerateRelatedEventForShow(currentEvent));
                }
            }

            ShowMoreEventsStackPanel.Visibility = Visibility.Collapsed;
        }

        private RelatedEventForShowInBrowser GenerateRelatedEventForShow(RelationshipBasedKWLink relationship)
        {
            var newItem = new RelatedEventForShowInBrowser
            {
                Type = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(
                    relationship.Source.ID != ObjectToBrowse.ID
                        ? relationship.Source.TypeURI
                        : relationship.Target.TypeURI),
                Relationship = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(relationship.TypeURI),
                Label = relationship.Source.ID != ObjectToBrowse.ID
                    ? relationship.Source.GetObjectLabel()
                    : relationship.Target.GetObjectLabel(),
                Text = relationship.Text,
                TimeBegin = relationship.Relationship.TimeBegin == null
                    ? string.Empty
                    : relationship.Relationship.TimeBegin.ToString(),
                TimeEnd = relationship.Relationship.TimeEnd == null
                    ? string.Empty
                    : relationship.Relationship.TimeEnd.ToString(),
                RelationshipBasedKwLink = relationship
            };

            return newItem;
        }

        private void hypRelatedEventName_Click(object sender, RoutedEventArgs e)
        {
            if (RelatedEventsDataGrid.SelectedItem == null)
                return;

            if (RelatedEventsDataGrid.SelectedIndex == -1)
            {

                return;
            }

            var relateEvent = (RelatedEventForShowInBrowser)RelatedEventsDataGrid.SelectedItem;
            OnShowInBrowser(relateEvent.RelationshipBasedKwLink.Source.ID != ObjectToBrowse.ID
                ? relateEvent.RelationshipBasedKwLink.Source : relateEvent.RelationshipBasedKwLink.Target);
        }

        private void btnAddRelatedEventToGraph_Click(object sender, RoutedEventArgs e)
        {
            if (RelatedEventsDataGrid.SelectedIndex == -1)
                return;

            OnAddRelationshipToGraph(((RelatedEventForShowInBrowser)RelatedEventsDataGrid.SelectedItem).RelationshipBasedKwLink);
        }

        private void ShowAllRelatedEventsButton_OnClick(object sender, RoutedEventArgs e)
        {
            ShowAllRelatedEvents();
        }

        private async void ShowAllRelatedEvents()
        {
            try
            {
                RelatedEventsWaitingControl.TaskIncrement();
                await ShowNotLoadedRelatedEventsAsync();
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message);
            }
            finally
            {
                RelatedEventsWaitingControl.TaskDecrement();
            }
        }

        private async void RefreshRelatedEventsButton_OnClick(object sender, RoutedEventArgs e)
        {
            await ShowRelatedEvents();
        }

        private void RelatedEventsDataGrid_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            DataGridsOnMouseDownHandler(e, RelatedEventsDataGrid);
        }

        #endregion

        #region Related entities section

        //توابع مربوط به بخش موجودیت‌‌ های مرتبط

        private async Task ShowRelatedEntities()
        {
            try
            {
                if (RelatedEntities.Count != 0)
                    RelatedEntities.Clear();

                relatedEntitiesLoaded = true;
                RelatedEntitiesWaitingControl.TaskIncrement();

                RelationshipBasedResult searchResult =
                    await Logic.Search.SearchAround.GetRelatedEntities(new List<KWObject> { ObjectToBrowse });

                relatedEntitiesCountMoreThanThreshold = searchResult.IsResultsCountMoreThanThreshold;

                relatedEntitiesBrowsedObjResults =
                    searchResult.Results.FirstOrDefault(r => r.SearchedObject == ObjectToBrowse);
                if (relatedEntitiesBrowsedObjResults == null)
                {
                    ShowMoreEntitiesStackPanel.Visibility = Visibility.Collapsed;
                    return;
                }

                if (relatedEntitiesBrowsedObjResults.NotLoadedResults.Count == 0)
                {
                    ShowMoreEntitiesStackPanel.Visibility = Visibility.Collapsed;
                }
                else if (relatedEntitiesBrowsedObjResults.NotLoadedResults.Count > 0)
                {
                    ShowMoreEntitiesStackPanel.Visibility = Visibility.Visible;
                }

                List<RelationshipBasedKWLink> loadedRelationships =
                    await LinkManager.RetrieveRelationshipBaseLinksAsync(relatedEntitiesBrowsedObjResults.LoadedResults
                        .SelectMany(r => r.RelationshipIDs).ToList());

                var relatedEntities = new List<RelatedEntityForShowInBrowser>(loadedRelationships.Count);

                foreach (RelationshipBasedKWLink item in loadedRelationships)
                {
                    var newItem = GenerateRelatedEntityForShowInBrowser(item);
                    if (newItem != null)
                    {
                        BindDeletedChangeToLink(item);
                        RelatedEntities.Add(newItem);
                    }
                }

                ListCollectionView collection = new ListCollectionView(RelatedEntities);
                collection.GroupDescriptions.Add(new PropertyGroupDescription("Relationship"));
                RelatedEntitiesDataGrid.ItemsSource = collection;
            }
            catch
            {
                //lblRelatedEntitiesCountMoreThanTresholdMessage.Visibility = Visibility.Collapsed;
                //txbShowAllRelatedEntities.Visibility = Visibility.Collapsed;
            }
            finally
            {
                RelatedEntitiesWaitingControl.TaskDecrement();
            }
        }

        private async Task ShowNotLoadedRelatedEntitiesAsync()
        {
            if (relatedEntitiesCountMoreThanThreshold)
                ShowMoreEntitiesStackPanel.Visibility = Visibility.Visible;

            if (relatedEntitiesBrowsedObjResults == null)
                return;

            var notLoadedRelationships =
                await LinkManager.RetrieveRelationshipBaseLinksAsync(relatedEntitiesBrowsedObjResults.NotLoadedResults
                    .SelectMany(r => r.RelationshipIDs).ToList());

            foreach (RelationshipBasedKWLink item in notLoadedRelationships)
            {
                var newItem = GenerateRelatedEntityForShowInBrowser(item);
                if (newItem != null)
                {
                    BindDeletedChangeToLink(item);
                    RelatedEntities.Add(newItem);
                }
            }

            ShowMoreEntitiesStackPanel.Visibility = Visibility.Hidden;

            /*
            List<RelatedEntityForShowInBrowser> relatedEntities = (List<RelatedEntityForShowInBrowser>)relatedEntitiesCollection.SourceCollection;
            relatedEntities.AddRange(notLoadedRelatedEntities);

            relatedEntitiesCollection = CollectionViewSource.GetDefaultView(relatedEntities);
            // txtRelatedEntitiesCount.DataContext = relatedEntitiesCollection;
            relatedEntitiesCollection.GroupDescriptions.Clear();
            relatedEntitiesCollection.GroupDescriptions.Add(new PropertyGroupDescription("Relationship"));
            relatedEntitiesListView.ItemsSource = relatedEntitiesCollection;
            */
        }

        private RelatedEntityForShowInBrowser GenerateRelatedEntityForShowInBrowser(RelationshipBasedKWLink relationship)
        {
            if (OntologyProvider.GetOntology().IsDocument(relationship.Source.TypeURI) ||
                OntologyProvider.GetOntology().IsDocument(relationship.Target.TypeURI))
                return null;

            RelatedEntityForShowInBrowser newItem = new RelatedEntityForShowInBrowser
            {
                Relationship = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(relationship.TypeURI),
                Related =
                    relationship.Source.ID != ObjectToBrowse.ID
                        ? relationship.Source.GetObjectLabel()
                        : relationship.Target.GetObjectLabel(),
                DateBegin =
                    relationship.Relationship.TimeBegin == null
                        ? string.Empty
                        : relationship.Relationship.TimeBegin.ToString(),
                DateEnd =
                    relationship.Relationship.TimeEnd == null
                        ? string.Empty
                        : relationship.Relationship.TimeEnd.ToString(),
                Text = relationship.Text,
                RelationshipBasedKwLink = relationship,
                Name = relationship.Source.ID != ObjectToBrowse.ID
                    ? relationship.Source.GetObjectLabel()
                    : relationship.Target.GetObjectLabel(),
                Type = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(
                    relationship.Source.ID != ObjectToBrowse.ID
                        ? relationship.Source.TypeURI
                        : relationship.Target.TypeURI),
                RelationshipType = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(relationship.TypeURI)
            };

            return newItem;

        }

        private void ShowAllRelatedEntitiesButton_OnClick(object sender, RoutedEventArgs e)
        {
            ShowAllRelatedEntities();
        }

        private async void ShowAllRelatedEntities()
        {
            try
            {
                RelatedEntitiesWaitingControl.TaskIncrement();
                await ShowNotLoadedRelatedEntitiesAsync();
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message);
            }
            finally
            {
                RelatedEntitiesWaitingControl.TaskDecrement();
            }
        }

        private void RelatedEntityHyperlink_OnClick(object sender, RoutedEventArgs e)
        {
            if (RelatedEntitiesDataGrid.SelectedItem == null)
                return;

            var relatedEntity = (RelatedEntityForShowInBrowser)RelatedEntitiesDataGrid.SelectedItem;
            OnShowInBrowser(relatedEntity.RelationshipBasedKwLink.Source.ID != ObjectToBrowse.ID
                ? relatedEntity.RelationshipBasedKwLink.Source
                : relatedEntity.RelationshipBasedKwLink.Target);
        }

        private void ShowEntityOnGraphButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (RelatedEntitiesDataGrid.SelectedItem == null)
                return;

            OnAddRelationshipToGraph(((RelatedEntityForShowInBrowser)RelatedEntitiesDataGrid.SelectedItem).RelationshipBasedKwLink);
        }

        private async void ReloadRelatedEntitiesButton_OnClick(object sender, RoutedEventArgs e)
        {
            await ShowRelatedEntities();
        }

        private void RelatedEntitiesDataGrid_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            DataGridsOnMouseDownHandler(e, RelatedEntitiesDataGrid);
        }

        #endregion

        #region Related documents section

        //توابع مربوط به بخش سند‌های مرتبط

        private async Task ShowRelatedDocuments()
        {
            try
            {
                if (RelatedDocuments.Count != 0)
                    RelatedDocuments.Clear();

                relatedDocumentsLoaded = true;

                RelatedDocumentsWaitingControl.TaskIncrement();

                // lblRelatedDocumentsCountMoreThanTresholdMessage.Visibility = Visibility.Collapsed;
                var searchResult = await Logic.Search.SearchAround.GetRelatedDocuments(new KWObject[] { ObjectToBrowse });
                relatedDocumentsCountMoreThanThreshold = searchResult.IsResultsCountMoreThanThreshold;

                relatedDocumentsBrowsedObjResults =
                    searchResult.Results.FirstOrDefault(r => r.SearchedObject == ObjectToBrowse);
                if (relatedDocumentsBrowsedObjResults == null)
                {
                    ShowMoreDocumentsStackPanel.Visibility = Visibility.Collapsed;
                    return;
                }

                if (relatedDocumentsBrowsedObjResults.NotLoadedResults.Count == 0)
                {
                    ShowMoreDocumentsStackPanel.Visibility = Visibility.Collapsed;
                }
                else if (relatedDocumentsBrowsedObjResults.NotLoadedResults.Count > 0)
                {
                    ShowMoreDocumentsStackPanel.Visibility = Visibility.Visible;
                }

                List<RelationshipBasedKWLink> loadedRelationships =
                    await LinkManager.RetrieveRelationshipBaseLinksAsync(relatedDocumentsBrowsedObjResults.LoadedResults
                        .SelectMany(r => r.RelationshipIDs).ToList());

                foreach (var item in loadedRelationships)
                {
                    var newItem = GenerateRelatedDocumentForShowInBrowser(item);
                    if (newItem != null)
                    {
                        BindDeletedChangeToLink(item);
                        RelatedDocuments.Add(newItem);
                    }
                }

                ListCollectionView collection = new ListCollectionView(RelatedDocuments);
                collection.GroupDescriptions.Add(new PropertyGroupDescription("Relationship"));
                RelatedDocumentsDataGrid.ItemsSource = collection;
            }
            catch
            {
                // lblRelatedDocumentsCountMoreThanTresholdMessage.Visibility = Visibility.Collapsed;
                // txbShowAllRelatedDocuments.Visibility = Visibility.Collapsed;
            }
            finally
            {
                RelatedDocumentsWaitingControl.TaskDecrement();
            }
        }

        private async Task ShowNotLoadedRelatedDocumentsAsync()
        {
            if (relatedDocumentsCountMoreThanThreshold)
                ShowMoreDocumentsStackPanel.Visibility = Visibility.Collapsed;

            if (relatedDocumentsBrowsedObjResults == null)
                return;

            List<RelationshipBasedKWLink> notLoadedRelationships = await LinkManager.RetrieveRelationshipBaseLinksAsync(relatedDocumentsBrowsedObjResults.NotLoadedResults.SelectMany(r => r.RelationshipIDs).ToList());

            foreach (var item in notLoadedRelationships)
            {
                var newItem = GenerateRelatedDocumentForShowInBrowser(item);
                if (newItem != null)
                {
                    BindDeletedChangeToLink(item);
                    RelatedDocuments.Add(newItem);
                }
            }

            //  relatedDocumentsCollection.GroupDescriptions.Clear();
            //  relatedDocumentsCollection.GroupDescriptions.Add(new PropertyGroupDescription("Relationship"));


            ShowMoreDocumentsStackPanel.Visibility = Visibility.Collapsed;
        }

        private RelatedDocumentForShowInBrowser GenerateRelatedDocumentForShowInBrowser(RelationshipBasedKWLink relationship)
        {
            var newItem = new RelatedDocumentForShowInBrowser
            {
                Relationship = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(relationship.TypeURI),
                Related = relationship.Source.ID != ObjectToBrowse.ID
                    ? relationship.Source.GetObjectLabel()
                    : relationship.Target.GetObjectLabel(),
                DateBegin = relationship.Relationship.TimeBegin == null
                    ? string.Empty
                    : relationship.Relationship.TimeBegin.ToString(),
                DateEnd = relationship.Relationship.TimeEnd == null
                    ? string.Empty
                    : relationship.Relationship.TimeEnd.ToString(),
                Text = relationship.Text,
                RelationshipBasedKwLink = relationship,
                Name = relationship.Source.ID != ObjectToBrowse.ID ? relationship.Source.GetObjectLabel() : relationship.Target.GetObjectLabel(),
                Type = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(relationship.Source.ID != ObjectToBrowse.ID ? relationship.Source.TypeURI : relationship.Target.TypeURI),
                RelationshipType = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(relationship.TypeURI)
            };

            return newItem;
        }

        private void btnAddRelatedDocumentToGraph_Click(object sender, RoutedEventArgs e)
        {
            if (RelatedDocumentsDataGrid.SelectedItem == null)
                return;

            OnAddRelationshipToGraph(((RelatedDocumentForShowInBrowser)RelatedDocumentsDataGrid.SelectedItem).RelationshipBasedKwLink);
        }

        private void hypRelatedDocumentName_Click(object sender, RoutedEventArgs e)
        {
            if (RelatedDocumentsDataGrid.SelectedItem == null)
                return;

            var relatedDocument = (RelatedDocumentForShowInBrowser)RelatedDocumentsDataGrid.SelectedItem;
            OnShowInBrowser(relatedDocument.RelationshipBasedKwLink.Source.ID != ObjectToBrowse.ID
                ? relatedDocument.RelationshipBasedKwLink.Source
                : relatedDocument.RelationshipBasedKwLink.Target);
        }

        private void ShowAllRelatedDocumentsButton_OnClick(object sender, RoutedEventArgs e)
        {
            ShowAllRelatedDocuments();
        }

        private async void ShowAllRelatedDocuments()
        {
            try
            {
                RelatedDocumentsWaitingControl.TaskIncrement();
                await ShowNotLoadedRelatedDocumentsAsync();
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(ex.Message);
            }
            finally
            {
                RelatedDocumentsWaitingControl.TaskDecrement();
            }
        }

        private void RelatedDocumentsDataGrid_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            DataGridsOnMouseDownHandler(e, RelatedDocumentsDataGrid);
        }

        private async void ReloadRelatedDocumentsButton_OnClick(object sender, RoutedEventArgs e)
        {
            await ShowRelatedDocuments();
        }

        #endregion

        #region Content analysis section

        //توابع مربوط به بخش آنالیز محتوا

        private async void SetContentAnalysisControlsAsync()
        {
            ContentAnalysisUnableToLoadDocumentContentPromptStackPanel.Visibility =
                ContentAnalysisUnableToLoadTagCloudPromptStackPanel.Visibility =
                    ContentAnalysisUnableToLoadLanguageStatisticsPromptStackPanel.Visibility = Visibility.Hidden;

            try
            {
                ContentAnalysisProgressBarWaitingControl.TaskIncrement();
                ContentAnalysisGrid.IsEnabled = false;

                contentAnalysisInfo.DocContent = await LoadTextDocumentContent();
                await ShowTagCloud(contentAnalysisInfo.DocContent);
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
            }
            finally
            {
                ContentAnalysisGrid.IsEnabled = true;
                ContentAnalysisProgressBarWaitingControl.TaskDecrement();
            }

            if (string.IsNullOrWhiteSpace(contentAnalysisInfo.DocContent))
                return;

            try
            {
                ContentAnalysisProgressBarWaitingControl.TaskIncrement();
                ContentAnalysisGrid.IsEnabled = false;

                await ShowLanguagesSpectrum(contentAnalysisInfo.DocContent);
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
            }
            finally
            {
                ContentAnalysisGrid.IsEnabled = true;
                ContentAnalysisProgressBarWaitingControl.TaskDecrement();
            }
        }

        private async Task ShowLanguagesSpectrum(string docContent)
        {
            try
            {
                NLPProvider provider = new NLPProvider();
                DetectedLanguage[] languages = await provider.DetectLanguageAsync(docContent);
                LangSpectrumControl.ShowLanguages(languages);
            }
            catch
            {
                ContentAnalysisUnableToLoadLanguageStatisticsPromptStackPanel.Visibility = Visibility.Visible;
                throw;
            }
        }

        private async Task ShowTagCloud(string docContent)
        {
            try
            {
                NLPProvider provider = new NLPProvider();
                TagCloudKeyPhrase[] keyPhrases = await provider.GetTagCloudAsync(docContent);
                ValidateKeyPhrases(keyPhrases);
                contentAnalysisInfo.RetrievedKeyPhrases = PrepareKeyPhraseModelList(keyPhrases);
                contentAnalysisInfo.IsContentAnalysisControlsSetBefore = true;
                ShowTagCloudForRetrievedScores();
            }
            catch
            {
                ContentAnalysisUnableToLoadTagCloudPromptStackPanel.Visibility = Visibility.Visible;
                throw;
            }
        }

        private KeyPhraseModel[] PrepareKeyPhraseModelList(TagCloudKeyPhrase[] tagCloudKeyPhrases)
        {
            return tagCloudKeyPhrases.Select(keyPhrase => new KeyPhraseModel
            { Key = keyPhrase.TextOfKeyPhrase, Weight = keyPhrase.Score }).ToArray();
        }

        private void ContentAnalysisTabItem_GotFocus()
        {
            if (isRefreshingContentAnalysisTab)
                return;

            if (contentAnalysisInfo.IsContentAnalysisControlsSetBefore)
            {
                ShowTagCloudForRetrievedScores();
            }
            else
            {
                try
                {
                    isRefreshingContentAnalysisTab = true;
                    SetContentAnalysisControlsAsync();
                }
                finally
                {
                    isRefreshingContentAnalysisTab = false;
                }
            }
        }

        private void SaveTagCloudButton_OnClick(object sender, RoutedEventArgs e)
        {
            JsTagCloudControl.SaveTagCloudImage();
        }

        #endregion

        #region Summarization section

        //توابع مربوط به بخش خلاصه‌سازی

        private async void SetSummarizationControlsAsync()
        {
            if (contentAnalysisInfo.IsSummarizationControlsSetBefore)
                return;

            SummarizationUnableToLoadDocumentContentPromptStackPanel.Visibility =
                SummarizationUnableToLoadTagCloudPromptStackPanel.Visibility =
                    SummarizationUnableToLoadLanguageStatisticsPromptStackPanel.Visibility = Visibility.Hidden;

            try
            {
                SummarizationWaitingControl.TaskIncrement();
                contentAnalysisInfo.DocContent = await LoadTextDocumentContent();
                await ShowSummarization(contentAnalysisInfo.DocContent, summarizationRateValue);
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
            }
            finally
            {
                SummarizationWaitingControl.TaskDecrement();
            }

            if (string.IsNullOrWhiteSpace(contentAnalysisInfo.DocContent))
                return;

            try
            {
                SummarizationWaitingControl.TaskIncrement();
                await ShowSummarizationTabLanguagesSpectrum(contentAnalysisInfo.DocContent);
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
            }
            finally
            {
                SummarizationWaitingControl.TaskDecrement();
            }
        }

        private async Task ShowSummarizationTabLanguagesSpectrum(string docContent)
        {
            try
            {
                NLPProvider provider = new NLPProvider();
                DetectedLanguage[] languages = await provider.DetectLanguageAsync(docContent);
                SummarizationlangSpectrumControl.ShowLanguages(languages);
            }
            catch
            {
                ContentAnalysisUnableToLoadLanguageStatisticsPromptStackPanel.Visibility = Visibility.Visible;
                throw;
            }
        }

        private async Task ShowSummarization(string docContent, double size)
        {
            try
            {
                NLPProvider provider = new NLPProvider();
                SummarizationRequest summarizationRequest = new SummarizationRequest
                {
                    Content = docContent,
                    Rate = new SummarizationRate
                    {
                        RateType = summarizationRateType,
                        RateValue = summarizationRateValue
                    }

                };

                string[] result = await provider.GetSummarizationdAsync(summarizationRequest);
                contentAnalysisInfo.RetrievedSummarization = result.ToList();
                contentAnalysisInfo.IsSummarizationControlsSetBefore = true;
                ShowSummarizationForRetrievedContent();
            }
            catch
            {
                SummarizationUnableToLoadTagCloudPromptStackPanel.Visibility = Visibility.Visible;
                throw;
            }
        }

        private void ShowSummarizationForRetrievedContent()
        {
            if (contentAnalysisInfo.RetrievedSummarization != null)
            {
                string str = string.Empty;
                SummarizationTextBox.Text = string.Empty;

                foreach (var summurization in contentAnalysisInfo.RetrievedSummarization)
                {
                    str += summurization + "\n\n";
                }

                SummarizationTextBox.Text = str;
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private void SummarizationTabItem_GotFocus()
        {
            if (isRefreshingSummarizationTab)
                return;

            if (contentAnalysisInfo.IsSummarizationControlsSetBefore)
            {
                ShowSummarizationForRetrievedContent();
            }
            else
            {
                try
                {
                    isRefreshingSummarizationTab = true;
                    SetSummarizationControlsAsync();
                }
                finally
                {
                    isRefreshingSummarizationTab = false;
                }
            }
        }

        private void SummarizationRateType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SummarizationRateType.SelectedItem == ParagraphComboBoxItem)
            {
                summarizationRateType = Dispatch.Entities.NLP.Summarization.SummarizationRateType.Paragraph;
            }
            else if (SummarizationRateType.SelectedItem == PercentComboBoxItem)
            {
                summarizationRateType = Dispatch.Entities.NLP.Summarization.SummarizationRateType.Percent;
            }
        }

        string lang = "OTHER";
        private async void SummarizationlangSpectrumControl_PieChartClicked(object sender, LanguageSpectrumControl.PieChartClickEventArgs e)
        {
            lang = e.SelectedPieChartItem.Title;
            await PerformLanguageBaseSummarization();
        }

        private async Task PerformLanguageBaseSummarization()
        {
            NLPProvider provider = new NLPProvider();
            string[] summarize;
            SummarizationRequest summarizationRequest = new SummarizationRequest
            {
                Content = contentAnalysisInfo.DocContent,
                Rate = new SummarizationRate
                {
                    RateType = summarizationRateType,
                    RateValue = summarizationRateValue
                }
            };

            try
            {
                SummarizationWaitingControl.TaskIncrement();

                switch (lang.ToUpper())
                {
                    case "ENGLISH":
                        summarize = await provider.GetLanguageSummarizationdAsync(summarizationRequest,
                            Dispatch.Entities.NLP.Language.en);
                        break;
                    case "PERSIAN":
                        summarize = await provider.GetLanguageSummarizationdAsync(summarizationRequest,
                            Dispatch.Entities.NLP.Language.fa);
                        break;
                    case "ARABIC":
                        summarize = await provider.GetLanguageSummarizationdAsync(summarizationRequest,
                            Dispatch.Entities.NLP.Language.ar);
                        break;
                    default:
                        summarize = await provider.GetSummarizationdAsync(summarizationRequest);
                        break;
                }

                contentAnalysisInfo.RetrievedSummarization = summarize.ToList();
                ShowSummarizationForRetrievedContent();
            }
            catch
            {
                ContentAnalysisUnableToLoadTagCloudPromptStackPanel.Visibility = Visibility.Visible;
                throw;
            }
            finally
            {
                SummarizationWaitingControl.TaskDecrement();
            }
        }

        #endregion

        public async void RefreshView()
        {
            await GetObjectProperties();
        }

        private async Task<bool> CanShowNlpTools()
        {
            bool nlpServiceVisibilityStatus = await Logic.System.GetNLPServiceVisibilityStatus();
            return nlpServiceVisibilityStatus && OntologyProvider.GetOntology().IsTextDocument(ObjectToBrowse.TypeURI);
        }

        private async Task<bool> CanShowImageAnalysisTools()
        {
            bool imageAnalysisServiceVisibilityStatus = await Logic.System.GetImageAnalysisServiceVisibilityStatus();
            return imageAnalysisServiceVisibilityStatus &&
                   OntologyProvider.GetOntology().IsDocument(ObjectToBrowse.TypeURI) &&
                   OntologyProvider.GetOntology().IsImageDocument(ObjectToBrowse.TypeURI);
        }

        private void BindDeletedChangeToLink(KWLink item)
        {
            if (item is RelationshipBasedKWLink link)
            {
                link.Relationship.Deleted += BrowseControl_Deleted;
            }
            else if (item is EventBasedKWLink kwLink)
            {
                kwLink.SecondRelationship.Deleted += BrowseControl_Deleted;
                kwLink.FirstRelationship.Deleted += BrowseControl_Deleted;
            }
        }

        private void BrowseControl_Deleted(object sender, EventArgs e)
        {
            ListCollectionView collectionView = (ListCollectionView)RelatedEntitiesDataGrid.ItemsSource;
            for (int i = 0; i < collectionView.Count; i++)
            {
                if (((RelatedEntityForShowInBrowser)collectionView.GetItemAt(i)).RelationshipBasedKwLink.Relationship
                    .ID == ((KWRelationship)sender).ID)
                {
                    if (collectionView.GetItemAt(i) is RelatedEntityForShowInBrowser item)
                    {
                        collectionView.Remove(item);
                    }
                }
            }

            collectionView.Refresh();
            ListCollectionView itemsSource = (ListCollectionView)RelatedEventsDataGrid.ItemsSource;
            for (int i = 0; i < itemsSource.Count; i++)
            {
                if (((RelatedEventForShowInBrowser)itemsSource.GetItemAt(i)).RelationshipBasedKwLink.Relationship.ID ==
                    ((KWRelationship)sender).ID)
                {
                    RelatedEventForShowInBrowser xx = itemsSource.GetItemAt(i) as RelatedEventForShowInBrowser;
                    itemsSource.Remove(xx);
                }
            }

            itemsSource.Refresh();
        }

        private async Task ShowObjectIcon()
        {
            if (OntologyProvider.GetOntology().IsImageDocument(ObjectToBrowse.TypeURI))
            {
                string imageFilePath = await Logic.DataSourceProvider.DownloadDocumentAsync(ObjectToBrowse);
                ObjectIcon.ImageSource = new BitmapImage(new Uri(imageFilePath, UriKind.RelativeOrAbsolute));
            }
            else
            {
                ObjectIcon.ImageSource = new BitmapImage(OntologyIconProvider.GetTypeIconPath(ObjectToBrowse.TypeURI));
            }
        }

        private async Task UpdateDataSourceACL()
        {
            IEnumerable<KWProperty> objectProperties = await PropertyManager.GetPropertiesOfObjectAsync(ObjectToBrowse);
            UserAccountControlProvider userAccountControlProvider = new UserAccountControlProvider();
            DataSourceAcls = await userAccountControlProvider.GetDataSourceAcl(objectProperties.Select(p => p.DataSourceId).Distinct().ToArray());
            Logic.AccessControl.GroupManagement groupManagement = new Logic.AccessControl.GroupManagement();
            UserAccountControlProvider authentication = new UserAccountControlProvider();
            List<string> groupsName = await groupManagement.GetGroupsOfUser(authentication.GetLoggedInUserName());
            ClassificationBasedPermissions = (await userAccountControlProvider.GetClassificationBasedPermissionForGroups(groupsName.ToArray())).ToList();
        }

        private void DeleteUnpublishedProperty(KWProperty propertyToDelete)
        {
            try
            {
                PropertyManager.DeletePropertyForObject(propertyToDelete);
                OnPropertiesChanged(
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, new List<KWProperty>(), new List<KWProperty>() { propertyToDelete }));

                foreach (var propertyModel in ObjectProperties)
                {
                    if (propertyModel.Id != propertyToDelete.ID)
                        continue;

                    ObjectProperties.Remove(propertyModel);
                    break;
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
            }
        }

        private async Task<string> LoadTextDocumentContent()
        {
            if (!string.IsNullOrEmpty(contentAnalysisInfo.DocContent))
                return contentAnalysisInfo.DocContent;

            try
            {
                NLPProvider provider = new NLPProvider();
                return await provider.GetDocumentPlaneTextAsync(ObjectToBrowse.ID);
            }
            catch
            {
                ContentAnalysisUnableToLoadDocumentContentPromptStackPanel.Visibility = Visibility.Visible;
                throw;
            }
        }

        private void ShowTagCloudForRetrievedScores()
        {
            if (contentAnalysisInfo.RetrievedKeyPhrases != null)
            {
                JsTagCloudControl.KeyPhrasesCollection =
                    new ObservableCollection<KeyPhraseModel>(contentAnalysisInfo.RetrievedKeyPhrases);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        private void ValidateKeyPhrases(TagCloudKeyPhrase[] keyPhrases)
        {
            foreach (TagCloudKeyPhrase phrase in keyPhrases)
            {
                if (string.IsNullOrWhiteSpace(phrase.TextOfKeyPhrase))
                    throw new ArgumentException(Properties.Resources.Invalid_Key_Phrase_Title);
                if (phrase.Score < 0 || phrase.Score > 1)
                    throw new ArgumentOutOfRangeException(Properties.Resources.Invalid_Key_Phrase_Score);
            }
        }

        private async void langSpectrumControl_PieChartClicked(object sender, LanguageSpectrumControl.PieChartClickEventArgs e)
        {
            NLPProvider provider = new NLPProvider();
            TagCloudKeyPhrase[] keyPhrases;
            try
            {
                ContentAnalysisProgressBarWaitingControl.TaskIncrement();
                ContentAnalysisGrid.IsEnabled = false;

                switch (e.SelectedPieChartItem.Title.ToUpper())
                {
                    case "ENGLISH":
                        keyPhrases = await provider.GetLanguageTagCloudAsync(contentAnalysisInfo.DocContent, Dispatch.Entities.NLP.Language.en);
                        break;
                    case "PERSIAN":
                        keyPhrases = await provider.GetLanguageTagCloudAsync(contentAnalysisInfo.DocContent, Dispatch.Entities.NLP.Language.fa);
                        break;
                    case "ARABIC":
                        keyPhrases = await provider.GetLanguageTagCloudAsync(contentAnalysisInfo.DocContent, Dispatch.Entities.NLP.Language.ar);
                        break;
                    default:
                        keyPhrases = await provider.GetTagCloudAsync(contentAnalysisInfo.DocContent);
                        break;
                }
                ValidateKeyPhrases(keyPhrases);
                contentAnalysisInfo.RetrievedKeyPhrases = PrepareKeyPhraseModelList(keyPhrases);
                ShowTagCloudForRetrievedScores();
            }
            catch
            {
                ContentAnalysisUnableToLoadTagCloudPromptStackPanel.Visibility = Visibility.Visible;
                throw;
            }
            finally
            {
                ContentAnalysisGrid.IsEnabled = true;
                ContentAnalysisProgressBarWaitingControl.TaskDecrement();
            }
        }

        private async void Apply_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(SummarizationRateValue.Text))
            {
                double.TryParse(SummarizationRateValue.Text, out summarizationRateValue);
            }
            await PerformLanguageBaseSummarization();
        }

        /// <summary>
        /// وقتی متغییر
        /// ObjectToBrowse
        /// تغییر می‌کند این تابع فراخوانی می‌شود
        /// </summary>
        /// <param name="d"></param>
        /// <param name="e"></param>
        private static void OnSetObjectToBrowseChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((BrowseControl)d).OnSetObjectToBrowseChanged(e);
        }

        private void OnSetObjectToBrowseChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue == null)
            {
                ObjectToBrowse.DisplayName.ValueChanged += DisplayNameOnValueChanged;
                ObjectDisplayName = ((KWObject)e.NewValue).GetObjectLabel();
            }
        }

        /// <summary>
        /// وقتی روی قسمت خالی از لیست کلیک می‌شود آیتم انتخاب شده 
        /// لغو انتخاب می‌شود
        /// </summary>
        /// <param name="e"/>
        /// <param name="dataGrid">لیست انتخاب شده</param>
        private void DataGridsOnMouseDownHandler(MouseButtonEventArgs e, DataGrid dataGrid)
        {
            HitTestResult hitTestResult = VisualTreeHelper.HitTest(this, e.GetPosition(this));
            if (hitTestResult.VisualHit.GetType() != typeof(DataGridRow))
                dataGrid.UnselectAll();
        }

        #endregion
    }
}
