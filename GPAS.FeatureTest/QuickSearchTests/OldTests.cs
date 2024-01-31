using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Logic.Search;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.FeatureTest.QuickSearchTests
{
    [TestClass]
    public class OldTests
    {
        private bool isInitialized = false;

        [TestInitialize]
        public async Task Init()
        {
            if (!isInitialized)
            {
                var authentication = new UserAccountControlProvider();
                bool result = await authentication.AuthenticateAsync("admin", "admin");
                await Workspace.Logic.System.InitializationAsync();
                isInitialized = true;
            }
        }

        int defaultQuickSearchUnpublishedCount = 100;
        //[TestMethod]
        //public async void QuickSearchAsyncTest()
        //{
        //    // Arrange
        //    Dictionary<string, IEnumerable<KWObject>> keywordResultsDictionary = new Dictionary<string, IEnumerable<KWObject>>();
        //    keywordResultsDictionary.Add("", null);
        //    // Act
        //    foreach (string item in keywordResultsDictionary.Keys)
        //        keywordResultsDictionary[item] = (await Logic.SearchProvider.QuickSearchAsync(item));
        //    // Assert
        //    Assert.IsTrue(keywordResultsDictionary[""])
        //}
        
        [TestMethod]
        public async Task QuickSearchAsyncTest()
        {
            //===============| Arrange |==============
            bool isNullKeywordCauseException = false;

            string emptyKeyword = string.Empty;
            bool isEmptyKeywordCauseException = false;

            string whitespaceKeyword = ' '.ToString();
            bool isWhitespaceKeywordCauseException = false;

            string minimumLengthKeyword = '$'.ToString();
            IEnumerable<KWObject> minimumLengthKeywordResult = null;

            string maximumLengthKeyword = string.Empty;
            while (maximumLengthKeyword.Length < 1000)
                maximumLengthKeyword += 'آ';
            IEnumerable<KWObject> maximumLengthKeywordResult = null;

            bool isKeywordBiggerThanMaximumLengthCauseException = false;

            string[] otherKeywords = { "y", "x", "!@#$%^&*()<>><.", "abc cba  abc" };

            //===============| Act |==============
            // آماده‌سازی اولیه لایه منطق برای شروع تست
            await Workspace.Logic.System.InitializationAsync();
            // تست جستجو با ورودی نال
            try
            {
                SearchProvider.QuickSearchAsync(null, defaultQuickSearchUnpublishedCount);
            }
            catch (ArgumentNullException)
            {
                isNullKeywordCauseException = true;
            }
            // تست جستجوی رشته خالی
            try
            {
                SearchProvider.QuickSearchAsync(emptyKeyword, defaultQuickSearchUnpublishedCount);
            }
            catch (ArgumentException)
            {
                isEmptyKeywordCauseException = true;
            }
            // تست جستجوی رشته حاوی یک یا چند فاصله سفید
            try
            {
                SearchProvider.QuickSearchAsync(whitespaceKeyword, defaultQuickSearchUnpublishedCount);
                SearchProvider.QuickSearchAsync(whitespaceKeyword + whitespaceKeyword, defaultQuickSearchUnpublishedCount);
                SearchProvider.QuickSearchAsync(whitespaceKeyword + whitespaceKeyword + whitespaceKeyword, defaultQuickSearchUnpublishedCount);
            }
            catch (ArgumentException)
            {
                isWhitespaceKeywordCauseException = true;
            }
            // تست جستجوی رشته با حداقل طول قابل قبول
            minimumLengthKeywordResult = SearchProvider.QuickSearchAsync(minimumLengthKeyword, defaultQuickSearchUnpublishedCount);
            // تست جستجوی رشته با حداکثر طول قابل قبول
            maximumLengthKeywordResult = SearchProvider.QuickSearchAsync(maximumLengthKeyword, defaultQuickSearchUnpublishedCount);
            // تست جستجوی رشته با طول بیشتر از حداکثر
            try
            {
                SearchProvider.QuickSearchAsync(maximumLengthKeyword + "۴", defaultQuickSearchUnpublishedCount);
            }
            catch (ArgumentException)
            {
                isKeywordBiggerThanMaximumLengthCauseException = true;
            }
            // انجام جستجو روی دیگر رشته‌ها
            foreach (var item in otherKeywords)
                SearchProvider.QuickSearchAsync(item, defaultQuickSearchUnpublishedCount);
            // خاتمه کار با لایه منطق برنامه جهت تست
            Workspace.Logic.System.Finalization();

            //===============| Assert |==============
            Assert.IsTrue(isNullKeywordCauseException);
            Assert.IsTrue(isEmptyKeywordCauseException);
            Assert.IsTrue(isWhitespaceKeywordCauseException);
            Assert.IsNotNull(minimumLengthKeywordResult);
            Assert.IsNotNull(maximumLengthKeywordResult);
            Assert.IsTrue(isKeywordBiggerThanMaximumLengthCauseException);
        }
    }
}
