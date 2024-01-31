using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GPAS.Dispatch.LogicTests
{
    /// <summary>
    /// Summary description for FtpUtilityTest
    /// </summary>
    [TestClass]
    public class FtpUtilityTest
    {
        public FtpUtilityTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        //[TestMethod]
        //public void TestMethod1()
        //{
        //    //
        //    // TODO: Add test logic here
        //    //
        //}

        //[TestMethod]
        //public void FtpUpload()
        //{
        //    FileStream stream = new FileStream(@"D:\\ftpTest.txt", FileMode.Open);
        //    FtpUtility ftp = new FtpUtility("10.1.2.43","arya","17571757");
        //    ftp.Upload(stream,stream.Length,"tst.txt");
        //}
        //[TestMethod]
        //public void FtpDownload()
        //{
        //    FtpUtility ftp = new FtpUtility("10.1.2.43", "arya", "17571757");
        //    ftp.Download("ftp://10.1.2.185/", "ftpTest.txt");
        //}
        //[TestMethod]
        //public void FtpCopy()
        //{
        //    FtpUtility.Copy("ftp://10.1.2.43/tst.txt", "ftp://10.1.2.185/tst.txt");
        //}
        //[TestMethod]
        //public void Delete()
        //{
        //    string address="ftp://10.1.2.43/tst.txt";
        // //   FtpUtility.Delete(address, "arya", "17571757");
        //}
        //[TestMethod]
        //public void Import()
        //{
        //    FileStream stream = new FileStream(@"D:\\2.jpg", FileMode.Open);
        //    FileImport im = new FileImport();
        //    im.UploadFileForImport(stream, stream.Length, "2.jpg");


        //}

        //[TestMethod]
        //public void TestMethod1()
        //{
        //    GPAS.Dispatch.FtpUtility.FtpUtility ftp =
        //        new GPAS.Dispatch.FtpUtility.FtpUtility("10.1.2.43", "arya", "17571757");
        //    ftp.MakeDirectory(new Guid().ToString());
        //}
    }
}
