using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GPAS.Workspace.Entities;
using GPAS.Workspace.ServiceAccess.RemoteService;
using GPAS.DataImport.DataMapping.SemiStructured;
using System.ServiceModel;
using GPAS.Workspace.Entities.Investigation;
using GPAS.Dispatch.Entities.Concepts;

namespace GPAS.Workspace.Logic
{
    /// <summary>
    /// مدیریت شئ ها در سمت محیط کاربری
    /// </summary>
    public class ObjectManager
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <remarks>این سازنده برای جلوگیری از ایجاد نمونه از این کلاس، محلی شده است</remarks>
        private ObjectManager()
        { }

#pragma warning disable CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        /// <summary>
        /// ایجاد یک شئ جدید در مخزن داده ها
        /// </summary>
        /// <returns>شئی که با اجرای این دستور در مخزن داده ها ایجاد شده است را برمی گرداند</returns>
        public async static Task<KWObject> CreateNewObject(string TypeURI, string DisplayName, bool isMasterOfGroup = false)
#pragma warning restore CS1998 // This async method lacks 'await' operators and will run synchronously. Consider using the 'await' operator to await non-blocking API calls, or 'await Task.Run(...)' to do CPU-bound work on a background thread.
        {
            if (TypeURI == null)
                throw new ArgumentNullException("TypeURI");
            if (DisplayName == null)
                throw new ArgumentNullException("DisplayName");
            if (string.IsNullOrWhiteSpace(TypeURI))
                throw new ArgumentException("Invalid argument", "TypeURI");
            if (DisplayName == null)
                throw new ArgumentException("Invalid argument", "DisplayName");

            if (isMasterOfGroup)
                return DataAccessManager.ObjectManager.CreateNewGroupMasterObject(TypeURI, DisplayName);
            else
                return DataAccessManager.ObjectManager.CreateNewObject(TypeURI, DisplayName, OntologyProvider.GetOntology().IsDocument(TypeURI));
        }

        /// <summary>
        /// ایجاد یک شئ میزبان گروه جدید در مخزن داده ها؛ با تعیین اکنون به عنوان زمان های آغاز و پایان رابطه های داخلی عضویت در گروه
        /// </summary>
        /// <param name="objectsToGroup">مجموعه اشیائی که می بایست به عنوان عضو گروه در نظر گرفته شوند</param>
        /// <param name="groupMasterObjectDisplayName">عنوان نمایشی برای گروه - این عنوان به گره میزبان گروه بندی منتسب خواهد شد</param>
        /// <param name="groupRelationshipsDisplayText">عنوان نمایشی اختیاری برای رابطه های عضویت در گروه (بین میزبان گروه بندی با اعضای گروه) می تواند تعیین شود</param>
        /// <returns>نمونه شی گروه قابل استفاده در سمت محیط کاربری را برای گروه ایجاد شده برمی گرداند</returns>
        public static GroupMasterKWObject CreateNewGroup
            (IEnumerable<KWObject> objectsToGroup
            , string groupMasterObjectDisplayName
            , string groupRelationshipsDisplayText = "")
        {
            // بررسی اعتبار حداقلی آرگومانهای ورودی
            if (objectsToGroup == null)
                throw new ArgumentNullException("objectsToGroup");
            if (string.IsNullOrEmpty(groupMasterObjectDisplayName))
                throw new ArgumentException("Invalid Argument (Null, Empty or Whitespace)", "groupMasterObjectDisplayName");
            if (groupRelationshipsDisplayText == null)
                throw new ArgumentNullException("groupRelationshipsDisplayText");
            // امکان ایجاد گروه بدون زیرگروه وجود ندارد
            if (objectsToGroup.Count() < 1)
                throw new ArgumentException("Unable to create a group with no member", "objectsToGroup");
            // فراخوانی ریخت دیگر عملکرد، با تعیین زمان «اکنون» به عنوان زمان شروع و پایان رابطه های داخلی عضویت در گروه
            return CreateNewGroup(objectsToGroup, groupMasterObjectDisplayName, null, null, groupRelationshipsDisplayText);
        }

