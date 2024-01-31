using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GPAS.HistogramViewer
{
    class HistogramPropertyNode : HistogramNode
    {
        /// <summary>
        /// سازنده کلاس
        /// ویژگی به صورت خودکار به گروه مربوطه اش اضافه می شود
        /// </summary>
        internal HistogramPropertyNode(string PropertyTitle, HistogramGroupNode HeadGroup)
            : base(PropertyTitle)
        {
            relatedLabel.Background = Appearance.Properties.RowBackground;
            relatedLabel.Foreground = Appearance.Properties.RowForeground;
            relatedLabel.FontSize = Appearance.Properties.RowFontSize;
            relatedLabel.FontWeight = Appearance.Properties.RowFontWeight;
            relatedLabel.MouseLeftButtonDown += relatedLabel_MouseLeftButtonDown;
            RelatedLabel.MouseLeftButtonUp += RelatedLabel_MouseLeftButtonUp;
            valueCounts = new List<HistogramPropertyValueNode>();
            Max = 0;
            MaxSecond = 0;
            NodeMouseLeftButtonDown += NodeMouseLeftButtonDown_EmptyEventHandler;
            NodeMouseLeftButtonUp += NodeMouseLeftButtonUp_EmptyEventHandler;
            group = HeadGroup;
            group.AddASubProperty(this);

            OrderByChanged += HistogramPropertyNode_OrderByChanged;
            ValueCountAdded += HistogramPropertyNode_ValueCountAdded;
        }

        #region رخدادها و رخداد گردان ها
        void RelatedLabel_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            NodeMouseLeftButtonUp.Invoke(this, e);
        }
        void relatedLabel_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            NodeMouseLeftButtonDown.Invoke(this, e);
        }

        private void HistogramPropertyNode_OrderByChanged(object source, HistogramCategoryOrderByChangedEventArgs e)
        {
            HistogramValueCountsOrderBy(e.NewValue);
        }

        private void HistogramPropertyNode_ValueCountAdded(object sender, HistogramPropertyNodeValueCountItemsAddedEventArgs e)
        {
            HistogramValueCountsOrderBy(OrderBy);
        }
        /// <summary>
        /// زمانی رخ می دهد که موس روی گره فشرده شده باشد
        /// </summary>
        public event HistogramPropertyNodeMouseButtonEventHandler NodeMouseLeftButtonDown;
        /// <summary>
        /// زمانی رخ می دهد که موس روی گره رها شود
        /// </summary>
        public event HistogramPropertyNodeMouseButtonEventHandler NodeMouseLeftButtonUp;

        public event HistogramCategoryOrderByChangedHandler OrderByChanged;

        /// <summary>
        /// ساختار رخدادگردان عمل کلیک موس روی این نوع گره
        /// </summary>
        public delegate void HistogramPropertyNodeMouseButtonEventHandler
            (HistogramPropertyNode sender, System.Windows.Input.MouseButtonEventArgs e);

        static void NodeMouseLeftButtonUp_EmptyEventHandler(object sender, System.Windows.Input.MouseEventArgs e)
        { }
        static void NodeMouseLeftButtonDown_EmptyEventHandler(object sender, System.Windows.Input.MouseEventArgs e)
        { }
        #endregion

        /// <summary>
        /// گروه دربرگیرنده ی این ویژگی
        /// </summary>
        private HistogramGroupNode group;
        /// <summary>
        /// گروهی که این ویژگی زیرمجموعه آن است برمیگرداند
        /// </summary>
        internal HistogramGroupNode Group
        {
            get { return group; }
        }

        private HistogramPropertyNodeOrderBy orderBy;
        public HistogramPropertyNodeOrderBy OrderBy
        {
            get { return orderBy; }
            set
            {
                if(value != orderBy)
                {
                    OrderByChanged.Invoke(this, new HistogramCategoryOrderByChangedEventArgs(orderBy, value));
                }
                orderBy = value;
            }
        }

        /// <summary>
        /// گره های مقدار-تعداد مربوط به این ویژگی
        /// </summary>
        private List<HistogramPropertyValueNode> valueCounts;
        /// <summary>
        /// لیست گره های مقدار-تعداد مربوط به این ویژگی را برمیگرداند
        /// </summary>
        internal List<HistogramPropertyValueNode> ValueCounts
        {
            get { return valueCounts; }
        }

        private int Max, MaxSecond;

        public event EventHandler<HistogramPropertyNodeValueCountItemsAddedEventArgs> ValueCountAdded;
        /// <summary>
        /// یک گره به لیست گره های مقدار-تعداد مربوط به این ویژگی می افزاید
        /// </summary>
        /// <remarks>این عملگر مرتب بودن لیست براساس تعداد مربوط به مقدار را تضمین می کند</remarks>
        /// <param name="ValueCountNodeToAdd">گرهی که قرارا است به لیست مقدار-تعدادهای زیر مجموعه افزوده شود</param>
        internal void AddValueCountNodeCollection(IEnumerable<HistogramPropertyValueNode> ValueCountNodeToAddCollection)
        {
            List<HistogramPropertyValueNode> ValueCountNodeToAddCollectionSuccesed = new List<HistogramPropertyValueNode>();

            foreach (HistogramPropertyValueNode ValueCountNodeToAdd in ValueCountNodeToAddCollection)
            {
                int indexOfNewNode = 0;
                if (!ValueCountNodeToAdd.IsTopMostNode)
                    for (; indexOfNewNode < valueCounts.Count; indexOfNewNode++)
                        if (ValueCountNodeToAdd.Count >= valueCounts[indexOfNewNode].Count && !valueCounts[indexOfNewNode].IsTopMostNode)
                            break;

                valueCounts.Insert(indexOfNewNode, ValueCountNodeToAdd);
                ValueCountNodeToAddCollectionSuccesed.Add(ValueCountNodeToAdd);
            }

            ValueCountAdded.Invoke(this, new HistogramPropertyNodeValueCountItemsAddedEventArgs(ValueCountNodeToAddCollectionSuccesed));

            if (valueCounts == null)
                return;
            if (valueCounts.Count == 0)
                return;

            if (ValueCounts.Count > 1)
            {
                var vca = ValueCounts.OrderByDescending(vc => vc.Count).ToList();
                Max = vca[0].Count;
                MaxSecond = vca[1].Count;
            }
            else
            {
                Max = ValueCounts[0].Count;
                MaxSecond = Max;
            }
        }

        /// <summary>
        /// کوچکترین تعداد قابل نمایش برای زوج های «مقدار-تعداد» را برمیگرداند
        /// </summary>
        internal int MinimumShowingCount
        {
            get
            {
                return 0;
                /*
                if (valueCounts == null)
                    return 0;
                if(valueCounts.Count == 0)
                    return 0;
                return valueCounts[valueCounts.Count - 1].Count;
                 */
            }
        }

        /// <summary>
        /// بزرگترین تعداد قابل نمایش برای زوج های «مقدار-تعداد» را برمیگرداند
        /// </summary>
        internal int MaximumShowingCount
        {
            get
            {
                if (valueCounts == null)
                    return 0;
                if (valueCounts.Count == 0)
                    return 0;
                //if (valueCounts.Count >= 4 && valueCounts[2].Count > valueCounts[3].Count * 1.8)
                //    return Convert.ToInt32(Math.Round(valueCounts[3].Count * 1.05));
                //if (valueCounts.Count >= 3 && valueCounts[1].Count > valueCounts[2].Count * 1.6)
                //    return Convert.ToInt32(Math.Round(valueCounts[2].Count * 1.05));

                if (valueCounts.Count >= 2 && Max > MaxSecond * 1.4)
                    return Convert.ToInt32(Math.Round(MaxSecond * 1.05));

                return Max;
            }
        }

        public void HistogramValueCountsOrderBy(HistogramPropertyNodeOrderBy orderBy)
        {
            if (orderBy == HistogramPropertyNodeOrderBy.Count)
            {
                valueCounts = ValueCounts.OrderByDescending(vc => vc.IsTopMostNode).
                    ThenByDescending(vc => vc.Count).ThenBy(vc => vc.Title).ToList();
            }
            else if (orderBy == HistogramPropertyNodeOrderBy.Title)
            {
                valueCounts = ValueCounts.OrderByDescending(vc => vc.IsTopMostNode).
                    ThenBy(vc => vc.Title).ThenByDescending(vc => vc.Count).ToList();
            }
            else { }
        }
    }
}