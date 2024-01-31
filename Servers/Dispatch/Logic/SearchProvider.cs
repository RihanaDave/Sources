using GPAS.AccessControl;
using GPAS.Dispatch.Entities;
using GPAS.Dispatch.Entities.Concepts;
using GPAS.Dispatch.Entities.Search;
using GPAS.Dispatch.ServiceAccess.SearchService;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GPAS.Dispatch.Logic
{
    /// <summary>
    /// این کلاس جست و جوی سریع را مدیریت می کند.
    /// </summary>
    public class SearchProvider
    {
        private string CallerUserName = "";
        public SearchProvider(string callerUserName)
        {
            CallerUserName = callerUserName;
        }
        /// <summary>
        /// جست و جوی سریع را در پایگاه داده براساس کلیدواژه دریافتی انجام می کند.
        /// </summary>
        /// <param name="keyword">   کلیدواژه مورد نظر که براساس آن جست و جو صورت می گیرد.   </param>
        /// <returns>    لیستی از اشیا که در نتایج جست و جو آمده است را برمی گرداند.    </returns>
        public List<KObject> QuickSearch(string keyword)
        {
            AuthorizationParametters authorizationParametter
                = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

            ServiceClient proxy = null;
            try
            {
                List<KObject> resultKObject = new List<KObject>();
                List<SearchObject> resultSearchObject = new List<SearchObject>();

                proxy = new ServiceClient();
                EntityConvertor entityConvertor = new EntityConvertor();
                resultSearchObject = proxy.QuickSearch(keyword, authorizationParametter).ToList();

                foreach (var item in resultSearchObject)
                {
                    KObject kObject = entityConvertor.ConvertSearchObjectToKObject(item, CallerUserName);
                    resultKObject.Add(kObject);
                }
                return resultKObject;
            }
            finally
            {
                if (proxy != null)
                    proxy.Close();
            }
        }

        //public List<Entities.Search.SearchResultModel> Search(Dispatch.Entities.Search.SearchModel searchModel)
        //{
        //    AuthorizationParametters authorizationParametter
        //        = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);


        //    ServiceClient proxy = null;
        //    try
        //    {
        //        proxy = new ServiceClient();
        //        EntityConvertor entityConvertor = new EntityConvertor();
        //        var model = ConvertDispatchSearchModelToSearchServerSearchModel(searchModel);
        //        var resultSearchObject = proxy.Search(model, authorizationParametter).ToList();
        //        return ConvertSearchServerResultModelToDispatch(resultSearchObject);
        //    }
        //    finally
        //    {
        //        if (proxy != null)
        //            proxy.Close();
        //    }
        //}

        //private static GPAS.Dispatch.ServiceAccess.SearchService.SearchModel ConvertDispatchSearchModelToSearchServerSearchModel(Dispatch.Entities.Search.SearchModel searchModel)
        //{
        //    GPAS.Dispatch.ServiceAccess.SearchService.SearchModel model = new GPAS.Dispatch.ServiceAccess.SearchService.SearchModel
        //    {
        //        AnyWord = searchModel.AnyWord,
        //        CreationDateOF = searchModel.CreationDateOF,
        //        CreationDateUntil = searchModel.CreationDateUntil,
        //        ExactKeyWord = searchModel.ExactKeyWord,
        //        FileSizeOF = searchModel.FileSizeOF,
        //        FileSizeUntil = searchModel.FileSizeUntil,
        //        FileType = searchModel.FileType,
        //        ImportDateOf = searchModel.ImportDateOf,
        //        ImportDateUntil = searchModel.ImportDateUntil,
        //        KeyWordSearch = searchModel.KeyWordSearch,
        //        Language = searchModel.Language,
        //        NoneWord = searchModel.NoneWord,
        //        SearchIn = searchModel.SearchIn,
        //        SortOrder = searchModel.SortOrder,
        //        SortOrderType = searchModel.SortOrderType,
        //        TypeSearch = searchModel.TypeSearch,
        //        From = searchModel.From,
        //        To = searchModel.To

        //    };
        //    return model;
        //}

        //private static List<Dispatch.Entities.Search.SearchResultModel> ConvertSearchServerResultModelToDispatch(List<GPAS.Dispatch.ServiceAccess.SearchService.SearchResultModel> searchResultModels)
        //{
        //    List<Entities.Search.SearchResultModel> models = new List<Entities.Search.SearchResultModel>();
        //    foreach (var item in searchResultModels)
        //    {
        //        Entities.Search.SearchResultModel model = new Entities.Search.SearchResultModel()
        //        {
        //            CountOfFind = item.CountOfFind,
        //            FileName = item.FileName,
        //            FileSize = item.FileSize,
        //            Image = item.Image,
        //            PartOfText = item.PartOfText.ToList(),
        //            PublishDate = item.PublishDate,
        //            //RelatedWord = item.RelatedWord.ToList(),
        //            Type = item.Type,
        //        };
        //        models.Add(model);
        //    }
        //    return models;
        //}
        //public long GetTotalTextDocResults(Dispatch.Entities.Search.SearchModel searchModel)
        //{

        //    AuthorizationParametters authorizationParametter
        //        = UserAccountControlProvider.GetUserAuthorizationParametters(CallerUserName);

        //    ServiceClient proxy = null;
        //    try
        //    {
        //        //List<SearchResultModel> resultSearchObject = new List<SearchResultModel>();
        //        proxy = new ServiceClient();
        //        var model = ConvertDispatchSearchModelToSearchServerSearchModel(searchModel);
        //        return proxy.GetTotalTextDocResults(model, authorizationParametter);
        //    }
        //    finally
        //    {
        //        if (proxy != null)
        //            proxy.Close();
        //    }
        //}
    }
}
