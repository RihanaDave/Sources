using GPAS.Workspace.Entities;
using GPAS.Workspace.Entities.SearchAroundResult;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GPAS.Workspace.Entities.KWLinks;
using DAM = GPAS.Workspace.DataAccessManager;

namespace GPAS.Workspace.Logic
{
    /// <summary>
    /// مدیریت رابطه (لینک) ها در سمت محیط کاربری
    /// </summary>
    public class LinkManager
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <remarks>این سازنده برای جلوگیری از ایجاد نمونه از این کلاس، محلی شده است</remarks>
        private LinkManager()
        { }

        /// <summary>
        /// ایجاد یک رابطه (لینک) مبتنی بر وابستگی (ریلیشنشیپ) در مخزن داده ها
        /// </summary>
        /// <returns>رابطه (لینک) مبتنی بر وابستگی که با اجرای این دستور در مخزن داده ها ایجاد شده است را برمی گرداند</returns>
        public static async Task<RelationshipBasedKWLink> CreateRelationshipBaseLinkAsync(KWObject source,
            KWObject target, string relationshipTypeURI, LinkDirection linkDirection, DateTime? timeBegin,
            DateTime? timeEnd, string description)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (relationshipTypeURI == null)
                throw new ArgumentNullException(nameof(relationshipTypeURI));
            if (description == null)
                throw new ArgumentNullException(nameof(description));

            // TODO: clean!
            // بررسی عضویت در خاندان عضویتی گروه

            // در صورتی که درخواست دریافت شده، در خواست عضویت یک شئ در یک گروه باشد، ابتدا
            // عضویت شئ در زیرگروه‌های میزبان و نیز زیرگروه‌های آن‌ها بررسی می‌شود و در صورت
            // عدم عضویت در زیرگروه ها، عضویت در گروه ثبت می‌شود
            if (relationshipTypeURI == OntologyProvider.GetOntology().DefaultGroupRelationshipType())
                if (linkDirection == LinkDirection.TargetToSource && target is GroupMasterKWObject)
                {
                    if (ObjectManager.IsMemberOfAncestrySubGroups(source, target as GroupMasterKWObject))
                        throw new ArgumentException(string.Format
                        ("Unable to Group objects that have ancestry membership;\r\rObject {0} (ID: {1}) is ancestry member of Group {2} (ID: {3})."
                            , source.GetObjectLabel()
                            , source.ID
                            , target.GetObjectLabel()
                            , target.ID));

                    // برعکس حالت بالا: بررسی عضویت قبلی در اجداد گروهی که اکنون می خواهد عضو آن شود
                    foreach (GroupMasterKWObject group in await (ObjectManager
                        .RetriveGroupsThatObjectIsMemberOfThemAsync(target)))
                        if (ObjectManager.IsMemberOfAncestrySubGroups(source, group))
                            throw new ArgumentException(string.Format
                            ("Unable to Group objects that have ancestry membership;\rThe object you try to membership it, is currently member of one of the super-groups of the group\r\rObject {0} (ID: {1}) is ancestry member of Group {2} (ID: {3}) by the Super-Group {4} (ID: {5})."
                                , source.GetObjectLabel()
                                , source.ID
                                , target.GetObjectLabel()
                                , target.ID
                                , group.GetObjectLabel()
                                , group.ID));
                }
                else if (linkDirection == LinkDirection.SourceToTarget && source is GroupMasterKWObject)
                {
                    if (ObjectManager.IsMemberOfAncestrySubGroups(target, source as GroupMasterKWObject))
                        throw new ArgumentException(string.Format
                        ("Unable to Group objects that have ancestry membership;\r\rObject {0} (ID: {1}) is ancestry member of Group {2} (ID: {3})."
                            , target.GetObjectLabel()
                            , target.ID
                            , source.GetObjectLabel()
                            , source.ID));
                    // برعکس حالت بالا: بررسی عضویت قبلی در اجداد گروهی که اکنون می خواهد عضو آن شود
                    foreach (GroupMasterKWObject group in await (ObjectManager
                        .RetriveGroupsThatObjectIsMemberOfThemAsync(source)))
                        if (ObjectManager.IsMemberOfAncestrySubGroups(target, group))
                            throw new ArgumentException(string.Format
                            ("Unable to Group objects that have ancestry membership;\rThe object you try to membership it, is currently member of one of the super-groups of the group\r\rObject {0} (ID: {1}) is ancestry member of Group {2} (ID: {3}) by the Super-Group {4} (ID: {5})."
                                , target.GetObjectLabel()
                                , target.ID
                                , source.GetObjectLabel()
                                , source.ID
                                , group.GetObjectLabel()
                                , group.ID));
                }

