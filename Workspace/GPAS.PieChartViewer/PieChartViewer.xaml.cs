using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace GPAS.PieChartViewer
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class PieChartViewer
    {
        public class PieChartClickEventArgs
        {
            public PieChartClickEventArgs(PieChartItem selectedPieChartItem)
            {
                if (selectedPieChartItem == null)
                    throw new ArgumentNullException("selectedPieChartItem");

                SelectedPieChartItem = selectedPieChartItem;
            }

            public PieChartItem SelectedPieChartItem
            {
                get;
                private set;
            }
        }

        public event EventHandler<PieChartClickEventArgs> PieChartClicked;
        protected virtual void OnPieChartClicked(PieChartItem selectedPieChartItem)
        {
            if (selectedPieChartItem == null)
                throw new ArgumentNullException("selectedPieChartItem");

            PieChartClicked?.Invoke(this, new PieChartClickEventArgs(selectedPieChartItem));
        }

        private ObservableCollection<PieChartItem> pieChartItems;
        public PieChartViewer()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            pieChartItems = new ObservableCollection<PieChartItem>();
            this.DataContext = pieChartItems;
        }

        public void ShowPieChart(List<PieChartItem> itemsToShow)
        {
            pieChartItems.Clear();
            if (itemsToShow.Count == 1)
            {
                double UnknownPercent = 100 - itemsToShow.First().Percent;
                itemsToShow.Add(new PieChartItem()
                {
                    Title = "Unknown",
                    Percent = UnknownPercent
                });
            }
            foreach (var currentItem in itemsToShow)
            {
                pieChartItems.Add(currentItem);
                
            }

            pieChartLayout.piePlotter.IsClickEventDisabled = false;
            pieChartLayout.Visibility = Visibility.Visible;
            pieChartLayout.piePlotter.PieChartClicked += PiePlotter_PieChartClicked;
        }

        private void PiePlotter_PieChartClicked(object sender, ScottLogic.Controls.PieChart.PiePlotter.PieChartClickEventArgs e)
        {
            OnPieChartClicked(e.SelectedPieChartItem);
        }
    }
}
