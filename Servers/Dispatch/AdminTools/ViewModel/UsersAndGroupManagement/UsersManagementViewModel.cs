using GPAS.AccessControl;
using GPAS.AccessControl.Groups;
using GPAS.AccessControl.Users;
using GPAS.Dispatch.AdminTools.Model.UserAccountsControl.Windows.ACL;
using GPAS.Dispatch.AdminTools.Model.UserAndGroup;
using GPAS.Dispatch.Logic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.Dispatch.AdminTools.ViewModel.UsersAndGroupManagement
{
    public class UsersManagementViewModel : BaseViewModel
    {
        public UsersManagementViewModel()
        {
            UsersCollection = new ObservableCollection<UserModel>();
            UsersCollectionToShow = new ObservableCollection<UserModel>();
            SelectedUser = new UserModel();
            NewUser = new UserModel();
            AllGroups = new ObservableCollection<GroupModel>();
            UserGroups = new ObservableCollection<GroupModel>();
            SelectedGroup = new GroupModel();
            NewGroup = new GroupModel();

            //Group
            GroupsToShowInModels = new ObservableCollection<GroupModel>();
            GroupsCollection = new ObservableCollection<GroupModel>();
            ShowMemberInGroup = new ObservableCollection<UserModel>();
            ShowingClassifications = new ObservableCollection<ClassificationToShowModel>();
            permissions = new List<ShowingPermissionModel>();
            ClickTreeEnableOrDisableCombo = new ClickTreeEnableOrDisableCombo();
        }

        /// <summary>
        /// لیست گروه ها
        /// </summary>
        public ObservableCollection<GroupModel> GroupsToShowInModels { get; set; }
        public ObservableCollection<GroupModel> GroupsCollection { get; set; }
        public GroupModel SelectedGroup { get; set; }
        public GroupModel NewGroup { get; set; }
        public ObservableCollection<UserModel> ShowMemberInGroup { get; set; }
        public string GroupNameInPermission { get; set; }
        public ObservableCollection<ClassificationToShowModel> ShowingClassifications { get; set; }
        public List<ShowingPermissionModel> permissions { get; set; }
        public ClickTreeEnableOrDisableCombo ClickTreeEnableOrDisableCombo { get; set; }

        /// <summary>
        /// لیست کاربران
        /// </summary>
        public ObservableCollection<UserModel> UsersCollection { get; set; }
        public ObservableCollection<UserModel> UsersCollectionToShow { get; set; }

        /// <summary>
        /// کاربر انتخاب شده برای نمایش اطلاعات
        /// </summary>
        public UserModel SelectedUser { get; set; }

        public UserModel NewUser { get; set; }

        /// <summary>
        /// لیست تمام گروه‌ها
        /// </summary>
        public ObservableCollection<GroupModel> AllGroups { get; set; }
        public List<GroupInfo> DefaultAllGroups { get; set; }

        /// <summary>
        /// لیست گروه‌هایی که کاربر در آنها عضویت دارد
        /// </summary>
        public ObservableCollection<GroupModel> UserGroups { get; set; }
        public string[] DefaultUserGroups { get; set; }

        /// <summary>
        /// عملیات گرفتن لیست کاربران
        /// </summary>
        /// <returns></returns>
        public async Task GetUsers()
        {
            List<UserInfo> users = new List<UserInfo>();
            List<UserInfo> allAdminUsers = new List<UserInfo>();

            await Task.Run(() =>
            {
                UserAccountManagement userAccountManagement = new UserAccountManagement();
                users = userAccountManagement.GetUserAccounts();

                allAdminUsers = GroupMembershipManagement.GetMembershipUsers("Administrators");
            });

            PrepareUsersListToShow(users, allAdminUsers);
        }

        /// <summary>
        /// عملیات تغییر رمز عبور برای کاربر انتخاب شده
        /// </summary>
        /// <param name="selectedUser">کاربر انتخاب شده</param>
        /// <param name="newPassword">رمز عبور جدید</param>
        /// <returns></returns>
        public async Task ChangePassword(object selectedUser, string newPassword)
        {
            await Task.Run(() =>
            {
                UserAccountManagement userAccountManagement = new UserAccountManagement();
                userAccountManagement.ChangePassword(((UserModel)selectedUser).UserName, newPassword);
            });
        }

        /// <summary>
        /// عملیات ویرایش اطلاعات کاربر
        /// </summary>
        /// <returns></returns>
        public async Task EditUserInfo()
        {
            await Task.Run(() =>
            {
                UserAccountManagement userAccountManagement = new UserAccountManagement();
                userAccountManagement.ChangeUserAccountProfile
                (
                    SelectedUser.UserName,
                    SelectedUser.FirstName,
                    SelectedUser.LastName,
                    SelectedUser.Email
                );
            });
        }

        /// <summary>
        /// عملیات گرفتن لیست تمام گروه‌ها و گروه‌هایی که
        /// کاربر در انها عضویت دارد
        /// </summary>
        /// <returns></returns>
        public async Task GetUserGroups(object selectedUser, bool firstTime)
        {
            string[] userGroups = null;
            List<GroupInfo> allGroups = new List<GroupInfo>();

            await Task.Run(() =>
            {
                GroupMembershipManagement groupMembershipManagement = new GroupMembershipManagement();
                userGroups = groupMembershipManagement.GetGroupsOfUser(((UserModel)selectedUser).UserName);

                GroupManagement groupManagement = new GroupManagement();
                allGroups = groupManagement.GetGroups();
            });

            if (firstTime)
            {
                DefaultAllGroups = allGroups;
                DefaultUserGroups = userGroups;
            }

            PrepareGroupsList(allGroups, userGroups, (UserModel)selectedUser);
        }

        public async Task AddUserToGroups(object selectedUser)
        {
            var groupsToAdd = (from @group in AllGroups where @group.IsSelected select @group.Name).ToArray();

            await Task.Run(() =>
            {
                GroupMembershipManagement groupMembershipManagement = new GroupMembershipManagement();
                groupMembershipManagement.SetGroupsMembershipForUser(((UserModel)selectedUser).UserName, groupsToAdd);
            });
        }

        public async Task RemoveUserFromGroups(object selectedUser)
        {
            var groupsToRemove = (from @group in UserGroups where @group.IsSelected select @group.Name).ToArray();

            await Task.Run(() =>
            {
                GroupMembershipManagement groupMembershipManagement = new GroupMembershipManagement();
                groupMembershipManagement.RemoveGroupMemebershipOfUser(((UserModel)selectedUser).UserName, groupsToRemove);
            });
        }

        public async Task RestoreUserGroups(object selectedUser)
        {
            var groupsToRemove = (from groupInfo in DefaultAllGroups
                                  where UserGroups.Any(userGroup => groupInfo.GroupName.Equals(userGroup.Name))
                                  select groupInfo.GroupName).ToList();

            await Task.Run(() =>
            {
                GroupMembershipManagement groupMembershipManagement = new GroupMembershipManagement();
                groupMembershipManagement.RemoveGroupMemebershipOfUser(((UserModel)selectedUser).UserName, groupsToRemove.ToArray());
                groupMembershipManagement.SetGroupsMembershipForUser(((UserModel)selectedUser).UserName, DefaultUserGroups);
            });
        }

        public async Task<bool> CreateNewUser()
        {
            if (NewUser == null)
                throw new ArgumentNullException(nameof(NewUser));

            bool result = true;

            await Task.Run(() =>
            {
                UserAccountManagement userAccountManagement = new UserAccountManagement();
                if (userAccountManagement.CheckUserAccountExists(NewUser.UserName))
                {
                    result = false;
                    return;
                }

                if (NewUser.FirstName == null) NewUser.FirstName = string.Empty;
                if (NewUser.LastName == null) NewUser.LastName = string.Empty;
                if (NewUser.Email == null) NewUser.Email = string.Empty;

                userAccountManagement.CreateNewAccount(NewUser.UserName, NewUser.Password, NewUser.FirstName,
                    NewUser.LastName, NewUser.Email);

                GroupMembershipManagement groupMembershipManagement = new GroupMembershipManagement();
                groupMembershipManagement.CreateNewMembership(NativeGroup.EveryOne.ToString(), NewUser.UserName);
            });

            if (result)
            {
                NewUser.Reset();
            }

            return result;
        }

        public async Task DeleteUser(object selectedUser)
        {
            if (SelectedUser == null)
                throw new ArgumentNullException(nameof(selectedUser));

            await Task.Run(() =>
            {
                GroupMembershipManagement groupMembershipManagement = new GroupMembershipManagement();
                var userGroups = groupMembershipManagement.GetGroupsOfUser(((UserModel)selectedUser).UserName);

                UserAccountManagement userAccountManagement = new UserAccountManagement();
                userAccountManagement.DeleteAccount(((UserModel)selectedUser).Id);

                groupMembershipManagement.RemoveGroupMemebershipOfUser(((UserModel)selectedUser).UserName, userGroups);
            });
        }

        public string GetUserNameOfUser(object selectedUser)
        {
            if (selectedUser == null)
                throw new ArgumentNullException(nameof(selectedUser));

            return ((UserModel)selectedUser).UserName;
        }

        private void PrepareUsersListToShow(List<UserInfo> usersList, List<UserInfo> adminUsers)
        {
            if (UsersCollection.Count != 0)
                UsersCollection.Clear();

            if (UsersCollectionToShow.Count != 0)
                UsersCollectionToShow.Clear();

            foreach (var user in usersList)
            {
                var result = adminUsers.Find(x => x.Id == user.Id);

                bool isAdmin = result != null;

                UsersCollection.Add(new UserModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    CreatedBy = Properties.Resources.Admin,
                    CreatedTime = user.CreatedTime,
                    Email = user.Email,
                    Status = Properties.Resources.Active,
                    IsAdmin = isAdmin
                });

                UsersCollectionToShow.Add(new UserModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    CreatedBy = Properties.Resources.Admin,
                    CreatedTime = user.CreatedTime,
                    Email = user.Email,
                    Status = Properties.Resources.Active,
                    IsAdmin = isAdmin
                });
            }
        }

        /// <summary>
        /// آنالیز لیست تمام گروه ها و لیست گروه‌هایی که کاربر
        /// در آنها عضویت دارد
        /// </summary>
        /// <param name="allGroups">لیست تمام گروه‌ها</param>
        /// <param name="userGroups">لیست گروه‌هایی که کاربر در آنها عضویت دارد</param>
        /// <param name="selectedUser">کاربر انتخاب شده</param>
        private void PrepareGroupsList(List<GroupInfo> allGroups, string[] userGroups, UserModel selectedUser)
        {
            allGroups.RemoveAll(x => userGroups.Contains(x.GroupName));

            if (UserGroups.Count > 0) UserGroups.Clear();
            if (AllGroups.Count > 0) AllGroups.Clear();

            foreach (var group in userGroups)
            {
                UserGroups.Add(new GroupModel
                {
                    Name = group,
                    IsSelected = false,
                    IsEnabled = true
                });
            }

            foreach (var group in allGroups)
            {
                AllGroups.Add(new GroupModel
                {
                    Id = group.Id,
                    Name = group.GroupName,
                    Description = group.Description,
                    CreatedTime = group.CreatedTime,
                    IsSelected = false,
                    IsEnabled = true
                });
            }

            foreach (var group in UserGroups)
            {
                if (group.Name.ToLower().Equals("everyone"))
                {
                    group.IsEnabled = false;
                    break;
                }
            }

            if (selectedUser.UserName.ToLower().Equals("admin"))
            {
                foreach (var group in UserGroups)
                {
                    if (group.Name.ToLower().Equals("administrators"))
                    {
                        group.IsEnabled = false;
                        break;
                    }
                }
            }
        }

        public void SelectUserToShowDetails(object selectedUser)
        {
            var user = (UserModel)selectedUser;

            SelectedUser.Id = user.Id;
            SelectedUser.UserName = user.UserName;
            SelectedUser.FirstName = user.FirstName;
            SelectedUser.LastName = user.LastName;
            SelectedUser.CreatedTime = user.CreatedTime;
            SelectedUser.CreatedBy = user.CreatedBy;
            SelectedUser.Email = user.Email;
            SelectedUser.Status = user.Status;
        }

        public void FilterUsersList(string userName)
        {
            if (UsersCollectionToShow.Count > 0)
                UsersCollectionToShow.Clear();

            foreach (var user in UsersCollection)
            {
                if (user.UserName.Contains(userName))
                {
                    UsersCollectionToShow.Add(user);
                }
            }
        }

        //کد‌های مربوط به بخش مدیریت گروه‌ها

        public string GetNameGroup(object selectedGroup)
        {
            try
            {
                var groupName = (GroupModel)selectedGroup;
                return groupName?.Name;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task GetGroups()
        {
            List<GroupInfo> groups = new List<GroupInfo>();

            await Task.Run(() =>
            {
                GroupManagement groupManagement = new GroupManagement();
                groups = groupManagement.GetGroups();
            });

            PrepareGroupsListToShow(groups);
        }

        private void PrepareGroupsListToShow(List<GroupInfo> groups)
        {
            if (GroupsCollection.Count != 0)
                GroupsCollection.Clear();

            if (GroupsToShowInModels.Count != 0)
                GroupsToShowInModels.Clear();

            foreach (var group in groups)
            {
                GroupsCollection.Add(new GroupModel()
                {
                    Name = group.GroupName,
                    CreatedBy = group.CreatedBy,
                    CreatedTime = group.CreatedTime,
                    Description = group.Description,
                    Id = group.Id,

                });

                GroupsToShowInModels.Add(new GroupModel()
                {
                    Name = group.GroupName,
                    CreatedBy = group.CreatedBy,
                    CreatedTime = group.CreatedTime,
                    Description = group.Description,
                    Id = group.Id,
                });
            }
        }

        public void SelectGroupToShowDetails(object selectedGroup)
        {
            var group = (GroupModel)selectedGroup;

            SelectedGroup.Id = group.Id;
            SelectedGroup.Name = group.Name;
            SelectedGroup.CreatedBy = group.CreatedBy;
            SelectedGroup.CreatedTime = group.CreatedTime;
            SelectedGroup.Description = group.Description;
        }

        public void FilterGroupList(string userName)
        {
            if (GroupsToShowInModels.Count > 0)
                GroupsToShowInModels.Clear();

            foreach (var group in GroupsCollection)
            {
                if (group.Name.Contains(userName))
                {
                    GroupsToShowInModels.Add(group);
                }
            }
        }

        public async Task<bool> CreateNewGroup()
        {
            if (NewGroup == null)
                throw new ArgumentNullException(nameof(NewGroup));

            bool result = true;

            await Task.Run(() =>
            {
                GroupManagement groupManagement = new GroupManagement();

                if (groupManagement.CheckGroupExists(NewGroup.Name))
                {
                    result = false;
                    return;
                }

                if (NewGroup.Name == null) NewGroup.Name = string.Empty;
                if (NewGroup.Description == null) NewGroup.Description = string.Empty;

                groupManagement.CreateNewGroup(NewGroup.Name, NewGroup.Description, NativeUser.Admin.ToString());
            });

            if (result)
            {
                NewGroup.Reset();
            }

            return result;
        }

        public async Task DeleteGroup(object selectedGroup)
        {
            var group = (GroupModel)selectedGroup;

            if (group.Name == "Administrators" || group.Name == "EveryOne")
                return;

            await Task.Run(() =>
            {
                GroupManagement groupManagement = new GroupManagement();
                groupManagement.DeleteGroup(group.Id);
                GroupMembershipManagement groupMembershipManagement = new GroupMembershipManagement();
                groupMembershipManagement.RemoveMembership(group.Name);
            });
        }

        public void GetShipGroupUserMember(object selectedGroup)
        {
            if (ShowMemberInGroup.Count > 0)
            {
                ShowMemberInGroup.Clear();
            }
            var group = (GroupModel)selectedGroup;
            foreach (var item in GroupMembershipManagement.GetMembershipUsers(group.Name))
            {
                ShowMemberInGroup.Add(new UserModel
                {
                    UserName = item.UserName
                });
            }
            //  var usersList = GroupMembershipManagement.GetMembershipUsers(group.Name);
        }

        public void SelectGroupMemberList(object memberName)
        {
            var userModel = (UserModel)memberName;

            if (UsersCollectionToShow.Count > 0)
            {
                UsersCollectionToShow.Clear();
            }

            foreach (var user in UsersCollection)
            {
                user.IsSelected = false;
                if (user.UserName == userModel.UserName)
                {
                    user.IsSelected = true;
                    SelectedUser.Id = user.Id;
                    SelectedUser.UserName = user.UserName;
                    SelectedUser.LastName = user.LastName;
                    SelectedUser.Email = user.Email;
                    SelectedUser.Status = user.Status;
                    SelectedUser.CreatedBy = user.CreatedBy;
                }
                UsersCollectionToShow.Add(user);
            }
        }

        /// <summary>
        /// Create permissions
        /// </summary>
        /// <returns></returns>
        public List<string> GeneratePermissionsToShow()
        {
            return Enum.GetNames(typeof(Permission)).ToList();
        }

        public ObservableCollection<ClassificationToShowModel> GenerateTree(string groupName)
        {
            try
            {
                GroupManagement groupManagement = new GroupManagement();

                List<GroupClassificationBasedPermission> classificationBasedPermissionForGroup =
                    groupManagement.GetClassificationBasedPremissionForGroups(new[] { groupName });

                List<GroupClassificationBasedPermission> sortedClassificationBasedPermissionForGroup =
                    GetSortedGroupClassificationBasedPermission(classificationBasedPermissionForGroup);

                ShowingClassifications.Clear();
                ShowingClassifications = GenerateClassificationsFromClassificationBasedPermissionForGroup(
                        sortedClassificationBasedPermissionForGroup);

                return ShowingClassifications;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private List<GroupClassificationBasedPermission> GetSortedGroupClassificationBasedPermission(
            List<GroupClassificationBasedPermission> classificationBasedPermissionForGroup)
        {
            List<GroupClassificationBasedPermission> result = new List<GroupClassificationBasedPermission>();
            foreach (var currentItem in classificationBasedPermissionForGroup)
            {
                GroupClassificationBasedPermission groupClassificationBasedPermission = new GroupClassificationBasedPermission()
                {
                    GroupName = currentItem.GroupName,
                    Permissions = GenerateSortedPermissions(currentItem.Permissions)
                };
                result.Add(groupClassificationBasedPermission);
            }

            return result;
        }

        private List<ClassificationBasedPermission> GenerateSortedPermissions(List<ClassificationBasedPermission> permissionList)
        {
            List<ClassificationBasedPermission> result = new List<ClassificationBasedPermission>();
            List<ClassificationEntry> classificationEntries = Classification.EntriesTree.ToList();

            foreach (var currentClassificationEntry in classificationEntries)
            {
                foreach (var currentPermission in permissionList)
                {

                    if (currentPermission.Classification.IdentifierString.Equals(currentClassificationEntry.IdentifierString))
                    {
                        result.Add(currentPermission);
                    }
                }
            }

            return result;
        }

        public void SelectionComboBox(object selectionComboBox, object treeSelectedItem, object treeItemSource)
        {
            ChangeClassificationTreeViewStatus(selectionComboBox, treeSelectedItem, treeItemSource);
        }

        private void ChangeClassificationTreeViewStatus(object showingPermission, object treeSelectedItem, object treeItemSource)
        {
            if (treeSelectedItem != null &&
                showingPermission != null)
            {
                ClassificationToShowModel selectedClassification = (ClassificationToShowModel)treeSelectedItem;
                ObservableCollection<ClassificationToShowModel> showingClassifications =
                    treeItemSource as ObservableCollection<ClassificationToShowModel>;
                int selectedClassificationIndex = Classification.GetClassificationIndex(selectedClassification.ClassificationIdentifier);
                int selectedIndex = (int)Enum.Parse(typeof(Permission), showingPermission.ToString());

                if (selectedClassificationIndex == 1)
                {
                    selectedClassification.ClassPermission = GetValueFromPermissionEnum(selectedIndex);
                    SetClassificationForChildren(selectedClassification, ref showingClassifications, selectedIndex);
                }
                else if (selectedClassificationIndex == Classification.EntriesTree.ToList().Count)
                {
                    selectedClassification.ClassPermission = GetValueFromPermissionEnum(selectedIndex);
                    SetClassificationForParents(selectedClassification, ref showingClassifications, selectedIndex);
                }
                else
                {
                    selectedClassification.ClassPermission = GetValueFromPermissionEnum(selectedIndex);
                    SetClassificationForChildren(selectedClassification, ref showingClassifications, selectedIndex);
                    SetClassificationForParents(selectedClassification, ref showingClassifications, selectedIndex);
                }
            }
        }

        private static Permission GetValueFromPermissionEnum(int index)
        {
            Permission permission = Permission.Discovery;
            int value = index;

            if (Enum.IsDefined(typeof(Permission), value))
                permission = ((Permission)value);

            return permission;
        }

        private void SetClassificationForParents(ClassificationToShowModel selectedClassification,
            ref ObservableCollection<ClassificationToShowModel> showingClassifications, int selectedIndex)
        {
            int selectedClassificationIndex = Classification.GetClassificationIndex(selectedClassification.ClassificationIdentifier);
            List<ClassificationToShowModel> classifications = GetClassificationsFromTreeViewStructure(showingClassifications);

            for (int i = 0; i < selectedClassificationIndex; i++)
            {
                int parentIndex = (int)Enum.Parse(typeof(Permission), classifications[i].ClassPermission.ToString());

                if (parentIndex < selectedIndex)
                {
                    classifications[i].ClassPermission = GetValueFromPermissionEnum(selectedIndex);
                }
            }

            showingClassifications = GetTreeViewStructureFromClassifications(classifications);
        }

        private void SetClassificationForChildren(ClassificationToShowModel selectedClassification,
            ref ObservableCollection<ClassificationToShowModel> showingClassifications, int selectedIndex)
        {
            int selectedClassificationIndex = Classification.GetClassificationIndex(selectedClassification.ClassificationIdentifier);
            List<ClassificationToShowModel> classifications = GetClassificationsFromTreeViewStructure(showingClassifications);

            for (int i = selectedClassificationIndex; i < classifications.Count; i++)
            {
                int childIndex = (int)Enum.Parse(typeof(Permission), classifications[i].ClassPermission.ToString());

                if (childIndex > selectedIndex)
                {
                    classifications[i].ClassPermission = GetValueFromPermissionEnum(selectedIndex);
                }
            }

            showingClassifications = GetTreeViewStructureFromClassifications(classifications);
        }

        private ObservableCollection<ClassificationToShowModel> GetTreeViewStructureFromClassifications(
            List<ClassificationToShowModel> classifications)
        {
            ObservableCollection<ClassificationToShowModel> result = new ObservableCollection<ClassificationToShowModel>();
            List<ClassificationToShowModel> temp = new List<ClassificationToShowModel>();
            var classificationToShow = new ClassificationToShowModel()
            {
                IsExpanded = true,
                IsLeafClassification = false,
                IsRootClassification = true,
                ClassificationIdentifier = "N",
                ClassPermission = Permission.None,
                Title = "None"
            };

            classificationToShow.Classifications.Add(classifications.ElementAt(0));
            temp.Add(classificationToShow);

            foreach (var item in classifications)
            {
                temp.Add(item);
            }

            result.Add(temp[0]);
            return result;
        }

        private List<ClassificationToShowModel> GetClassificationsFromTreeViewStructure(
            ObservableCollection<ClassificationToShowModel> treeViewStructureClassifications)
        {
            List<ClassificationToShowModel> result = new List<ClassificationToShowModel>();
            foreach (var currentClassification in treeViewStructureClassifications)
            {
                if (currentClassification.Classifications.Count > 0)
                {
                    AddClassificationToList(currentClassification.Classifications.ElementAt(0), ref result);
                }
            }

            return result;
        }

        private void AddClassificationToList(ClassificationToShowModel classification,
            ref List<ClassificationToShowModel> showingClassifications)
        {
            if (!classification.IsLeafClassification)
            {
                showingClassifications.Add(classification);
                AddClassificationToList(classification.Classifications.ElementAt(0), ref showingClassifications);
            }
            else
            {
                showingClassifications.Add(classification);
            }
        }

        private ObservableCollection<ClassificationToShowModel> GenerateClassificationsFromClassificationBasedPermissionForGroup(
                List<GroupClassificationBasedPermission> classificationBasedPermissionForGroup)
        {
            ObservableCollection<ClassificationToShowModel> result =
                new ObservableCollection<ClassificationToShowModel>();

            List<ClassificationToShowModel> temp = new List<ClassificationToShowModel>();

            var classificationToShow = new ClassificationToShowModel()
            {
                IsExpanded = true,
                IsLeafClassification = false,
                IsRootClassification = true,
                ClassificationIdentifier = "N",
                ClassPermission = Permission.None,
                Title = "None"
            };

            temp.Add(classificationToShow);

            foreach (var currentGroup in classificationBasedPermissionForGroup)
            {
                foreach (var currentPermission in currentGroup.Permissions)
                {
                    ClassificationEntry relatedEntry =
                        Classification.GetClassificationEntryByIdentifier(currentPermission.Classification
                            .IdentifierString);

                    if (currentPermission == currentGroup.Permissions.Last())
                    {
                        classificationToShow = new ClassificationToShowModel()
                        {
                            IsExpanded = true,
                            IsLeafClassification = true,
                            IsRootClassification = false,
                            ClassificationIdentifier = relatedEntry.IdentifierString,
                            ClassPermission = currentPermission.AccessLevel,
                            Title = relatedEntry.Title
                        };
                    }
                    else
                    {
                        classificationToShow = new ClassificationToShowModel()
                        {
                            IsExpanded = true,
                            IsLeafClassification = false,
                            IsRootClassification = false,
                            ClassificationIdentifier = relatedEntry.IdentifierString,
                            ClassPermission = currentPermission.AccessLevel,
                            Title = relatedEntry.Title
                        };
                    }

                    temp.Add(classificationToShow);
                }
            }

            for (int i = 0; i < temp.Count; i++)
            {
                if (i != (temp.Count - 1))
                {
                    temp[i].Classifications.Add(temp[i + 1]);
                }
            }

            result.Add(temp[0]);
            return result;
        }

        //توابع زیر برای کلیک برروی درخت هستند

        public void SelectTreeNode(object selectedTree)
        {
            try
            {
                if (selectedTree != null)
                {
                    ClassificationToShowModel selectedClassification = (ClassificationToShowModel)selectedTree;
                    int selectedClassificationIndex = Classification.GetClassificationIndex(selectedClassification.ClassificationIdentifier);

                    if (selectedClassificationIndex == 0)
                    {
                        ClickTreeEnableOrDisableCombo.IsEnabled = false;
                        ClickTreeEnableOrDisableCombo.Title = selectedClassification.Title;
                    }
                    else
                    {
                        int index = (int)Enum.Parse(typeof(Permission), selectedClassification.ClassPermission.ToString());
                        ClickTreeEnableOrDisableCombo.IsEnabled = true;
                        ClickTreeEnableOrDisableCombo.Title = selectedClassification.Title;
                        ClickTreeEnableOrDisableCombo.Index = index;
                    }
                }
            }
            catch (Exception)
            {
                // Do nothing
            }
        }

        /// <summary>
        /// ذخیره تغییرات سطح دسترسی
        /// </summary>
        /// <param name="itemSourceTree">درخت سطح دسترسی</param>
        /// <returns>نتیجه</returns>
        public bool SavePermission(object itemSourceTree)
        {
            try
            {
                if (itemSourceTree != null)
                {
                    ObservableCollection<ClassificationToShowModel> showingClassifications = itemSourceTree as ObservableCollection<ClassificationToShowModel>;
                    List<KeyValuePair<string, Permission>> permissionPerClassification = GeneratePermissionPerClassification(showingClassifications);
                    GroupManagement groupManagement = new GroupManagement();
                    groupManagement.SetClassificationBasedPremissionForGroup(GroupNameInPermission, permissionPerClassification);
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private List<KeyValuePair<string, Permission>> GeneratePermissionPerClassification(
            ObservableCollection<ClassificationToShowModel> treeViewClassifications)
        {
            List<ClassificationToShowModel> showingClassifications = GetClassificationsFromTreeViewStructure(treeViewClassifications);
            List<KeyValuePair<string, Permission>> result = new List<KeyValuePair<string, Permission>>();
            foreach (var currentClassification in showingClassifications)
            {
                result.Add(new KeyValuePair<string, Permission>(currentClassification.ClassificationIdentifier,
                    currentClassification.ClassPermission));
            }
            return result;
        }
    }
}