        /// <summary>
        /// ایجاد یک شئ میزبان گروه جدید در مخزن داده ها
        /// </summary>
        /// <param name="objectsToGroup">مجموعه اشیائی که می بایست به عنوان عضو گروه در نظر گرفته شوند</param>
        /// <param name="groupMasterObjectDisplayName">عنوان نمایشی برای گروه - این عنوان به گره میزبان گروه بندی منتسب خواهد شد</param>
        /// <param name="groupRelationshipsTimeBegin">زمان شروع برای ثبت زمان شروع رابطه عضویت در گروه - زمان شروع رابطه ای که بین عضو و میزبان گروه برقرار خواهد شد</param>
        /// <param name="groupRelationshipsTimeEnd">زمان پایان برای ثبت زمان پایان رابطه عضویت در گروه - زمان پایان رابطه ای که بین عضو و میزبان گروه برقرار خواهد شد</param>
        /// <param name="groupRelationshipsDisplayText">عنوان نمایشی اختیاری برای رابطه های عضویت در گروه (بین میزبان گروه بندی با اعضای گروه) می تواند تعیین شود</param>
        /// <returns>نمونه شی گروه قابل استفاده در سمت محیط کاربری را برای گروه ایجاد شده برمی گرداند</returns>
        public static GroupMasterKWObject CreateNewGroup
            (IEnumerable<KWObject> objectsToGroup
            , string groupMasterObjectDisplayName
            , DateTime? groupRelationshipsTimeBegin
            , DateTime? groupRelationshipsTimeEnd
            , string groupRelationshipsDisplayText = "")
        {
            // بررسی اعتبار حداقلی آرگومانهای ورودی
            if (objectsToGroup == null)
                throw new ArgumentNullException("objectsToGroup");
            if (string.IsNullOrEmpty(groupMasterObjectDisplayName))
                throw new ArgumentException("Invalid Argument (Null, Empty or Whitespace)", "groupMasterObjectDisplayName");
            if (groupRelationshipsDisplayText == null)
                throw new ArgumentNullException("groupRelationshipsDisplayText");
            // امکان ایجاد گروه بدون زیرگروه وجود ندارد
            if (objectsToGroup.Count() < 1)
                throw new ArgumentException("Unable to create a group with no member", "objectsToGroup");

            // TODO: Clean!
            // عضویت در خاندان دیگر گروه‌هایی که می خواهد با آن‌ها هم گروه شود، بررسی می شود

            // در صورتی که درخواست دریافت شده، در خواست عضویت چند شئ در یک گروه باشد که
            // یکی از اشیا قبلا عضو نیاکانی یکی دیگر از آن‌ها باشد، امکان عضویت در یک گروه
            // برای آن‌ها وجود ندارد
            foreach (var groupMasterObjectToMembership in objectsToGroup
                .Where(obj => obj is GroupMasterKWObject)
                .Select(group => group as GroupMasterKWObject))
            {
                foreach (var objectToCheckLaterAncestryMembership in objectsToGroup)
                {
                    if (groupMasterObjectToMembership.Equals(objectToCheckLaterAncestryMembership))
                        continue;
                    if (IsMemberOfAncestrySubGroups(objectToCheckLaterAncestryMembership, groupMasterObjectToMembership))
                        throw new ArgumentException(string.Format
                            ("Unable to Group objects that have ancestry membership;\rAtleast one ancestry membership detected in objects given for Grouping, example:\r\rObject {0} (ID: {1}) is ancestry member of Group {2} (ID: {3})."
                            , objectToCheckLaterAncestryMembership.GetObjectLabel()
                            , objectToCheckLaterAncestryMembership.ID
                            , groupMasterObjectToMembership.GetObjectLabel()
                            , groupMasterObjectToMembership.ID));
                }
            }

            // برعکس حالت بالا: ممکن نیست، چون در اینجا می خواهیم
            // گروه جدید ایجاد کنیم و مسلما این گروه اجداد عضویتی ندارد!

            // نوع های زیرگروه ها لیست می شوند
            Ontology.Ontology currentOntology = OntologyProvider.GetOntology();
            ArrayList groupMembersTypes = new ArrayList();
            foreach (var item in objectsToGroup)
                groupMembersTypes.Add(item.TypeURI);
            // نوع میزبان براساس نوع های زیرگروه ها از هستان شناسی استنتاج می شود
            string groupMasterObjectTypeURI = currentOntology.InferGroupType(groupMembersTypes);

            // نوع رابطه ی عضویت بین زیرگروه ها و شی میزبان گروه از هستان شناسی استخراج می شود
            string groupRelationshipsTypeURI = currentOntology.DefaultGroupRelationshipType();

            // در صورت عدم تعیین مقدار ورودی برای عنوان نمایشی رابطه های عضویت داخلی گروه (بین زیرگروه ها و میزبان گروه)، عنوان
            // کاربرپسند نوع آن رابطه از هستان شناسی استخراج و به عنوان نام نمایشی برای روابط داخلی در نظر گرفته می شود
            if (groupRelationshipsDisplayText == "")
                groupRelationshipsDisplayText = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(groupRelationshipsTypeURI);

            //// شی قابل فهم برای سمت سرور را براساس اشیا زیرگروه ها آماده می کند
            //List<KObject> groupMembersRelatedKObjects = new List<KObject>();
            //foreach (var item in objectsToGroup)
            //    groupMembersRelatedKObjects.Add(DataAccessManager.ObjectManager.GetKObjectFromKWObject(item));

            return DataAccessManager.ObjectManager.CreateNewGroupOfObjects
                    (objectsToGroup
                    , groupMasterObjectTypeURI
                    , groupMasterObjectDisplayName
                    , groupRelationshipsDisplayText
                    , groupRelationshipsTimeBegin
                    , groupRelationshipsTimeEnd);
        }

