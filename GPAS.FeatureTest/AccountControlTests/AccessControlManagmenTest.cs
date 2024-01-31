using GPAS.FeatureTest.DispatchTestServiceAccess.TestService;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.FeatureTest.AccountControlTests
{
    [TestClass]
    public class AccessControlManagmenTest
    {
        [TestMethod]
        public async Task CreatGroups()
        {
            // Assign
            TestServiceClient proxy = new TestServiceClient();
            string groupName1 = $"g{Guid.NewGuid().ToString()}g1".Replace("-", "");
            string groupDescription1 = "g1";
            string groupCreatedBy1 = "test feature";

            string groupName2 = $"g{Guid.NewGuid().ToString()}g2".Replace("-", "");
            string groupDescription2 = "g2";
            string groupCreatedBy2 = "test feature";
            // Act
            await proxy.CreateNewGroupAsync(groupName1, groupDescription1, groupCreatedBy1, true);
            await proxy.CreateNewGroupAsync(groupName2, groupDescription2, groupCreatedBy2, true);
            var groups = proxy.GetGroups();

            // Assert
            Assert.AreEqual(groupName1, groups.Where(g => g.GroupName == groupName1).FirstOrDefault().GroupName.ToString());
            Assert.AreEqual(groupName2, groups.Where(g => g.GroupName == groupName2).FirstOrDefault().GroupName.ToString());
        }

        [TestMethod]
        public async Task CreateNewAccount()
        {
            // Assign
            TestServiceClient proxy = new TestServiceClient();
            string groupName1 = $"g{Guid.NewGuid().ToString()}g1".Replace("-", "");
            string groupDescription1 = "g1";
            string groupCreatedBy1 = "test feature";

            string groupName2 = $"g{Guid.NewGuid().ToString()}g2".Replace("-", "");
            string groupDescription2 = "g2";
            string groupCreatedBy2 = "test feature";

            string userName1 = $"{Guid.NewGuid().ToString()} u1";
            string userFirsName1 = "u1";
            string userLastName1 = "u1";
            string userEmailName1 = "u1@mail.com";
            string userName1Pass = "1";


            string userName2 = $"{Guid.NewGuid().ToString()} u2";
            string userFirsName2 = "u2";
            string userLastName2 = "u2";
            string userEmailName2 = "u2@mail.com";
            string userName2Pass = "2";

            // Act
            await proxy.CreateNewGroupAsync(groupName1, groupDescription1, groupCreatedBy1, true);
            await proxy.CreateNewGroupAsync(groupName2, groupDescription2, groupCreatedBy2, true);

            await proxy.CreateNewAccountAsync(userName1, userName1Pass, userFirsName1, userLastName1, userEmailName1);
            await proxy.CreateNewAccountAsync(userName2, userName2Pass, userFirsName2, userLastName2, userEmailName2);
            var groups = proxy.GetGroups();

            var users = proxy.GetUserAccounts();

            // Assert
            Assert.AreEqual(groupName1, groups.Where(g => g.GroupName == groupName1).FirstOrDefault().GroupName.ToString());
            Assert.AreEqual(userName1, users.Where(g => g.UserName == userName1).FirstOrDefault().UserName.ToString());
            Assert.AreEqual(userName2, users.Where(g => g.UserName == userName2).FirstOrDefault().UserName.ToString());

        }

        [TestMethod]
        public async Task GetMembershipUsers()
        {
            // Assign
            TestServiceClient proxy = new TestServiceClient();
            string groupName1 = $"g{Guid.NewGuid().ToString()}g1".Replace("-", "");
            string groupDescription1 = "g1";
            string groupCreatedBy1 = "test feature";

            string groupName2 = $"g{Guid.NewGuid().ToString()}g2".Replace("-", "");
            string groupDescription2 = "g2";
            string groupCreatedBy2 = "test feature";

            string userName1 = $"{Guid.NewGuid().ToString()} u1";
            string userFirsName1 = "u1";
            string userLastName1 = "u1";
            string userEmailName1 = "u1@mail.com";
            string userName1Pass = "1";


            string userName2 = $"{Guid.NewGuid().ToString()} u2";
            string userFirsName2 = "u2";
            string userLastName2 = "u2";
            string userEmailName2 = "u2@mail.com";
            string userName2Pass = "2";

            // Act
            await proxy.CreateNewGroupAsync(groupName1, groupDescription1, groupCreatedBy1, true);
            await proxy.CreateNewGroupAsync(groupName2, groupDescription2, groupCreatedBy2, true);

            await proxy.CreateNewAccountAsync(userName1, userName1Pass, userFirsName1, userLastName1, userEmailName1);
            await proxy.CreateNewAccountAsync(userName2, userName2Pass, userFirsName2, userLastName2, userEmailName2);

            await proxy.CreateNewMembershipAsync(groupName1, userName1);
            await proxy.CreateNewMembershipAsync(groupName2, userName2);

            var membershipUsersg1 = proxy.GetMembershipUsers(groupName1);
            var membershipUsersg2 = proxy.GetMembershipUsers(groupName2);

            // Assert
            Assert.AreEqual(userName1, membershipUsersg1.Where(g => g.UserName == userName1).FirstOrDefault().UserName.ToString());
            Assert.AreEqual(userName2, membershipUsersg2.Where(g => g.UserName == userName2).FirstOrDefault().UserName.ToString());
        }

        [TestMethod]
        public async Task GetGroupsOfUser()
        {
            // Assign
            TestServiceClient proxy = new TestServiceClient();
            string groupName1 = $"g{Guid.NewGuid().ToString()}g1".Replace("-", "");
            string groupDescription1 = "g1";
            string groupCreatedBy1 = "test feature";

            string groupName2 = $"g{Guid.NewGuid().ToString()}g2".Replace("-", "");
            string groupDescription2 = "g2";
            string groupCreatedBy2 = "test feature";

            string userName1 = $"{Guid.NewGuid().ToString()} u1";
            string userFirsName1 = "u1";
            string userLastName1 = "u1";
            string userEmailName1 = "u1@mail.com";
            string userName1Pass = "1";

            string userName2 = $"{Guid.NewGuid().ToString()} u2";
            string userFirsName2 = "u2";
            string userLastName2 = "u2";
            string userEmailName2 = "u2@mail.com";
            string userName2Pass = "2";

            string userName3 = $"{Guid.NewGuid().ToString()} u3";
            string userFirsName3 = "u3";
            string userLastName3 = "u3";
            string userEmailName3 = "u3@mail.com";
            string userName3Pass = "3";

            // Act
            await proxy.CreateNewGroupAsync(groupName1, groupDescription1, groupCreatedBy1, true);
            await proxy.CreateNewGroupAsync(groupName2, groupDescription2, groupCreatedBy2, true);

            await proxy.CreateNewAccountAsync(userName1, userName1Pass, userFirsName1, userLastName1, userEmailName1);
            await proxy.CreateNewAccountAsync(userName2, userName2Pass, userFirsName2, userLastName2, userEmailName2);
            await proxy.CreateNewAccountAsync(userName3, userName3Pass, userFirsName3, userLastName3, userEmailName3);

            await proxy.CreateNewMembershipAsync(groupName1, userName1);
            await proxy.CreateNewMembershipAsync(groupName2, userName1);
            await proxy.CreateNewMembershipAsync(groupName2, userName2);
            await proxy.CreateNewMembershipAsync(groupName1, userName3);

            var groupsOfUser1 = proxy.GetGroupsOfUser(userName1);
            var groupsOfUser2 = proxy.GetGroupsOfUser(userName2);
            var groupsOfUser3 = proxy.GetGroupsOfUser(userName3);

            // Assert
            Assert.AreEqual(2, groupsOfUser1.Count());
            Assert.AreEqual(groupName1, groupsOfUser1.Where(g => g == groupName1).FirstOrDefault());
            Assert.AreEqual(groupName2, groupsOfUser1.Where(g => g == groupName2).FirstOrDefault());

            Assert.AreEqual(1, groupsOfUser2.Count());
            Assert.AreEqual(groupName2, groupsOfUser2.Where(g => g == groupName2).FirstOrDefault());

            Assert.AreEqual(1, groupsOfUser3.Count());
            Assert.AreEqual(groupName1, groupsOfUser3.Where(g => g == groupName1).FirstOrDefault());
        }

        [TestMethod]
        public async Task RemoveMembership()
        {
            // Assign
            TestServiceClient proxy = new TestServiceClient();
            string groupName = $"g{Guid.NewGuid().ToString()}g1".Replace("-", "");
            string groupDescription1 = "g1";
            string groupCreatedBy1 = "test feature";

            string userName1 = $"{Guid.NewGuid().ToString()} u1";
            string userFirsName1 = "u1";
            string userLastName1 = "u1";
            string userEmailName1 = "u1@mail.com";
            string userName1Pass = "1";


            string userName2 = $"{Guid.NewGuid().ToString()} u2";
            string userFirsName2 = "u2";
            string userLastName2 = "u2";
            string userEmailName2 = "u2@mail.com";
            string userName2Pass = "2";


            // Act
            await proxy.CreateNewGroupAsync(groupName, groupDescription1, groupCreatedBy1, true);

            await proxy.CreateNewAccountAsync(userName1, userName1Pass, userFirsName1, userLastName1, userEmailName1);
            await proxy.CreateNewAccountAsync(userName2, userName2Pass, userFirsName2, userLastName2, userEmailName2);

            await proxy.CreateNewMembershipAsync(groupName, userName1);
            await proxy.CreateNewMembershipAsync(groupName, userName2);

            var groupsOfUsersBeforRemove = proxy.GetMembershipUsers(groupName);

            proxy.RemoveMembership(groupName);
            var groupsOfUsersAfterRemove = proxy.GetMembershipUsers(groupName);

            // Assert
            Assert.AreEqual(0, groupsOfUsersAfterRemove.Count());
            Assert.AreEqual(2, groupsOfUsersBeforRemove.Count());

        }

        [TestMethod]
        public async Task RevokeMembership()
        {
            // Assign
            TestServiceClient proxy = new TestServiceClient();
            string groupName = $"g{Guid.NewGuid().ToString()}g1".Replace("-", "");
            string groupDescription1 = "g1";
            string groupCreatedBy1 = "test feature";

            string userName1 = $"{Guid.NewGuid().ToString()} u1";
            string userFirsName1 = "u1";
            string userLastName1 = "u1";
            string userEmailName1 = "u1@mail.com";
            string userName1Pass = "1";

            string userName2 = $"{Guid.NewGuid().ToString()} u2";
            string userFirsName2 = "u2";
            string userLastName2 = "u2";
            string userEmailName2 = "u2@mail.com";
            string userName2Pass = "2";

            // Act
            await proxy.CreateNewGroupAsync(groupName, groupDescription1, groupCreatedBy1, true);

            await proxy.CreateNewAccountAsync(userName1, userName1Pass, userFirsName1, userLastName1, userEmailName1);
            await proxy.CreateNewAccountAsync(userName2, userName2Pass, userFirsName2, userLastName2, userEmailName2);

            await proxy.CreateNewMembershipAsync(groupName, userName1);
            await proxy.CreateNewMembershipAsync(groupName, userName2);

            UserInfo[] usersBeforRemove = proxy.GetMembershipUsers(groupName);

            proxy.RevokeMembership(userName1, groupName);
            UserInfo[] usersAfterRemove = proxy.GetMembershipUsers(groupName);

            // Assert
            Assert.AreEqual(2, usersBeforRemove.Count());
            Assert.AreEqual(1, usersAfterRemove.Count());

            Assert.AreEqual(userName1, usersBeforRemove.Where(g => g.UserName == userName1).FirstOrDefault().UserName);
            Assert.AreEqual(userName2, usersBeforRemove.Where(g => g.UserName == userName2).FirstOrDefault().UserName);
            Assert.AreEqual(userName2, usersAfterRemove.Where(g => g.UserName == userName2).FirstOrDefault().UserName);
        }

        [TestMethod]
        public async Task ChangeUserAccountProfile()
        {
            // Assign
            TestServiceClient proxy = new TestServiceClient();
            string groupName = $"g{Guid.NewGuid().ToString()}g1".Replace("-", "");
            string groupDescription1 = "g1";
            string groupCreatedBy1 = "test feature";

            string userName = $"{Guid.NewGuid().ToString()} u1";
            string userFirsName = "u1";
            string userLastName = "u1";
            string userEmailName = "u1@mail.com";
            string userNamePass = "1";

            string newUserFirsName = "newU1";
            string newUserLastName = "newU1";
            string newUserEmailName = "newU1@mail.com";

            // Act
            await proxy.CreateNewGroupAsync(groupName, groupDescription1, groupCreatedBy1, true);

            await proxy.CreateNewAccountAsync(userName, userNamePass, userFirsName, userLastName, userEmailName);

            await proxy.CreateNewMembershipAsync(groupName, userName);

            await proxy.ChangeUserAccountProfileAsync(userName, newUserFirsName, newUserLastName, newUserEmailName);

            UserInfo user = proxy.GetUserAccounts().Where(u => u.UserName == userName).FirstOrDefault();
            // Assert
            Assert.AreEqual(newUserFirsName, user.FirstName);
            Assert.AreEqual(newUserLastName, user.LastName);
            Assert.AreEqual(newUserEmailName, user.Email);
        }

        [TestMethod]
        public async Task Authenticate()
        {
            // Assign
            TestServiceClient proxy = new TestServiceClient();
            string groupName = $"g{Guid.NewGuid().ToString()}g1".Replace("-", "");
            string groupDescription1 = "g1";
            string groupCreatedBy1 = "test feature";

            string userName = $"{Guid.NewGuid().ToString()} u1";
            string userFirsName = "u1";
            string userLastName = "u1";
            string userEmailName = "u1@mail.com";
            string userNamePass = "1";

            // Act
            await proxy.CreateNewGroupAsync(groupName, groupDescription1, groupCreatedBy1, true);
            await proxy.CreateNewAccountAsync(userName, userNamePass, userFirsName, userLastName, userEmailName);
            await proxy.CreateNewMembershipAsync(groupName, userName);

            string hashedPassword = GetHash(userNamePass);
            bool isChecked = proxy.Authenticate(userName, Base64Encode(hashedPassword));
            // Assert
            Assert.IsTrue(isChecked);
        }

        [TestMethod]
        public async Task ChangePassword()
        {
            // Assign
            TestServiceClient proxy = new TestServiceClient();
            string groupName = $"g{Guid.NewGuid().ToString()}g1".Replace("-", "");
            string groupDescription1 = "g1";
            string groupCreatedBy1 = "test feature";

            string userName = $"{Guid.NewGuid().ToString()} u1";
            string userFirsName = "u1";
            string userLastName = "u1";
            string userEmailName = "u1@mail.com";
            string userNamePass = "1";

            string newUserNamePass = "11";

            // Act
            await proxy.CreateNewGroupAsync(groupName, groupDescription1, groupCreatedBy1, true);
            await proxy.CreateNewAccountAsync(userName, userNamePass, userFirsName, userLastName, userEmailName);
            await proxy.CreateNewMembershipAsync(groupName, userName);

            await proxy.ChangePasswordAsync(userName, newUserNamePass);

            string hashedPassword = GetHash(newUserNamePass);
            bool isChecked = proxy.Authenticate(userName, Base64Encode(hashedPassword));
            // Assert
            Assert.IsTrue(isChecked);
        }

        [TestMethod]
        public async Task SetGroupsMembershipForUser()
        {
            // Assign
            TestServiceClient proxy = new TestServiceClient();
            string groupName1 = $"g{Guid.NewGuid().ToString()}g1".Replace("-", "");
            string groupDescription1 = "g1";
            string groupCreatedBy1 = "test feature";

            string groupName2 = $"g{Guid.NewGuid().ToString()}g2".Replace("-", "");
            string groupDescription2 = "g2";
            string groupCreatedBy2 = "test feature";

            string userName1 = $"{Guid.NewGuid().ToString()} u1";
            string userFirsName1 = "u1";
            string userLastName1 = "u1";
            string userEmailName1 = "u1@mail.com";
            string userName1Pass = "1";


            string userName2 = $"{Guid.NewGuid().ToString()} u2";
            string userFirsName2 = "u2";
            string userLastName2 = "u2";
            string userEmailName2 = "u2@mail.com";
            string userName2Pass = "2";

            // Act
            await proxy.CreateNewGroupAsync(groupName1, groupDescription1, groupCreatedBy1, true);
            await proxy.CreateNewGroupAsync(groupName2, groupDescription2, groupCreatedBy2, true);

            await proxy.CreateNewAccountAsync(userName1, userName1Pass, userFirsName1, userLastName1, userEmailName1);
            await proxy.CreateNewAccountAsync(userName2, userName2Pass, userFirsName2, userLastName2, userEmailName2);

            await proxy.SetGroupsMembershipForUserAsync(userName1, new[] { groupName1,groupName2});
            await proxy.SetGroupsMembershipForUserAsync(userName2, new[] { groupName2});

            var membershipUsersg1 = proxy.GetMembershipUsers(groupName1);
            var membershipUsersg2 = proxy.GetMembershipUsers(groupName2);

            // Assert
            Assert.AreEqual(userName1, membershipUsersg1.Where(g => g.UserName == userName1).FirstOrDefault().UserName.ToString());
            Assert.AreEqual(userName2, membershipUsersg2.Where(g => g.UserName == userName2).FirstOrDefault().UserName.ToString());
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        private static string GetHash(string inputString)
        {
            HashAlgorithm algorithm = MD5.Create();
            return Encoding.UTF8.GetString(algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString)));
        }

    }
}
