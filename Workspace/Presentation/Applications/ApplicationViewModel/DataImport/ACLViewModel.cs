using GPAS.AccessControl;
using GPAS.Workspace.Presentation.Controls.DataImport.Model.Permission;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GPAS.Workspace.Presentation.Applications.ApplicationViewModel.DataImport
{
    public class ACLViewModel : BaseViewModel
    {
        public event EventHandler AclChanged;
        protected void OnAclChanged()
        {
            AclChanged?.Invoke(this, EventArgs.Empty);
        }

        private ObservableCollection<ClassificationModel> classificationCollection;
        public ObservableCollection<ClassificationModel> ClassificationCollection
        {
            get => classificationCollection;
            set
            {
                ObservableCollection<ClassificationModel> oldValue = classificationCollection;

                if (SetValue(ref classificationCollection, value))
                {
                    if (ClassificationCollection == null)
                    {
                        ClassificationCollectionCollectionChanged(this,
                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
                    }
                    else
                    {
                        ClassificationCollection.CollectionChanged -= ClassificationCollectionCollectionChanged;
                        ClassificationCollection.CollectionChanged += ClassificationCollectionCollectionChanged;

                        if (oldValue is null)
                        {
                            ClassificationCollectionCollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, ClassificationCollection));
                        }
                        else
                        {
                            ClassificationCollectionCollectionChanged(this,
                                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, ClassificationCollection, oldValue));
                        }
                    }
                }
            }
        }

        private ACLModel acl;
        public ACLModel Acl
        {
            get => acl;
            set => SetValue(ref acl, value);
        }

        private Visibility waitingVisibility = Visibility.Visible;
        public Visibility WaitingVisibility
        {
            get => waitingVisibility;
            set => SetValue(ref waitingVisibility, value);
        }

        #region Methodes

        public ACLViewModel(ACLModel acl)
        {
            Acl = acl;

            ClassificationCollection = new ObservableCollection<ClassificationModel>();
            Init();

            Acl.ClassificationChanged += Acl_ClassificationChanged;
            Acl.PermissionsChanged += Acl_PermissionsChanged;
            Acl.GroupAccessLevelChanged += Acl_GroupAccessLevelChanged;
        }   

        private void Acl_PermissionsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnAclChanged();
        }

        private void Acl_ClassificationChanged(object sender, EventArgs e)
        {
            OnAclChanged(); 
        }

        private void Acl_GroupAccessLevelChanged(object sender, EventArgs e)
        {
            OnAclChanged();
        }

        private async void Init()
        {
            await GenerateClassificationsCollection();
            await GenerateGroups();
            WaitingVisibility = Visibility.Collapsed;
        }

        private void ClassificationCollectionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems?.Count > 0)
            {
                foreach (ClassificationModel classification in e.OldItems)
                {
                    classification.SelectionChange -= ClassificationSelectionChange;
                }
            }

            if (e.NewItems?.Count > 0)
            {
                foreach (ClassificationModel classification in e.NewItems)
                {
                    classification.SelectionChange -= ClassificationSelectionChange;
                    classification.SelectionChange += ClassificationSelectionChange;
                }
            }
        }

        private void ClassificationSelectionChange(object sender, EventArgs e)
        {
            Acl.Classification = ClassificationCollection.FirstOrDefault(x => x.IsSelected);
        }

        private async Task GenerateClassificationsCollection()
        {
            var result = new List<ClassificationModel>();
            Logic.AccessControl.GroupManagement groupManagement = new Logic.AccessControl.GroupManagement();
            Logic.UserAccountControlProvider authentication = new Logic.UserAccountControlProvider();
            List<string> groupsName = await groupManagement.GetGroupsOfUser(authentication.GetLoggedInUserName());

            var classificationBasedPermissionForGroups = await groupManagement.GetClassificationBasedPermissionForGroups(groupsName);

            IEnumerable<string> relatedClassification = classificationBasedPermissionForGroups.
                SelectMany(gcbp => gcbp.Permissions.Select(p => p.Classification.IdentifierString)).Distinct();

            foreach (var currentEntry in Classification.EntriesTree)
            {
                result.Add(new ClassificationModel()
                {
                    Title = currentEntry.Title,
                    Identifier = currentEntry.IdentifierString,
                    IsSelectable = relatedClassification.Contains(currentEntry.IdentifierString),
                    IsSelected = currentEntry.IdentifierString.Equals(Acl.Classification.Identifier),
                });
            }

            ClassificationCollection = new ObservableCollection<ClassificationModel>(result);
        }

        private async Task GenerateGroups()
        {
            Logic.AccessControl.GroupManagement groupManagement = new Logic.AccessControl.GroupManagement();
            var groups = await groupManagement.GetGroups();

            foreach (var currentGroup in groups)
            {
                if (!Acl.Permissions.Any(x => x.GroupName == currentGroup.GroupName))
                {
                    Acl.Permissions.Add(new ACIModel()
                    {
                        GroupName = currentGroup.GroupName,
                        AccessLevel = Permission.None
                    });
                }
            }
        }
    }

    #endregion
}
