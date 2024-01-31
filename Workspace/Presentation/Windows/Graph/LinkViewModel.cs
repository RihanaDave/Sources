using GPAS.Logger;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Entities.KWLinks;
using GPAS.Workspace.Logic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GPAS.Workspace.Presentation.Windows.Graph
{
    public class LinkViewModel : RelayNotifyPropertyChanged
    {
        public LinkViewModel()
        {
            NewLinkModel = new LinkModel();
            UnpublishedLinksToShow = new ObservableCollection<LinkModel>();
            PublishedLinksToShow = new ObservableCollection<LinkModel>();
            UnpublishedLinksToShow.CollectionChanged += AllLinksCollectionOnCollectionChanged;
            SelectedLinkModel = new LinkModel();
            UnpublishedLinksDictionary = new Dictionary<long, KWLink>();
            PublishedAndNewLinks = new List<KWLink>();
        }

        #region Edit link

        public ObservableCollection<LinkModel> UnpublishedLinksToShow { get; set; }
        public ObservableCollection<LinkModel> PublishedLinksToShow { get; set; }

        public Dictionary<long, KWLink> UnpublishedLinksDictionary;

        public List<KWLink> PublishedAndNewLinks;

        private LinkModel selectedLinkModel;
        public LinkModel SelectedLinkModel
        {
            get => selectedLinkModel;
            set => SetValue(ref selectedLinkModel, value);
        }

        private bool applyButtonIsEnable;
        public bool ApplyButtonIsEnable
        {
            get => applyButtonIsEnable;
            set => SetValue(ref applyButtonIsEnable, value);
        }

        public void PrepareLinkToEdit(List<KWLink> links)
        {
            if (UnpublishedLinksDictionary.Count != 0)
                UnpublishedLinksDictionary.Clear();

            if (PublishedAndNewLinks.Count != 0)
                PublishedAndNewLinks.Clear();

            if (UnpublishedLinksToShow.Count != 0)
                UnpublishedLinksToShow.Clear();

            if (PublishedLinksToShow.Count != 0)
                PublishedLinksToShow.Clear();

            int notLoadedRelationsCount = 0;
            int notLoadedEventsCount = 0;

            for (int i = 0; i < links.Count; i++)
            {
                var link = links[i];

                if (links[i] is NotLoadedRelationshipBasedKWLink)
                {
                    notLoadedRelationsCount += ((NotLoadedRelationshipBasedKWLink)link).IntermediaryRelationshipIDs.Count;
                    PublishedAndNewLinks.Add(link);
                }
                else if (links[i] is NotLoadedEventBasedKWLink)
                {
                    notLoadedEventsCount += ((NotLoadedEventBasedKWLink)link).IntermediaryLinksRelationshipIDs.Count;
                    PublishedAndNewLinks.Add(link);
                }
                else
                {
                    if (LinkManager.IsUnpublishedRelationship(link))
                    {
                        UnpublishedLinksToShow.Add(new LinkModel
                        {
                            OldDescription = link.Text,
                            OldDirection = link.LinkDirection,
                            OldLinkType = link.TypeURI,
                            Id = i,
                            Selectable = true,
                            Description = link.Text,
                            LinkType = link.TypeURI,
                            LinkTypeToShow = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(link.TypeURI),
                            SourceTypeUri = link.Source.TypeURI,
                            SourceDisplayName = link.Source.DisplayName.Value,
                            TargetTypeUri = link.Target.TypeURI,
                            TargetDisplayName = link.Target.DisplayName.Value,
                            Direction = link.LinkDirection,
                            IsUnpublished = true,
                            Tag = link
                        });

                        UnpublishedLinksDictionary.Add(i, link);
                    }
                    else
                    {
                        PublishedLinksToShow.Add(new LinkModel
                        {
                            OldDescription = link.Text,
                            OldDirection = link.LinkDirection,
                            OldLinkType = link.TypeURI,
                            Id = i,
                            Selectable = true,
                            Description = link.Text,
                            LinkType = link.TypeURI,
                            LinkTypeToShow = OntologyProvider.GetUserFriendlyNameOfOntologyTypeNode(link.TypeURI),
                            SourceTypeUri = link.Source.TypeURI,
                            SourceDisplayName = link.Source.DisplayName.Value,
                            TargetTypeUri = link.Target.TypeURI,
                            TargetDisplayName = link.Target.DisplayName.Value,
                            Direction = link.LinkDirection,
                            IsUnpublished = false,
                            Tag = link
                        });

                        PublishedAndNewLinks.Add(link);
                    }
                }
            }

            if (notLoadedRelationsCount != 0)
            {
                var text = "Published " + notLoadedRelationsCount + " relation(s)";

                PublishedLinksToShow.Add(new LinkModel
                {
                    OldDescription = string.Empty,
                    OldDirection = LinkDirection.Bidirectional,
                    OldLinkType = text,
                    LinkType = text,
                    Selectable = false,
                    Description = string.Empty,
                    LinkTypeToShow = text,
                    SourceTypeUri = links.First().Source.TypeURI,
                    SourceDisplayName = links.First().Source.DisplayName.Value,
                    TargetTypeUri = links.First().Target.TypeURI,
                    TargetDisplayName = links.First().Target.DisplayName.Value,
                    Direction = LinkDirection.Bidirectional,
                    IsUnpublished = false,
                    Tag = links.First()
                });
            }

            if (notLoadedEventsCount != 0)
            {
                string text = "Published " + notLoadedEventsCount + " event(s)";

                PublishedLinksToShow.Add(new LinkModel
                {
                    OldDescription = string.Empty,
                    OldDirection = LinkDirection.Bidirectional,
                    OldLinkType = text,
                    LinkType = text,
                    Selectable = false,
                    Description = string.Empty,
                    LinkTypeToShow = text,
                    SourceTypeUri = links.First().Source.TypeURI,
                    SourceDisplayName = links.First().Source.DisplayName.Value,
                    TargetTypeUri = links.First().Target.TypeURI,
                    TargetDisplayName = links.First().Target.DisplayName.Value,
                    Direction = LinkDirection.Bidirectional,
                    IsUnpublished = false,
                    Tag = links.First()
                });
            }

            ApplyButtonIsEnable = false;
        }

        public async Task<List<KWLink>> DeleteLinksForUpdate()
        {
            List<KWLink> linksToDelete = new List<KWLink>();

            foreach (var linkModel in UnpublishedLinksToShow)
            {
                if (!linkModel.Edited)
                    continue;

                linksToDelete.Add(UnpublishedLinksDictionary[linkModel.Id]);
                UnpublishedLinksDictionary.Remove(linkModel.Id);
            }

            await LinkManager.DeleteLinksList(linksToDelete);

            return linksToDelete;
        }

        public async Task<List<KWLink>> CreateLinksForUpdate()
        {
            foreach (var linkModel in UnpublishedLinksToShow)
            {
                if (!linkModel.Edited)
                    continue;

                KWLink newLink = null;
                if (OntologyProvider.GetOntology().IsRelationship(linkModel.LinkType))
                {
                    newLink = await LinkManager.CreateRelationshipBaseLinkAsync(((KWLink)linkModel.Tag).Source,
                        ((KWLink)linkModel.Tag).Target, linkModel.LinkType, linkModel.Direction,
                        null, null, linkModel.Description);
                }
                else if (OntologyProvider.GetOntology().IsEvent(linkModel.LinkType))
                {
                    await Task.Run(() =>
                    {
                        newLink = LinkManager.CreateEventBaseLink(((KWLink)linkModel.Tag).Source,
                            ((KWLink)linkModel.Tag).Target, linkModel.LinkType, linkModel.Direction,
                            null, null, linkModel.Description);
                    });
                }

                PublishedAndNewLinks.Add(newLink);
            }

            PublishedAndNewLinks.AddRange(UnpublishedLinksDictionary.Values);

            return PublishedAndNewLinks;
        }

        private void AllLinksCollectionOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (LinkModel item in e.OldItems)
                {
                    //Removed items
                    item.PropertyChanged -= ItemOnPropertyChanged;
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (LinkModel item in e.NewItems)
                {
                    //Added items
                    item.PropertyChanged += ItemOnPropertyChanged;
                }
            }
        }

        private void ItemOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            ApplyButtonIsEnable = UnpublishedLinksToShow.Any(item => item.Edited);
        }

        #endregion

        #region Create link

        private LinkModel newLinkModel;
        public LinkModel NewLinkModel
        {
            get => newLinkModel;
            set => SetValue(ref newLinkModel, value);
        }

        public void PrepareCreateLink(object sourceObject, object targetObject)
        {
            if (sourceObject == null)
                throw new ArgumentNullException(nameof(sourceObject));

            if (targetObject == null)
                throw new ArgumentNullException(nameof(targetObject));

            NewLinkModel.SourceTypeUri = ((KWObject)sourceObject).TypeURI;
            NewLinkModel.SourceObject = sourceObject;
            NewLinkModel.TargetTypeUri = ((KWObject)targetObject).TypeURI;
            NewLinkModel.TargetObject = targetObject;

            NewLinkModel.SourceDisplayName = ((KWObject)sourceObject).GetObjectLabel().Length >= 10
                ? $"{((KWObject)sourceObject).GetObjectLabel().Substring(0, 10)} ..."
                : ((KWObject)sourceObject).GetObjectLabel();

            NewLinkModel.TargetDisplayName = ((KWObject)targetObject).GetObjectLabel().Length >= 10
                ? $"{((KWObject)targetObject).GetObjectLabel().Substring(0, 10)} ..."
                : ((KWObject)targetObject).GetObjectLabel();
        }

        /// <summary>
        /// ایجاد یک رابطه (لینک) جدید
        /// </summary>
        public async Task<KWLink> CreateNewLink()
        {
            try
            {
                KWLink newLink = null;

                // TODO: آینده - DateTime of link may be set to create a link
                if (OntologyProvider.GetOntology().IsRelationship(NewLinkModel.LinkType))
                {
                    newLink = await LinkManager.CreateRelationshipBaseLinkAsync((KWObject)NewLinkModel.SourceObject,
                        (KWObject)NewLinkModel.TargetObject, NewLinkModel.LinkType,
                        NewLinkModel.Direction, null, null, NewLinkModel.Description);
                }
                else if (OntologyProvider.GetOntology().IsEvent(NewLinkModel.LinkType))
                {
                    await Task.Run(() =>
                    {
                        newLink = LinkManager.CreateEventBaseLink((KWObject)NewLinkModel.SourceObject,
                            (KWObject)NewLinkModel.TargetObject, NewLinkModel.LinkType, NewLinkModel.Direction,
                            null, null, NewLinkModel.Description);
                    });
                }
                else
                {
                    throw new Exception(Properties.Resources
                        .Unable_To_Creat_A_Link_With_A_Type_Except_Children_Of_Relationship_Or_Event_types);
                }

                return newLink;
            }
            catch (Exception ex)
            {
                ExceptionHandler exceptionHandler = new ExceptionHandler();
                exceptionHandler.WriteErrorLog(ex);
                KWMessageBox.Show($"{Properties.Resources.Invalid_Server_Response}\n\n{ex.Message}");
                return null;
            }
        }

        #endregion
    }
}
