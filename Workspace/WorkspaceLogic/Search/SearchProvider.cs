using GPAS.Workspace.Entities;
using GPAS.Workspace.ServiceAccess.RemoteService;
using GPAS.FilterSearch;
using System;
using System.Windows;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using GPAS.Workspace.Entities.Search;

namespace GPAS.Workspace.Logic.Search
{
    /// <summary>
    /// مدیریت جستجوها در سمت محیط کاربری
    /// </summary>
    public class SearchProvider
    {
        /// <summary>
        /// سازنده کلاس
        /// </summary>
        /// <remarks>این سازنده برای جلوگیری از ایجاد نمونه از این کلاس، محلی شده است</remarks>
        private SearchProvider()
        { }
        /// <summary>
        /// انجام «جستجوی سریع»؛
        /// جستجوی سریع شامل اشیائی می شود که نام نمایشی یا یکی از ویژگی های آن دقیقا برابر کلیدواژه و یا مشابه آن باشد.
        /// این جستجو محدودیت تعدادی بازگشتی دارد و لزوما همه موارد دارای شرایط فوق را برنمی گرداند
        /// </summary>
        /// <param name="searchKeyword">عبارتی که می بایست جستجو براساس آن انجام گیرد</param>
        public static IEnumerable<KWObject> QuickSearchAsync(string searchKeyword , int quickSearchUnpublishedMaxCount)
        {
            IEnumerable<KWObject> result = DataAccessManager.Search.SearchProvider.GetQuickSearchResult(searchKeyword , quickSearchUnpublishedMaxCount).ToList();
            return result;
        }

        //public static IEnumerable<Workspace.Entities.Search.SearchResultModel> Search(Workspace.Entities.Search.SearchModel searchModel)
        //{
        //    IEnumerable<Workspace.Entities.Search.SearchResultModel> result = DataAccessManager.Search.SearchProvider.Search(searchModel);
        //    return result;
        //}

        //public static long GetTextDocTotalResults(Workspace.Entities.Search.SearchModel searchModel)
        //{
        //    return DataAccessManager.Search.SearchProvider.GetTextDocTotalResults(searchModel);
        //}
    }
}
