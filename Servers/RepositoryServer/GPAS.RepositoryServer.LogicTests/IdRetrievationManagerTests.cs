using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPAS.RepositoryServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.RepositoryServer.Logic.Tests
{
    [TestClass()]
    public class IdRetrievationManagerTests
    {
        [TestMethod()]
        public void CanRetrieveSomthingFromRepository()
        {
            // Act
            IdRetrievationManager idRetrievationManager = new IdRetrievationManager();
            long retrievedID = idRetrievationManager.GetLastAsignedObjectId();
            // Assert
            Assert.IsTrue(retrievedID > -1);
        }
    }
}