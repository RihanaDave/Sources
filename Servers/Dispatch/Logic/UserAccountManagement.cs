using GPAS.AccessControl.Users;
using GPAS.Dispatch.DataAccess;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace GPAS.Dispatch.Logic
{
    public class UserAccountManagement
    {
        private static string GetHash(string inputString)
        {
            HashAlgorithm algorithm = MD5.Create();
            return Encoding.UTF8.GetString(algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString)));
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public void Init()
        {
            UserAccountControlDatabaseAccess databaseAccess = new UserAccountControlDatabaseAccess();
            databaseAccess.CreateDataBase();
            databaseAccess.CreateTable();
            databaseAccess.CreateIndex();
            bool checkAdminUser = databaseAccess.CheckAdminUserAccount();
            if (!checkAdminUser)
            {
                databaseAccess.InsertAdminUser();
            }
        }

        public bool Authenticate(string userName, string password)
        {
            return (new UserAccountControlDatabaseAccess()).CheckUserAccount(userName, password);
        }

        public void CreateNewAccount(string userName, string password, string firstName, string lastName, string email)
        {
            if (userName == null)
                throw new ArgumentNullException("userName");
            if (password == null)
                throw new ArgumentNullException("password");
            if (firstName == null)
                throw new ArgumentNullException("firstName");
            if (lastName == null)
                throw new ArgumentNullException("lastName");
            if (email == null)
                throw new ArgumentNullException("email");

            UserAccountControlDatabaseAccess databaseAccess = new UserAccountControlDatabaseAccess();

            if (!databaseAccess.CheckUserAccountExists(userName))
            {
                string hashedPassword = Base64Encode(GetHash(password));
                databaseAccess.CreateNewAccount(userName, hashedPassword, firstName, lastName, email);
            }
        }

        public void DeleteAccount(int id)
        {
            if (id == 0)
                throw new ArgumentException(nameof(id));

            UserAccountControlDatabaseAccess databaseAccess = new UserAccountControlDatabaseAccess();
            databaseAccess.DeleteAccount(id);
        }

        public bool CheckUserAccountExists(string userName)
        {
            if (userName == null)
                throw new ArgumentNullException("userName");

            UserAccountControlDatabaseAccess databaseAccess = new UserAccountControlDatabaseAccess();
            if (databaseAccess.CheckUserAccountExists(userName))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public void ChangeUserAccountProfile(
            string userName,
            string newFirstName, string newLastName, string newEmail
            )
        {
            if (userName == null)
                throw new ArgumentNullException("userName");
            if (newFirstName == null)
                throw new ArgumentNullException("newFirstName");
            if (newLastName == null)
                throw new ArgumentNullException("newLastName");
            if (newEmail == null)
                throw new ArgumentNullException("newEmail");

            UserAccountControlDatabaseAccess databaseAccess = new UserAccountControlDatabaseAccess();
            databaseAccess.ChangeUserAccountProfile(userName, newFirstName, newLastName, newEmail);
        }


        public void ChangePassword(string userName, string newPassword)
        {
            if (userName == null)
                throw new ArgumentNullException("userName");
            if (newPassword == null)
                throw new ArgumentNullException("newPassword");

            string hashedPassword = Base64Encode(GetHash(newPassword));

            UserAccountControlDatabaseAccess databaseAccess = new UserAccountControlDatabaseAccess();
            databaseAccess.ChangePassword(userName, hashedPassword);
        }

        public List<UserInfo> GetUserAccounts()
        {
            UserAccountControlDatabaseAccess databaseAccess = new UserAccountControlDatabaseAccess();
            return databaseAccess.GetUserAccounts(); ;
        }
    }
}
