using GPAS.AccessControl;
using GPAS.AccessControl.Groups;
using GPAS.AccessControl.Users;
using GPAS.Dispatch.DataAccess;
using System;
using System.Collections.Generic;

namespace GPAS.Dispatch.Logic
{
    public class GroupManagement
    {
        private List<KeyValuePair<string, Permission>> SetDefaultClassificationBasedPermissions()
        {
            List<KeyValuePair<string, Permission>> DefaultClassificationBasedPermissions = new List<KeyValuePair<string, Permission>>();
            foreach (var entity in Classification.EntriesTree)
            {
                DefaultClassificationBasedPermissions.Add(new KeyValuePair<string, Permission>(entity.IdentifierString, Permission.Owner));
            }
            return DefaultClassificationBasedPermissions;
        }

        public void Init()
        {
            GroupControlDatabaseAccess databaseAccess = new GroupControlDatabaseAccess();
            databaseAccess.CreateDataBase();
            databaseAccess.CreateTable();
            databaseAccess.CreateIndex();
            ClassificationBasedGroupPermissionsDataAccess classificationBasedPermissions = new ClassificationBasedGroupPermissionsDataAccess();
            classificationBasedPermissions.CreateDataBase();
            classificationBasedPermissions.CreateTable();

            bool checkAdminGroup = databaseAccess.CheckGroupExists(NativeGroup.Administrators.ToString());
            if (!checkAdminGroup)
            {
                CreateNewGroup(NativeGroup.Administrators.ToString(), NativeGroup.Administrators.ToString(), "Dispatch Initializer", false);
            }

            bool checkEveryOneGroup = databaseAccess.CheckGroupExists(NativeGroup.EveryOne.ToString());
            if (!checkEveryOneGroup)
            {
                CreateNewGroup(NativeGroup.EveryOne.ToString(), NativeGroup.EveryOne.ToString(), "Dispatch Initializer", false);
            }
        }

        public void CreateNewGroup(string groupName, string description, string createdBy, bool appendGroupToSearchSchemas = true)
        {
            if (!GroupNameValidator.IsGroupNameValid(groupName))
                throw new ArgumentException("groupName");
            if (description == null)
                throw new ArgumentNullException("description");
            if (createdBy == null)
                throw new ArgumentNullException("createdBy");

            GroupControlDatabaseAccess databaseAccess = new GroupControlDatabaseAccess();

            if (!databaseAccess.CheckGroupExists(groupName))
            {
                databaseAccess.CreateNewGroup(groupName, description, createdBy);
                List<KeyValuePair<string, Permission>> defaultClassification = SetDefaultClassificationBasedPermissions();
                SetClassificationBasedPremissionForGroup(groupName, defaultClassification);

                if (appendGroupToSearchSchemas)
                {
                    string[] groupArray = new string[] { groupName };

                    //var horizonClient = new ServiceAccess.HorizonService.ServiceClient();
                    //horizonClient.AddNewGroupPropertiesToEdgeClass(groupArray);

                    var searchClient = new ServiceAccess.SearchService.ServiceClient();
                    searchClient.AddNewGroupFieldsToSearchServer(groupArray);
                }
            }
            else
            {
                throw new InvalidOperationException("A group with specified name already exist");
            }
        }

        public List<GroupInfo> GetGroups()
        {
            GroupControlDatabaseAccess databaseAccess = new GroupControlDatabaseAccess();
            return databaseAccess.GetGroups();
        }

        public bool CheckGroupExists(string groupName)
        {
            GroupControlDatabaseAccess databaseAccess = new GroupControlDatabaseAccess();
            return databaseAccess.CheckGroupExists(groupName);
        }

        public void DeleteGroup(int id)
        {
            GroupControlDatabaseAccess databaseAccess = new GroupControlDatabaseAccess();
            databaseAccess.DeleteGroup(id);
        }
        public void SetClassificationBasedPremissionForGroup(string groupName, List<KeyValuePair<string, Permission>> permissionPerClassification)
        {
            ClassificationBasedGroupPermissionsDataAccess classificationBasedPermissions = new ClassificationBasedGroupPermissionsDataAccess();
            classificationBasedPermissions.UpdateGroupClassificationBasedPermissions(groupName, permissionPerClassification);
            classificationBasedPermissions.ApplyChanges();
        }
        public List<GroupClassificationBasedPermission> GetClassificationBasedPremissionForGroups(string[] groupNames)
        {
            ClassificationBasedGroupPermissionsDataAccess classificationBasedPermissions = new ClassificationBasedGroupPermissionsDataAccess();
            List<GroupClassificationBasedPermission> classificationBasedPremissionForGroups = new List<GroupClassificationBasedPermission>();
            foreach (string groupName in groupNames)
            {
                classificationBasedPremissionForGroups.Add(new GroupClassificationBasedPermission
                {
                    GroupName = groupName,
                    Permissions = classificationBasedPermissions.GetPermissionsForGroup(groupName)
                });
            }
            return classificationBasedPremissionForGroups;

        }
    }
}
