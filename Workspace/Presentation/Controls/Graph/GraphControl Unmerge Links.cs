using GPAS.Workspace.Entities;
using GPAS.Workspace.Entities.SearchAroundResult;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Presentation.Controls.Graph;
using GPAS.Workspace.Presentation.Windows.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GPAS.Workspace.Presentation.Controls
{
    public partial class GraphControl
    {
        bool unmergingLinks = false;
        public event EventHandler UnmergingLinks;
        public void OnUnmergingLinks()
        {
            unmergingLinks = true;
            UnmergingLinks?.Invoke(this, new EventArgs());
        }

        public event EventHandler UnmergedLinks;

        public async void OnUnmergedLinks()
        {
            unmergingLinks = false;
            UnmergedLinks?.Invoke(this, new EventArgs());
            await ResetFlowsIfShown();
        }

        public async Task UnmergeSelectedLinks()
        {
            if (!CanUnmergeSelectedLinks())
                return;

            List<EventBasedKWLink> eventBasedLinksToUnmerge = new List<EventBasedKWLink>();
            List<CompoundKWLink> selectedNotLoadedLinks = new List<CompoundKWLink>();
            foreach (EdgeMetadata metadata in GetSelectedUnmergableEdgeMetadatas())
            {
                eventBasedLinksToUnmerge.AddRange(metadata.EventBasedLinks);
                NotLoadedRelationshipBasedKWLink NLRs = metadata.GetNotLoadedRelationshipBasedKWLink();
                if (NLRs.IntermediaryRelationshipIDs.Any())
                {
                    selectedNotLoadedLinks.Add(NLRs);
                }
                NotLoadedEventBasedKWLink NLEs = metadata.GetNotLoadedEventBasedKWLink();
                if (NLEs.IntermediaryLinksRelationshipIDs.Any())
                {
                    selectedNotLoadedLinks.Add(NLEs);
                }
            }

            // TODO: نیاز به بازطراحی جداسازی
            List<KWLink> loadedLinks = await LoadNotLoadedLinks(selectedNotLoadedLinks);
            ShowLinks(loadedLinks);
            eventBasedLinksToUnmerge.AddRange(loadedLinks.Where(l => l is EventBasedKWLink).Select(l => (EventBasedKWLink)l));

            if (eventBasedLinksToUnmerge.Any())
            {
                EventBasedLinksUnmergeProvider unmergeProvider = new EventBasedLinksUnmergeProvider(this, eventBasedLinksToUnmerge);
                if (unmergeProvider.IsUnmergeNeedsWarningForUnmergedLinkCount())
                {
                    MessageBoxResult promptResult = KWMessageBox.Show(string.Format(Properties.Resources.Count_of_unmerged_links_is_greater_than_0_Do_you_want_to_show, CountOfUnmergeLinksWarning.ToString())
                        , MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (promptResult == MessageBoxResult.No)
                    {
                        return;
                    }
                }

                OnUnmergingLinks();
                await unmergeProvider.UnmergeLinks();
                OnUnmergedLinks();
            }
        }

        private async Task<List<KWLink>> LoadNotLoadedLinks(List<CompoundKWLink> selectedNotLoadedLinks)
        {
            if (selectedNotLoadedLinks.Count == 0)
                return new List<KWLink>();
            int notLoadedEventsCount = 0;
            int notLoadedRelationshipsCount = 0;
            foreach (CompoundKWLink item in selectedNotLoadedLinks)
            {
                if (item is NotLoadedEventBasedKWLink)
                {
                    notLoadedEventsCount += ((NotLoadedEventBasedKWLink)item).IntermediaryLinksRelationshipIDs.Count;
                }
                else if (item is NotLoadedRelationshipBasedKWLink)
                {
                    notLoadedRelationshipsCount += ((NotLoadedRelationshipBasedKWLink)item).IntermediaryRelationshipIDs.Count;
                }
            }
            int totalLinksToLoadCount = notLoadedEventsCount + notLoadedRelationshipsCount;
            if (totalLinksToLoadCount > Logic.Search.SearchAround.LoadingDefaultBatchSize)
            {
                totalLinksToLoadCount = ShowUnmergeMoreLinksPrompt(totalLinksToLoadCount);
            }
            int pickupLinkIndex = 0;
            bool isMoreLinkNeeded = true;
            int lastLoopExpandingLinksCount = 0;
            List<EventBasedResultInnerRelationships> loadingInnerRelationships = new List<EventBasedResultInnerRelationships>(totalLinksToLoadCount);
            List<long> loadingDependentRelationshipIDs = new List<long>(totalLinksToLoadCount);
            while (isMoreLinkNeeded)
            {
                foreach (CompoundKWLink item in selectedNotLoadedLinks)
                {
                    if (item is NotLoadedEventBasedKWLink)
                    {
                        if (((NotLoadedEventBasedKWLink)item).IntermediaryLinksRelationshipIDs.Count < pickupLinkIndex + 1)
                            continue;
                        EventBasedResultInnerRelationships relID = ((NotLoadedEventBasedKWLink)item).IntermediaryLinksRelationshipIDs.ElementAt(pickupLinkIndex);
                        loadingInnerRelationships.Add(relID);
                    }
                    else if (item is NotLoadedRelationshipBasedKWLink)
                    {
                        if (((NotLoadedRelationshipBasedKWLink)item).IntermediaryRelationshipIDs.Count < pickupLinkIndex + 1)
                            continue;
                        long relID = ((NotLoadedRelationshipBasedKWLink)item).IntermediaryRelationshipIDs.ElementAt(pickupLinkIndex);
                        loadingDependentRelationshipIDs.Add(relID);
                    }
                    isMoreLinkNeeded = (loadingInnerRelationships.Count + loadingDependentRelationshipIDs.Count < totalLinksToLoadCount);
                }
                pickupLinkIndex++;
                if (lastLoopExpandingLinksCount < loadingInnerRelationships.Count + loadingDependentRelationshipIDs.Count)
                {
                    lastLoopExpandingLinksCount = loadingInnerRelationships.Count + loadingDependentRelationshipIDs.Count;
                }
                else
                {
                    // Interminable loop!
                    break;
                }
            }

            Dictionary<long, RelationshipBasedKWLink> retrieveResultsPerRelID
                = (await LinkManager.RetrieveRelationshipBaseLinksAsync
                    (loadingDependentRelationshipIDs
                        .Concat(loadingInnerRelationships.Select(n => n.FirstRelationshipID))
                        .Concat(loadingInnerRelationships.Select(n => n.SecondRelationshipID)).Distinct()
                        .ToList()
                    )).ToDictionary(r => r.Relationship.ID);

            List<KWLink> expandingLinks = new List<KWLink>(totalLinksToLoadCount);
            expandingLinks.AddRange
                (loadingDependentRelationshipIDs
                    .Select(i => retrieveResultsPerRelID[i]));
            expandingLinks.AddRange
                (loadingInnerRelationships
                    .Select(n => LinkManager.GetEventBaseKWLinkFromLinkInnerRelationships
                        (retrieveResultsPerRelID[n.FirstRelationshipID], retrieveResultsPerRelID[n.SecondRelationshipID])));
            return expandingLinks;
        }

        private int ShowUnmergeMoreLinksPrompt(int totalLinksToUnmergeCount)
        {
            if (totalLinksToUnmergeCount < Logic.Search.SearchAround.LoadingDefaultBatchSize)
                return totalLinksToUnmergeCount;
            int currentlyLoadedResultsCount = Logic.Search.SearchAround.LoadingDefaultBatchSize;
            int recommandedLoadMoreNResultsCount = 100;
            int recommandedLoadMore500ResultsCount = 500;
            int maximummLoadableResultsCount = Logic.Search.SearchAround.TotalResultsThreshould;
            
           
            UnmergeMoreLinksWindow unmergeMoreLinksWindow = new UnmergeMoreLinksWindow()
            {
                Owner = Window.GetWindow(this)
            };
            unmergeMoreLinksWindow.Init(currentlyLoadedResultsCount,
                                        recommandedLoadMoreNResultsCount,
                                        recommandedLoadMore500ResultsCount,
                                        maximummLoadableResultsCount);
            unmergeMoreLinksWindow.ShowDialog();
            

            return unmergeMoreLinksWindow.ResultWindow;
        }

        public bool CanUnmergeSelectedLinks()
        {
            return GetSelectedUnmergableEdgeMetadatas().Any();
        }

        private IEnumerable<EdgeMetadata> GetSelectedUnmergableEdgeMetadatas()
        {
            return TotalEdges
                .Where(edgeMetadata
                    => graphviewerMain.IsSelectedEdge(edgeMetadata.RelatedEdge)
                    && edgeMetadata.IsUnmergable());
        }
    }
}
