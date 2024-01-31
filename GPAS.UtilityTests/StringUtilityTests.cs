using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPAS.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Utility.Tests
{
    [TestClass()]
    public class StringUtilityTests
    {
        [TestMethod()]
        public void SeperateIDsByComma_NoInputID_ReturnsExpectedString()
        {
            StringUtility strUtil = new StringUtility();
            string testResult = strUtil.SeperateIDsByComma(new long[] { });
            Assert.AreEqual("", testResult);
        }

        [TestMethod()]
        public void SeperateIDsByComma_SingleInputID_ReturnsExpectedString()
        {
            StringUtility strUtil = new StringUtility();
            string testResult = strUtil.SeperateIDsByComma(new long[] { 1 });
            Assert.AreEqual("1", testResult);
        }

        [TestMethod()]
        public void SeperateIDsByComma_ThreeInputIDs_ReturnsExpectedString()
        {
            StringUtility strUtil = new StringUtility();
            string testResult = strUtil.SeperateIDsByComma(new long[] { 1, 2, 3 });
            Assert.AreEqual("1,2,3", testResult);
        }

        [TestMethod()]
        public void SeperateByComma_NoInputString_ReturnsExpectedString()
        {
            StringUtility strUtil = new StringUtility();
            string testResult = strUtil.SeperateByComma(new string[] { });
            Assert.AreEqual("", testResult);
        }

        [TestMethod()]
        public void SeperateByComma_SingleInputString_ReturnsExpectedString()
        {
            StringUtility strUtil = new StringUtility();
            string testResult = strUtil.SeperateByComma(new string[] { "1" });
            Assert.AreEqual("1", testResult);
        }

        [TestMethod()]
        public void SeperateByComma_ThreeInputStrings_ReturnsExpectedString()
        {
            StringUtility strUtil = new StringUtility();
            string testResult = strUtil.SeperateByComma(new string[] { "1", "2", "3" });
            Assert.AreEqual("1,2,3", testResult);
        }
        
        [TestMethod()]
        public void SeperateByInputSeperator_NoInputString_ReturnsExpectedString()
        {
            StringUtility strUtil = new StringUtility();
            string testResult = strUtil.SeperateByInputSeperator(new string[] { }, ",");
            Assert.AreEqual("", testResult);
        }

        [TestMethod()]
        public void SeperateByInputSeperator_SingleInputString_ReturnsExpectedString()
        {
            StringUtility strUtil = new StringUtility();
            string testResult = strUtil.SeperateByInputSeperator(new string[] { "1" }, ",");
            Assert.AreEqual("1", testResult);
        }

        [TestMethod()]
        public void SeperateByInputSeperator_SingleInputStringAndTwoCharSeperator_ReturnsExpectedString()
        {
            StringUtility strUtil = new StringUtility();
            string testResult = strUtil.SeperateByInputSeperator(new string[] { "1" }, ",");
            Assert.AreEqual("1", testResult);
        }

        [TestMethod()]
        public void SeperateByInputSeperator_ThreeInputStrings_ReturnsExpectedString()
        {
            StringUtility strUtil = new StringUtility();
            string testResult = strUtil.SeperateByInputSeperator(new string[] { "1", "2", "3" }, ",");
            Assert.AreEqual("1,2,3", testResult);
        }

        [TestMethod()]
        public void SeperateByInputSeperator_ThreeInputStringsAndTwoCharSeperator_ReturnsExpectedString()
        {
            StringUtility strUtil = new StringUtility();
            string testResult = strUtil.SeperateByInputSeperator(new string[] { "1", "2", "3" }, "$$");
            Assert.AreEqual("1$$2$$3", testResult);
        }
    }
}