using GPAS.Workspace.ViewModel.ObjectExplorer.ObjectSet;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace GPAS.Workspace.Presentation.Controls.ObjectExplorer.FormulaPanel
{
    /// <summary>
    /// Interaction logic for FormulaPanel.xaml
    /// </summary>
    public partial class FormulaPanel
    {

        public class RecomputeEventArgs
        {
            public RecomputeEventArgs(ObjectSetBase selectedObjectSetBase)
            {
                if (selectedObjectSetBase == null)
                    throw new ArgumentNullException(nameof(selectedObjectSetBase));

                SelectedObjectSetBase = selectedObjectSetBase;
            }

            public ObjectSetBase SelectedObjectSetBase
            {
                get;
                private set;
            }
        }

        public event EventHandler<RecomputeEventArgs> RecomputeRequested;
        protected virtual void OnRecomputeRequested(ObjectSetBase selectedObjectSetBase)
        {
            if (selectedObjectSetBase == null)
                throw new ArgumentNullException(nameof(selectedObjectSetBase));

            RecomputeRequested?.Invoke(this, new RecomputeEventArgs(selectedObjectSetBase));
        }

        public FormulaPanel()
        {
            InitializeComponent();
        }

        List<ObjectSetBase> ObjectSetBaseItems = new List<ObjectSetBase>();
        List<FormulaItem> FormulaItemControls = new List<FormulaItem>();
        public ReadOnlyCollection<ObjectSetBase> Items
        {
            get
            {
                return ObjectSetBaseItems.AsReadOnly();
            }
        }

        public void AddItems(IEnumerable<ObjectSetBase> items)
        {
            this.ObjectSetBaseItems.AddRange(items);

            foreach (var item in items)
            {
                var fi = new FormulaItem();
                fi.SetValue(FormulaItem.ObjectSetProperty, item);
                item.ActivatedSet -= Item_ActivatedSet;
                item.ActivatedSet += Item_ActivatedSet;

                item.RecomputeSet -= Item_RecomputeSet;
                item.RecomputeSet += Item_RecomputeSet;
                FormulaItemControls.Add(fi);
                mainStackPanel.Children.Add(fi);
            }

            FormulaItemControls.First().ElementLocation = ElementLocationOfOfSequence.First;
            var last = FormulaItemControls.Last();
            last.ElementLocation = ElementLocationOfOfSequence.Last;

            FormulaItemControls.ForEach(fi => fi.IsSelected = false);
            last.IsSelected = true;
        }        

        private void Item_ActivatedSet(object sender, RoutedEventArgs e)
        {
            var obs = sender as ObjectSetBase;
            SelectElement(obs, e.RoutedEvent);
        }

        private void SelectElement(ObjectSetBase objectSetBase, RoutedEvent routedEvent)
        {
            var old = ObjectSetBaseItems.Where(i => i.IsActiveSet).Except(new List<ObjectSetBase>() { objectSetBase }).ToList();
            foreach (ObjectSetBase item in old)
            {
                item.IsActiveSet = false;
            }

            SelectionChanged?.Invoke(this, new SelectionChangedEventArgs(routedEvent, old, new List<ObjectSetBase>() { objectSetBase }));
        }

        private void Item_RecomputeSet(object sender, RecomputeSetEventArgs e)
        {
            ObjectSetBase objectSetBase = sender as ObjectSetBase;

            OnRecomputeRequested(objectSetBase);
        }

        public void ClearItems()
        {
            mainStackPanel.Children.Clear();
            FormulaItemControls.Clear();
            ObjectSetBaseItems.Clear();
        }

        public ObjectSetBase SelectedItem
        {
            get
            {
                return FormulaItemControls.FirstOrDefault(i => i.IsSelected).ObjectSet;
            }
        }

        public event SelectionChangedEventHandler SelectionChanged;

        
    }
}