        // TODO: Clean!
        public static bool IsMemberOfAncestrySubGroups(KWObject objectToCheckItsMembershipment, GroupMasterKWObject groupToCheckItsAncestrySubGroups)
        {
            if (groupToCheckItsAncestrySubGroups.GetSubGroupObjects()
                    .Where(obj => obj.ID == objectToCheckItsMembershipment.ID)
                    .Count() != 0)
                return true;
            else
                return
                    groupToCheckItsAncestrySubGroups.GetSubGroupObjects()
                        .Where(obj => obj is GroupMasterKWObject)
                        .Where(group => IsMemberOfAncestrySubGroups(objectToCheckItsMembershipment, group as GroupMasterKWObject))
                        .Count() != 0;
        }
        
        /// <summary>
        /// مسیر نمایه (آیکن) متناسب با یک شئ را برمی گرداند؛ در صورت بروز خطا در دریافت مسیر آیکن، «نال» برمی گرداند
        /// </summary>
        public static Uri GetIconPath(KWObject ObjectToGetItsIcon)
        {
            // بررسی اعتبار حداقلی آرگومانهای ورودی
            if (ObjectToGetItsIcon == null)
                throw new ArgumentNullException("ObjectToGetItsIcon");
            // مسیر آیکن متناظر را با درخواست از متصدی هستان شناسی لایه منطق محیط کاربری به دست آورده و برمی گرداند
            return OntologyIconProvider.GetTypeIconPath(ObjectToGetItsIcon.TypeURI);
        }

        /// <summary>
        /// بازیابی دسته ای از اشیا طبق شناسه های داده شده، از مخزن داده ها
        /// </summary>
        public async static Task<IEnumerable<KWObject>> RetriveObjectsAsync(IEnumerable<long> objectsId, bool applyResolutionTree = true)
        {
            if (objectsId == null)
                throw new ArgumentNullException("objectsId");

            return await DataAccessManager.ObjectManager.GetObjectsListByIdAsync(objectsId, applyResolutionTree);
        }
        // TODO: Clean!
        public async static Task<IEnumerable<GroupMasterKWObject>> RetriveGroupsThatObjectIsMemberOfThemAsync(KWObject objectToCheckMembership)
        {
            if (objectToCheckMembership == null)
                throw new ArgumentNullException("objectToCheckMembership");

            return
                (await LinkManager.RetriveRelationshipsSourcedByObjectAsync
                    (objectToCheckMembership
                    , OntologyProvider.GetOntology().DefaultGroupRelationshipType()))
                .Select(relationship => relationship.Target as GroupMasterKWObject);
        }
        public static void ChangeDisplayNameOfObject(KWObject editingObject, string newDisplayName)
        {
            if (editingObject == null)
                throw new ArgumentNullException("editingObject");
            if (newDisplayName == null)
                throw new ArgumentNullException("newDisplayName");
            DataAccessManager.ObjectManager.ChangeDisplayNameOfObject(editingObject, newDisplayName);
        }

