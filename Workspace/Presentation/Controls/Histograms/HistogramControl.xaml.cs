using GPAS.Histogram;
using GPAS.HistogramViewer;
using GPAS.Logger;
using GPAS.Ontology;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.Controls.Histograms
{
    public partial class HistogramControl
    {
        // برای شناخت بهتر کد این کلاس، نمودار کلاس
        // HistogramFillingTypes
        // و نیز نمودار کلاس طراحی شده در کنترل
        // HistogrtamViewer
        // را ببینید

        #region مدیریت رخدادها
        public class ObjectsSelectionArgs
        {
            public ObjectsSelectionArgs(IEnumerable<KWObject> selectedObjects)
            {
                SelectedObjects = selectedObjects;
            }

            public IEnumerable<KWObject> SelectedObjects
            {
                private set;
                get;
            }
        }
        public event EventHandler<ObjectsSelectionArgs> SelectionChanged;
        public void OnSelectionChanged(IEnumerable<KWObject> selectedObjects)
        {
            if (SelectionChanged != null)
                SelectionChanged(this, new ObjectsSelectionArgs(selectedObjects));
        }

        public event EventHandler<TakeSnapshotEventArgs> SnapshotTaken;
        private void OnSnapshotTaken(TakeSnapshotEventArgs e)
        {
            SnapshotTaken?.Invoke(this, e);
        }
        #endregion

        public HistogramControl()
        {
            InitializeComponent();
            DataContext = this;
            mainHistogramViewer.CancelHistogramRequestButtonClicked += MainHistogramViewer_CancelHistogramRequestButtonClicked;
        }

        private void MainHistogramViewer_CancelHistogramRequestButtonClicked(object sender, EventArgs e)
        {
            isLastRequestCanceledByUser = true;
        }

        private Ontology.Ontology currentOntology = null;
        protected Ontology.Ontology GetCurrentOntology()
        {
            if (currentOntology == null)
                currentOntology = Logic.OntologyProvider.GetOntology();
            return currentOntology;
        }

        private Guid lastShowObjectsRequestGuid = new Guid();
        private bool isLastRequestCanceledByUser = false;
        IEnumerable<KWObject> ObjectsToShow = new List<KWObject>();

        public async void ShowObjectsHistogramAsync(IEnumerable<KWObject> objectsToShow)
        {
            ObjectsToShow = objectsToShow;

            if (objectsToShow == null)
                throw new ArgumentNullException("objectsToShow");

            if (!objectsToShow.Any())
                return;

            Guid currentShowObjectsRequestGuid = AssignGuidToRequest();

            isLastRequestCanceledByUser = false;
            mainHistogramViewer.ResetHistogramUIToDefaultStatus();
            mainHistogramViewer.ShowWaiting(Properties.Resources.Loading_Object_Information);
#if DEBUG
            // پیام‌های زمان اشکال‌زدایی، جهت تست صحت کارایی هیستوگرام در زمان همزمانی درخواست‌ها تعبیه شده‌اند
            Debug.WriteLine("Histogram::ShowObjects | New Request registered; GUID: " + currentShowObjectsRequestGuid.ToString() + " | Last GUID: " + lastShowObjectsRequestGuid.ToString());
#endif
            try
            {
                mainHistogramViewer.Clear();

                // یک مکث کوتاه برای اطمینان از عدم وجود درخواست جدیدی بعد از ثبت درخواست جاری
                await Task.Run(() => { Task.Delay(20); });
                if (!IsLastRequest(currentShowObjectsRequestGuid))
                {
                    throw new NewerShowObjectsRequestRegisteredException();
                }

                ShowObjectTypesGroup(objectsToShow, currentShowObjectsRequestGuid);

                IEnumerable<KWObject> entitiesToShow = objectsToShow.Where(o => GetCurrentOntology().IsEntity(o.TypeURI));
                await ShowObjectPropertiesGroup
                    (entitiesToShow, Properties.Resources.Entity_Properties, currentShowObjectsRequestGuid);

                IEnumerable<KWObject> eventsToShow = objectsToShow.Where(o => GetCurrentOntology().IsEvent(o.TypeURI));
                await ShowObjectPropertiesGroup
                    (eventsToShow, Properties.Resources.Event_Properties, currentShowObjectsRequestGuid);

                IEnumerable<KWObject> documentsToShow = objectsToShow.Where(o => GetCurrentOntology().IsDocument(o.TypeURI));
                await ShowObjectPropertiesGroup
                    (documentsToShow, Properties.Resources.Document_Properties, currentShowObjectsRequestGuid);

                mainHistogramViewer.StatusMessage = Properties.Resources.Ready;
                mainHistogramViewer.HideWaiting();
#if DEBUG
                Debug.WriteLine("Histogram::ShowObjects | Request processing finished; GUID: " + currentShowObjectsRequestGuid.ToString());
#endif
            }
            catch (NewerShowObjectsRequestRegisteredException)
            {
#if DEBUG
                Debug.WriteLine("Histogram::ShowObjects | Request stoped, newer request detected; GUID: " + currentShowObjectsRequestGuid.ToString() + " | Last request GUID: " + lastShowObjectsRequestGuid.ToString());
#endif
            }
            catch (OperationCanceledException ex)
            {
                mainHistogramViewer.ChangeHistogramUIToCancelStatus();
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
#if DEBUG
                Debug.WriteLine("Histogram::ShowObjects | Request canceled; GUID: " + currentShowObjectsRequestGuid.ToString());
#endif
            }
            catch (Exception ex)
            {
                mainHistogramViewer.StatusMessage = string.Format("{0}", Properties.Resources.Unable_to_load_Histogram);
                mainHistogramViewer.HideWaiting();

                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
#if DEBUG
                Debug.WriteLine("Histogram::ShowObjects | Request process stoped by exception; GUID: " + currentShowObjectsRequestGuid.ToString() + " | Exception message: " + ex.Message + " | Call Stack: " + ex.StackTrace);
#endif
            }
        }

        private async Task ShowObjectPropertiesGroup(IEnumerable<KWObject> objectsToFillProperties, string groupTitle, Guid requestGuid)
        {
            if (!objectsToFillProperties.Any())
                return;

            mainHistogramViewer.ShowWaiting(Properties.Resources.Loading_Properties);

            KWObject[] objectsToFillPropertiesArray = objectsToFillProperties.ToArray();
            List<string> objectsPossiblePropertyTypeUris = new List<string>();
            IEnumerable<string> objectTypeUris = objectsToFillPropertiesArray
                .Select(o => o.TypeURI)
                .Distinct();

            // استخراج نوع ویژگی‌هایی که ممکن است به اشیا ورودی انتساب داده شوند
            foreach (string currentObjectType in objectTypeUris)
            {
                if (ObjectPropertiesList.Count > 0)
                    ObjectPropertiesList.Clear();

                GetHierarchyPropertiesName(OntologyProvider.GetOntology().GetHierarchyPropertiesOfObject(currentObjectType));

                foreach (DataType currentPropertyType in ObjectPropertiesList)
                {
                    BaseDataTypes dataType = GetCurrentOntology().GetBaseDataTypeOfProperty(currentPropertyType.TypeName);
                    // TODO: پس از راه|‌اندازی ابزارک خط زمان، دیگر نیازی به بارگذاری ویژگی‌های مبتنی برزمان نیست
                    // و می‌توان خط زیر را برای جلوگیری از بارگذاری مذکور فعال کرد
                    //&& GetCurrentOntology().GetBaseDataTypeOfProperty(currentPropertyType.TypeName) != Ontology.Ontology.BaseDataTypes.DateTime
                    if (!objectsPossiblePropertyTypeUris.Contains(currentPropertyType.TypeName)
                        && dataType != BaseDataTypes.GeoTime && dataType != BaseDataTypes.GeoPoint)
                    {
                        objectsPossiblePropertyTypeUris.Add(currentPropertyType.TypeName);
                    }
                }
            }


            ObjectPropertiesHistogramGroup objectPropertiesGroup = new ObjectPropertiesHistogramGroup(groupTitle);
            List<Task<IEnumerable<KWProperty>>> retrieveTasks = new List<Task<IEnumerable<KWProperty>>>();

            // منبع الگوی پیاده‌سازی همزمانی بازیابی ویژگی‌ها
            // Source:  https://msdn.microsoft.com/en-us/library/hh873173(v=vs.110).aspx
            // Title:   Consuming the Task - based Asynchronous Pattern
            // Section: Throttling

            // سطح همزمانی درخواست‌های همزمان بازیابی ویژگی‌ها
            const int CONCURRENCY_LEVEL = 2;
            int nextIndex = 0;
            while (nextIndex < CONCURRENCY_LEVEL && nextIndex < objectsPossiblePropertyTypeUris.Count)
            {
                retrieveTasks.Add(Logic.PropertyManager.GetPropertiesOfObjectsAsync
                       (objectsToFillPropertiesArray, new string[] { objectsPossiblePropertyTypeUris[nextIndex] }));
                nextIndex++;
            }

            int UrisWithRetrievedProperties = 0;

            while (retrieveTasks.Count > 0)
            {
                Task<IEnumerable<KWProperty>> task = await Task.WhenAny(retrieveTasks);
                retrieveTasks.Remove(task);
                UrisWithRetrievedProperties++;

                IEnumerable<KWProperty> propertiesWithCurrentType = await task;
#if DEBUG
                Debug.WriteLine("Histogram::Show Object Properties | Retrieved properties count: " + propertiesWithCurrentType.Count());
#endif
                // در صورت ثبت شدن درخواست جدید، از ایجاد تغییر در هیستوگرام جلوگیری می شود
                if (!IsLastRequest(requestGuid))
                {
                    throw new NewerShowObjectsRequestRegisteredException();
                }
                if (isLastRequestCanceledByUser)
                {
                    throw new OperationCanceledException();
                }

                if (propertiesWithCurrentType.Any())
                {
                    ObjectPropertiesHistogramCategory currentTypeCategory =
                        new ObjectPropertiesHistogramCategory(OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(propertiesWithCurrentType.First().TypeURI));

                    IEnumerable<ObjectPropertiesHistogramValueCountPair> sameValuePropertiesPairs =
                        BreakdownSameValueProperties(propertiesWithCurrentType);
                    currentTypeCategory.AddValueCountPairCollection(sameValuePropertiesPairs);

                    currentTypeCategory.OrderBy = HistogramCategoriesOrderBy;
                    objectPropertiesGroup.AddSubCategory(currentTypeCategory);

                    // نمایش گروه در هیستوگرام یک بار بیشتر لازم نیست
                    // آن هم در زمانیست که گروه دارای حداقل یک دسته باشد؛
                    // با افزوده شدن دسته‌‌های بعدی، نمایش گروه در هیستوگرام
                    // به صورت خودکار به‌روز می‌شود
                    if (objectPropertiesGroup.HistogramSubItems.Count == 1)
                    {
                        mainHistogramViewer.UpdateHistogramItems(objectPropertiesGroup);
                    }
                }

                mainHistogramViewer.ShowWaiting
                    (UrisWithRetrievedProperties, objectsPossiblePropertyTypeUris.Count
                    , string.Format(Properties.Resources.Loading_something_, groupTitle));

                if (nextIndex < objectsPossiblePropertyTypeUris.Count)
                {
                    retrieveTasks.Add(Logic.PropertyManager.GetPropertiesOfObjectsAsync
                           (objectsToFillPropertiesArray, new string[] { objectsPossiblePropertyTypeUris[nextIndex] }));
                    nextIndex++;
                }
            }

            mainHistogramViewer.HideWaiting();
        }

        List<DataType> ObjectPropertiesList = new List<DataType>();

        private void GetHierarchyPropertiesName(ObservableCollection<OntologyNode> properties)
        {
            foreach (OntologyNode item in properties)
            {
                if (!(item is PropertyNode))
                    continue;

                PropertyNode property = item as PropertyNode;

                if (property.IsLeaf)
                {
                    ObjectPropertiesList.Add(new DataType() { TypeName = property.TypeUri, BaseDataType = property.BaseDataType });
                }
                else
                {
                    if (property.Children.Count <= 0)
                        return;

                    GetHierarchyPropertiesName(property.Children);
                }
            }
        }

        private bool IsLastRequest(Guid requestGuid)
        {
            return requestGuid.Equals(lastShowObjectsRequestGuid);
        }

        private IEnumerable<ObjectPropertiesHistogramValueCountPair> BreakdownSameValueProperties(IEnumerable<KWProperty> sameTypeProperties)
        {
            Dictionary<string, List<KWProperty>> pairs = new Dictionary<string, List<KWProperty>>();
            foreach (KWProperty item in sameTypeProperties)
            {
                if (!pairs.ContainsKey(item.Value.ToString()))
                    pairs.Add(item.Value.ToString(), new List<KWProperty>());
                pairs[item.Value.ToString()].Add(item);
            }

            List<ObjectPropertiesHistogramValueCountPair> result = new List<ObjectPropertiesHistogramValueCountPair>();
            foreach (KeyValuePair<string, List<KWProperty>> item in pairs)
                result.Add(new ObjectPropertiesHistogramValueCountPair(item.Key, item.Value));

            return result;
        }

        private void ShowObjectTypesGroup(IEnumerable<KWObject> objectsToShow, Guid requestGuid)
        {
            if (!objectsToShow.Any())
                return;

            // Dictionary of "Object type URI", "Instances (KWObjects) with the Type" per any category (Entities, Events & Documents)
            Dictionary<string, List<KWObject>> entitySubtypesWithInstatnces;
            Dictionary<string, List<KWObject>> eventSubtypesWithInstatnces;
            Dictionary<string, List<KWObject>> documentSubtypesWithInstatnces;

            BreakdownObjectsToTypeDictionaries
                (objectsToShow
                , out entitySubtypesWithInstatnces
                , out eventSubtypesWithInstatnces
                , out documentSubtypesWithInstatnces);

            ObjectTypesHistogramGroup group = new ObjectTypesHistogramGroup();
            List<ObjectTypesHistogramCategory> categoriesToAddToGroup = new List<ObjectTypesHistogramCategory>();

            if (entitySubtypesWithInstatnces.Count > 0)
            {
                ObjectTypesHistogramCategory entitiesCategory = GetCategoryByObjectTypesDictionary(entitySubtypesWithInstatnces, GetCurrentOntology().GetEntityTypeURI());
                entitiesCategory.OrderBy = HistogramCategoriesOrderBy;
                categoriesToAddToGroup.Add(entitiesCategory);
            }
            if (eventSubtypesWithInstatnces.Count > 0)
            {
                ObjectTypesHistogramCategory eventsCategoty = GetCategoryByObjectTypesDictionary(eventSubtypesWithInstatnces, GetCurrentOntology().GetEventTypeURI());
                eventsCategoty.OrderBy = HistogramCategoriesOrderBy;
                categoriesToAddToGroup.Add(eventsCategoty);
            }
            if (documentSubtypesWithInstatnces.Count > 0)
            {
                ObjectTypesHistogramCategory documentsCategory = GetCategoryByObjectTypesDictionary(documentSubtypesWithInstatnces, GetCurrentOntology().GetDocumentTypeURI());
                documentsCategory.OrderBy = HistogramCategoriesOrderBy;
                categoriesToAddToGroup.Add(documentsCategory);
            }

            if (categoriesToAddToGroup.Count > 0)
            {
                group.AddSubCategories(categoriesToAddToGroup);
                // در صورت ثبت شدن درخواست جدید، از ایجاد تغییر در هیستوگرام جلوگیری می شود
                if (!IsLastRequest(requestGuid))
                {
                    return;
                    //throw new NewerShowObjectsRequestRegisteredException();
                }
                if (isLastRequestCanceledByUser)
                {
                    return;
                    //throw new OperationCanceledException();
                }

                mainHistogramViewer.FillMainHistogram(group);
            }
        }

        /// <summary>
        /// </summary>
        /// <returns>if no Object Type in Dictionary returns 'null'</returns>
        private ObjectTypesHistogramCategory GetCategoryByObjectTypesDictionary(Dictionary<string, List<KWObject>> categorySubtypesWithInstatnces, string categoryTypeUri)
        {
            if (categorySubtypesWithInstatnces.Count > 0)
            {
                string categoryTitle = "";
                string allInstancesOfCategoryPairTitle = "";
                if (categoryTypeUri == GetCurrentOntology().GetEntityTypeURI())
                {
                    categoryTitle = Properties.Resources.Entity_Types;
                    allInstancesOfCategoryPairTitle = Properties.Resources.All_Entities;
                }
                else if (categoryTypeUri == GetCurrentOntology().GetEventTypeURI())
                {
                    categoryTitle = Properties.Resources.Event_Types;
                    allInstancesOfCategoryPairTitle = Properties.Resources.All_Evevnts;
                }
                else if (categoryTypeUri == GetCurrentOntology().GetDocumentTypeURI())
                {
                    categoryTitle = Properties.Resources.Document_Types;
                    allInstancesOfCategoryPairTitle = Properties.Resources.All_Documents;
                }
                else
                    throw new ArgumentException("categoryTypeUri");

                ObjectTypesHistogramCategory category = new ObjectTypesHistogramCategory(categoryTitle);
                // Add total instances statistics to histogram (like "All Entities", etc.)
                List<KWObject> allInstancesOfCategory = new List<KWObject>();
                foreach (KeyValuePair<string, List<KWObject>> item in categorySubtypesWithInstatnces)
                    allInstancesOfCategory.AddRange(item.Value);

                List<ObjectTypesHistogramTypeCountPair> objectTypeList = new List<ObjectTypesHistogramTypeCountPair>(categorySubtypesWithInstatnces.Count + 1);
                objectTypeList.Add(new ObjectTypesHistogramTypeCountPair
                    (categoryTypeUri, allInstancesOfCategory, allInstancesOfCategoryPairTitle, true));

                // Add same-type instances type-by-type
                foreach (KeyValuePair<string, List<KWObject>> item in categorySubtypesWithInstatnces)
                    objectTypeList.Add(new ObjectTypesHistogramTypeCountPair(item.Key, item.Value));

                category.AddObjectTypeCollection(objectTypeList);

                category.OrderBy = HistogramCategoriesOrderBy;
                return category;
            }
            else
                return null;
        }

        private void BreakdownObjectsToTypeDictionaries(IEnumerable<KWObject> objectsToShow, out Dictionary<string, List<KWObject>> entitySubtypesWithInstatnces, out Dictionary<string, List<KWObject>> eventSubtypesWithInstatnces, out Dictionary<string, List<KWObject>> documentSubtypesWithInstatnces)
        {
            entitySubtypesWithInstatnces = new Dictionary<string, List<KWObject>>();
            eventSubtypesWithInstatnces = new Dictionary<string, List<KWObject>>();
            documentSubtypesWithInstatnces = new Dictionary<string, List<KWObject>>();

            foreach (KWObject currentObject in objectsToShow)
                if (GetCurrentOntology().IsEntity(currentObject.TypeURI))
                {
                    if (!entitySubtypesWithInstatnces.ContainsKey(currentObject.TypeURI))
                        entitySubtypesWithInstatnces.Add(currentObject.TypeURI, new List<KWObject>());
                    entitySubtypesWithInstatnces[currentObject.TypeURI].Add(currentObject);
                }
                else if (GetCurrentOntology().IsEvent(currentObject.TypeURI))
                {
                    if (!eventSubtypesWithInstatnces.ContainsKey(currentObject.TypeURI))
                        eventSubtypesWithInstatnces.Add(currentObject.TypeURI, new List<KWObject>());
                    eventSubtypesWithInstatnces[currentObject.TypeURI].Add(currentObject);
                }
                else if (GetCurrentOntology().IsDocument(currentObject.TypeURI))
                {
                    if (!documentSubtypesWithInstatnces.ContainsKey(currentObject.TypeURI))
                        documentSubtypesWithInstatnces.Add(currentObject.TypeURI, new List<KWObject>());
                    documentSubtypesWithInstatnces[currentObject.TypeURI].Add(currentObject);
                }
        }

        public void ClearHistogram()
        {
            Guid currentShowObjectsRequestGuid = AssignGuidToRequest();
#if DEBUG
            Debug.WriteLine("Histogram::ClearContent | Histogram clear request received; GUID: " + currentShowObjectsRequestGuid.ToString());
#endif
            mainHistogramViewer.Clear();
            mainHistogramViewer.HideWaiting();
        }

        private Guid AssignGuidToRequest()
        {
            Guid currentShowObjectsRequestGuid = Guid.NewGuid();
            lastShowObjectsRequestGuid = currentShowObjectsRequestGuid;
            return currentShowObjectsRequestGuid;
        }

        private void mainHistogramViewer_SelectionChanged(object sender, IEnumerable<HistogramItem> e)
        {
            List<KWObject> result = new List<KWObject>();

            foreach (HistogramItem item in e)
            {
                if (item.RelatedElement is ObjectTypesHistogramTypeCountPair)
                {
                    result.AddRange((item.RelatedElement as ObjectTypesHistogramTypeCountPair).ObjectsWithType);
                }
                else if (item.RelatedElement is ObjectPropertiesHistogramValueCountPair)
                {
                    result.AddRange((item.RelatedElement as ObjectPropertiesHistogramValueCountPair).ObjectsWithPerpertiesOfTypeAndValue);
                }

                result = result.Distinct().ToList();
            }

            OnSelectionChanged(result);
        }

        public HistogramPropertyNodeOrderBy HistogramCategoriesOrderBy
        {
            get { return (HistogramPropertyNodeOrderBy)GetValue(PropertyOrderByProperty); }
            set { SetValue(PropertyOrderByProperty, value); }
        }

        // Using a DependencyProperty as the backing store for HistogramCategoriesOrderBy.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty PropertyOrderByProperty =
            DependencyProperty.Register("HistogramCategoriesOrderBy", typeof(HistogramPropertyNodeOrderBy), typeof(HistogramControl), new PropertyMetadata(HistogramPropertyNodeOrderBy.Count, OnSetPropertyOrderByChanged));

        private static void OnSetPropertyOrderByChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            HistogramControl hc = d as HistogramControl;
            hc.OnSetPropertyOrderByChanged(e);
        }

        private void OnSetPropertyOrderByChanged(DependencyPropertyChangedEventArgs e)
        {
            ShowObjectsHistogramAsync(ObjectsToShow);
        }

        private void mainHistogramViewer_SnapshotTaken(object sender, TakeSnapshotEventArgs e)
        {
            OnSnapshotTaken(e);
        }

        public void Reset()
        {
            mainHistogramViewer.Reset();
        }
    }
}