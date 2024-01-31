using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPAS.SearchServer.Logic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPAS.SearchServer.Entities.SearchEngine.Documents;
using System.Globalization;
using GPAS.AccessControl;

namespace GPAS.SearchServer.Logic.Tests
{
    [TestClass()]
    public class SearchEngineDocumentConvertorTests
    {
        [TestMethod()]
        public void ConvertDataSourceInfoToDataSourceTest()
        {
            //Arrangment
            List<DataSourceDocument> dataSources = new List<DataSourceDocument>();
            DataSourceDocument dataSource1 = new DataSourceDocument()
            {
                Id = "99999",
                CreatedBy = "TestMethod",
                CreatedTime = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                Name = "Test1",
                Description = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                Type = 1,
                Acl = new Entities.SearchEngine.Documents.ACL()
                {
                    ClassificationIdentifier = "N",
                    Permissions = new List<GroupPermission>() { new GroupPermission() { AccessLevel = AccessControl.Permission.Write, GroupName = "Administrators" } }
                }
            };
            DataSourceDocument dataSource2 = new DataSourceDocument()
            {
                Id = "99998",
                CreatedBy = "TestMethod",
                CreatedTime = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                Name = "Test2",
                Description = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                Type = 1,
                Acl = new Entities.SearchEngine.Documents.ACL()
                {
                    ClassificationIdentifier = "N",
                    Permissions = new List<GroupPermission>() { new GroupPermission() { AccessLevel = AccessControl.Permission.Write, GroupName = "Administrators" } }
                }
            };
            dataSources.Add(dataSource1);
            dataSources.Add(dataSource2);
            //Act
            SearchEngineDocumentConvertor searchEngineConvertor = new SearchEngineDocumentConvertor();
            List<DataSourceInfo> dataSourcesInfo = searchEngineConvertor.ConvertDataSourceInfoToDataSource(dataSources);
            //Assert
            for (int i = 0; i < dataSourcesInfo.Count; i++)
            {
                var dataSourceInfo = dataSourcesInfo[i];
                var dataSource = dataSources[i];
                Assert.AreEqual(dataSourceInfo.Id.ToString(), dataSource.Id);
                Assert.AreEqual(dataSourceInfo.Name, dataSource.Name);
                Assert.AreEqual(dataSourceInfo.Description, dataSource.Description);
                Assert.AreEqual(dataSourceInfo.Acl.Classification, dataSource.Acl.ClassificationIdentifier);
                Assert.AreEqual(dataSourceInfo.Acl.Permissions.FirstOrDefault().AccessLevel, dataSource.Acl.Permissions.FirstOrDefault().AccessLevel);

            }

        }

        [TestMethod()]
        public void ConvertDataSourceInfoToDataSourceTest1()
        {
            //Arrangment
            DataSourceInfo dataSourceInfo = new DataSourceInfo()
            {
                Id = 99999,
                CreatedBy = "TestMethod",
                CreatedTime = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                Name = "Test1",
                Description = DateTime.Now.ToString(CultureInfo.InvariantCulture),
                Type = 1,
                Acl = new AccessControl.ACL()
                {
                    Classification = "N",
                    Permissions = new List<ACI>() { new ACI() { AccessLevel = AccessControl.Permission.Write, GroupName = "Administrators" } }
                }
            };
            //Act
            SearchEngineDocumentConvertor searchEngineConvertor = new SearchEngineDocumentConvertor();
            DataSourceDocument dataSource = searchEngineConvertor.ConvertDataSourceInfoToDataSource(dataSourceInfo);
            //Assert
            Assert.AreEqual(dataSourceInfo.Id.ToString(), dataSource.Id);
            Assert.AreEqual(dataSourceInfo.Name, dataSource.Name);
            Assert.AreEqual(dataSourceInfo.Description, dataSource.Description);
            Assert.AreEqual(dataSourceInfo.Acl.Classification, dataSource.Acl.ClassificationIdentifier);
            Assert.AreEqual(dataSourceInfo.Acl.Permissions.FirstOrDefault().AccessLevel, dataSource.Acl.Permissions.FirstOrDefault().AccessLevel);


        }
    }
}