        /// <summary>
        /// حذف نرم یک شئ(به همراه ویژگی‌ها، رابطه‌ها و مدیاهای آن)؛
        /// این تابع شئ داده شده را به صورت نرم، حذف می‌کند؛
        /// حذف نرم به معنای عدم دستکاری در مخزن داده‌ها بوده و
        /// فقط برای شئ‌های منتشر نشده قابل انجام است
        /// </summary>
        public static async Task DeleteObject(KWObject objectToDelete)
        {
            if (objectToDelete == null)
                throw new ArgumentNullException("objectToDelete");

            if (!CanDeleteObject(objectToDelete))
                throw new InvalidOperationException("Unable to delete object");

            await DataAccessManager.ObjectManager.DeleteObjectAsync(objectToDelete);
        }

        public static async Task DeleteObjectsList(List<KWObject> objectsToDelete)
        {
            if (objectsToDelete == null)
                throw new ArgumentNullException("objectsToDelete");

            for (int i = 0; i < objectsToDelete.Count; i++)
            {
                await DeleteObject(objectsToDelete[i]);
            }
        }

        /// <summary>
        /// بررسی امکان حذف نرم شئ
        /// </summary>
        public static bool CanDeleteObject(KWObject objectToDeletability)
        {
            if (objectToDeletability == null)
                throw new ArgumentNullException("objectToDeletability");

            return DataAccessManager.ObjectManager.IsUnpublishedObject(objectToDeletability);
        }

        public static bool CanDeleteObjectsList(List<KWObject> objectsToDelete)
        {
            if (objectsToDelete == null)
                throw new ArgumentNullException("linksToDelete");

            bool result = true;
            for (int i = 0; i < objectsToDelete.Count(); i++)
            {
                if (CanDeleteObject(objectsToDelete.ElementAt(i)) == false)
                {
                    result = false;
                    break;
                }
            }
            return result;
        }

        public static bool IsUnpublishObject(KWObject kwObject)
        {
            if (kwObject == null)
                throw new ArgumentNullException("kwObject");

            return DataAccessManager.ObjectManager.IsUnpublishedObject(kwObject);
        }

        public async static Task<List<KWObject>> GetObjectsById(IEnumerable<long> IDs)
        {            
            return await DataAccessManager.ObjectManager.GetObjectsListByIdAsync(IDs);
        }

        public async static Task<List<KWObject>> GetObjectsById(IEnumerable<KObject> kObjects)
        {
            return await DataAccessManager.ObjectManager.GetObjectsListByIdAsync(kObjects);
        }

        /// <summary></summary>
        /// <returns>Returns wanted object; If no object related to the ID returns 'null'</returns>
        /// 
        public async static Task<KWObject> GetObjectById(long id)
        {
            return (await GetObjectsById(new long[] { id })).FirstOrDefault();
        }

        internal async static Task<List<KWObject>> GetObjectsByIdWithoutApplyResolutionTree(IEnumerable<long> IDs)
        {
            return await DataAccessManager.ObjectManager.GetObjectsListByIdAsync(IDs, false);
        }

        public static KWObject GetResolveLeaf(KWObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            return DataAccessManager.ObjectManager.GetResolveLeaf(obj);
        }

        public static bool AreDocumentsSourcesUploaded(IEnumerable<KWObject> objects)
        {
            if (objects == null)
                throw new ArgumentNullException("objects");
            if (!objects.Any())
                throw new InvalidOperationException("Empty object sequence");

            foreach (KWObject obj in objects)
            {
                if (DataAccessManager.ObjectManager.IsNotUploadedSourceDocument(obj))
                    return false;
            }
            return true;
        }
    }
}