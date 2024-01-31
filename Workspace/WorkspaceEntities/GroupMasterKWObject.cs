using System;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.Workspace.Entities
{
    /// <summary>
    /// کلاس نگهداری و استفاده از شئی که میزبان یک گروه است
    /// اجزای این کلاس با پیش فرض استفاده صحیح توسط استفاده کننده قابل تغییر هستند و مدیریت مقداردهی معتبر اجزای در دسترس آن، برعهده استفاده کننده می باشد
    /// </summary>
    public class GroupMasterKWObject : KWObject
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <param name="groupRelationshipsWithMasterObject">لیست اشیا زیرگروه و رابطه عضویتی که هر یک با میزبان گروه خواهند داشت را مشخص می کند</param>
        /// <param name="defaultGroupMembershipRelationshipTypeURI">نوع پیش فرض برای رابطه (داخلی) عضویت در گروه (بین زیرگروه ها و میزبان گروه) را مشخص می کند</param>
        public GroupMasterKWObject(IEnumerable<KeyValuePair<KWRelationship, KWObject>> groupRelationshipsWithMasterObject, string defaultGroupMembershipRelationshipTypeURI)
            : base()
        {
            goupMembershipRelationshipTypeURI = defaultGroupMembershipRelationshipTypeURI;
            GroupLinks = new List<RelationshipBasedKWLink>();
            foreach (KeyValuePair<KWRelationship, KWObject> item in groupRelationshipsWithMasterObject)
                GroupLinks.Add(
                    new RelationshipBasedKWLink()
                    // TODO: بهبود - جهت رابطه عضویت نیز می بایست بررسی شود و بهتر است هستان شناسی آن را تعیین کند در این صورت می بایست تمام عملکردهایی که از این ویژگی ها استفاده می کنند، تغییر نمایند!
                    // شئ مبدا، زیرگروه و مقصد، میزبان گروه تلقی می شود
                    { Source = item.Value, Target = this, Relationship = item.Key });
        }
        /// <summary>
        /// نوع ثابت رابطه «عضویت در گروه» را نگهداری می کند
        /// </summary>
        private readonly string goupMembershipRelationshipTypeURI;
        /// <summary>
        /// لینک های داخلی عضویت را نگهداری می کند؛
        /// لینک های عضویت که از زیرگروه ها به میزبان گروه برقرار شده است
        /// </summary>
        public List<RelationshipBasedKWLink> GroupLinks
        {
            get;
            private set;
        }
        /// <summary>
        /// یک لینک عضویت در گروه (لینک رابطه عضویت یک گره در گروهی که این گره میزبان آن است) را در صورتی که قبلا افزوده نشده باشد، به مجموعه لینک های عضویت موجود می افزاید
        /// </summary>
        /// <param name="subgroupLinkToAppendGroup">لینک جدیدی که می خواهیم بیافزاییم</param>
        public void AddSubGroupLink(RelationshipBasedKWLink subgroupLinkToAppendGroup)
        {
            // اعتبارسنجی لینک ورودی برای امکان پذیرش آن به عنوان لینک عضویت
            if (subgroupLinkToAppendGroup.Target != this)
                throw new Exception("Unable to append Sub-Group Link; Group master Object is not match to group");
            if (subgroupLinkToAppendGroup.TypeURI != goupMembershipRelationshipTypeURI)
                throw new Exception("Unable to append Sub-Group Link; Group membership relationship is not valid");
            foreach (var item in GroupLinks)
                if (item.Relationship.ID == subgroupLinkToAppendGroup.Relationship.ID)
                    // در صورتی که قبلا افزوده شده باشد، نیازی به افزودن مجدد نیست
                    return;
            // در نهایت افزودن به لیست لینک های عضویت
            GroupLinks.Add(subgroupLinkToAppendGroup);
        }
        /// <summary>
        /// مجموعه ای از لینک های عضویت در گروه (لینک رابطه عضویت یک گره در گروهی که این گره میزبان آن است) را به مجموعه لینک های عضویت موجود می افزاید؛
        /// لینک های عضویتی که قبلا افزوده شده اند مجددا اضافه نخواهند شد
        /// </summary>
        /// <param name="subgroupLinksToAppendGroup">لینک های جدیدی که می بایست افزوده شوند</param>
        public void AddSubGroupLinksRange(IEnumerable<RelationshipBasedKWLink> subgroupLinksToAppendGroup)
        {
            foreach (var item in subgroupLinksToAppendGroup)
                AddSubGroupLink(item);
        }

        /// <summary>
        /// اشیا زیرگروه که این شی میزبان آن ها در گروه می باشد را برمی گرداند
        /// </summary>
        public IEnumerable<KWObject> GetSubGroupObjects()
        {
            return GroupLinks.Select(l => l.Source);
        }
        /// <summary>
        /// تعداد اشیا زیرگروه که این شی میزبان آن ها در گروه می باشد را برمی گرداند
        /// </summary>
        public int GetSubGroupObjectsCount
        { get { return GroupLinks.Count; } }
    }
}
