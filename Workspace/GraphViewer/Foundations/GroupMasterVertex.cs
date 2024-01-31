using System;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.Graph.GraphViewer.Foundations
{
    public class GroupMasterVertex : Vertex
    {
        #region مدیریت رخداد
        /// <summary>
        /// عملگر صدور رخداد «تغییر در ویژگی های شئ»
        /// </summary>
        protected void OnPropertyChanged()
        {
            base.OnPropertyChanged(new string[] { "SubGroupsCount", "SubGroupsTitles" });
        }
        #endregion

        protected internal GroupMasterVertex(string vertexTitle, IEnumerable<Vertex> subGroups, VertexControl relatedVertexControl = null)
            : base(vertexTitle, relatedVertexControl, false)
        {
            if (vertexTitle == null)
                throw new ArgumentNullException("vertexTitle");
            if (subGroups == null)
                throw new ArgumentNullException("subGroups");
            if (string.IsNullOrWhiteSpace(vertexTitle))
                throw new ArgumentException("Invalid argument", "vertexTitle");

            SubGroup = subGroups.ToList();
            ResetSubGroupsTitles();
            RelatedVertexControl = (relatedVertexControl == null) ? new VertexControl(this) : relatedVertexControl;
        }

        /// <summary>
        /// گره هایی که زیرمجموعه گروه این گره می باشند و روی گراف نمایش داده می شوند، را برمی گرداند
        /// </summary>
        public List<Vertex> SubGroup
        {
            get;
            private set;
        }

        /// <summary>
        /// یک گره به گره های زیرمجموعه این گره می افزاید
        /// </summary>
        public void AddSubGroup(Vertex vertexToAddGroup, GraphViewer masterViewer)
        {
            if (vertexToAddGroup == null)
                throw new ArgumentNullException("vertexToAddGroup");
            if (masterViewer == null)
                throw new ArgumentNullException("masterViewer");
            
            // در صورت افزوده شدن قبلی گره به لیست زیرگروه‌های نمایشی گروه، نیاز به کار خاصی نیست
            if (SubGroup.Contains(vertexToAddGroup))
                return;
            bool isCollapsedBeforeAddSub = masterViewer.IsGroupInCollapsedViewing(this);
            // در صورتی که در حال حاضر گروه در وضعیت جمع‌شده است،
            if (isCollapsedBeforeAddSub)
                // گروه بدون پویانمایی باز می‌شود
                masterViewer.ExpandGroup(this, false);
            // گره به زیرگروه‌های نمایشی گروه افزوده می‌شود
            SubGroup.Add(vertexToAddGroup);
            // در صورتی که گروه پیش از افزودن گره در وضعیت جمع‌شده بوده باشد،
            if (isCollapsedBeforeAddSub)
                // گروه (با پویانمایی) جمع می‌شود؛ تا کاربر متوجه عضو شدن در یک گروه جمع‌شده بشود
                masterViewer.CollapseGroup(this);
            ResetSubGroupsTitles();
        }

        /// <summary>
        /// گرهی از گره های زیرمجموعه این گره را حذف می کند
        /// </summary>
        public void RemoveSubGroup(Vertex vertexToRemoveFromGroup, GraphViewer masterViewer)
        {
            if (vertexToRemoveFromGroup == null)
                throw new ArgumentNullException("vertexToRemoveFromGroup");
            if (masterViewer == null)
                throw new ArgumentNullException("masterViewer");

            // در صورتی که گره در لیست زیرگروه‌های نمایشی گروه نباشد، نیاز به کار خاصی نیست
            if (!SubGroup.Contains(vertexToRemoveFromGroup))
                return;
            bool isCollapsedBeforeAddSub = masterViewer.IsGroupInCollapsedViewing(this);
            // در صورتی که در حال حاضر گروه در وضعیت جمع‌شده است،
            if (isCollapsedBeforeAddSub)
                // گروه بدون پویانمایی باز می‌شود
                masterViewer.ExpandGroup(this, false);
            // گره از زیرگروه‌های نمایشی گروه حذف می‌شود
            SubGroup.Remove(vertexToRemoveFromGroup);
            // در صورتی که گروه پیش از حذف گره در وضعیت جمع‌شده بوده باشد،
            if (isCollapsedBeforeAddSub)
                // گروه بدون پویانمایی جمع می‌شود؛ تا کاربر تغییری احساس نکند
                masterViewer.CollapseGroup(this, false);
            ResetSubGroupsTitles();
        }
        /// <summary>
        /// مقایسه دو گره با هم
        /// </summary>
        /// <remarks>پیاده سازی اینترفیس قابل مقایسه بودن کلاس</remarks>
        public new int CompareTo(Vertex other)
        {
            if (other == null)
                throw new ArgumentNullException("other");

            return base.CompareTo(other);
        }
        
        /// <summary>
        /// عناوین زیرگروه های گروهی که این گره میزبان آن است را برمی گرداند؛
        /// این عناوین شامل عناوین گره های زیرمجموعه ایست که از آستانه تعیین شده برای نمایش عناوین کمتر باشند و در صورت اضافه بودن تنها تعداد اضافه ها نمایش داده خواهد شد
        /// </summary>
        public string SubGroupsTitles
        {
            get;
            protected set;
        }
        /// <summary>
        /// عناوین زیرگروه های گروهی که این گره میزبان آن است را بازنشانی می کند؛
        /// </summary>
        /// <remarks>
        /// این عناوین شامل عناوین گره های زیرمجموعه ایست که از آستانه تعیین شده برای
        /// نمایش عناوین کمتر باشند و در صورت اضافه بودن تنها «تعداد» اضافه ها نمایش
        /// داده خواهد شد
        /// </remarks>
        protected virtual void ResetSubGroupsTitles()
        {
            string subGroupsTitles = "";
            if (SubGroup.Count() == 0)
                subGroupsTitles = "(No member)";
            else
                if (SubGroup.Count() <= ShowingSubGroupTitlesTreshould)
                    foreach (var item in SubGroup)
                        subGroupsTitles += item.Text + Environment.NewLine;
                else
                {
                    for (int i = 0; i <= ShowingSubGroupTitlesTreshould; i++)
                        subGroupsTitles += SubGroup.ElementAt(i).Text + Environment.NewLine;
                    subGroupsTitles += string.Format("And {0} more object(s)", SubGroup.Count() - ShowingSubGroupTitlesTreshould);
                }
            SubGroupsTitles = subGroupsTitles;
            // صدور رخداد «ایجاد تغییر در ویژگی» برای اطلاع دادن تغییرات در ویژگی های شئ
            OnPropertyChanged();
        }

        /// <summary>
        /// آستانه تعیین شده برای تعداد عناوین زیرگروه هایی که برای عنوان گروه نمایش داده می شوند را نگهداری می کند
        /// </summary>
        private static int showingSubGroupTitlesTreshould = 6;
        /// <summary>
        /// آستانه تعیین شده برای تعداد عناوین زیرگروه هایی که برای عنوان گروه نمایش داده می شوند را مقداردهی می کند و برمی گرداند
        /// </summary>
        public static int ShowingSubGroupTitlesTreshould
        {
            get { return showingSubGroupTitlesTreshould; }
            set { showingSubGroupTitlesTreshould = value; }
        }
    }
}
