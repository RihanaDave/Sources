using GPAS.BranchingHistoryViewer.ViewModel;
using GPAS.Workspace.Presentation.Windows.ObjectExplorer;
using GPAS.Workspace.ViewModel.ObjectExplorer;
using GPAS.Workspace.ViewModel.ObjectExplorer.ObjectSet;
using GPAS.Workspace.ViewModel.ObjectExplorer.ObjectSet.StartingObjectSet;
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
using GPAS.Workspace.Presentation.Windows;

namespace GPAS.Workspace.Presentation.Controls.ObjectExplorer
{
    /// <summary>
    /// Interaction logic for BranchingHistoryControl.xaml
    /// </summary>
    public partial class BranchingHistoryControl
    {
        public class SelectionChangedArgs
        {
            public SelectionChangedArgs(ObjectSetBase selectedObjectSetBases)
            {
                SelectedObjectSetBases = selectedObjectSetBases;
            }

            public ObjectSetBase SelectedObjectSetBases
            {
                private set;
                get;
            }
        }
        public event EventHandler<SelectionChangedArgs> SelectionChanged;
        public void OnSelectionChanged(ObjectSetBase selectedObjectBase)
        {
            if (SelectionChanged != null)
                SelectionChanged(this, new SelectionChangedArgs(selectedObjectBase));
        }

        public class RecomputeRequestedArgs
        {
            public RecomputeRequestedArgs(ObjectSetBase selectedObjectSetBases)
            {
                SelectedObjectSetBases = selectedObjectSetBases;
            }

            public ObjectSetBase SelectedObjectSetBases
            {
                private set;
                get;
            }
        }
        public event EventHandler<RecomputeRequestedArgs> RecomputeRequested;
        public void OnRecomputeRequested(ObjectSetBase selectedObjectBase)
        {
            if (RecomputeRequested != null)
                RecomputeRequested(this, new RecomputeRequestedArgs(selectedObjectBase));
        }


        public class ItemDragedAndDropedEventArgs : EventArgs
        {
            public ItemDragedAndDropedEventArgs(ObjectSetBase first, ObjectSetBase second, Point position)
            {
                First = first;
                Second = second;
                Position = position;
            }

            public ObjectSetBase First
            {
                get;
                private set;
            }

            public ObjectSetBase Second
            {
                get;
                private set;
            }

            public Point Position
            {
                get;
                private set;
            }
        }

        public event EventHandler<ItemDragedAndDropedEventArgs> ItemDragedAndDroped;

        private void OnItemDragedAndDroped(ItemDragedAndDropedEventArgs args)
        {
            ItemDragedAndDroped?.Invoke(this, args);
        }


