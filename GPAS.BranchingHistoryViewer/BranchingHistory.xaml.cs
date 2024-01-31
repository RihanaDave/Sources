using GPAS.BranchingHistoryViewer.ViewModel;
using System;
using System.Collections.Generic;
using System.Globalization;
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
    /// Interaction logic for BranchingHistory.xaml
    /// </summary>
    public partial class BranchingHistory : UserControl
    {
        public ThemeApplication CurrentTheme
        {
            get { return (ThemeApplication)GetValue(CurrentThemeProperty); }
            set { SetValue(CurrentThemeProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CurrentTheme.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CurrentThemeProperty =
            DependencyProperty.Register(nameof(CurrentTheme), typeof(ThemeApplication), typeof(BranchingHistory), new PropertyMetadata(null));

        

        public BranchingHistory()
        {
            InitializeComponent();
            DataContext = this;
        }

        List<TreeBranchingHistory> treeList = new List<TreeBranchingHistory>();

        public List<TreeBranchingHistory> TreeList {
            get { return treeList; }
            private set { treeList = value; }
        }

        public void Clear()
        {
            foreach (var tree in treeList)
            {
                tree.Clear();
            }

            treeList.Clear();
            mainStackPanel.Children.Clear();
        }
        
        public void AddItem(ObjectBase Item)
        {
            List<ObjectBase> Roots = new List<ObjectBase>();
            ObjectBase rootItem;

            if (!Item.IsDerivedObject) //is root
                rootItem = Item;
            else
                rootItem = Item.GetRoot();

            AddTree(rootItem);

            if (Item.IsActive)
                Item.OnGetSequence();
        }

        private void AddTree(ObjectBase root)
        {
            if (root == null)
                return;

            var oldRoot = TreeList.Select(tl => tl.Root);
            if (oldRoot.Contains(root))
            {
                TreeList.Where(tl => tl.Root == root).First().RegenerateTree();
            }
            else
            {
                var tbh = new TreeBranchingHistory();
                tbh.SelectedSequnceChanged -= Tree_SelectedSequnceChanged;
                tbh.SelectedSequnceChanged += Tree_SelectedSequnceChanged;
                tbh.Recompute -= Tree_Recompute;
                tbh.Recompute += Tree_Recompute;
                tbh.ItemDragedAndDroped -= Tree_ItemDragedAndDroped;
                tbh.ItemDragedAndDroped += Tree_ItemDragedAndDroped;
                tbh.Root = root;
                Binding myBinding = new Binding(nameof(CurrentTheme));
                myBinding.Source = this;
                tbh.SetBinding(TreeBranchingHistory.CurrentThemeProperty, myBinding);
                treeList.Add(tbh);
                mainStackPanel.Children.Add(tbh);
            }
        }

        private void Tree_ItemDragedAndDroped(object sender, ItemDragedAndDropedEventArgs e)
        {
            ItemDragedAndDroped?.Invoke(this, e);
        }

        private void Tree_Recompute(object sender, RecomputeEventArgs e)
        {
            Recompute?.Invoke(this, e);
        }

        private void Tree_SelectedSequnceChanged(object sender, SelectionChangedEventArgs e)
        {
            var sequence = e.AddedItems;
            SelectedSequnceChanged?.Invoke(this, new SelectionChangedEventArgs(e.RoutedEvent, new List<ObjectBase>(), sequence));
        }

        public event SelectionChangedEventHandler SelectedSequnceChanged;
        public event EventHandler<RecomputeEventArgs> Recompute;
        public event EventHandler<ItemDragedAndDropedEventArgs> ItemDragedAndDroped;
    }
}
