using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch.AdminTools.Model
{
    /// <summary>
    /// مدل دیمایش و صفحه بندی
    /// </summary>
    public class PagingOptionModel : BaseModel
    {
        private int _pageIndex = 1;
        public int PageIndex
        {
            get => _pageIndex;
            set
            {
                _pageIndex = value;
                if (IsValidPageIndex())
                {
                    SetButtonsStatus();
                    SetCurrentPageNumber();
                    OnPropertyChanged();
                }
                else
                {
                    ValidatePageIndex();                   
                }
            }
        }

        private void ValidatePageIndex()
        {
            if (PageIndex < 1)
            {
                PageIndex = 1;
            }
            else if (PageIndex > LastPageNumber)
            {
                PageIndex = LastPageNumber;
            }
        }

        private bool IsValidPageIndex()
        {
            return (PageIndex >= 1 && PageIndex <= LastPageNumber);
        }

        private void SetButtonsStatus()
        {
            PreviousButtonEnabel = PageIndex > 1;
            NextButtonEnabel = PageIndex < _lastPageNumber;
        }

        private string _currentPageNumber;
        public string CurrentPageNumber
        {
            get => _currentPageNumber;
            set
            {
                _currentPageNumber = value;
                OnPropertyChanged();
            }
        }

        private int _pageSize = 5;
        public int PageSize
        {
            get => _pageSize;
            set
            {
                _oldPageSize = _pageSize;
                _pageSize = value;
                SetLastPageNumber();
                SetButtonsStatus();
                SetCurrentPageNumber();
                OnPropertyChanged();
            }
        }
        private int _oldPageSize ;
        public int OldPageSize
        {
            get => _oldPageSize;
        }

        private bool _nextButtonEnabel = true;
        public bool NextButtonEnabel
        {
            get => _nextButtonEnabel;
            set
            {
                _nextButtonEnabel = value;
                OnPropertyChanged();
            }
        }

        private bool _previousButtonEnabel = true;
        public bool PreviousButtonEnabel
        {
            get => _previousButtonEnabel;
            set
            {
                _previousButtonEnabel = value;
                OnPropertyChanged();
            }
        }

        private int _lastPageNumber = 1;
        public int LastPageNumber
        {
            get => _lastPageNumber;
            private set
            {
                _lastPageNumber = value;
                SetCurrentPageNumber();
                OnPropertyChanged();
            }
        }

        private int _allItemNumber = 0;
        public int AllItemNumber
        {
            get => _allItemNumber;
            set
            {
                _allItemNumber = value;
                SetLastPageNumber();
                SetButtonsStatus();
                OnPropertyChanged();
            }
        }

        private void SetCurrentPageNumber()
        {
            CurrentPageNumber = string.Format("{0} of {1}", PageIndex, LastPageNumber);
        }

        private void SetLastPageNumber()
        {
            if (AllItemNumber == 0)
            {
                _lastPageNumber = 1;
            }
            else
            {
                _lastPageNumber = (int)(Math.Floor((double)(AllItemNumber - 1) / PageSize)) + 1;
            }
        }
    }
}
