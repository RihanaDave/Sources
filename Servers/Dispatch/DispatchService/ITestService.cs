using GPAS.AccessControl.Groups;
using GPAS.AccessControl.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch
{
    [ServiceContract]
    public interface ITestService
    {
        #region Group Managment

        [OperationContract]
        void CreateNewGroup(string groupName, string description, string createdBy, bool appendGroupToSearchSchemas = true);

        [OperationContract]
        List<GroupInfo> GetGroups();

        #endregion

        #region User Managment

        [OperationContract]
        void CreateNewMembership(string groupName, string userName);

        [OperationContract]
        List<UserInfo> GetMembershipUsers(string groupName);

        [OperationContract]
        string[] GetGroupsOfUser(string username);

        [OperationContract]
        void SetGroupsMembershipForUser(string userName, string[] groupNames);

        [OperationContract]
        void RevokeMembership(string userName, string groupName);

        [OperationContract]
        void RemoveMembership(string groupName);

        [OperationContract]
        void CreateNewAccount(string userName, string password, string firstName, string lastName, string email);

        [OperationContract]
        void ChangeUserAccountProfile(string userName, string newFirstName, string newLastName, string newEmail);

        [OperationContract]
        void ChangePassword(string userName, string newPassword);

        [OperationContract]
        List<UserInfo> GetUserAccounts();

        [OperationContract]
        bool Authenticate(string userName, string password);
        #endregion
    }
}
