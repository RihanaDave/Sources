using GPAS.Workspace.Presentation.Controls.Sesrch.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Controls.Sesrch.Model
{
    public class SearchModel : BaseModel
    {
        private string typeSearch;
        //نوع جستجو را مشخص میکند مثل جستوجو در اسناد یا همه منابع 
        public string TypeSearch
        {
            get => typeSearch;
            set
            {
                SetValue(ref typeSearch, value);
            }
        }

        private string exactKeyWord;
        //کلید واژه جست و جو CaseSensetive
        //دقیقا همانند عبارت باید جستوجو صورت گیرد
        public string ExactKeyWord
        {
            get => exactKeyWord;
            set
            {
                SetValue(ref exactKeyWord, value);
            }
        }

        private string keyWordSearch= String.Empty;
        //کلید واژه جستوجو
        public string KeyWordSearch
        {
            get => keyWordSearch;
            set
            {
                SetValue(ref keyWordSearch, value);
            }
        }

        private string anyWord;
        //بین عبارت جست و جو Or میگذاریم
        public string AnyWord
        {
            get => anyWord;
            set
            {
                SetValue(ref anyWord, value);
            }
        }

        private string noneWord;
        //جستوجو فاقد این عبارت باشد
        public string NoneWord
        {
            get => noneWord;
            set
            {
                SetValue(ref noneWord, value);
            }
        }

        private DateTime? importDateOf;
        //تاریخ import شی
        public DateTime? ImportDateOf
        {
            get => importDateOf;
            set
            {
                SetValue(ref importDateOf, value);
            }
        }

        private DateTime? importDateUntil;
        public DateTime? ImportDateUntil
        {
            get => importDateUntil;
            set
            {
                SetValue(ref importDateUntil, value);
            }
        }

        private SortOrder sortOrder=SortOrder.SortAscending;
        //صعودی و نزولی سرچ
        public SortOrder SortOrder
        {
            get => sortOrder;
            set
            {
                SetValue(ref sortOrder, value);
            }
        }


        private string sortOrderType;
        //صعودی و نزولی سرچ
        public string SortOrderType
        {
            get => sortOrderType;
            set
            {
                SetValue(ref sortOrderType, value);
            }
        }

        #region Just Doc
        private string language;
        //زبان فایل مورد جست و جو
        // TODO: ااز حالت string خارج شود
        public string Language
        {
            get => language;
            set
            {
                SetValue(ref language, value);
            }
        }

        private string fileType;
        //نوع سند مورد جست و جو را انتخاب میکنیم مانند PDF ,Doc ,...
        //نوع سند مورد جست و جو را انتخاب میکنیم مانند PDF ,Doc ,...
        public string FileType
        {
            get => fileType;
            set
            {
                SetValue(ref fileType, value);
            }
        }


        private string searchIn;
        //مشخص میکند در عنوان یا محتوا جست و حو انجام شود
        public string SearchIn
        {
            get => searchIn;
            set
            {
                SetValue(ref searchIn, value);
            }
        }

        private List<string> topic;
        //موضوع
        public List<string> Topic
        {
            get => topic;
            set
            {
                SetValue(ref topic, value);
            }
        }

        private DateTime? creationDateOF;
        //از فراداه ها تاریخ ساخت را بررسی میکنیم
        public DateTime? CreationDateOF
        {
            get => creationDateOF;
            set
            {
                SetValue(ref creationDateOF, value);
            }
        }

        private DateTime? creationDateUntil;
        public DateTime? CreationDateUntil
        {
            get => creationDateUntil;
            set
            {
                SetValue(ref creationDateUntil, value);
            }
        }


        private double fileSizeOF;
        private double fileSizeUnti;
        //حجم فایل 
        public double FileSizeOF
        {
            get => fileSizeOF;
            set
            {
                SetValue(ref fileSizeOF, value);
            }
        }

        public double FileSizeUntil
        {
            get => fileSizeUnti;
            set
            {
                SetValue(ref fileSizeUnti, value);
            }
        }
        #endregion


        #region PropertyPagination

        private int itemPerPage = 10;
        public int ItemPerPage
        {
            get => itemPerPage;
            set
            {
                SetValue(ref itemPerPage, value);
            }
        }

        private int preSelectedIndexRow = 0;
        public int PreSelectedIndexRow
        {
            get => preSelectedIndexRow;
            set
            {
                SetValue(ref preSelectedIndexRow, value);
            }
        }

        private int currentselectedIndexRow = 0;
        public int CurrentselectedIndexRow
        {
            get => currentselectedIndexRow;
            set
            {
                if (value != -1)
                {
                    PreSelectedIndexRow = value;
                }
                SetValue(ref currentselectedIndexRow, value);
            }
        }


        private int pageNumber = 0;
        public int PageNumber
        {
            get => pageNumber;
            set
            {
                SetValue(ref pageNumber, value);
            }
        }
        #endregion

        private long from;
        public long From
        {
            get => from;
            set
            {
                SetValue(ref from, value);
            }
        }

        private long to;
        public long To
        {
            get => to;
            set
            {
                SetValue(ref to, value);
            }
        }

    }
}