            return DataAccessManager.LinkManager.CreateNewRelationshipBaseLink
                (source, target, relationshipTypeURI, description, linkDirection, timeBegin, timeEnd);
        }

        /// <summary>
        /// ایجاد یک رابطه (لینک) مبتنی بر رخداد در مخزن داده ها
        /// </summary>
        /// <returns>رابطه (لینک) مبتنی بر رخداد که با اجرای این دستور در مخزن داده ها ایجاد شده است را برمی گرداند</returns>
        public static EventBasedKWLink CreateEventBaseLink(KWObject source, KWObject target, string intermediaryEventTypeURI, LinkDirection linkDirection, DateTime? timeBegin, DateTime? timeEnd, string description)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            if (intermediaryEventTypeURI == null)
                throw new ArgumentNullException(nameof(intermediaryEventTypeURI));
            if (description == null)
                throw new ArgumentNullException(nameof(description));

            // تعیین نوع وابستگی های داخلی برای ایجاد رابطه مبتنی بر رخداد میانی از طریق عملکردهای هستان شناسی
            string sourceToIntermediaryEventRelationshipType, intermediaryEventToTargetRelationshipType;
            sourceToIntermediaryEventRelationshipType = intermediaryEventToTargetRelationshipType = OntologyProvider.GetOntology().GetDefaultRelationshipTypeForEventBasedLink(source.TypeURI, intermediaryEventTypeURI, target.TypeURI);

            return DataAccessManager.LinkManager.CreateNewEventBaseLink
                (source, target, intermediaryEventTypeURI, description, linkDirection, timeBegin, timeEnd, sourceToIntermediaryEventRelationshipType, intermediaryEventToTargetRelationshipType);
        }

        /// <summary>
        /// بازیابی دسته ای از لینک های مبتنی بر رابطه طبق شناسه های داده شده، از مخزن داده ها
        /// </summary>
        public async static Task<List<RelationshipBasedKWLink>> RetrieveRelationshipBaseLinksAsync(List<long> relationshipsId)
        {
            if (relationshipsId == null)
                throw new ArgumentNullException(nameof(relationshipsId));

            return await DAM.LinkManager.GetRelationshipRangeByIdAsync(relationshipsId);
        }
        public async static Task<List<RelationshipBasedKWLink>> RetriveRelationshipsSourcedByObjectAsync(KWObject objectToGetRelationships, string relationshipsTypeUri)
        {
            if (objectToGetRelationships == null)
                throw new ArgumentNullException("objectToCheckMembership");

            return await DAM.LinkManager.GetRelationshipsBySourceObjectAsync(objectToGetRelationships, relationshipsTypeUri);
        }

        public static RelationshipBasedKWLink GetRelationshipBasedKWLink(KWRelationship baseRelationship)
        {
            return DAM.LinkManager.GetRelationshipBasedKWLink(baseRelationship);
        }

        public static RelationshipBasedKWLink GetCachedRelationshipBasedKWLink(long cachedRelationshipID)
        {
            return DAM.LinkManager.GetRelationshipBasedKWLink(cachedRelationshipID);
        }

        /// <summary>
        /// مسیر نمایه (آیکن) متناسب با یک رابطه (لینک) را برمی گرداند؛ در صورت بروز خطا در دریافت مسیر آیکن، «نال» برمی گرداند
        /// </summary>
        public static Uri GetIconPath(KWLink LinkToGetItsIcon)
        {
            if (LinkToGetItsIcon == null)
                throw new ArgumentNullException(nameof(LinkToGetItsIcon));
            // مسیر آیکن متناظر را با درخواست از متصدی هستان شناسی لایه منطق محیط کاربری به دست آورده و برمی گرداند
            return OntologyIconProvider.GetTypeIconPath(LinkToGetItsIcon.TypeURI);
        }

        /// <summary>
        /// لینک مبتنی بررخدادی که امکان تشکیل آن با استفاده از دو رابطه ورودی وجود دارد را برمی گرداند؛
        /// دو رابطه می بایست امکان استفاده به عنوان روابط داخلی یک لینک مبتنی بر رخداد را داشته باشند.
        /// نمونه لینک مبتنی بر رخداد ذاتا می تواند تکراری باشد ولی رابطه های تشکیل دهنده آن نه
        /// </summary>
        /// <param name="firstRelationship">رابطه ای که مقصد آن، رخداد میانی است</param>
        /// <param name="secondRelationship">رابطه ای که مبدا آن، رخداد میانی است</param>
        public static EventBasedKWLink GetEventBaseKWLinkFromLinkInnerRelationships(RelationshipBasedKWLink firstRelationship, RelationshipBasedKWLink secondRelationship)
        {
            if (firstRelationship == null)
                throw new ArgumentNullException(nameof(firstRelationship));
            if (secondRelationship == null)
                throw new ArgumentNullException(nameof(secondRelationship));

            KWObject[] innerObjects =
            {
                firstRelationship.Source,
                firstRelationship.Target,
                secondRelationship.Source,
                secondRelationship.Target
            };

            Dictionary<long, KWObject> innerObjectsDictionary = new Dictionary<long, KWObject>(4);

            foreach (KWObject obj in innerObjects)
            {
                if (!innerObjectsDictionary.ContainsKey(obj.ID))
                {
                    innerObjectsDictionary.Add(obj.ID, obj);
                }
            }

            return DAM.LinkManager.GenerateEventBasedLinkByInnerRelationshipsAndObjects
                (firstRelationship.Relationship, secondRelationship.Relationship, innerObjectsDictionary);
        }
        public static PropertyBasedKWLink GetPropertyBasedKWLinkFromLinkInnerProperties(KWProperty firstProperty, KWProperty secondProperty)
        {
            if (firstProperty == null)
                throw new ArgumentNullException(nameof(firstProperty));
            if (secondProperty == null)
                throw new ArgumentNullException(nameof(secondProperty));

            // اعتبارسنجی امکان تشکیل یک رابطه مبتنی بر ویژگی هم مقدار، با استفاده از ویژگی‌های ورودی
            if (firstProperty.ID == secondProperty.ID)
                throw new ArgumentException("Given Properties could not be base of an Property-Base Link; Both Properties has same ID");
            if (firstProperty.Owner.ID == secondProperty.Owner.ID)
                throw new ArgumentException("Given Properties could not be base of an Property-Base Link; Properties are assigned to same Object");
            if (!firstProperty.TypeURI.Equals(secondProperty.TypeURI))
                throw new ArgumentException("Given Properties could not be base of an Property-Base Link; Properties' Types are not same");
            //if (!OntologyProvider.GetOntology().IsProperty(firstProperty.TypeURI))
            //    throw new ArgumentException(string.Format("Given Properties could not be base of an Property-Base Link; Properties' Types are not an Property. Properties' Types type: {0}", OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(firstProperty.TypeURI)));
            if (!firstProperty.Value.Equals(secondProperty.Value))
                throw new ArgumentException("Given Properties could not be base of an Property-Base Link; Properties' Values are not same");
            // ایجاد نمونه لینک مبتنی بر رخداد قابل تشکیل براساس رابطه های ورودی
            return
                new PropertyBasedKWLink
                {
                    Source = firstProperty.Owner,
                    Target = secondProperty.Owner,
                    SamePropertyTypeUri = firstProperty.TypeURI,
                    SamePropertyValue = secondProperty.Value,
                    LinkCaption = string.Format("\"{0}\""
                        , OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(firstProperty.TypeURI))
                };
        }

        public static NotLoadedRelationshipBasedKWLink GetNotLoadedLinkFormRelationships
            (KWObject linkEnd1, KWObject linkEnd2, List<long> relationshipIDsBetweenEnds)
        {
            return new NotLoadedRelationshipBasedKWLink
            {
                Source = linkEnd1,
                Target = linkEnd2,
                IntermediaryRelationshipIDs = new HashSet<long>(relationshipIDsBetweenEnds)
            };
        }

        public static NotLoadedEventBasedKWLink GetNotLoadedLinkFormEventBasedLinks
            (KWObject linkEnd1, KWObject linkEnd2, List<EventBasedResultInnerRelationships> relationshipIdOfEventBasedLinksBetweenEnds)
        {
            return new NotLoadedEventBasedKWLink
            {
                Source = linkEnd1,
                Target = linkEnd2,
                IntermediaryLinksRelationshipIDs = new HashSet<EventBasedResultInnerRelationships>(relationshipIdOfEventBasedLinksBetweenEnds)
            };
        }

        /// <summary>
        /// حذف نرم یک لینک؛
        /// این تابع لینک داده شده را به صورت نرم، حذف می‌کند؛
        /// حذف نرم به معنای عدم دستکاری در مخزن داده‌ها بوده و
        /// فقط برای شئ‌های منتشر نشده قابل انجام است
        /// </summary>
        public static async Task DeleteLink(KWLink linkToDelete)
        {
            if (linkToDelete == null)
                throw new ArgumentNullException(nameof(linkToDelete));

            if (!CanDeleteLink(linkToDelete))
                throw new InvalidOperationException("Unable to delete link");

            if (linkToDelete is PropertyBasedKWLink)
            {
                // Nothing to do for the virtual link
            }
            else if (linkToDelete is RelationshipBasedKWLink)
            {
                var innerRelationship = (linkToDelete as RelationshipBasedKWLink).Relationship;
                DAM.LinkManager.DeleteRelationship(innerRelationship);
            }
            else if (linkToDelete is EventBasedKWLink)
            {
                var link = linkToDelete as EventBasedKWLink;
                DAM.LinkManager.DeleteRelationship(link.FirstRelationship);
                DAM.LinkManager.DeleteRelationship(link.SecondRelationship);
                await ObjectManager.DeleteObject(link.IntermediaryEvent);
            }
            else if (linkToDelete is NotLoadedEventBasedKWLink)
            {
                // Nothing to do for the virtual link
            }
            else if (linkToDelete is NotLoadedRelationshipBasedKWLink)
            {
                // Nothing to do for the virtual link
            }
            else
                throw new NotSupportedException("Unknown link type");
        }

        public static async Task DeleteLinksList(List<KWLink> linksToDelete)
        {
            if (linksToDelete == null)
                throw new ArgumentNullException(nameof(linksToDelete));

            for (int i = 0; i < linksToDelete.Count; i++)
            {
                await DeleteLink(linksToDelete[i]);
            }
        }

        /// <summary>
        /// بررسی امکان حذف نرم شئ
        /// </summary>
        public static bool CanDeleteLink(KWLink linkToDeletability)
        {
            if (linkToDeletability == null)
                throw new ArgumentNullException(nameof(linkToDeletability));

            if (linkToDeletability is PropertyBasedKWLink)
                return false;
            if (linkToDeletability is RelationshipBasedKWLink)
            {
                var innerRelationship = (linkToDeletability as RelationshipBasedKWLink).Relationship;
                return DAM.LinkManager.IsUnpublishedRelationship(innerRelationship);
            }

            if (linkToDeletability is EventBasedKWLink)
            {
                var link = linkToDeletability as EventBasedKWLink;
                bool canDeleteIntermediaryEvent = ObjectManager.CanDeleteObject(link.IntermediaryEvent);
                bool canDeleteFirstRelationship = DAM.LinkManager.IsUnpublishedRelationship(link.FirstRelationship);
                bool canDeleteSecondRelationship = DAM.LinkManager.IsUnpublishedRelationship(link.SecondRelationship);
                return (canDeleteIntermediaryEvent && canDeleteFirstRelationship && canDeleteSecondRelationship);
            }
            if (linkToDeletability is NotLoadedEventBasedKWLink)
                return true;
            if (linkToDeletability is NotLoadedRelationshipBasedKWLink)
                return true;
            throw new NotSupportedException("Unknown link type");
        }

        public static bool CanDeleteLinksList(IEnumerable<KWLink> linksToDelete)
        {
            if (linksToDelete == null)
                throw new ArgumentNullException(nameof(linksToDelete));

            return !linksToDelete.Any(l => !CanDeleteLink(l));
        }

        public static Tuple<RelationshipBasedKWLink, RelationshipBasedKWLink> ConvertEventBaseKWLinkToRelationshipBasedKWLink(EventBasedKWLink eventBasedKWLink)
        {
            Dictionary<long, KWObject> objDic = new Dictionary<long, KWObject>(3)
            {
                { eventBasedKWLink.Source.ID, eventBasedKWLink.Source }
            };
            if (!objDic.ContainsKey(eventBasedKWLink.IntermediaryEvent.ID))
                objDic.Add(eventBasedKWLink.IntermediaryEvent.ID, eventBasedKWLink.IntermediaryEvent);
            if (!objDic.ContainsKey(eventBasedKWLink.Target.ID))
                objDic.Add(eventBasedKWLink.Target.ID, eventBasedKWLink.Target);

            var firstRelationshipMetadata = DAM.LinkManager.cachedRelationshipsMetadataIdentifiedByID[eventBasedKWLink.FirstRelationship.ID];
            RelationshipBasedKWLink source = new RelationshipBasedKWLink
            {
                Relationship = eventBasedKWLink.FirstRelationship,
                Source = objDic[firstRelationshipMetadata.RelationshipSourceId],
                Target = objDic[firstRelationshipMetadata.RelationshipTargetId]
            };

            var secondRelationshipMetadata = DAM.LinkManager.cachedRelationshipsMetadataIdentifiedByID[eventBasedKWLink.SecondRelationship.ID];
            RelationshipBasedKWLink target = new RelationshipBasedKWLink
            {
                Relationship = eventBasedKWLink.SecondRelationship,
                Source = objDic[secondRelationshipMetadata.RelationshipSourceId],
                Target = objDic[secondRelationshipMetadata.RelationshipTargetId]
            };
            return new Tuple<RelationshipBasedKWLink, RelationshipBasedKWLink>(source, target);
        }

        public static bool IsUnpublishedRelationship(KWLink link)
        {
            switch (link)
            {
                case RelationshipBasedKWLink relationshipBasedLink:
                    return DAM.LinkManager.IsUnpublishedRelationship(relationshipBasedLink.Relationship);
                case EventBasedKWLink eventBasedLink:
                    return DAM.LinkManager.IsUnpublishedRelationship(eventBasedLink.FirstRelationship) &&
                           DAM.LinkManager.IsUnpublishedRelationship(eventBasedLink.SecondRelationship);
                default:
                    return false;
            }
        }

        public async static Task<KWObject> GetAnotherSideOfKWRelationship(KWRelationship selectedRelationship, KWObject selectedSideOfKWRelationship)
        {
            if (selectedRelationship == null)
                throw new ArgumentNullException(nameof(selectedRelationship));
            if (selectedSideOfKWRelationship == null)
                throw new ArgumentNullException(nameof(selectedSideOfKWRelationship));
            KWObject result = new KWObject();
            DAM.LinkManager.CachedRelationshipMetadata cachedRelationshipMetadata = DataAccessManager.LinkManager.GetCachedMetadataById(selectedRelationship.ID);
            if (cachedRelationshipMetadata.RelationshipSourceId == selectedSideOfKWRelationship.ID)
            {
                result = await DataAccessManager.ObjectManager.GetObjectByIdAsync(cachedRelationshipMetadata.RelationshipTargetId);
            }
            else if (cachedRelationshipMetadata.RelationshipTargetId == selectedSideOfKWRelationship.ID)
            {
                result = await DataAccessManager.ObjectManager.GetObjectByIdAsync(cachedRelationshipMetadata.RelationshipSourceId);
            }
            return result;
        }

        public static async Task<Tuple<bool, EventBasedKWLink>> TryGenerateEventBasedLinkByInnerRelationshipsAsync(KWRelationship firstRelationship, KWRelationship secondRelationship)
        {
            try
            {
                EventBasedKWLink eventBasedKWLink = await DataAccessManager.LinkManager.GenerateEventBasedLinkByInnerRelationshipsAsync(firstRelationship, secondRelationship);
                return new Tuple<bool, EventBasedKWLink>(true, eventBasedKWLink);
            }
#pragma warning disable CS0168 // The variable 'ex' is declared but never used
            catch (Exception ex)
#pragma warning restore CS0168 // The variable 'ex' is declared but never used
            {
                return new Tuple<bool, EventBasedKWLink>(false, null);
            }
        }
    }
}