        protected MainWindow MainWindow
        {
            get { return (MainWindow)GetValue(MainWindowProperty); }
            set { SetValue(MainWindowProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MainWindow.  This enables animation, styling, binding, etc...
        protected static readonly DependencyProperty MainWindowProperty =
            DependencyProperty.Register(nameof(MainWindow), typeof(MainWindow), typeof(MainWindow), new PropertyMetadata(null));



        public BranchingHistoryControl()
        {
            InitializeComponent();
            Loaded += BranchingHistoryControl_Loaded;
        }

        private void BranchingHistoryControl_Loaded(object sender, RoutedEventArgs e)
        {
            var w = Window.GetWindow(this);
            MainWindow = w as MainWindow;

            if (MainWindow == null)
                return;

            MainWindow.CurrentThemeChanged -= MainWindow_CurrentThemeChanged;
            MainWindow.CurrentThemeChanged += MainWindow_CurrentThemeChanged;
        }

        private void MainWindow_CurrentThemeChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            mainBranchingHisory.CurrentTheme = MainWindow.CurrentTheme == ThemeApplication.Light ?
                BranchingHistoryViewer.ThemeApplication.Light :
                BranchingHistoryViewer.ThemeApplication.Dark;
        }

        public void ClearAll()
        {
            mainBranchingHisory.Clear();
        }

        public void AddITems(ObjectSetBase objectSetBase)
        {
            //foreach (var currentObjectSetBase in objectSetBase)
            ObjectBase parentObject = null;
            if (objectSetBase is DerivedObjectSet)
            {
                DerivedObjectSet derivedObjectSet = objectSetBase as DerivedObjectSet;
                foreach (var tree in mainBranchingHisory.TreeList)
                {
                    foreach (var child in tree.Root.GetAllObjectsInTree())
                    {
                        if (child.Tag == derivedObjectSet.ParentSet)
                        {
                            parentObject = child;
                            break;
                        }
                    }

                    if (parentObject != null)
                        break;
                }
            }

            ObjectBase objectBase = new ObjectBase()
            {
                Tag = objectSetBase,
                ParentObject = parentObject
            };

            objectBase.SetBinding(ObjectBase.TitleProperty, new Binding("Title") { Source = objectSetBase, Mode = BindingMode.TwoWay });
            objectBase.SetBinding(ObjectBase.IconProperty, new Binding("Icon") { Source = objectSetBase, Mode = BindingMode.TwoWay });
            objectBase.SetBinding(ObjectBase.IsActiveProperty, new Binding("IsActiveSet") { Source = objectSetBase, Mode = BindingMode.OneWay });
            objectBase.SetBinding(ObjectBase.IsInActiveSequenceProperty, new Binding("IsInActiveSetSequence") { Source = objectSetBase, Mode = BindingMode.TwoWay });
            objectBase.SetBinding(ObjectBase.DescriptionProperty, new Binding("ObjectsCount")
            {
                Source = objectSetBase,
                Mode = BindingMode.TwoWay,
                Converter = new ObjectsCountToObjectsCountTextConverter()
            });
            objectBase.SetBinding(ObjectBase.AllowDragDropProperty, new Binding("ObjectsCount")
            {
                Source = objectSetBase,
                Mode = BindingMode.TwoWay,
                Converter = new ObjectSetBaseToAllowDropObjectBaseConverter(),
                ConverterParameter = objectSetBase is DerivedObjectSet
            });

            if (objectSetBase is DerivedObjectSet)
            {
                objectBase.InputLink = new ConnectionLink();
                objectBase.InputLink.SetBinding(ConnectionLink.DescriptionProperty, new Binding("ObjectsCountDifference")
                {
                    Source = (objectSetBase as DerivedObjectSet).AppliedFormula,
                    Mode = BindingMode.TwoWay,
                    Converter = new ObjectsCountDifferenceToInputLinkObjectBaseDescriptionConverter()
                });
                objectBase.InputLink.SetBinding(ConnectionLink.IconProperty, new Binding("Icon") { Source = (objectSetBase as DerivedObjectSet).AppliedFormula, Mode=BindingMode.TwoWay });
            }
            
            mainBranchingHisory.AddItem(objectBase);
        }

        internal List<ObjectSetBase> GetBranchingHistoryLeaves()
        {
            List<ObjectSetBase> branchingHistoryLeaves = new List<ObjectSetBase>();
            if (mainBranchingHisory.TreeList?.Count > 0)
            {
                List<ObjectBase> allChild = mainBranchingHisory.TreeList[0].Root.GetAllObjectsInTree();

                branchingHistoryLeaves =  allChild.Where(ob => ob.Children == null || ob.Children.Count == 0).Select(ob => (ObjectSetBase)ob.Tag).ToList();
            }
            return branchingHistoryLeaves;
        }

        private void mainBranchingHisory_SelectedSequnceChanged(object sender, SelectionChangedEventArgs e)
        {
            List<ObjectSetBase> objectSetBases = new List<ObjectSetBase>();
            foreach (ObjectBase item in e.AddedItems)
            {
                objectSetBases.Add(item.Tag as ObjectSetBase);
            }

            OnSelectionChanged(objectSetBases.First());
        }

        private void mainBranchingHisory_Recompute(object sender, RecomputeEventArgs e)
        {
            ObjectSetBase objectSetBases = e.ObjectBase.Tag as ObjectSetBase;
            OnRecomputeRequested(objectSetBases);
        }

        private void mainBranchingHisory_ItemDragedAndDroped(object sender, BranchingHistoryViewer.ItemDragedAndDropedEventArgs e)
        {
            if (e.SourceItem == e.DestinationItem)
                return;

            OnItemDragedAndDroped(new ItemDragedAndDropedEventArgs(
                e.SourceItem.Tag as ObjectSetBase,
                e.DestinationItem.Tag as ObjectSetBase,
                e.Position)
            );
        }
    }

    internal class ObjectSetBaseToAllowDropObjectBaseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if(parameter is bool)
            {
                if ((bool)parameter)
                {
                    if ((long)value > 0)
                        return true;
                    else
                        return false;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class ObjectsCountDifferenceToInputLinkObjectBaseDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if ((long)value >= 0)
                return "+" + value.ToString();
            else
                return value.ToString();
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class ObjectsCountToObjectsCountTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value.ToString() + " " + Properties.Resources.objects;
        }
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return long.Parse(value.ToString());
        }
    }
}
