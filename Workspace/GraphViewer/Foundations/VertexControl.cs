using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GPAS.Graph.GraphViewer.Foundations
{
    public class VertexControl : GraphX.Controls.VertexControl
    {
        #region مدیریت رخدادها
        /// <summary>
        /// رخداد «خاتمه ی جمع شدن گروه»
        /// </summary>
        public event EventHandler<EventArgs> CollapseGroupCompleted;
        /// <summary>
        /// عملگر صدور رخداد «خاتمه ی جمع شدن گروه»
        /// </summary>
        protected virtual void OnCollapseGroupCompleted()
        {
            // تعیین وضعیت جمع/باز شدن متناسب
            CollapseStatus = CollapseState.GroupCollapsed;
            // صدور رخداد مربوطه در صورت نیاز
            if (CollapseGroupCompleted != null)
                CollapseGroupCompleted(this, EventArgs.Empty);
        }
        /// <summary>
        /// رخداد «خاتمه ی باز شدن گروه»
        /// </summary>
        public event EventHandler<EventArgs> ExpandGroupCompleted;
        /// <summary>
        /// عملگر صدور رخداد «خاتمه ی باز شدن گروه»
        /// </summary>
        protected virtual void OnExpandGroupCompleted()
        {
            // تعیین وضعیت جمع/باز شدن متناسب
            CollapseStatus = CollapseState.GroupExpanded;
            // صدور رخداد مربوطه در صورت نیاز
            if (ExpandGroupCompleted != null)
                ExpandGroupCompleted(this, EventArgs.Empty);
        }
        #endregion

        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register(nameof(IsSelected),
            typeof(bool), typeof(VertexControl), new PropertyMetadata(false, OnSetIsSelectedChanged));

        private static void OnSetIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((VertexControl)d).OnSetIsSelectedChanged(e);
        }

        private void OnSetIsSelectedChanged(DependencyPropertyChangedEventArgs e)
        {
            OnIsSelectedChanged(e);
        }

        public event DependencyPropertyChangedEventHandler IsSelectedChanged;

        protected void OnIsSelectedChanged(DependencyPropertyChangedEventArgs e)
        {
            IsSelectedChanged?.Invoke(this, e);
        }

        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <param name="relatedVertex">گرهی که می خواهیم برای آن کنترل (نمایشی) بسازیم</param>
        /// <param name="groupVertexIsCollapsed">آیا گره میزبان گروه بندی به صورت جمع شده نمایش داده شود یا خیر</param>
        public VertexControl(Vertex relatedVertex, bool groupVertexIsCollapsed = false)
            : base(relatedVertex)
        {
            if (relatedVertex == null)
                throw new ArgumentNullException("relatedVertex");

            IsGroup = Vertex is GroupMasterVertex;
            IsSourceSet = Vertex is SourceSetVertex;
            IsFrozen = false;
            CollapseStatus = (IsGroup)
                ? (groupVertexIsCollapsed ? CollapseState.GroupCollapsed : CollapseState.GroupExpanded)
                : CollapseState.NotAGroupMasterVertex;
        }

        /// <summary>
        /// وضعیت کنونی گره جمع/باز بودن گروهی که میزبانش این گره است
        /// </summary>
        public CollapseState CollapseStatus
        {
            get;
            private set;
        }
        /// <summary>
        /// وضعیت های مختلفی که یک گره برای نمایش جمع شده/باز بودن گروه برای میزبانی آن می تواند داشته باشد
        /// </summary>
        public enum CollapseState
        {
            NotAGroupMasterVertex,
            GroupCollapsing,
            GroupCollapsed,
            GroupExpanding,
            GroupExpanded
        }
        /// <summary>
        /// این عملکرد گروهی که این گره میزبان آن است و در وضعیت باز شده قرار دارد را جمع می کند
        /// </summary>
        public void CollapseGroup(GraphViewer masterViewer, bool collapseWithAnimation = true)
        {
            if (masterViewer == null)
                throw new ArgumentNullException("masterViewer");

            // وضعیت های غیر از گره میزبان گروه که در حالت باز شده (برعکس جمع شده) باشند را نمی پذیرد
            // به صورت غیرمستقیم، این شرط میزبان گروه بودن این گره را تضمین می کند
            if (CollapseStatus != CollapseState.GroupExpanded)
                return;
            // مقداردهی نمایشگر گراف میزبان جمع شدن گروه بندی
            masterGraphViewerForCollapse = masterViewer;
            // مقداردهی متغیر نشاندهنده نیاز به پویانمایی حین جمع شدن گروه
            isCurrentCollapsingWithAnimation = collapseWithAnimation;
            // نمایشگر گراف نال را نمی پذیرد
            if (masterGraphViewerForCollapse == null)
                throw new ArgumentNullException("masterViewer");
            // تعیین وضعیت جمع/باز شدن متناسب
            CollapseStatus = CollapseState.GroupCollapsing;
            // شروع گام های جمع شدن گره های میزبان گروه
            // برداشتن این گام ها برای گروه بندی های تودرتو لازم است؛
            // برای توضیحات بیشتر ر.ک. توضیحات عملکرد صدا زده شده
            DoCollapsingNextStep();
        }

        /// <summary>
        /// نمایشگر گرافی که برای نمایش جمع شدن گروه، میزبان این کنترل (نمایشی) گره است
        /// </summary>
        private GraphViewer masterGraphViewerForCollapse;
        /// <summary>
        /// گام بعدی جمع شدن گره میزان گروه را انجام می دهد
        /// </summary>
        /// <remarks>
        /// به خاطر وجود احتمال تودرتو بودن گروه بندی های انجام شده در گراف
        /// و همچنین نیاز کاربر به تشخیص اتفاقی که در جمع شدن گره ها می افتد،
        /// لازم است گروه ها به صورت گام به گام، از داخلی ترین گروه و همراه
        /// با پویا نمایی جمع شوند؛
        /// روش کار جمع شدن گروه در این گراف بدین صورت است که در هر گام داخل ترین
        /// گره میزبان داخلی ترین گروه شناسایی می شود و جمع شدن آن با پویانمایی
        /// انجام می گیرد.
        /// به خاطر غیرهمگام بودن این پویا نمایی، پس از صدور رخداد پایان
        /// پویانمایی، جمع شدن واقعی گره ها (حذف یال های داخلی گروه، نگاشت
        /// زیرگروه ها به میزبان گروه و...) صورت می گیرد.
        /// در نهایت گام بعدی جمع شدن به همین منوال تا رسیدن به گرهی ابتدایی
        /// ادامه خواهد یافت
        /// </remarks>
        private void DoCollapsingNextStep()
        {
            // شناسایی گرهی بعدی که می بایست جمع شود
            List<GroupMasterVertex> nextVertexToCollapsePath = NextVertexToCollapsePath(Vertex as GroupMasterVertex);
            // در صورت وجود گره میزبان گروه دیگری که می بایست جمع شود،
            if (nextVertexToCollapsePath.Count() != 0)
                // عملکرد شروع جمع شدن آن گره صدا زده می شود
                StartNastedGroupLeafCollapse(nextVertexToCollapsePath[0]);
            else
                // و در غیر اینصورت تلاش برای اصلاح موقعیت رسم یال ها
                masterGraphViewerForCollapse.area.UpdateAllEdges();
        }
        /// <summary>
        /// عملکرد بازگشتی یافتن مسیر گره (میزبان گروه) بعدی که می بایست جمع شود
        /// </summary>
        /// <remarks>
        /// این عملکرد براساس آخرین وضعیت جمع/باز بودن گره ها کار می کند.
        /// این عملکرد، گره میزبان گروهی که در دورترین مسیر سلسله مراتب گروه ها تا گره ورودی است را برمی گرداند
        /// </remarks>
        /// <param name="checkingVertex">گرهی که می خواهیم بررسی با محوریت آن انجام شود؛ مسیر برگردانده شده توسط این عملکرد به این گره منتهی خواهد شد</param>
        /// <returns>
        /// لیستی که مسیر سلسله مراتب گره های میربان گروه را از گرهی که در نوبت بعد باید جمع شود (اندیس صفر) تا گره ورودی را برمی گرداند؛
        /// در صورت عدم نیاز به جمع شدن گره ها، لیست خالی برگردانده می شود
        /// </returns>
        private List<GroupMasterVertex> NextVertexToCollapsePath(GroupMasterVertex checkingVertex)
        {
            if (checkingVertex == null)
                throw new ArgumentNullException("checkingVertex");

            // درصورتی که گره میزبان گروه (ورودی)، جمع شده باشد، یک لیست خالی برمی گرداند
            if (checkingVertex.RelatedVertexControl.CollapseStatus == CollapseState.GroupCollapsed)
                return new List<GroupMasterVertex>();
            // درغیراینصورت، اگر گروه فاقد زیرگروهی باشد که خودش گروه است،
            // لیستی شامل یک گره (ورودی) برگردانده می شود
            bool checkingVertexHasAGroupSubset = false;
            foreach (Vertex item in checkingVertex.SubGroup)
            {
                if (item is GroupMasterVertex)
                {
                    checkingVertexHasAGroupSubset = true;
                    break;
                }
            }
            if (!checkingVertexHasAGroupSubset)
            {
                List<GroupMasterVertex> result = new List<GroupMasterVertex>();
                result.Add(checkingVertex);
                return result;
            }
            // در غیراینصورت، مسیرهای رسیدن به گره های بعدی برای جمع شدن گره بررسی می شوند
            List<List<GroupMasterVertex>> nextVertexForCollapsePaths = new List<List<GroupMasterVertex>>();
            foreach (Vertex item in checkingVertex.SubGroup)
            {
                if (item is GroupMasterVertex)
                    nextVertexForCollapsePaths.Add(NextVertexToCollapsePath(item as GroupMasterVertex));
            }
            // و مسیر گره میزبان گروهی که در طولانی ترین مسیر از گره در حال بررسی (ورودی) تا خود گره در حال بررسی را بر می گرداند
            int longestPathLength = 0;
            foreach (var item in nextVertexForCollapsePaths)
            {
                if (item.Count > longestPathLength)
                    longestPathLength = item.Count;
            }
            List<GroupMasterVertex> longestPath = nextVertexForCollapsePaths[0];
            foreach (var item in nextVertexForCollapsePaths)
            {
                if (item.Count == longestPathLength)
                {
                    longestPath = item;
                    break;
                }
            }
            List<GroupMasterVertex> longestPathFromCheckingVertex = new List<GroupMasterVertex>();
            foreach (var item in longestPath)
            {
                longestPathFromCheckingVertex.Add(item);
            }
            longestPathFromCheckingVertex.Add(checkingVertex);
            return longestPathFromCheckingVertex;
        }
        /// <summary>
        /// گره میزبان گروهی که در حال حاضر در حال جمع شدن است؛
        /// این متغیر محلی برای پاس دادن گره در حال جمع شدن بین عملکردهای محلی استفاده می شود
        /// </summary>
        private GroupMasterVertex groupmastervertexCurrentlyCollapsing;
        // TODO: Clean!
        private bool isCurrentCollapsingWithAnimation = true;
        /// <summary>
        /// جمع شدن برگ یک سلسله مراتب گروه بندی (گره میزبان گروهی که نوبت جمع شدن آن است) را شروع می کند
        /// </summary>
        /// <param name="groupLeafToCollapse">گره میزبان گروهی که نوبت جمع شدن آن است؛ به عبارت دیگر در سلسله مراتب گروه بندی ها برگ محسوب می شود</param>
        private void StartNastedGroupLeafCollapse(GroupMasterVertex groupLeafToCollapse)
        {
            if (groupLeafToCollapse == null)
                throw new ArgumentNullException("groupLeafToCollapse");

            // ثبت موقعیت گره های زیرگروه نسبت به گره میزبان جهت بازیابی موقعیت گره در زمان باز گردن گروه
            groupLeafToCollapse.RelatedVertexControl.StoreGroupSubVerticesExpandState();
            // مقداردهی محلی متغیر نگهدارنده گره میزبان گروهی که در حال جمع شدن است
            groupmastervertexCurrentlyCollapsing = groupLeafToCollapse;
            // آماده سازی اجرای پویانمایی جمع شدن گره های گروه
            Dictionary<Vertex, GraphX.Measure.Point> verticesNewPosition = new Dictionary<Vertex, GraphX.Measure.Point>();
            foreach (var item in groupLeafToCollapse.SubGroup)
            {
                if (!masterGraphViewerForCollapse.IsVertexCurrentlyCollapsedByAGroup(item))
                {
                    verticesNewPosition.Add(item, new GraphX.Measure.Point(groupLeafToCollapse.RelatedVertexControl.GetPosition().X, groupLeafToCollapse.RelatedVertexControl.GetPosition().Y));
                }
            }
            masterGraphViewerForCollapse.VerticesMoveAnimationCompleted += MasterGraphViewer_CollapseMoveAnimationCompleted;
            if (isCurrentCollapsingWithAnimation)
            {
                // شروع اجرای پویا نمایی جمع شدن گره های زیرمجموعه گروه
                masterGraphViewerForCollapse.AnimateVerticesMove(verticesNewPosition);
            }
            else
            {
                foreach (var item in verticesNewPosition)
                {
                    item.Key.RelatedVertexControl.SetPosition(item.Value.X, item.Value.Y);
                }
                CompleteNastedGroupLeafCollapse();
            }
        }
        /// <summary>
        /// رخدادگردان خاتمه اجرای پویانمایی جمع شدن زیرگروه های یک گره میزبان گروه
        /// </summary>
        private void MasterGraphViewer_CollapseMoveAnimationCompleted(object sender, EventArgs e)
        {
            CompleteNastedGroupLeafCollapse();
        }
        // TODO: Clean!
        private void CompleteNastedGroupLeafCollapse()
        {
            // از آنجایی که این رخداد مربوط به نمایشگر گراف میزان این جمع شدن است
            // و احتمال صدور آن به دلایلی غیر از جمع شدن این گره وجود دارد
            // وضعیت جاری اجرای جمع/باز شدن گروه بررسی می شود؛
            // در صورتی که وضعیت، خاتمه پویا نمایی جمع شدن گروه را نشان دهد،
            if (CollapseStatus == CollapseState.GroupCollapsing)
            {
                // عملکرد تکمیل جمع شدن برگ یک سلسله مراتب گروه بندی برای گره در حال جمع شدن صدا زده می شود
                FinalizeNastedGroupLeafCollapse(groupmastervertexCurrentlyCollapsing);
            }
        }
        /// <summary>
        /// جمع شدن برگ یک سلسله مراتب گروه بندی (گره میزبان گروهی که نوبت جمع شدن آن است) را تکمیل می کند
        /// </summary>
        /// <param name="nastedGroupLeafToCollapse"></param>
        /// <remarks>پس از اجرای امور نمایشی جمع شدن گروه (که زمان اجرای این عملکرد است)، نوبت به جمع شدن واقعی زیرگروه هاست</remarks>
        private void FinalizeNastedGroupLeafCollapse(GroupMasterVertex nastedGroupLeafToCollapse)
        {
            if (nastedGroupLeafToCollapse == null)
                throw new ArgumentNullException("nastedGroupLeafToCollapse");

            // خاتمه دادن به گرداندن رخداد تکمیل اجرای پویانمایی گره ها
            masterGraphViewerForCollapse.VerticesMoveAnimationCompleted -= MasterGraphViewer_CollapseMoveAnimationCompleted;
            // تکمیل جمع شدن واقعی گروه:
            // دریافت یال های داخلی گروه
            List<Edge> groupInternalEdges = GetGroupInternalEdges(masterGraphViewerForCollapse, nastedGroupLeafToCollapse);
            // ثبت یال های داخلی گروه برای استفاده در زمان باز کردن گروه
            nastedGroupLeafToCollapse.RelatedVertexControl.StoreGroupInternaEdges(groupInternalEdges);
            // حذف یال های داخلی گروه از روی گراف
            foreach (var item in groupInternalEdges)
            {
                item.RelatedEdgeControl.Visibility = Visibility.Collapsed;
                item.RelatedEdgeControl.PrepareEdgePath();
            }
            // نگاشت زیرگروه ها با میزبان گروه
            foreach (var item in nastedGroupLeafToCollapse.SubGroup
                .Where(s => !masterGraphViewerForCollapse.IsVertexCurrentlyCollapsedByAGroup(s)))
            {
                CollapseMapping(item, nastedGroupLeafToCollapse);
            }
            // یال های مرتبط با زیرگروه ها را ماسک می کند؛ مبدا و مقصد جدید (غیرواقعی) برای یال تعریف می شود
            // برای توضیحات بیشتر ر.ک. مستندات توابع ماسکی که در قطعه کد زیر صدا زده شده است
            foreach (var currentSubGroup in nastedGroupLeafToCollapse.SubGroup
                    .Where(s => !masterGraphViewerForCollapse.IsVertexCurrentlyCollapsedByAGroup(s)))
            {
                foreach (var currentEdgeRelatedToSubGroup in masterGraphViewerForCollapse.GetRelatedEdges(currentSubGroup))
                {
                    if (groupInternalEdges.Contains(currentEdgeRelatedToSubGroup))
                        continue;
                    if (currentEdgeRelatedToSubGroup.Source == currentSubGroup)
                        currentEdgeRelatedToSubGroup.CollapseMask(nastedGroupLeafToCollapse, currentEdgeRelatedToSubGroup.Target, masterGraphViewerForCollapse);
                    else if (currentEdgeRelatedToSubGroup.Target == currentSubGroup)
                        currentEdgeRelatedToSubGroup.CollapseMask(currentEdgeRelatedToSubGroup.Source, nastedGroupLeafToCollapse, masterGraphViewerForCollapse);
                    else
                        throw new InvalidProgramException();
                }
            }
            // پس از ماسک کردن یال ها، محل اتصال یال به گره جدید به روز نمی شود (و در آخرین
            // نقطه ای که با گره قبل از ماسک اتصال داشته باقی می ماند) و تا زمانی که گره جدید
            // جابجا نشود در همان نقطه خواهد ماند. برای دور زدن این مشکل، گره ماسک شده (میزبان
            // گروه) را به صورت دستی، یک واحد جابجا و برمی گردانیم تا مشکل حل شود
            nastedGroupLeafToCollapse.RelatedVertexControl.SetPosition(nastedGroupLeafToCollapse.RelatedVertexControl.GetPosition().X + 1, nastedGroupLeafToCollapse.RelatedVertexControl.GetPosition().Y);
            nastedGroupLeafToCollapse.RelatedVertexControl.SetPosition(nastedGroupLeafToCollapse.RelatedVertexControl.GetPosition().X - 1, nastedGroupLeafToCollapse.RelatedVertexControl.GetPosition().Y);
            // پنهان کردن گره های زیرگروه از روی گراف؛
            // نمایشگر گراف برای نمایش یک گره نیاز دارد تا یک کنترل (نمایشی) گره «ایجاد» کند
            // و نمی تواند کنترل گره های قبلا ایجاد شده را بپذیرد به همین دلیل
            // در صورتی که این گره ها از روی گراف حذف شوند، در زمان باز شدن گروه
            // تنظیمات جمع شدن گروه از بین می رود،
            // در نتیجه به مخفی کردن گره ها به جای حذف آن ها از گراف بسنده شده است
            foreach (var item in nastedGroupLeafToCollapse.SubGroup
                    .Where(s => !masterGraphViewerForCollapse.IsVertexCurrentlyCollapsedByAGroup(s)))
            {
                item.RelatedVertexControl.Visibility = Visibility.Collapsed;
                // بر همین اساس به خاطر اینکه گره ها واقعا از روی گراف حذف نشده اند،
                // برای جلوگیری از تداخل های احتمالی، همه گره هایی که دیگر دیده نخواهند شد
                // از حالت انتخاب خارج می شوند
                masterGraphViewerForCollapse.DeselectVertices(new Vertex[] { item });
            }
            // صدور رخداد تکمیل جمع شدن گروه برای گره میزبان گروهی که کار جمع شدن آن به پایان رسیده
            nastedGroupLeafToCollapse.RelatedVertexControl.OnCollapseGroupCompleted();
            // فراخوانی اجرای گام بعدی جمع شدن گروه
            DoCollapsingNextStep();
        }

        /// <summary>
        /// لیست نگهداری موقعیت گره های زیرگروهی که جمع شده اند، نسبت به گره میزبان آن گروه 
        /// </summary>
        private Dictionary<Vertex, Point> groupSubVerticesPositionRelatedToMaster = new Dictionary<Vertex, Point>();
        /// <summary>
        /// لیست نگهداری موقعیت گره های زیرگروهی که جمع شده اند، نسبت به گره میزبان گروه (این گره) را برمی گرداند
        /// </summary>
        public Dictionary<Vertex, Point> CollapsedSubGroupsExpandedModePositionRelatedToMaster
        {
            get { return groupSubVerticesPositionRelatedToMaster; }
        }
        /// <summary>
        /// موقعیت گره های زیرگروه نسبت به گره میزبان آن گروه را ثبت می کند
        /// </summary>
        private void StoreGroupSubVerticesExpandState()
        {
            if (groupSubVerticesPositionRelatedToMaster.Count > 0)
            {
                groupSubVerticesPositionRelatedToMaster.Clear();
            }
            foreach (var item in ((GroupMasterVertex)Vertex).SubGroup)
            {
                groupSubVerticesPositionRelatedToMaster.Add
                    (item, new Point(item.RelatedVertexControl.GetPosition().X - GetPosition().X, item.RelatedVertexControl.GetPosition().Y - GetPosition().Y));
            }
        }
        ///// <summary>
        ///// موقعیت گره های زیرگروه نسبت به گره میزبان آن گروه در حالت باز شده را برمی گرداند
        ///// </summary>
        //public List<KeyValuePair<Vertex, Point>> GroupSubVerticesPositionRelatedToMaster
        //{
        //    get { return groupSubVerticesPositionRelatedToMaster; }
        //}

        /// <summary>
        /// لیست نگهداری یال های داخلی گره های هم گروه؛
        /// این لیست شامل یال هایی خواهد بود که مبدا و مقصدشان (هر دو) در گروه هستند
        /// </summary>
        private List<Edge> groupInternalEdges;
        /// <summary>
        /// یال های داخلی گره های هم گروه را دریافت و ثبت می کند
        /// </summary>
        private void StoreGroupInternaEdges(List<Edge> subGroupEdgesToStore)
        {
            if (subGroupEdgesToStore == null)
                throw new ArgumentNullException("subGroupEdgesToStore");

            if (groupInternalEdges != null && groupInternalEdges.Count > 0)
            {
                groupInternalEdges.Clear();
            }
            groupInternalEdges = subGroupEdgesToStore;
        }
        /// <summary>
        /// یال های داخلی گروه را براساس آنچه قبلا ذخیره شده بازیابی می کند؛
        /// گره های زیرگروه می بایست قبل از صدار زدن این عملکرد روی نمایشگر گراف وجود داشته باشند
        /// </summary>
        /// <param name="graphviewerToAddEdges">نمایشگر گرافی که بازیابی یال های داخلی گروه روی آن در حال انجام شدن است</param>
        private void RestoreGroupInternalEdges(GraphViewer graphviewerToAddEdges)
        {
            if (graphviewerToAddEdges == null)
                throw new ArgumentNullException("graphviewerToAddEdges");
            if (groupInternalEdges == null)
                throw new NullReferenceException("Internal error 'groupInternalEdges' not set before");

            foreach (var item in groupInternalEdges)
            {
                //item.RegenerateRelatedEdgeControl();
                //graphviewerToAddEdges.AddEdge(item);
                item.RelatedEdgeControl.Visibility = Visibility.Visible;
            }
        }
        /// <summary>
        /// لیست گره های داخلی یک گروه را برمی گرداند؛
        /// این لیست شامل همه یال هایی خواهد بود که مبدا و مقصدشان (هر دو) در گروه هستند
        /// </summary>
        /// <param name="graphviewerMaster">نمایشگر گرافی گروه در حال نمایش روی آن است</param>
        /// <param name="vertexMastersTheGroup">گره میزبان گروه</param>
        internal List<Edge> GetGroupInternalEdges(GraphViewer graphviewerMaster, GroupMasterVertex vertexMastersTheGroup)
        {
            if (graphviewerMaster == null)
                throw new ArgumentNullException("graphviewerMaster");
            if (vertexMastersTheGroup == null)
                throw new ArgumentNullException("vertexMastersTheGroup");

            List<Edge> groupInternalEdges = new List<Edge>();
            // بررسی تک تک زیرگروه ها برای روابطشان با میزبان و دیگر اعضای گروه
            for (int i = 0; i < vertexMastersTheGroup.SubGroup.Count(); i++)
            {
                // بررسی رابطه/روابط با میزبان گروه
                groupInternalEdges.AddRange
                    (graphviewerMaster.GetEdgesBetween(vertexMastersTheGroup.SubGroup.ElementAt(i), vertexMastersTheGroup));
                // بررسی رابطه/روابط با میزبان دیگر اعضای گروه (که رابطه داشتن شان بررسی نشده)
                for (int j = i; j < vertexMastersTheGroup.SubGroup.Count(); j++)
                {
                    groupInternalEdges.AddRange
                        (graphviewerMaster.GetEdgesBetween(vertexMastersTheGroup.SubGroup.ElementAt(i), vertexMastersTheGroup.SubGroup.ElementAt(j)));
                }
            }
            return groupInternalEdges;
        }
        /// <summary>
        /// جدول نگاشت گره ها برای پیاده سازی جمع شدن گره های گروه بندی شده
        /// </summary>
        private static Dictionary<Vertex, Vertex> collapseVerticesMapTable = new Dictionary<Vertex, Vertex>();
        /// <summary>
        /// عملکرد نگاشت گره ها برای پیاده سازی جمع شدن گره های گروه
        /// </summary>
        /// <param name="vertexToCollapse">گرهی که می بایست جمع شود</param>
        /// <param name="vertexToShowInstead">گرهی که جایگزین نمایشی آن خواهد بود</param>
        private void CollapseMapping(Vertex vertexToCollapse, Vertex vertexToShowInstead)
        {
            if (vertexToCollapse == null)
                throw new ArgumentNullException("vertexToCollapse");
            if (vertexToShowInstead == null)
                throw new ArgumentNullException("vertexToShowInstead");

            // افزودن نگاشت درخواست شده به جدول نگاشت های مربوطه
            collapseVerticesMapTable.Add(vertexToCollapse, vertexToShowInstead);
        }
        /// <summary>
        /// گره بعدی جایگزین برای یک گره نگاشت شده را برمی گرداند؛
        /// از این جهت جایگزین بعدی است که نگاشت ها می تواند سلسله مراتبی انجام شده باشند
        /// </summary>
        /// <param name="collapsedVertexToGetItsShowingAlternate">گرهی که به دنبال جایگزین بعدی آن در نگاشت ها هستیم</param>
        private Vertex GetCollapseMappingNextAlternate(Vertex collapsedVertexToGetItsShowingAlternate)
        {
            if (collapsedVertexToGetItsShowingAlternate == null)
                throw new ArgumentNullException("collapsedVertexToGetItsShowingAlternate");

            Vertex nextLevelMappingAlternate;
            if (!collapseVerticesMapTable.TryGetValue(collapsedVertexToGetItsShowingAlternate, out nextLevelMappingAlternate))
            {
                throw new ArgumentOutOfRangeException(collapsedVertexToGetItsShowingAlternate.GetType().ToString(), "Vertex is not set stored before in Collapse-Mapping table");
            }
            return nextLevelMappingAlternate;
        }
        /// <summary>
        /// حذف نگاشت هایی که به یک گره خاص شده است
        /// </summary>
        /// <param name="vertexToRemoveMapping">گرهی که می خواهیم نگاشت های مختوم به آن را حذف کنیم</param>
        private void RemoveMappingsEndsWith(Vertex vertexToRemoveMapping)
        {
            if (vertexToRemoveMapping == null)
                throw new ArgumentNullException("vertexToRemoveMapping");

            for (int i = collapseVerticesMapTable.Count; i > 0; i--)
            {
                if (collapseVerticesMapTable.ElementAt(i - 1).Value == vertexToRemoveMapping)
                {
                    collapseVerticesMapTable.Remove(collapseVerticesMapTable.ElementAt(i - 1).Key);
                }
            }
        }

        /// <summary>
        /// باز کردن گروهی که قبلا جمع شده است و این گره میزبان آن گروه است
        /// </summary>
        /// <remarks>
        /// عملکرد باز شدن گروه برعکس جمع شدن آن، گام به گام نیست
        /// و برای هر گره، تنها به باز شدن خود آن گره می انجامد؛
        /// در نتیجه، ابتدا باز شدن واقعی گروه (شامل نمایش گره های
        /// زیرگروه، یال های داخلی، بازیابی نگاشت ها و...) صورت
        /// می گیرد و سپس باز شدن نمایشی که همان پویانمایی باز شدن است
        /// </remarks>
        public void ExpandGroup(GraphViewer masterViewer, bool expandWithAnimation = true)
        {
            if (masterViewer == null)
                throw new ArgumentNullException("masterViewer");

            // بررسی صحت وضعیت جمع/باز شدن گروه؛
            // به صورت غیرمستقیم، این شرط میزبان گروه بودن این گره را تضمین می کند
            if (CollapseStatus != CollapseState.GroupCollapsed)
                return;
            // در صورتی که صحت وضعیت می بایست «نمایشگر گراف برای جمع شدن گروه» قبلا مقداردهی شده باشد
            // نمایشگر گراف نال را نمی پذیرد
            if (masterViewer == null)
                throw new ArgumentNullException("masterViewer");
            // تعیین وضعیت جمع/باز شدن متناسب
            CollapseStatus = CollapseState.GroupExpanding;
            // باز شدن واقعی شامل:
            // افزودن زیرگروه ها به گراف
            foreach (var item in (Vertex as GroupMasterVertex).SubGroup
                .Where(v => !masterViewer.IsVertexCurrentlyCollapsedByAGroup(v, Vertex as GroupMasterVertex)))
            {
                item.RelatedVertexControl.SetPosition(GetPosition().X, GetPosition().Y);
                item.RelatedVertexControl.Visibility = Visibility.Visible;
            }
            // برای هریک از روابط کنونی گره میزبان - که در حال حاضر جمع شده است
            foreach (var item in masterViewer.GetRelatedEdges(Vertex as GroupMasterVertex))
            {
                // بررسی شروط لازم
                bool isGroupMasterTheSourceVertexOfEdge = (item.Source == (Vertex as GroupMasterVertex));
                bool isGroupMasterTheTargetVertexOfEdge = (item.Target == (Vertex as GroupMasterVertex));
                bool isGroupMasterOrGroupMembersTheRealSourceOfEdge =
                    (!item.IsCollapseMask()
                    || (isGroupMasterTheSourceVertexOfEdge
                        && (item.RealSource == (Vertex as GroupMasterVertex) || (Vertex as GroupMasterVertex).SubGroup.Contains(item.RealSource))));
                bool isGroupMasterOrGroupMembersTheRealTargetOfEdge =
                    (!item.IsCollapseMask()
                    || (isGroupMasterTheTargetVertexOfEdge
                        && (item.RealTarget == (Vertex as GroupMasterVertex) || (Vertex as GroupMasterVertex).SubGroup.Contains(item.RealTarget))));
                // در صورت رسیدن به مبدا یا مقصد واقعی یال ...
                if (isGroupMasterOrGroupMembersTheRealSourceOfEdge)
                    item.RemoveCollapseMaskSource(masterViewer);
                else if (isGroupMasterOrGroupMembersTheRealTargetOfEdge)
                    item.RemoveCollapseMaskTarget(masterViewer);
                else
                {
                    // در غیر اینصورت، یافتن جایگزین (غیر واقعی) بعدی
                    Vertex nextAlternate;
                    if (isGroupMasterTheSourceVertexOfEdge)
                        nextAlternate = GetCollapseMappingNextAlternate(item.RealSource);
                    else if (isGroupMasterTheTargetVertexOfEdge)
                        nextAlternate = GetCollapseMappingNextAlternate(item.RealTarget);
                    else
                        throw new InvalidProgramException();
                    // پیمایش سلسله نگاشت ها تا رسیدن به جایگزینی که از زیرگروه های گروه باشد
                    while (!(Vertex as GroupMasterVertex).SubGroup.Contains(nextAlternate) && nextAlternate != (Vertex as GroupMasterVertex))
                        nextAlternate = GetCollapseMappingNextAlternate(nextAlternate);
                    // تغییر ماسک یال به مقدار جایگزین که یکی از زیرگروه های کنونی گروه می باشد
                    if (isGroupMasterTheSourceVertexOfEdge)
                        item.CollapseMask(nextAlternate, item.Target, masterViewer);
                    else if (isGroupMasterTheTargetVertexOfEdge)
                        item.CollapseMask(item.Source, nextAlternate, masterViewer);
                    else
                        throw new InvalidProgramException();
                }
            }
            // حذف نگاشت های مختوم به میزبان گروه
            RemoveMappingsEndsWith(Vertex as GroupMasterVertex);
            // بازنشانی یال های داخلی گروه
            RestoreGroupInternalEdges(masterViewer);
            // رفتن زیرگروه ها به حالت انتخاب شده، در صورت در حالت انتخاب بودن میزبان گروه
            if (((Vertex)Vertex).IsSelected)
            {
                foreach (var item in ((GroupMasterVertex)Vertex).SubGroup)
                {
                    masterViewer.SelectVertices(new Vertex[] { item });
                }
            }
            // باز کردن ظاهری گروه شامل:
            // آماده سازی پویانمایی باز شدن گره های گروه
            Dictionary<Vertex, GraphX.Measure.Point> verticesNewPosition = new Dictionary<Vertex, GraphX.Measure.Point>();
            foreach (var item in ((GroupMasterVertex)Vertex).SubGroup
                    .Where(v => !masterViewer.IsVertexCurrentlyCollapsedByAGroup(v, Vertex as GroupMasterVertex)))
            {
                foreach (var innerItem in groupSubVerticesPositionRelatedToMaster)
                {
                    if (item == innerItem.Key)
                    {
                        verticesNewPosition.Add(item, new GraphX.Measure.Point(GetPosition().X + innerItem.Value.X, GetPosition().Y + innerItem.Value.Y));
                        break;
                    }
                }
            }
            groupSubVerticesPositionRelatedToMaster.Clear();
            masterViewer.VerticesMoveAnimationCompleted += MasterGraphViewer_ExpandMoveAnimationCompleted;
            if (expandWithAnimation)
                // اجرای پویانمایی باز شدن گره های گروه
                masterViewer.AnimateVerticesMove(verticesNewPosition);
            else
            {
                foreach (var item in verticesNewPosition)
                {
                    item.Key.RelatedVertexControl.SetPosition(item.Value.X, item.Value.Y);
                }
                FinalizeExpand(masterViewer);
            }
        }
        /// <summary>
        /// رخدادگردان خاتمه اجرای پویانمایی باز شدن زیرگروه های یک گره میزبان گروه
        /// </summary>
        private void MasterGraphViewer_ExpandMoveAnimationCompleted(object sender, EventArgs e)
        {
            FinalizeExpand(sender as GraphViewer);
        }
        // TODO: Clean!
        private void FinalizeExpand(GraphViewer masterViewer)
        {
            if (masterViewer == null)
                throw new ArgumentNullException("masterViewer");

            // از آنجایی که این رخداد مربوط به نمایشگر گراف میزان این باز شدن است
            // و احتمال صدور آن به دلایلی غیر از باز شدن این گره وجود دارد
            // وضعیت جاری اجرای جمع/باز شدن گروه بررسی می شود؛
            // در صورتی که وضعیت، خاتمه پویا نمایی باز شدن گروه را نشان دهد،
            if (CollapseStatus == CollapseState.GroupExpanding)
            {
                // خاتمه دادن به گرداندن رخداد تکمیل اجرای پویانمایی گره ها
                masterViewer.VerticesMoveAnimationCompleted -= MasterGraphViewer_ExpandMoveAnimationCompleted;
                // صدور رخداد تکمیل باز شدن گروه (برای گره جاری که میزبان گروه است)
                OnExpandGroupCompleted();
            }
        }

        /// <summary>
        /// نشان می دهد که گره مربوط به این کنترل، مربوط به یک گروه است یا خیر
        /// </summary>
        public bool IsGroup
        {
            get { return (bool)GetValue(IsGroupProperty); }
            private set { SetValue(IsGroupProperty, value); }
        }

        /// <summary>
        /// ویژگی استقلال نشاندهنده میزبانی یک گروه توسط گره مربوط به این کنترل
        /// </summary>
        public static readonly DependencyProperty IsGroupProperty =
            DependencyProperty.Register(nameof(IsGroup), typeof(bool), typeof(VertexControl));


        public bool IsSourceSet
        {
            get { return (bool)GetValue(IsSourceSetProperty); }
            private set { SetValue(IsSourceSetProperty, value); }
        }

        public static readonly DependencyProperty IsSourceSetProperty =
           DependencyProperty.Register(nameof(IsSourceSet), typeof(bool), typeof(VertexControl));

        /// <summary>
        /// نشان می دهد که گره مربوط به این کنترل، در وضعیت منجمد هست یا خیر؛
        /// در وضعیت منجمد، گره قابل انتخاب و جابجایی نیست
        /// </summary>
        public bool IsFrozen
        {
            get { return (bool)GetValue(IsFrozenProperty); }
            set { SetValue(IsFrozenProperty, value); }
        }
        /// <summary>
        /// ویژگی استقلال نشاندهنده انجماد گره مربوط به این کنترل
        /// </summary>
        public static readonly DependencyProperty IsFrozenProperty =
            DependencyProperty.Register(nameof(IsFrozen), typeof(bool), typeof(VertexControl));

        public ImageDetails ImageDetails
        {
            get { return (ImageDetails)GetValue(ImageDetailsProperty); }
            set
            {
                // از آنجایی که تنها استفاده کننده‌ی این ویژگی، شرط زیر را برای همه‌ی
                // گره‌ها بررسی می‌کند، برای جلوگیری از سربار پردازشی، این شرط غیرفعال
                // شده است
                //if ((ImageDetails)GetValue(ImageDetailsProperty) == value)
                //    return;
                SetValue(ImageDetailsProperty, value);
            }
        }

        public static readonly DependencyProperty ImageDetailsProperty =
            DependencyProperty.Register(nameof(ImageDetails), typeof(ImageDetails), typeof(VertexControl));

        public static ImageDetails GetImageDetails(DependencyObject obj)
        {
            return (ImageDetails)obj.GetValue(ImageDetailsProperty);
        }



        public bool IsMaster
        {
            get { return (bool)GetValue(IsMasterProperty); }
            set { SetValue(IsMasterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsMaster.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsMasterProperty =
            DependencyProperty.Register(nameof(IsMaster), typeof(bool), typeof(VertexControl), new PropertyMetadata(false));




        public bool IsSlave
        {
            get { return (bool)GetValue(IsSlaveProperty); }
            set { SetValue(IsSlaveProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSlave.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSlaveProperty =
            DependencyProperty.Register(nameof(IsSlave), typeof(bool), typeof(VertexControl), new PropertyMetadata(false));



        public int NumberOfInnerVertices
        {
            get { return (int)GetValue(NumberOfInnerVerticesProperty); }
            set { SetValue(NumberOfInnerVerticesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NumberOfInnerVertices.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NumberOfInnerVerticesProperty =
            DependencyProperty.Register(nameof(NumberOfInnerVertices), typeof(int), typeof(VertexControl), new PropertyMetadata(0));


    }
}