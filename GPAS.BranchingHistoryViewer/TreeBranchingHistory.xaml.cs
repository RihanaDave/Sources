using GPAS.BranchingHistoryViewer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GPAS.BranchingHistoryViewer
{
    /// <summary>
    /// Interaction logic for TreeBranchingHistory.xaml
    /// </summary>
    public partial class TreeBranchingHistory : UserControl
    {
        public ThemeApplication CurrentTheme
        {
            get { return (ThemeApplication)GetValue(CurrentThemeProperty); }
            set { SetValue(CurrentThemeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentTheme.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentThemeProperty =
            DependencyProperty.Register(nameof(CurrentTheme), typeof(ThemeApplication), typeof(TreeBranchingHistory), new PropertyMetadata(null));

        public TreeBranchingHistory()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event SelectionChangedEventHandler SelectedSequnceChanged;
        public event EventHandler<RecomputeEventArgs> Recompute;
        public event EventHandler<ItemDragedAndDropedEventArgs> ItemDragedAndDroped;

        List<BranchItem> branchItems = new List<BranchItem>();
        List<BranchLink> branchLinks = new List<BranchLink>();

        BranchItem DraggingItem = new BranchItem()
        {
            Visibility = Visibility.Hidden,
            Opacity = .3,
            IsHitTestVisible = false
        };


        private double branchItemWidth = 200;
        private double branchItemHeight = 100;
        private double distanceBetweenBranchItems = 80;
        private Point startPoint;
        private bool IsDragging;
        StackPanel mainStackPanel;
        ScrollViewer mainScrollViewer;

        public ObjectBase Root
        {
            get { return (ObjectBase)GetValue(RootProperty); }
            set { SetValue(RootProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Root.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty RootProperty =
            DependencyProperty.Register("Root", typeof(ObjectBase), typeof(TreeBranchingHistory), new PropertyMetadata(null, OnSetRootChanged));

        private static void OnSetRootChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var tbh = d as TreeBranchingHistory;
            tbh.OnSetRootChanged(e);
        }

        private void OnSetRootChanged(DependencyPropertyChangedEventArgs e)
        {
            if (Root == null)
                return;

            RegenerateTree();
        }

        private void BranchItem_Recompute(object sender, RecomputeEventArgs e)
        {
            Recompute?.Invoke(this, e);
        }

        private void Obj_GetSequence(object sender, RoutedEventArgs e)
        {
            SetOrder();
            var sequence = (sender as ObjectBase).FindSequence();
            SelectedSequnceChanged?.Invoke(this, new SelectionChangedEventArgs(e.RoutedEvent, new List<ObjectBase>(), sequence));
        }

        public void Clear()
        {
            branchItems.Clear();
            branchLinks.Clear();
            mainGrid.Children.Clear();
        }

        public void RegenerateTree()
        {
            int verticalDept = 0;

            List<BranchLink> ContingencyList = new List<BranchLink>();

            mainGrid.Children.Clear();
            mainGrid.Children.Add(DraggingItem);
            branchLinks.Clear();

            GenerateTree(new List<ObjectBase>() { Root }, 0, ref verticalDept);
            branchLinks.Reverse();
            SetOrder();
            ShowTree();
        }

        private void SetOrder()
        {
            int i = 0;
            foreach (var bi in branchItems)
            {
                Panel.SetZIndex(bi, i);
                i++;
            }

            i = 0;
            int activeIndex = branchLinks.Count;
            foreach (var bl in branchLinks)
            {
                if (bl.Link != null && bl.Link.To != null && bl.Link.To.IsInActiveSequence)
                {
                    Panel.SetZIndex(bl, activeIndex);
                    activeIndex++;
                }
                else
                {
                    Panel.SetZIndex(bl, i);
                    i++;
                }
            }
        }

        private void ShowTree()
        {
            foreach (var bi in branchItems)
            {
                if (!mainGrid.Children.Contains(bi))
                    mainGrid.Children.Add(bi);
            }

            foreach (var bl in branchLinks)
            {
                if (!mainGrid.Children.Contains(bl))
                    mainGrid.Children.Add(bl);
            }
        }

        private void GenerateTree(List<ObjectBase> objects, int horizontalDept, ref int verticalDept)
        {
            if (objects == null)
                return;

            horizontalDept++;
            int i = 0;
            foreach (var obj in objects)
            {
                if (i > 0)
                    verticalDept += 1;

                TreeAddItem(obj, horizontalDept, verticalDept);
                GenerateTree(obj.Children, horizontalDept, ref verticalDept);
                i++;
            }
        }

        private void TreeAddItem(ObjectBase obj, int horizontalDept, int verticalDept)
        {
            obj.VerticalDept = verticalDept;
            obj.HorizontalDept = horizontalDept - 1;
            obj.GetSequence -= Obj_GetSequence;
            obj.GetSequence += Obj_GetSequence;

            var bi = branchItems.Where(b => b.ObjectBase == obj).FirstOrDefault();
            if (bi == null)
            {
                bi = new BranchItem()
                {
                    Width = branchItemWidth,
                    Height = branchItemHeight
                };
                Binding myBinding = new Binding(nameof(CurrentTheme));
                myBinding.Source = this;
                bi.SetBinding(BranchItem.CurrentThemeProperty, myBinding);

                bi.Recompute -= BranchItem_Recompute;
                bi.AllowDragDropChanged -= Bi_AllowDragDropChanged;

                bi.Recompute += BranchItem_Recompute;
                bi.AllowDragDropChanged += Bi_AllowDragDropChanged;

                bi.ObjectBase = obj;

                branchItems.Add(bi);
            }

            bi.Margin = new Thickness(
                (branchItemWidth + distanceBetweenBranchItems) * (obj.HorizontalDept),
                branchItemHeight * (obj.VerticalDept),
                0,
                0
            );

            if (obj.InputLink == null)
                obj.InputLink = new ConnectionLink();

            if (obj.ParentObject != null)
            {
                obj.InputLink.From = obj.ParentObject;
                obj.InputLink.To = obj;

                var bl = branchLinks.Where(b => b.Link == obj.InputLink).FirstOrDefault();
                if (bl == null)
                {
                    bl = new BranchLink() { };
                    Binding myBinding = new Binding(nameof(CurrentTheme));
                    myBinding.Source = this;
                    bl.SetBinding(BranchLink.CurrentThemeProperty, myBinding);
                    bl.SetBinding(BranchLink.LinkProperty, new Binding("InputLink") { Source = obj, Mode = BindingMode.TwoWay });

                    branchLinks.Add(bl);
                }

                bl.Margin = new Thickness(
                    (branchItemWidth + distanceBetweenBranchItems) * (obj.ParentObject.HorizontalDept) + branchItemWidth,
                    branchItemHeight * (obj.VerticalDept),
                    0,
                    0
                );
            }

            if (obj?.Children == null || obj?.Children?.Count == 0) //Add Contingency Link
            {
                ConnectionLink link = new ConnectionLink() { From = obj, To = obj };

                var bl = new BranchLink() { Link = link };
                Binding myBinding = new Binding(nameof(CurrentTheme));
                myBinding.Source = this;
                bl.SetBinding(BranchLink.CurrentThemeProperty, myBinding);
                bl.IsContingency = true;

                bl.Margin = new Thickness(
                    (branchItemWidth + distanceBetweenBranchItems) * (obj.HorizontalDept) + branchItemWidth,
                    branchItemHeight * (obj.VerticalDept),
                    0,
                    0
                );

                branchLinks.Add(bl);
            }
        }

        private void Bi_AllowDragDropChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var bi = sender as BranchItem;
            bi.AllowDrop = (bool)e.NewValue;

            if (bi.AllowDrop)
            {
                bi.MouseLeftButtonDown += Bi_PreviewMouseLeftButtonDown;
                bi.MouseMove += Bi_PreviewMouseMove;
                bi.Drop += Bi_Drop;
            }
            else
            {
                bi.MouseLeftButtonDown += Bi_PreviewMouseLeftButtonDown;
                bi.MouseMove += Bi_PreviewMouseMove;
                bi.Drop += Bi_Drop;
            }
        }

        private void Bi_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            startPoint = e.GetPosition(sender as BranchItem);
            IsDragging = true;
            this.MaxHeight = this.ActualHeight;
        }

        private void Bi_Drop(object sender, DragEventArgs e)
        {
            e.Handled = true;
            if (e.Data.GetDataPresent("BranchItem"))
            {
                var srcBi = e.Data.GetData("BranchItem") as BranchItem;
                var destBi = sender as BranchItem;

                var pos = PointToScreen(e.GetPosition(this));
                ItemDragedAndDroped?.Invoke(this, new ItemDragedAndDropedEventArgs(srcBi.ObjectBase, destBi.ObjectBase, pos));
            }
        }

        private void Bi_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (!IsDragging)
                return;

            // Get the current mouse position
            Point mousePos = e.GetPosition(null);
            Vector diff = startPoint - mousePos;

            if (e.LeftButton == MouseButtonState.Pressed &&
                Math.Abs(diff.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(diff.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                // Initialize the drag & drop operation
                BranchItem bi = sender as BranchItem;
                DraggingItem.Title = bi.Title;
                DraggingItem.Description = bi.Description;
                DraggingItem.IsInActiveSequence = bi.IsInActiveSequence;
                Panel.SetZIndex(DraggingItem, mainGrid.Children.Count);
                DraggingItem.Visibility = Visibility.Visible;

                mainStackPanel = GetMainStackPanel();
                mainScrollViewer = GetScroll();

                GiveFeedbackEventHandler handler = new GiveFeedbackEventHandler(DragSource_GiveFeedback);
                this.GiveFeedback += handler;
                DataObject dragData = new DataObject("BranchItem", bi);
                DragDrop.DoDragDrop(bi, dragData, DragDropEffects.Copy | DragDropEffects.Move);
                IsDragging = false;
                DraggingItem.Visibility = Visibility.Hidden;
                DraggingItem.Margin = new Thickness(0);
                this.MaxHeight = double.PositiveInfinity;

                this.GiveFeedback -= handler;
            }
        }

        private StackPanel GetMainStackPanel()
        {
            FrameworkElement parent = (FrameworkElement)this.Parent;
            while (parent != null && parent.Name != "mainStackPanel")
            {
                parent = (FrameworkElement)parent.Parent;
            }

            if (parent is StackPanel)
                return parent as StackPanel;
            else
                return null;
        }

        private ScrollViewer GetScroll()
        {
            FrameworkElement parent = (FrameworkElement)this.Parent;
            while (parent != null && parent.Name != "mainScrollViewer")
            {
                parent = (FrameworkElement)parent.Parent;
            }

            if (parent is ScrollViewer)
                return parent as ScrollViewer;
            else
                return null;
        }

        private void DragSource_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            try
            {
                BranchItem branchItem = (BranchItem)e.OriginalSource;
                var p = System.Windows.Forms.Control.MousePosition;
                var myPos = PointToScreen(new Point(0, 0));
                var mousePosition = new Point(p.X - myPos.X, p.Y - myPos.Y);
                double left = mousePosition.X - startPoint.X;
                double top = mousePosition.Y - startPoint.Y;

                DraggingItem.Margin = new Thickness(left, top, 0, 0);

                //Scroll شدن Branching History در حالت Drag اشیاء
                if (left > mainScrollViewer.ActualWidth - branchItem.ActualWidth)
                {
                    mainScrollViewer.LineRight();
                }
                if (left < mainScrollViewer.ContentHorizontalOffset)
                {
                    mainScrollViewer.LineLeft();
                }
                if (top + branchItem.ActualHeight > mainScrollViewer.ActualHeight + mainScrollViewer.ContentVerticalOffset)
                {
                    mainScrollViewer.LineDown();
                }
                if (top < mainScrollViewer.ContentVerticalOffset)
                {
                    mainScrollViewer.LineUp();
                }
            }
            finally { }
        }
    }
}
