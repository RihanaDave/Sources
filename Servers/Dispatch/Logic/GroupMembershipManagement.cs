using GPAS.AccessControl.Groups;
using GPAS.AccessControl.Users;
using GPAS.Dispatch.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.Dispatch.Logic
{
    public class GroupMembershipManagement
    {
        public void Init()
        {
            GroupMembershipDatabaseAccess databaseAccess = new GroupMembershipDatabaseAccess();
            databaseAccess.CreateTable();
            databaseAccess.CreateNewMembership(NativeGroup.Administrators.ToString(), NativeUser.Admin.ToString());
            databaseAccess.CreateNewMembership(NativeGroup.EveryOne.ToString(), NativeUser.Admin.ToString());
        }
        public void CreateNewMembership(int groupId, int userId)
        {
            GroupMembershipDatabaseAccess databaseAccess = new GroupMembershipDatabaseAccess();
            databaseAccess.CreateNewMembership(groupId, userId);
        }

        public void CreateNewMembership(string groupName, string userName)
        {
            GroupMembershipDatabaseAccess databaseAccess = new GroupMembershipDatabaseAccess();
            databaseAccess.CreateNewMembership(groupName, userName);
        }

        public static List<UserInfo> GetMembershipUsers(string groupName)
        {
            GroupMembershipDatabaseAccess databaseAccess = new GroupMembershipDatabaseAccess();
            return databaseAccess.GetMembershipUsers(groupName);
        }
        public string[] GetGroupsOfUser(string username)
        {
            GroupMembershipDatabaseAccess databaseAccess = new GroupMembershipDatabaseAccess();
            return databaseAccess.GetGroupsOfUser(username);
        }
        public void SetGroupsMembershipForUser(string userName, string[] groupNames)
        {
            GroupMembershipDatabaseAccess databaseAccess = new GroupMembershipDatabaseAccess();          
            foreach (string groupName in groupNames)
            {
                CreateNewMembership(groupName, userName);
            }
        }
        public void RemoveMembership(string groupName)
        {
            GroupMembershipDatabaseAccess databaseAccess = new GroupMembershipDatabaseAccess();
            databaseAccess.RemoveMembership(groupName);
        }
        public void RevokeMembership(string userName, string groupName)
        {
            GroupMembershipDatabaseAccess databaseAccess = new GroupMembershipDatabaseAccess();
            databaseAccess.RevokeMembership(userName, groupName);
        }
        public void RemoveGroupMemebershipOfUser(string userName, string[] groupNames)
        {
            string[] currentGroups = GetGroupsOfUser(userName);
            foreach (string groupName in groupNames)
            {
                if (!currentGroups.Contains(groupName))
                    throw new InvalidOperationException();
                RevokeMembership(userName, groupName);
            }
        }
    }
}
