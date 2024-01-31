using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;

namespace GPAS.Workspace.Presentation.Controls
{
    /// <summary>
    /// منطق تعامل با QuickSearchResultViewerControl.xaml
    /// </summary>
    public partial class QuickSearchResultViewerControl
    {
        #region مدیریت رخداد
        /// <summary>
        /// ارگومان های صدور رخداد «برگزیده شدن نتیجه ای از بین نتایج در حال نمایش
        /// </summary>
        public class QuickSearchResultChoosenEventArgs
        {
            /// <summary>
            /// سازنده کلاس
            /// </summary>
            /// <param name="choosenResult"></param>
            public QuickSearchResultChoosenEventArgs(KWObject choosenResult)
            {
                if (choosenResult == null)
                    throw new ArgumentNullException("choosenResult");

                ChoosenResult = choosenResult;
            } 
            /// <summary>
            /// نتیجه برگزیده شده
            /// </summary>
            public KWObject ChoosenResult
            {
                get;
                private set;
            }
        }
        /// <summary>
        /// رخداد «برگزیده شدن نتیجه ای از بین نتایج در حال نمایش»؛
        /// این اتفاق عموما با دوبار کلیک روی نتیجه اتفاق می افتد
        /// </summary>
        public event EventHandler<QuickSearchResultChoosenEventArgs> QuickSearchResultChoosen;
        /// <summary>
        /// عملکرد مدیریت رخداد «برگزیده شدن نتیجه ای از بین نتایج در حال نمایش»
        /// </summary>
        protected virtual void OnQuickSearchResultChoosen(KWObject choosenResult)
        {
            if (choosenResult == null)
                throw new ArgumentNullException("choosenResult");

            if (QuickSearchResultChoosen != null)
                QuickSearchResultChoosen(this, new QuickSearchResultChoosenEventArgs(choosenResult));
        }
        #endregion

        /// <summary>
        /// سازنده ملاس
        /// </summary>
        public QuickSearchResultViewerControl()
        {
            InitializeComponent();
        }

        public void ShowResults(IEnumerable<KWObject> resultsToShow)
        {
            if (resultsToShow == null)
                throw new ArgumentNullException("resultsToShow");

            ClearAllPanelsPreviousResults();

            foreach (var item in resultsToShow)
            {
                if (OntologyProvider.GetOntology().IsEntity(item.TypeURI))
                {
                    AddResultInPanel(item, entitiesResultsPanel);
                    HideNoResultPromptLabelIfNot(entityResultsPanelNoResultPromptLabel);
                    continue;
                }
                if (OntologyProvider.GetOntology().IsEvent(item.TypeURI))
                {
                    AddResultInPanel(item, eventsResultsPanel);
                    HideNoResultPromptLabelIfNot(eventResultsPanelNoResultPromptLabel);
                    continue;
                }
                if (OntologyProvider.GetOntology().IsDocument(item.TypeURI))
                {
                    AddResultInPanel(item, documentsResultsPanel);
                    HideNoResultPromptLabelIfNot(documentResultsPanelNoResultPromptLabel);
                    continue;
                }
            }

            //Visibility = Visibility.Visible;
        }

        private void ClearAllPanelsPreviousResults()
        {
            UnhideResultPromptLabelIfNot(entityResultsPanelNoResultPromptLabel);
            UnhideResultPromptLabelIfNot(eventResultsPanelNoResultPromptLabel);
            UnhideResultPromptLabelIfNot(documentResultsPanelNoResultPromptLabel);
            for (int i = entitiesResultsPanel.Children.Count - 1; i > 1; i--)
                entitiesResultsPanel.Children.RemoveAt(i);
            for (int i = eventsResultsPanel.Children.Count - 1; i > 1; i--)
                eventsResultsPanel.Children.RemoveAt(i);
            for (int i = documentsResultsPanel.Children.Count - 1; i > 1; i--)
                documentsResultsPanel.Children.RemoveAt(i);
        }

        private void HideNoResultPromptLabelIfNot(Label labelToHide)
        {
            if (labelToHide == null)
                throw new ArgumentNullException("labelToHide");

            if (IsResultPromptLabelHidden(labelToHide))
                return;
            labelToHide.Visibility = Visibility.Hidden;
            labelToHide.Height = 0;
        }
        private void UnhideResultPromptLabelIfNot(Label labelToUnide)
        {
            if (labelToUnide == null)
                throw new ArgumentNullException("labelToUnide");

            if (!IsResultPromptLabelHidden(labelToUnide))
                return;
            labelToUnide.Visibility = Visibility.Visible;
            labelToUnide.Height = double.NaN;
        }
        private bool IsResultPromptLabelHidden(Label labelToCheck)
        {
            if (labelToCheck == null)
                throw new ArgumentNullException("labelToCheck");

            return labelToCheck.Visibility == Visibility.Hidden;
        }

        private void AddResultInPanel(KWObject objectToShow, StackPanel panelToAddObject)
        {
            if (objectToShow == null)
                throw new ArgumentNullException("objectToShow");
            if (panelToAddObject == null)
                throw new ArgumentNullException("panelToAddObject");

            ObjectLabelControl tempObjLabel = new ObjectLabelControl()
                {
                    Margin = new Thickness(5, 0, 5, 0),
                    Padding = new Thickness(1),
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalContentAlignment = HorizontalAlignment.Stretch,
                    FontFamily = new FontFamily("Calibri"),
                    FontSize = 15
                };
            tempObjLabel.SetContent(objectToShow);
            tempObjLabel.ObjectChoosen += tempObjLabel_ObjectChoosen;
            
            tempObjLabel.SetEvenOrOddPlacement(panelToAddObject.Children.Count % 2 == 0);
            panelToAddObject.Children.Add(tempObjLabel);
        }

        private void tempObjLabel_ObjectChoosen(object sender, EventArgs e)
        {
            OnQuickSearchResultChoosen((sender as ObjectLabelControl).Content);
                        
        }
    }
}
