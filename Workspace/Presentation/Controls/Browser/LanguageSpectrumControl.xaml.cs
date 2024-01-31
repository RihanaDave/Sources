using GPAS.Dispatch.Entities.NLP;
using GPAS.PieChartViewer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.Workspace.Presentation.Controls
{
    /// <summary>
    /// Interaction logic for LanguageSpectrumControl.xaml
    /// </summary>
    public partial class LanguageSpectrumControl
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
        public LanguageSpectrumControl()
        {
            InitializeComponent();
        }

        public void ShowLanguages(DetectedLanguage[] languageStatistics)
        {            
            pieChartViewer.ShowPieChart(GetPieChartItemFromDetectedLanguages(languageStatistics));
        }

        private List<PieChartItem> GetPieChartItemFromDetectedLanguages(DetectedLanguage[] languageStatistics)
        {
            List<PieChartItem> result = new List<PieChartItem>();
            for (int i = 0; i < languageStatistics.Length; i++)
            {
                PieChartItem newPieChartItem = new PieChartItem();
                newPieChartItem.Title = CapitalizeFirstChar(languageStatistics[i].LanguageName);
                if (!languageStatistics[i].Percent.HasValue)
                {
                    throw new ArgumentException();
                }
                else
                {
                    newPieChartItem.Percent = languageStatistics[i].Percent.Value;
                }
                result.Add(newPieChartItem);
            }
            return result;
        }

        private string CapitalizeFirstChar(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            string firstChar = input.First().ToString().ToUpper();
            if (input.Length == 1)
                return firstChar;
            else
                return firstChar + input.Substring(1).ToLower();
        }

        private void pieChartViewer_PieChartClicked(object sender, PieChartViewer.PieChartViewer.PieChartClickEventArgs e)
        {
            OnPieChartClicked(e.SelectedPieChartItem);
        }
    }
}