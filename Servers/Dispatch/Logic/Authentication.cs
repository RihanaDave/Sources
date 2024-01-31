using GPAS.AccessControl;
using GPAS.Dispatch.DataAccess;
using System;
using System.Security.Cryptography;
using System.Text;

namespace GPAS.Dispatch.Logic
{
    public class Authentication
    {
        private static string GetHash(string inputString)
        {
            HashAlgorithm algorithm = MD5.Create();
            return Encoding.UTF8.GetString(algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString)));
        }

        private static string Base64Encode(string plainText)
        {
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            return Convert.ToBase64String(plainTextBytes);
        }

        public bool Authenticate(string userName, string password)
        {
            string hashedPassword = GetHash(password);
            hashedPassword = Base64Encode(hashedPassword);
            UserAccountControlDatabaseAccess databaseAccess = new UserAccountControlDatabaseAccess();
            return databaseAccess.CheckUserAccount(userName, hashedPassword);            
        }
    }
}
