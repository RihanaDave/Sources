using GPAS.TextualSearch;
using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using GPAS.Workspace.Logic.Search;
using GPAS.Workspace.Presentation.Controls.Sesrch.Enum;
using GPAS.Workspace.Presentation.Controls.Sesrch.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.Controls.Sesrch.ViewModel
{
    public class SearchViewModel : BaseViewModel
    {
        SearchModel modelSearch = new SearchModel();
        public SearchModel ModelSearch
        {
            get => modelSearch;
            set
            {
                SetValue(ref modelSearch, value);
            }
        }

        private int itemPerPage = 10;
        public int ItemPerPage
        {
            get => itemPerPage;
            set
            {
                SetValue(ref itemPerPage, value);
            }
        }

        public SearchResultModel currentResultModel = new SearchResultModel();
        public SearchResultModel CurrentResultModel
        {
            get => currentResultModel;
            set
            {
                SetValue(ref currentResultModel, value);
            }
        }

        private string countOfResult;
        public string CountOfResult
        {
            get => countOfResult;
            set
            {
                SetValue(ref countOfResult, value);
            }
        }

        private bool showSearchORProgressButton;
        public bool ShowSearchORProgressButton
        {
            get => showSearchORProgressButton;
            set
            {
                SetValue(ref showSearchORProgressButton, value);
            }
        }

        private int currentselectedIndexRow = -1;
        public int CurrentselectedIndexRow
        {
            get => currentselectedIndexRow;
            set
            {
                SetValue(ref currentselectedIndexRow, value);
            }
        }

        ObservableCollection<SearchResultModel> searchResultPaginationModels = new ObservableCollection<SearchResultModel>();
        public ObservableCollection<SearchResultModel> SearchResultPaginationModels
        {
            get => searchResultPaginationModels;
            set
            {
                SetValue(ref searchResultPaginationModels, value);
            }
        }

        ObservableCollection<string> documentTypCollection = new ObservableCollection<string>();
        public ObservableCollection<string> DocumentTypCollection
        {
            get => documentTypCollection;
            set
            {
                SetValue(ref documentTypCollection, value);
            }
        }

        ObservableCollection<SearchResultModel> searchResultModels = new ObservableCollection<SearchResultModel>();
        public ObservableCollection<SearchResultModel> SearchResultModels
        {
            get => searchResultModels;
            set
            {
                SetValue(ref searchResultModels, value);
            }
        }

        private ControlsEnum controlsEnum = ControlsEnum.Search;
        public ControlsEnum ControlsEnum
        {
            get => controlsEnum;
            set
            {
                SetValue(ref controlsEnum, value);
            }
        }

        private SearchState searchState = SearchState.All;
        public SearchState SearchState
        {
            get => searchState;
            set
            {
                SetValue(ref searchState, value);
            }
        }

        private bool isFound = false;
        public bool IsFound
        {
            get => isFound;
            set
            {
                SetValue(ref isFound, value);
            }
        }

        private long totalNumberCollection = 1;
        public long TotalNumberCollection
        {
            get => totalNumberCollection;
            set
            {
                SetValue(ref totalNumberCollection, value);
            }
        }

        public SearchViewModel()
        {
            FillDocumentTypCollection();
        }

        public void GetFromTo(long from, long to)
        {
            Search(from, to);
        }

        private void FillDocumentTypCollection()
        {
            string[] supportedTextFileExtensions = OntologyProvider.GetOntology().GetTextDocumentFileTypes();

            DocumentTypCollection.Add("All");

            foreach (string item in supportedTextFileExtensions)
            {
                DocumentTypCollection.Add(item);
            }
        }

        public async void Search(long from, long to)
        {
            try
            {
                List<Entities.Search.SearchResultModel> retrivedResults = new List<Entities.Search.SearchResultModel>();

                Entities.Search.SearchModel search = new Entities.Search.SearchModel
                {
                    TypeSearch = modelSearch.TypeSearch,
                    ExactKeyWord = modelSearch.ExactKeyWord,
                    KeyWordSearch = modelSearch.KeyWordSearch,
                    AnyWord = modelSearch.AnyWord,
                    NoneWord = modelSearch.NoneWord,
                    ImportDateOf = modelSearch.ImportDateOf,
                    ImportDateUntil = modelSearch.ImportDateUntil,
                    SortOrder = (int)modelSearch.SortOrder,
                    SortOrderType = modelSearch.SortOrderType,
                    Language = modelSearch.Language,
                    FileType = modelSearch.FileType,
                    SearchIn = modelSearch.SearchIn,
                    Topic = modelSearch.Topic,
                    CreationDateOF = modelSearch.CreationDateOF,
                    CreationDateUntil = modelSearch.CreationDateUntil,
                    FileSizeOF = modelSearch.FileSizeOF,
                    FileSizeUntil = modelSearch.FileSizeUntil,
                    From = from,
                    To = to
                };

                if (search.FileType.Equals("All"))
                {
                    search.FileType = string.Empty;
                }

                TextualSearchProvider textualSearchProvider = new TextualSearchProvider();

                TextualSearchQuery query = new TextualSearchQuery();
                query.StartIndex = search.From;
                query.ToIndex = search.To;
                query.Snippets = 5;
                query.Fragsize = 400;

                if (search.KeyWordSearch == "" && search.ExactKeyWord != "")
                {
                    query.HighlightMode = HighlightMode.ExactMatch;
                    query.QueryParam = search.ExactKeyWord;
                }
                else
                {
                    query.HighlightMode = HighlightMode.AllTheWords;
                    query.QueryParam = search.KeyWordSearch;
                }

                List<BaseSearchResult> result = new List<BaseSearchResult>();

                await Task.Run(() =>
                    {
                        if (searchState == SearchState.TextDoc)
                        {
                            query.SearchTarget = SearchTargetSet.Text;
                            query.Criterias = new List<BaseSearchCriteria>()
                            {
                                new StringBasedCriteria(){ CriteriaName = "Language", CriteriaValue= search.Language },
                                new StringBasedCriteria (){ CriteriaName ="FileType", CriteriaValue= search.FileType },
                                new DateRangeBasedCriteria(){ CriteriaName="CreatedTime", CriteriaStartValue = search.CreationDateOF, CriteriaEndValue = search.CreationDateUntil },
                                new DateRangeBasedCriteria(){ CriteriaName="ImportDate", CriteriaStartValue =search.ImportDateOf, CriteriaEndValue = search.ImportDateUntil },
                                new DoubleRangeBasedCriteria(){ CriteriaName="FileSize", CriteriaStartValue =  search.FileSizeOF, CriteriaEndValue = search.FileSizeUntil}
                            };

                            result = textualSearchProvider.PerformTextualSearchAsync(query);
                        }
                        else
                        {
                            query.SearchTarget = SearchTargetSet.All;
                            result = textualSearchProvider.PerformTextualSearchAsync(query);
                        }
                    });


                if (searchState == SearchState.TextDoc)
                {
                    SearchResultModels = ConvertSearchResultModWorkSpaceToSearchResultModel2(result);
                    TotalNumberCollection = result.Count == 0 ? 0 : result[0].TotalRow;
                    if (SearchResultModels.Count > 0)
                    {
                        IsFound = true;
                    }
                    else
                    {
                        IsFound = false;
                    }
                }

                if (searchState == SearchState.All)
                {
                    SearchResultModels = ConvertKWObjectToSearchResultModel2(result);
                    TotalNumberCollection = result.Count == 0 ? 0 : result[0].TotalRow;
                    if (SearchResultModels.Count > 0)
                    {
                        IsFound = true;
                    }
                    else
                    {
                        IsFound = false;
                    }
                }

                ControlsEnum = ControlsEnum.Result;
                ShowSearchORProgressButton = false;
            }
            catch (Exception e)
            {
                ShowSearchORProgressButton = false;
                ControlsEnum = ControlsEnum.Search;
                throw new Exception(e.Message);
            }
        }

        public async Task<KWObject> GetObjectById(long objectId)
        {
            return await ObjectManager.GetObjectById(objectId);
        }

        private ObservableCollection<SearchResultModel> ConvertKWObjectToSearchResultModel2(List<BaseSearchResult> workspaceWntity1)
        {
            ObservableCollection<SearchResultModel> customSearchResultModels = new ObservableCollection<SearchResultModel>();

            foreach (var item in workspaceWntity1)
            {
                var objectBasedSearchResult = (ObjectBasedSearchResult)item;

                SearchResultModel searchResultModel = new SearchResultModel();
                searchResultModel.Type = objectBasedSearchResult.TypeURI;
                searchResultModel.Id = objectBasedSearchResult.ObjectId;
                searchResultModel.Image = new BitmapImage(OntologyIconProvider.GetTypeIconPath(objectBasedSearchResult.TypeURI));
                searchResultModel.KeyWordSearched = ModelSearch.KeyWordSearch;
                searchResultModel.ExactKeyWord = ModelSearch.ExactKeyWord;
                searchResultModel.From = ModelSearch.From;
                searchResultModel.To = modelSearch.To;

                if (objectBasedSearchResult.TextResult.PartOfText.Count > 0)
                {
                    searchResultModel.PartOfText.Add(RemoveSpaceChar(objectBasedSearchResult.TextResult.PartOfText[0]));

                    for (int i = 0; i < objectBasedSearchResult.TextResult.PartOfText.Count; i++)
                    {
                        searchResultModel.PartOfTextToPreView.Add(RemoveSpaceChar(objectBasedSearchResult.TextResult.PartOfText[i]));
                    }
                }

                customSearchResultModels.Add(searchResultModel);
            }
            return customSearchResultModels;
        }

        private ObservableCollection<SearchResultModel> ConvertSearchResultModWorkSpaceToSearchResultModel2(List<BaseSearchResult> workspaceWntity1)
        {
            ObservableCollection<SearchResultModel> customSearchResultModels = new ObservableCollection<SearchResultModel>();

            foreach (var item in workspaceWntity1)
            {
                var documentBasedSearchResult = (DocumentBasedSearchResult)item;

                SearchResultModel searchResultModel = new SearchResultModel();
                if (documentBasedSearchResult.FileName != null)
                {
                    searchResultModel.FileName = RemoveSpaceChar(documentBasedSearchResult.FileName);
                    searchResultModel.ResultInDocument = RemoveSpaceChar("The Result Of The Phrase"
                        + " \"" + modelSearch.KeyWordSearch + "\" " + "In Document " + searchResultModel.FileName);
                }
                searchResultModel.Type = documentBasedSearchResult.TypeURI;
                searchResultModel.Id = documentBasedSearchResult.ObjectId;
                searchResultModel.Image = new BitmapImage(OntologyIconProvider.GetTypeIconPath(documentBasedSearchResult.TypeURI));
                searchResultModel.KeyWordSearched = ModelSearch.KeyWordSearch;
                searchResultModel.ExactKeyWord = ModelSearch.ExactKeyWord;
                searchResultModel.From = ModelSearch.From;
                searchResultModel.To = modelSearch.To;

                if (documentBasedSearchResult.TextResult.PartOfText.Count > 0)
                {
                    searchResultModel.PartOfText.Add(RemoveSpaceChar(documentBasedSearchResult.TextResult.PartOfText[0]));

                    for (int i = 0; i < documentBasedSearchResult.TextResult.PartOfText.Count; i++)
                    {
                        searchResultModel.PartOfTextToPreView.Add(RemoveSpaceChar(documentBasedSearchResult.TextResult.PartOfText[i]));
                    }
                }

                customSearchResultModels.Add(searchResultModel);
            }
            return customSearchResultModels;
        }

        private string RemoveSpaceChar(string myText)
        {
            string resultString = "";
            resultString = Regex.Replace(myText, @"[\r\n]+", " ");
            resultString = resultString.Replace("\r", " ");
            resultString = resultString.Replace("\t", " ");
            return resultString.Trim();
        }
    }
}
