using GPAS.Logger;
using GPAS.Workspace.Entities;
using GPAS.Graph.GraphViewer.Foundations;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Controls.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GPAS.Workspace.Presentation.Controls
{
    public partial class GraphControl
    {
        public GraphArrangment GetGraphArrangment()
        {
            GraphArrangment result = new GraphArrangment();

            foreach (var item in GetShowingObjects())
            {
                Vertex itemVertex = GetRelatedVertex(item);
                Point itemPosition = graphviewerMain.GetVertexPosition(itemVertex);
                if (item is GroupMasterKWObject)
                {
                    var node = new GraphArrangment.ObjectNode()
                    {
                        NotResolvedObjectId = item.ID,
                        Position = new GraphArrangment.Point()
                        {
                            X = itemPosition.X,
                            Y = itemPosition.Y
                        },
                        IsVisible = (graphviewerMain.GetVertexVisiblity(itemVertex) == Visibility.Visible),
                        IsMasterOfGroup = true,
                        IsMasterOfACollapsedGroup = IsGroupInCollapsedViewing(item as GroupMasterKWObject)
                    };
                    result.Objects.Add(node);

                    if (IsGroupInCollapsedViewing(item as GroupMasterKWObject))
                    {
                        var subgroupsPositions = new GraphArrangment.CollapsedGroupMembersPositionRelaterdToMasterInExpandedMode()
                        {
                            NotResolvedGroupMasterObjectId = item.ID,
                            GroupMembersRelativePositionsByObjectId = new Dictionary<long, GraphArrangment.Point>()
                        };
                        foreach (KWObject subGroupItem in (item as GroupMasterKWObject).GetSubGroupObjects())
                        {
                            Point itemRelatedPosition = graphviewerMain
                                .GetExpandModeSubGroupPositionRelatedToMasterOfCollapsedGroup
                                    (GetRelatedVertex(item) as GroupMasterVertex, GetRelatedVertex(subGroupItem));
                            subgroupsPositions.GroupMembersRelativePositionsByObjectId.Add
                                (subGroupItem.ID
                                , new GraphArrangment.Point() { X = itemRelatedPosition.X, Y = itemRelatedPosition.Y });
                        }
                        result.CollapsedGroupsRelativePoistions.Add(subgroupsPositions);
                    }
                }
                else
                {
                    var node = new GraphArrangment.ObjectNode()
                    {
                        NotResolvedObjectId = item.ID,
                        Position = new GraphArrangment.Point()
                        {
                            X = itemPosition.X,
                            Y = itemPosition.Y
                        },
                        IsVisible = (graphviewerMain.GetVertexVisiblity(itemVertex) == Visibility.Visible),
                        IsMasterOfGroup = false,
                        IsMasterOfACollapsedGroup = false
                    };
                    result.Objects.Add(node);
                }
            }
            foreach (var item in GetShowingLinks()
                    .Where(l => l.TypeURI != OntologyProvider.GetOntology().DefaultGroupRelationshipType()))
                if (item is RelationshipBasedKWLink)
                    result.RelationshipBasedLinksExceptGroupInnerLinks.Add(new GraphArrangment.RelationshipBasedLink()
                    {
                        RelationshipId = (item as RelationshipBasedKWLink).Relationship.ID,
                        SourceObjectId = item.Source.ID,
                        TargetObjectId = item.Target.ID
                    });
                else if (item is EventBasedKWLink)
                    result.EventBasedLinks.Add(new GraphArrangment.EventBasedLink()
                    {
                        intermediaryEventId = (item as EventBasedKWLink).IntermediaryEvent.ID,
                        SourceObjectId = item.Source.ID,
                        TargetObjectId = item.Target.ID,
                        FirstRelationshipId = (item as EventBasedKWLink).FirstRelationship.ID,
                        SecondRelationshipId = (item as EventBasedKWLink).SecondRelationship.ID
                    });
                else if (item is PropertyBasedKWLink)
                    result.PropertyBasedLinks.Add(new GraphArrangment.PropertyBasedLink()
                    {
                        SourceObjectId = item.Source.ID,
                        TargetObjectId = item.Target.ID,
                        SamePropertyTypeUri = (item as PropertyBasedKWLink).SamePropertyTypeUri,
                        SamePropertyValue = (item as PropertyBasedKWLink).SamePropertyValue
                    });
                else if (item is NotLoadedRelationshipBasedKWLink)
                    result.NotLoadedRelationshipBasedLinks.Add(new GraphArrangment.NotLoadedRelationshipBasedLink()
                    {
                        SourceObjectId = item.Source.ID,
                        TargetObjectId = item.Target.ID,
                        RelationshipIDs = ((NotLoadedRelationshipBasedKWLink)item).IntermediaryRelationshipIDs.ToArray()
                    });
                else if (item is NotLoadedEventBasedKWLink)
                    result.NotLoadedEventBasedLinks.Add(new GraphArrangment.NotLoadedEventBasedLink()
                    {
                        SourceObjectId = item.Source.ID,
                        TargetObjectId = item.Target.ID,
                        RelationshipIdPairs = ((NotLoadedEventBasedKWLink)item).IntermediaryLinksRelationshipIDs
                            .Select(p => new GraphArrangment.NotLoadedEventBasedLinkInnerRelIdPair()
                            {
                                FirstRelID = p.FirstRelationshipID,
                                SecondRelID = p.SecondRelationshipID
                            })
                            .ToArray()
                    });
                else
                    throw new InvalidOperationException("Unable to serialize Graph because of unknown Links");
            return result;
        }

        /// <summary>
        /// گراف در حال نمایش را پاک و گراف را براساس رشته ذخیره سازی چیدمان گراف به نمایش در می آورد
        /// </summary>
        /// <param name="graphArrangmentXmlStringToShowGraph">رشته ایکس ام ال نشاندهنده چیدمان گراف برای اعمال در گراف</param>
        public async Task ShowGraphByArrangmentAsync(GraphArrangment graphArrangmentToShowGraph)
        {
            GraphArrangment arrangment = graphArrangmentToShowGraph;
            // بازیابی اشیا و رابطه ها مورد نیاز برای رسم گراف، از مخزن داده ها
            GraphContent graphContentToShow = null;
            try
            {
                graphContentToShow = await RetriveObjectsAndRelationshipsNeededForArrangmentAsync(arrangment);
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show(string.Format("{0}\n\n{1}", Properties.Resources.Invalid_Server_Response, ex.Message));
            }
            // پاک کردن محتوای گراف
            ClearGraph();
            // افزودن اشیا و روابط به گراف
            try
            {
                List<ObjectShowMetadata> objectShowMetadatas = new List<ObjectShowMetadata>(arrangment.Objects.Count);
                // نمایش اشیائی که میزبان گروه نیستند
                foreach (var item in graphContentToShow.NotResolvedObjectsToShowOnGraph
                    .Where(i => !(i is GroupMasterKWObject)))
                {
                    // استخراج جزئیات نمایش شئ روی گراف
                    GraphArrangment.ObjectNode objectShowingInfo
                        = arrangment.Objects.Single(e => e.NotResolvedObjectId == item.ID);
                    objectShowMetadatas.Add(new ObjectShowMetadata()
                    {
                        ObjectToShow = ObjectManager.GetResolveLeaf(item),
                        NonDefaultPositionX = objectShowingInfo.Position.X,
                        NonDefaultPositionY = objectShowingInfo.Position.Y,
                        NewVisiblity = objectShowingInfo.IsVisible ? Visibility.Visible : Visibility.Hidden
                    });
                }
                ShowObjects(objectShowMetadatas);

                // نمایش اشیائی که میزبان گروهی بسته شده هستند:
                List<CollapsedGroupNode> rootCollapsedGroups = GetRootCollapsedNodesFromArrangment(arrangment, graphContentToShow.NotResolvedObjectsToShowOnGraph);

                // نمایش گروه ها و زیرگروه های مربوط به ریشه گروه های جمع شده تودرتو در حالت باز شده
                foreach (var item in rootCollapsedGroups)
                {
                    // تعیین موقعیت نمایش گره های میزبان گروه جمع شده ریشه (که سلسله مراتبی از گروه های جمع شده را در برمی گیرند)
                    var position = arrangment.Objects.Single(e => e.NotResolvedObjectId == item.NotResolvedGroupMasterObject.ID).Position;
                    item.GroupMasterMostlyExpandedPosition = new Point(position.X, position.Y);
                    // نمایش گروه و زیرگروه های آن در حالت باز شده
                    SetCollapsedGroupSubNodesHierarchyMostlyExpandedPositions(item, arrangment, graphContentToShow.NotResolvedObjectsToShowOnGraph);
                }

                // نمایش اشیائی که میزبان گروه های غیر بسته هستند
                foreach (var item in graphContentToShow.NotResolvedObjectsToShowOnGraph
                    .Where(o => (!arrangment.Objects
                        .Single(e => e.NotResolvedObjectId == o.ID).IsMasterOfACollapsedGroup))
                    .Where(i => i is GroupMasterKWObject)
                    .Select(i => i as GroupMasterKWObject))
                {
                    ShowExpandedGroup(item, arrangment);
                }

                // جمع کردن ریشه گره هایی که می بایست تودرتو به صورت جمع شده نمایش داده شوند
                foreach (var item in rootCollapsedGroups)
                {
                    graphviewerMain.CollapseGroup
                        (GetRelatedVertex
                            (ObjectManager.GetResolveLeaf(item.NotResolvedGroupMasterObject))
                                as GroupMasterVertex);
                }
                // نمایش لینک ها روی گراف
                ShowLinks(graphContentToShow.LinksToShowOnGraph);

                graphviewerMain.UpdateLayout();
                graphviewerMain.ZoomToFill();
                DeselectAllLinks();
            }
            catch
            {
                ClearGraph();
                throw;
            }
        }

        // TODO: Clean
        private List<CollapsedGroupNode> GetRootCollapsedNodesFromArrangment(GraphArrangment arrangment, List<KWObject> arrangmentNotResolvedObjects)
        {
            if (arrangmentNotResolvedObjects == null)
                throw new ArgumentNullException("arrangmentObjects");

            // استخراج سلسله مراتب گره های جمع شده روی گراف
            List<CollapsedGroupNode> collapsedGroupsList = new List<CollapsedGroupNode>();
            foreach (var notResolvedObj in arrangmentNotResolvedObjects
                .Where(o => arrangment.Objects
                    .Single(e => e.NotResolvedObjectId == o.ID)
                    .IsMasterOfACollapsedGroup)
                .Select(o => o as GroupMasterKWObject))
            {
                CollapsedGroupNode itemNode = new CollapsedGroupNode(notResolvedObj);
                collapsedGroupsList.Add(itemNode);
                // استخراج شناسه گره بالادست (پدر) در گروه بندی بسته شده تودرتو، در صورت وجود
                try
                {
                    long itemSuperGroupId = arrangment.CollapsedGroupsRelativePoistions.Single(
                        p => p.GroupMembersRelativePositionsByObjectId.ContainsKey(notResolvedObj.ID)).NotResolvedGroupMasterObjectId;
                    // تلاش برای یافتن گره بالادست (پدر) گره جاری از بین گره هایی که قبلا در این چرخه به
                    // لیست گروه های پیمایش شده اضافه شده اند

                    // و افزودن گره جاری به عنوان فرزند آن
                    collapsedGroupsList.Single(
                        n => n.NotResolvedGroupMasterObject.ID == itemSuperGroupId).CollapsedGroupSubNodes.Add(itemNode);
                }
                catch (InvalidOperationException)
                { }
                // استخراج شناسه گره های فرودست مرتبه یک (فرزند) در گروه بندی
                // بسته شده تودرتو، که قبلا در این چرخهبه لیست اضافه شده اند
                try
                {
                    List<long> itemSubGroupsId = arrangment.CollapsedGroupsRelativePoistions.Single(
                        p => p.NotResolvedGroupMasterObjectId == notResolvedObj.ID).GroupMembersRelativePositionsByObjectId.Keys.ToList();
                    // تلاش برای یافتن گره فرودست مرتبه یک (فرزند) گره جاری از بین گره هایی که قبلا
                    // در این چرخه به لیست گروه ها اضافه شده اند
                    foreach (var collapsedGroup in collapsedGroupsList)
                        if (itemSubGroupsId.Contains(collapsedGroup.NotResolvedGroupMasterObject.ID))
                            itemNode.CollapsedGroupSubNodes.Add(collapsedGroup);
                }
                catch (InvalidOperationException)
                { }
            }
            // یافتن سرشاخه های گروه های جمع شده تودرتو
            List<CollapsedGroupNode> rootCollapsedGroups = new List<CollapsedGroupNode>();
            foreach (var item in collapsedGroupsList)
                if (!IsCollapsedGroupInGrandChildrenOfCollapsedGroups(item, collapsedGroupsList.Where(
                        g => g.NotResolvedGroupMasterObject.ID != item.NotResolvedGroupMasterObject.ID)))
                    rootCollapsedGroups.Add(item);

            return rootCollapsedGroups;
        }

        private async Task<GraphContent> RetriveObjectsAndRelationshipsNeededForArrangmentAsync(GraphArrangment arrangment)
        {
            List<long> objectIdsToRetrive = new List<long>();
            List<long> relationshipIdsToRetrive = new List<long>();
            foreach (var item in arrangment.Objects)
                if (!objectIdsToRetrive.Contains(item.NotResolvedObjectId))
                    objectIdsToRetrive.Add(item.NotResolvedObjectId);
            foreach (var item in arrangment.CollapsedGroupsRelativePoistions)
                if (!objectIdsToRetrive.Contains(item.NotResolvedGroupMasterObjectId))
                    objectIdsToRetrive.Add(item.NotResolvedGroupMasterObjectId);
            foreach (var item in arrangment.RelationshipBasedLinksExceptGroupInnerLinks)
                if (!relationshipIdsToRetrive.Contains(item.RelationshipId))
                    relationshipIdsToRetrive.Add(item.RelationshipId);
            foreach (var item in arrangment.EventBasedLinks)
            {
                if (!objectIdsToRetrive.Contains(item.intermediaryEventId))
                    objectIdsToRetrive.Add(item.intermediaryEventId);
                if (!relationshipIdsToRetrive.Contains(item.FirstRelationshipId))
                    relationshipIdsToRetrive.Add(item.FirstRelationshipId);
                if (!relationshipIdsToRetrive.Contains(item.SecondRelationshipId))
                    relationshipIdsToRetrive.Add(item.SecondRelationshipId);
            }

            Dictionary<long, KWObject> retrivedNotResolvedObjectsById = new Dictionary<long, KWObject>();
            Dictionary<long, RelationshipBasedKWLink> retrivedRelationshipsById = new Dictionary<long, RelationshipBasedKWLink>();
            // نکته: اشیا یا روابطی که به هر دلیلی در سرور یافت نشوند، در لیست بازگشتی از سرور نخواهند بود
            // نکته: در صورت تکرار یک شناسه در لیست درخواست اشیا/روابط ارسالی به سرور، لیست نتیجه تکرار نخواهد داشت
            if (objectIdsToRetrive.Count > 0)
                foreach (KWObject obj in await ObjectManager.RetriveObjectsAsync(objectIdsToRetrive, false))
                    retrivedNotResolvedObjectsById.Add(obj.ID, obj);

            if (relationshipIdsToRetrive.Count > 0)
                foreach (var item in await LinkManager.RetrieveRelationshipBaseLinksAsync(relationshipIdsToRetrive))
                    retrivedRelationshipsById.Add(item.Relationship.ID, item);

            List<KWPropertiesPerPropertyBasedLink> arrangmentProperties
                = await RetrieveArrangementProperties(arrangment.PropertyBasedLinks, retrivedNotResolvedObjectsById);

            #region اعتبارسنجی نوع های اشیا، روابط و ویژگی‌های بازگشتی براساس هستان‌شناسی موجود
            //foreach (var objectType in retrivedObjectsById.Values
            //        .Select(retrivedObject => retrivedObject.TypeURI)
            //        .Distinct())
            //    if (!OntologyProvider.GetOntology().IsEntity(objectType) &&
            //        !OntologyProvider.GetOntology().IsEvent(objectType) &&
            //        !OntologyProvider.GetOntology().IsDocument(objectType))
            //        throw new Exception("At least one Object in new retrived Graph has invalid Type according to current Ontology;\nRestart the application to load the most new stored Ontology\nDetected invalid Object Type: " + objectType);
            //foreach (var relationshtpType in retrivedRelationshipsById.Values
            //        .Select(retrivedRelationship => retrivedRelationship.TypeURI)
            //        .Distinct())
            //    if (!OntologyProvider.GetOntology().IsRelationship(relationshtpType))
            //        throw new Exception("At least one Relationship in new retrived Graph has invalid Type according to current Ontology;\nRestart the application to load the most new stored Ontology\nDetected invalid Relationship Type: " + relationshtpType);
            //foreach (var propertyType in retrivedPropertiesById.Values
            //        .Select(retrivedProperty => retrivedProperty.TypeURI)
            //        .Distinct())
            //    if (!OntologyProvider.GetOntology().IsProperty(propertyType))
            //        throw new Exception("At least one Property in new retrived Graph has invalid Type according to current Ontology;\nRestart the application to load the most new stored Ontology\nDetected invalid Property Type: " + propertyType);
            #endregion

            // تبدیل اشیا و رابطه های دریافتی (خام) به اشیا قابل استفاده در گراف
            GraphContent graphContentToShow = new GraphContent();
            foreach (var item in arrangment.Objects)
            {
                KWObject itemObject;
                if (retrivedNotResolvedObjectsById.TryGetValue(item.NotResolvedObjectId, out itemObject))
                    graphContentToShow.NotResolvedObjectsToShowOnGraph.Add(itemObject);
            }
            foreach (var item in arrangment.RelationshipBasedLinksExceptGroupInnerLinks)
            {
                RelationshipBasedKWLink itemLink;
                // Get retrived Relationship
                if (retrivedRelationshipsById.TryGetValue(item.RelationshipId, out itemLink))
                {
                    graphContentToShow.LinksToShowOnGraph.Add(itemLink);
                }
            }
            foreach (var item in arrangment.EventBasedLinks)
            {
                RelationshipBasedKWLink firstRelationship, secondRelationship;
                // Get retrived Event-Based-Links by inner Relationships
                if (retrivedRelationshipsById.TryGetValue(item.FirstRelationshipId, out firstRelationship)
                    && retrivedRelationshipsById.TryGetValue(item.SecondRelationshipId, out secondRelationship))
                {
                    graphContentToShow.LinksToShowOnGraph.Add(LinkManager.GetEventBaseKWLinkFromLinkInnerRelationships(firstRelationship, secondRelationship));
                }
            }
            foreach (var item in arrangmentProperties)
            {
                if (item.SourceProperty != null
                    && item.TargetProperty != null)
                {
                    graphContentToShow.LinksToShowOnGraph.Add(LinkManager.GetPropertyBasedKWLinkFromLinkInnerProperties(item.SourceProperty, item.TargetProperty));
                }
            }
            foreach (var item in arrangment.NotLoadedRelationshipBasedLinks)
            {
                graphContentToShow.LinksToShowOnGraph.Add(new NotLoadedRelationshipBasedKWLink()
                {
                    Source = retrivedNotResolvedObjectsById[item.SourceObjectId],
                    Target = retrivedNotResolvedObjectsById[item.TargetObjectId],
                    IntermediaryRelationshipIDs = new HashSet<long>(item.RelationshipIDs)
                });
            }
            foreach (var item in arrangment.NotLoadedEventBasedLinks)
            {
                graphContentToShow.LinksToShowOnGraph.Add(new NotLoadedEventBasedKWLink()
                {
                    Source = retrivedNotResolvedObjectsById[item.SourceObjectId],
                    Target = retrivedNotResolvedObjectsById[item.TargetObjectId],
                    IntermediaryLinksRelationshipIDs = new HashSet<Entities.SearchAroundResult.EventBasedResultInnerRelationships>
                        (item.RelationshipIdPairs
                            .Select(p => new Entities.SearchAroundResult.EventBasedResultInnerRelationships()
                            {
                                FirstRelationshipID = p.FirstRelID,
                                SecondRelationshipID = p.SecondRelID
                            }))
                });
            }

            return graphContentToShow;
        }

        private async Task<List<KWPropertiesPerPropertyBasedLink>> RetrieveArrangementProperties
            (List<GraphArrangment.PropertyBasedLink> propertyBasedLinks
            , Dictionary<long, KWObject> retrivedNotResolvedObjectsByID)
        {
            List<KWPropertiesPerPropertyBasedLink> arrangmentProperties = new List<KWPropertiesPerPropertyBasedLink>();
            if (propertyBasedLinks.Count == 0)
            {
                return arrangmentProperties;
            }

            foreach (var item in propertyBasedLinks)
            {
                arrangmentProperties.Add(new KWPropertiesPerPropertyBasedLink()
                {
                    ArrangmentLink = item,
                    SourceProperty = null,
                    TargetProperty = null
                });
            }

            IEnumerable<IGrouping<string, KWPropertiesPerPropertyBasedLink>> metadatasWithSamePropertyType
                = arrangmentProperties.GroupBy(pp => pp.ArrangmentLink.SamePropertyTypeUri);
            foreach (var samePropertyTypeMetadatas in metadatasWithSamePropertyType)
            {
                string samePropertyTypeUri = samePropertyTypeMetadatas.Key;
                HashSet<KWObject> samePropertyTypeObjects = new HashSet<KWObject>();
                foreach (KWPropertiesPerPropertyBasedLink samePropertyTypeMetadata in samePropertyTypeMetadatas)
                {
                    KWObject source = ObjectManager.GetResolveLeaf(retrivedNotResolvedObjectsByID[samePropertyTypeMetadata.ArrangmentLink.SourceObjectId]);
                    if (!samePropertyTypeObjects.Contains(source))
                    {
                        samePropertyTypeObjects.Add(source);
                    }
                    KWObject target = ObjectManager.GetResolveLeaf(retrivedNotResolvedObjectsByID[samePropertyTypeMetadata.ArrangmentLink.TargetObjectId]);
                    if (!samePropertyTypeObjects.Contains(target))
                    {
                        samePropertyTypeObjects.Add(target);
                    }
                }

                IEnumerable<KWProperty> sameTypeRetrievedProperties
                    = await PropertyManager.GetPropertiesOfObjectsAsync
                        (samePropertyTypeObjects, new string[] { samePropertyTypeUri });

                foreach (KWPropertiesPerPropertyBasedLink samePropMetadata in samePropertyTypeMetadatas)
                {
                    foreach (var prop in sameTypeRetrievedProperties)
                    {
                        if (prop.TypeURI.Equals(samePropMetadata.ArrangmentLink.SamePropertyTypeUri)
                            && prop.Value.Equals(samePropMetadata.ArrangmentLink.SamePropertyValue))
                        {
                            // شناسه‌ی شئ برای احتمال ادغام شدن شئ پس از ذخیره‌سازی گراف، از
                            // میان اشیاء بازیابی شده بررسی می‌شود
                            if (prop.Owner.ID.Equals
                                    (ObjectManager.GetResolveLeaf(retrivedNotResolvedObjectsByID[samePropMetadata.ArrangmentLink.SourceObjectId]).ID))
                            {
                                samePropMetadata.SourceProperty = prop;
                            }
                            // احتمال دارد مبدا و مقصد لینک یکی باشند
                            if (prop.Owner.ID.Equals
                                    (ObjectManager.GetResolveLeaf(retrivedNotResolvedObjectsByID[samePropMetadata.ArrangmentLink.TargetObjectId]).ID))
                            {
                                samePropMetadata.TargetProperty = prop;
                            }

                            if (samePropMetadata.SourceProperty != null
                                && samePropMetadata.TargetProperty != null)
                            {
                                break;
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }
            return arrangmentProperties;
        }

        /// <summary>
        /// نمایش یک گروه باز (اکسپند) شده از بین گره های یک چینش بازیابی شده؛
        /// گروه هایی که می خواهیم به صورت جمع شده نمایش داده شوند می بایست از قبل به گراف افزوده شده باشند؛
        /// این عملکرد برای جلوگیری از بروز مشکل، ابتدا زیرگروه ها را -در صورت عدم وجود روی گراف- نمایش می دهد و سپس میزبان گروه را؛
        /// همچنین نمایش گروه های باز تودرتو، از برگ ها داخلی ترین گروه شروع شده به صورت سلسله مراتبی به میزبان این گروه می رسد
        /// </summary>
        /// <param name="arrangment">چینش بازیابی شده ای که گروه (ها می بایست براساس آن روی گراف نمایش داده شوند</param>
        private void ShowExpandedGroup(GroupMasterKWObject masterOfGroupToShow, GraphArrangment arrangment)
        {
            if (masterOfGroupToShow == null)
                throw new ArgumentNullException("masterOfGroupToShow");

            List<ObjectShowMetadata> objectShowMetadatas = new List<ObjectShowMetadata>(masterOfGroupToShow.GetSubGroupObjectsCount + 1);
            // به ازای زیرگروه های واقعی (همه زیرگروه های ثبت شده برای گروه)؛
            foreach (var item in masterOfGroupToShow.GetSubGroupObjects())
            {
                // اگر گرهی معادل زیرگروه وجود نداشته باشد
                if (GetRelatedVertex(item) == null)
                {
                    // تلاش برای استخراج جزئیات نمایش شئ از چینش بازیابی شده گراف و نمایش زیرگروه؛
                    // زیرگروه هایی که خودشان گروه اند و در وضعیت شده، می بایست پیش از این به گراف اضافه
                    // شده باشند تا تداخل ایجاد نشود
                    try
                    {
                        GraphArrangment.ObjectNode objectShowingInfo = arrangment.Objects.Single(e => e.NotResolvedObjectId == item.ID);
                        if (item is GroupMasterKWObject)
                            //گروه های جمع شده قبلا نمایش داده شده اند و اینجا فقط گره های باز به گراف افزوده می شوند
                            ShowExpandedGroup(item as GroupMasterKWObject, arrangment);
                        else
                            objectShowMetadatas.Add(new ObjectShowMetadata()
                            {
                                ObjectToShow = ObjectManager.GetResolveLeaf(item),
                                NonDefaultPositionX = objectShowingInfo.Position.X,
                                NonDefaultPositionY = objectShowingInfo.Position.Y,
                                NewVisiblity = objectShowingInfo.IsVisible ? Visibility.Visible : Visibility.Hidden
                            });
                    }
                    // در صورتی که شئ زیرگروه در چینش موجود نباشد،
                    // (یعنی زیرگروه بعد از ذخیره سازی چینش اضافه شده و یا پس از نمایش زیرگروه روی گراف حذف شده باشد)
                    // نمایش آن روی گراف صرفنظر می شود
                    catch (InvalidOperationException)
                    { }
                }
            }
            // استخراج جزئیات نمایش شئ (میزبان گروه) روی گراف
            GraphArrangment.ObjectNode masterOfGroupShowingInfo = arrangment.Objects.Single(e => e.NotResolvedObjectId == masterOfGroupToShow.ID);
            // و نمایش آن بدون در خواست اجبار نمایش کل زیرگروه های واقعی (ذخیره شده
            // در بانک)، بدین ترتیب فقط گره ها موجود در گراف بازیابی شده نمایش داده می شوند
            objectShowMetadatas.Add(new ObjectShowMetadata()
            {
                ObjectToShow = ObjectManager.GetResolveLeaf(masterOfGroupToShow),
                NonDefaultPositionX = masterOfGroupShowingInfo.Position.X,
                NonDefaultPositionY = masterOfGroupShowingInfo.Position.Y,
                NewVisiblity = masterOfGroupShowingInfo.IsVisible ? Visibility.Visible : Visibility.Hidden,
                ForceShowSubGroups = false
            });
            ShowObjects(objectShowMetadatas);
        }

        /// <summary>
        /// بررسی مجموعه ای از گره های گروه جمع شده برای یافتن یک گره دیگر بین آن ها یا نوادگان شان
        /// </summary>
        /// <returns>در صورت وجود گره در لیست داده شده یا نوادگان آن مقدار صحیح را برمی گرداند</returns>
        private bool IsCollapsedGroupInGrandChildrenOfCollapsedGroups(CollapsedGroupNode checkingNode, IEnumerable<CollapsedGroupNode> nodesToCheckGrandChildren)
        {
            if (checkingNode == null)
                throw new ArgumentNullException("checkingNode");
            if (nodesToCheckGrandChildren == null)
                throw new ArgumentNullException("nodesToCheckGrandChildren");

            foreach (var item in nodesToCheckGrandChildren)
            {
                // در صورتی که این گره از لیست در حال چک همان گره مورد جستجو باشد
                if (item == checkingNode
                    // یا بکی از نوادگان آن همان گره مورد جستجو باشد
                    || IsCollapsedGroupInGrandChildrenOfCollapsedGroups(checkingNode, item.CollapsedGroupSubNodes))
                {
                    // مقدار صحیح برگدانده می شود
                    return true;
                }
            }
            // اگر لیست گره های برای چک، خالی باشد یا پیمایش فوق باعث بازگرداندن مقدار
            // صحیح نشود، مقدار غلط به نشانه عدم وجود گره در نوادگان لیست برگردانده می شود
            return false;
        }

        /// <summary>
        /// موقعیت زیرگروه های یک گروه جمع شده را براساس موقعیت تعیین شده برای
        /// میزبان گروه تعیین و گروه -شامل میزبان و زیرگروه ها و
        /// زیر گروه های تودرتو- را در حالت باز شده به نمایش در می آورد؛
        /// موقعیت می بایست برای گره ورودی تعیین شده باشد
        /// </summary>
        /// <param name="groupMasterNode">گره جمع شده میزبان گروه که می خواهیم با زیرگروه هایش نمایش داده شوند</param>
        /// <param name="arrangment">کل تعاریف چینش های درنظر گرفته شده برای رسم کل گراف</param>
        /// <param name="objectsToShowOnGraph">لیست اشیائی که محتوای گراف براساس آن ها نمایش داده خواهد شد</param>
        private void SetCollapsedGroupSubNodesHierarchyMostlyExpandedPositions(CollapsedGroupNode groupMasterNode, GraphArrangment arrangment, List<KWObject> objectsToShowOnGraph)
        {
            if (groupMasterNode == null)
                throw new ArgumentNullException("groupMasterNode");
            if (objectsToShowOnGraph == null)
                throw new ArgumentNullException("objectsToShowOnGraph");

            List<ObjectShowMetadata> objectShowMetadatas = new List<ObjectShowMetadata>();
            // استخراج زیرگروه ها و موقعیت شان نسبته به میزبان گره گروه
            var subGroupsPositionsById = arrangment.CollapsedGroupsRelativePoistions.Single(
                    e => e.NotResolvedGroupMasterObjectId == groupMasterNode.NotResolvedGroupMasterObject.ID).GroupMembersRelativePositionsByObjectId;
            // نمایش گره های زیرگروه غیر جمع شده، براساس موقعیت گره میزبان گروه
            foreach (var item in (groupMasterNode.NotResolvedGroupMasterObject as GroupMasterKWObject).GetSubGroupObjects()
                .Where(o => !(o is GroupMasterKWObject)))
            {
                GraphArrangment.Point itemRawRelativePosition = subGroupsPositionsById.Single(
                       s => s.Key == item.ID).Value;
                Point itemRelativePosition = new Point(itemRawRelativePosition.X, itemRawRelativePosition.Y);
                objectShowMetadatas.Add(new ObjectShowMetadata()
                {
                    ObjectToShow = ObjectManager.GetResolveLeaf(item),
                    NonDefaultPositionX = itemRelativePosition.X + groupMasterNode.GroupMasterMostlyExpandedPosition.X,
                    NonDefaultPositionY = itemRelativePosition.Y + groupMasterNode.GroupMasterMostlyExpandedPosition.Y,
                    NewVisiblity = Visibility.Visible,
                    ForceShowSubGroups = false
                });
            }
            // به ازای هر یک از زیرگروه هایی که خودشان میزبان گروه و در وضعیت جمع شده
            // هستند، همین عملکرد برای نمایش گروه صدا زده می شود
            foreach (var item in groupMasterNode.CollapsedGroupSubNodes)
            {
                GraphArrangment.Point itemRawRelativePosition = subGroupsPositionsById.Single(
                    n => n.Key == item.NotResolvedGroupMasterObject.ID).Value;
                Point itemRelativePosition = new Point(itemRawRelativePosition.X, itemRawRelativePosition.Y);
                item.GroupMasterMostlyExpandedPosition = new Point(groupMasterNode.GroupMasterMostlyExpandedPosition.X + itemRelativePosition.X, groupMasterNode.GroupMasterMostlyExpandedPosition.Y + itemRelativePosition.Y);
                if (arrangment.CollapsedGroupsRelativePoistions.Select(e => e.NotResolvedGroupMasterObjectId == item.NotResolvedGroupMasterObject.ID).Count() > 0)
                    SetCollapsedGroupSubNodesHierarchyMostlyExpandedPositions(item, arrangment, objectsToShowOnGraph);
            }
            // نمایش گره در موقعیت تعیین شده
            objectShowMetadatas.Add(new ObjectShowMetadata()
            {
                ObjectToShow = ObjectManager.GetResolveLeaf(groupMasterNode.NotResolvedGroupMasterObject),
                NonDefaultPositionX = groupMasterNode.GroupMasterMostlyExpandedPosition.X,
                NonDefaultPositionY = groupMasterNode.GroupMasterMostlyExpandedPosition.Y
            });
            ShowObjects(objectShowMetadatas);
        }

        /// <summary>
        /// عملکرد بازگشتی تعیین میزبان گروهی که شئ، زیرگروه آن است، در صورت وجود
        /// </summary>
        /// <param name="collapsedGroupsHierarchy">
        /// گروه های جمع شده ای که می خواهیم بدانیم شئ زیرگروه آن ها هست یا نه؛
        /// در صورتی گروه ها دارای زیرگروه های بسته شده در خودشان باشند، بررسی به صورت بازگشتی انجام خواهد شد
        /// </param>
        /// <param name="objectToFindItsSuperGroup">شئی که می خواهیم عضویت آن را در گروه بررسی کینم</param>
        /// <param name="flatArrangment">چینش بازیابی شده گراف که می خواهیم نتیجه عملکرد براساس آن تعیین می شود</param>
        /// <param name="objectSuperGroup">میزبان گروهی که شئ عضو آن است</param>
        /// <returns>اگر شئ زیرگروه یک گروه باشد «صحیح» و در غیراینصورت «غلط» برگردانده می شود</returns>
        private bool TryGetSuperGroupNodeIfExistInHierarchy(List<CollapsedGroupNode> collapsedGroupsHierarchy, KWObject objectToFindItsSuperGroup, GraphArrangment flatArrangment, out CollapsedGroupNode objectSuperGroup)
        {
            if (collapsedGroupsHierarchy == null)
                throw new ArgumentNullException("collapsedGroupsHierarchy");
            if (objectToFindItsSuperGroup == null)
                throw new ArgumentNullException("objectToFindItsSuperGroup");

            objectSuperGroup = null;
            try
            {
                // از بین گروه های داده شده گروهی انتخاب می شود که...
                objectSuperGroup = collapsedGroupsHierarchy
                    .Single(g => g.NotResolvedGroupMasterObject.ID == flatArrangment.CollapsedGroupsRelativePoistions
                            // طبق تعاریف چینش، دارای زیرگروهی با شناسه مساوی با شئ داده شده باشد
                            .Single(f => f.GroupMembersRelativePositionsByObjectId.ContainsKey(objectToFindItsSuperGroup.ID))
                        .NotResolvedGroupMasterObjectId);
                // در صورت وجود چنین گره میزبان گروهی، مقدار آن به صورت خروجی تعیین (انتساب خط قبل) و مقدار «صحیح» برگردانده می شود
                return true;
            }
            // در صورت عدم وجود چنین گروهی بین گروه های ورودی
            catch (InvalidOperationException)
            {
                // این عملکرد به ازای هریک از گروه های داده شده صدا زده می شود،
                foreach (var item in collapsedGroupsHierarchy)
                    // و در صورت یافته شدن گروه بین زیر گروه ها مقدار صحیح برگردانده می شود
                    if (TryGetSuperGroupNodeIfExistInHierarchy(item.CollapsedGroupSubNodes, objectToFindItsSuperGroup, flatArrangment, out objectSuperGroup))
                        return true;
                // و در صورتی که شئ زیرگروه زیرگروه های گروه های ورودی هم نباشد، مقدار غلط برگردانده می شود
                return false;
            }
        }
    }
}
