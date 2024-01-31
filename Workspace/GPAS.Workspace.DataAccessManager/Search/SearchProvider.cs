using GPAS.Dispatch.Entities.Concepts;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Entities.Search;
using GPAS.Workspace.ServiceAccess;
using GPAS.Workspace.ServiceAccess.RemoteService;
using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.DataAccessManager.Search
{
    public class SearchProvider
    {
        private SearchProvider() { }
        #region تعامل با سرویس‌دهنده راه‌دور
        private static IEnumerable<KWObject> QuickSearchAsync(string searchKeyword)
        {
            if (searchKeyword == null)
                throw new ArgumentNullException("searchKeyword");
            if (string.IsNullOrWhiteSpace(searchKeyword))
                throw new ArgumentException("Invalid argument", "searchKeyword");

            if (searchKeyword.Length < 1 || searchKeyword.Length > 1000)
                throw new ArgumentException("Invalid keyword length", "searchKeyword");

            KObject[] retrivedObjects = null;
            WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
            try
            {
                // ارسال درخواست دریافت ویژگی های یک شئ، به سرویس دهنده ی راه دور
                retrivedObjects = sc.QuickSearchAsync(searchKeyword).GetAwaiter().GetResult();
            }
            finally
            {
                sc.Close();
            }

            if (retrivedObjects == null)
                throw new NullReferenceException("Invalid server Response.");

            // تبدیل به اشیا قابل استفاده در سمت محیط کاربری
            return ObjectManager.GetObjectsFromRetrievedDataAsync(retrivedObjects).GetAwaiter().GetResult();
        }
        #endregion
        #region توابع بازیابی 
        public static IEnumerable<KWObject> GetQuickSearchResult(string searchKeyword, int quickSearchUnpublishedMaxCount)
        {
            if (searchKeyword == null)
                throw new ArgumentNullException("searchKeyword");
            List<KWObject> result = new List<KWObject>();

            var unPublishedQuickSearchResult = PerformQuickSearchOnLocalData(searchKeyword, quickSearchUnpublishedMaxCount);
            if (unPublishedQuickSearchResult.Any())
            {
                result.AddRange(unPublishedQuickSearchResult);
            }

            var publishedQuickSearchResult = QuickSearchAsync(searchKeyword);
            if (publishedQuickSearchResult.Any())
            {
                result.AddRange(publishedQuickSearchResult);
            }
            return result.Distinct();
        }

        //public static long GetTextDocTotalResults(Workspace.Entities.Search.SearchModel searchModel)
        //{
        //    long retrivedTotalCount = 0;
        //    WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
        //    try
        //    {
        //        var myModel = ConvertWorsSpaceSearchModelToDispatchServerSearchModel(searchModel);
        //        ارسال درخواست دریافت ویژگی های یک شئ، به سرویس دهنده ی راه دور
        //        retrivedTotalCount = sc.GetTotalTextDocResults(myModel);
        //    }
        //    finally
        //    {
        //        sc.Close();
        //    }

        //    return retrivedTotalCount;
        //}

        //public static IEnumerable<Entities.Search.SearchResultModel> Search(Workspace.Entities.Search.SearchModel searchModel)
        //{
        //    List<GPAS.Workspace.ServiceAccess.RemoteService.SearchResultModel> retrivedObjects = null;
        //    WorkspaceServiceClient sc = RemoteServiceClientFactory.GetNewClient();
        //    try
        //    {
        //        var myModel = ConvertWorsSpaceSearchModelToDispatchServerSearchModel(searchModel);
        //        ارسال درخواست دریافت ویژگی های یک شئ، به سرویس دهنده ی راه دور
        //        retrivedObjects = sc.Search(myModel).ToList();
        //    }
        //    finally
        //    {
        //        sc.Close();
        //    }

        //    if (retrivedObjects == null)
        //        throw new NullReferenceException("Invalid server Response.");

        //    تبدیل به اشیا قابل استفاده در سمت محیط کاربری
        //    return ConvertDispatchResultModelToWorkSpace(retrivedObjects);
        //}


        //private static GPAS.Workspace.ServiceAccess.RemoteService.SearchModel ConvertWorsSpaceSearchModelToDispatchServerSearchModel(Workspace.Entities.Search.SearchModel searchModel)
        //{
        //    GPAS.Workspace.ServiceAccess.RemoteService.SearchModel model = new GPAS.Workspace.ServiceAccess.RemoteService.SearchModel
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
        //        To = searchModel.To,
        //    };
        //    return model;
        //}

        //private static List<Workspace.Entities.Search.SearchResultModel> ConvertDispatchResultModelToWorkSpace(List<ServiceAccess.RemoteService.SearchResultModel> searchResultModels)
        //{
        //    List<Workspace.Entities.Search.SearchResultModel> models = new List<Entities.Search.SearchResultModel>();
        //    foreach (var item in searchResultModels)
        //    {
        //        Workspace.Entities.Search.SearchResultModel model = new Entities.Search.SearchResultModel()
        //        {
        //            CountOfFind = item.CountOfFind,
        //            FileName = item.FileName,
        //            FileSize = item.FileSize,
        //            Image = item.Image,
        //            PartOfText = item.PartOfText == null ? new List<string>() : item.PartOfText.ToList(),
        //            PublishDate = item.PublishDate,
        //            RelatedWord = item.RelatedWord.ToList(),
        //            Type = item.Type,
        //        };
        //        models.Add(model);
        //    }
        //    return models;
        //}

        private static IEnumerable<KWObject> PerformQuickSearchOnLocalData(string searchKeyword, int quickSearchUnpublishedMaxCount)
        {
            if (searchKeyword == null)
                throw new ArgumentNullException("searchKeyword");

            searchKeyword = searchKeyword.ToLowerInvariant();

            List<KWObject> result = new List<KWObject>();
            IEnumerable<KWObject> unPublishedObjects = ObjectManager.GetLocallyChangedObjects();
            IEnumerable<KWProperty> unPublishedProperties = PropertyManager.GetLocallyChangedProperties();
            foreach (var item in unPublishedObjects)
            {
                if (item.DisplayName != null)
                {
                    if (LikeOperator.LikeString(item.DisplayName.Value.ToLowerInvariant(), searchKeyword, Microsoft.VisualBasic.CompareMethod.Text)
                                && !result.Contains(item))
                    {
                        result.Add(item);
                    }
                }
            }
            foreach (var item in unPublishedProperties)
            {
                if (LikeOperator.LikeString(item.Value.ToLowerInvariant(), searchKeyword, Microsoft.VisualBasic.CompareMethod.Text)
                    && !result.Contains(item.Owner))
                {
                    result.Add(item.Owner);
                }
            }
            if (result.Count < quickSearchUnpublishedMaxCount)
            {
                return result;
            }
            else
            {
                return result.Take(quickSearchUnpublishedMaxCount);
            }

        }
        #endregion
    }
}
