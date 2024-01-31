using GPAS.AccessControl.Groups;
using GPAS.AccessControl.Users;
using GPAS.Dispatch.Logic;
using GPAS.Logger;
using System;
using System.Collections.Generic;

namespace GPAS.Dispatch
{
    public class TestService : ITestService
    {
        private void WriteErrorLog(Exception ex)
        {
            ExceptionHandler exceptionHandler = new ExceptionHandler();
            exceptionHandler.WriteErrorLog(ex);
        }

        public List<GroupInfo> GetGroups()
        {
            try
            {
                GroupManagement groupAccountManagement = new GroupManagement();
                return groupAccountManagement.GetGroups();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void CreateNewGroup(string groupName, string description, string createdBy, bool appendGroupToSearchSchemas = true)
        {
            try
            {
                GroupManagement groupAccountManagement = new GroupManagement();
                if (groupAccountManagement.CheckGroupExists(groupName))
                {
                    return;
                }
                groupAccountManagement.CreateNewGroup(groupName, description, createdBy);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void CreateNewMembership(string groupName, string userName)
        {
            try
            {
                GroupMembershipManagement groupMembershipManagement = new GroupMembershipManagement();
                groupMembershipManagement.CreateNewMembership(groupName, userName);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public string[] GetGroupsOfUser(string username)
        {
            try
            {
                GroupMembershipManagement groupMembershipManagement = new GroupMembershipManagement();
                return groupMembershipManagement.GetGroupsOfUser(username);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<UserInfo> GetMembershipUsers(string groupName)
        {
            try
            {
                return GroupMembershipManagement.GetMembershipUsers(groupName);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void RemoveMembership(string groupName)
        {
            try
            {
                GroupMembershipManagement groupMembershipManagement = new GroupMembershipManagement();
                groupMembershipManagement.RemoveMembership(groupName);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void RevokeMembership(string userName, string groupName)
        {
            try
            {
                GroupMembershipManagement groupMembershipManagement = new GroupMembershipManagement();
                groupMembershipManagement.RevokeMembership(userName, groupName);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void SetGroupsMembershipForUser(string userName, string[] groupNames)
        {
            try
            {
                GroupMembershipManagement groupMembershipManagement = new GroupMembershipManagement();
                groupMembershipManagement.SetGroupsMembershipForUser(userName, groupNames);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void CreateNewAccount(string userName, string password, string firstName, string lastName, string email)
        {
            try
            {
                UserAccountManagement userAccountManagement = new UserAccountManagement();
                userAccountManagement.CreateNewAccount(userName, password, firstName, lastName, email);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void ChangeUserAccountProfile(string userName, string newFirstName, string newLastName, string newEmail)
        {
            try
            {
                UserAccountManagement userAccountManagement = new UserAccountManagement();
                userAccountManagement.ChangeUserAccountProfile(userName, newFirstName, newLastName, newEmail);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public void ChangePassword(string userName, string newPassword)
        {
            try
            {
                UserAccountManagement userAccountManagement = new UserAccountManagement();
                userAccountManagement.ChangePassword(userName, newPassword);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public List<UserInfo> GetUserAccounts()
        {
            try
            {
                UserAccountManagement userAccountManagement = new UserAccountManagement();
                return userAccountManagement.GetUserAccounts();
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }

        public bool Authenticate(string userName, string password)
        {
            try
            {
                UserAccountManagement userAccountManagement = new UserAccountManagement();
                return userAccountManagement.Authenticate(userName, password);
            }
            catch (Exception ex)
            {
                WriteErrorLog(ex);
                throw;
            }
        }
    }
}
