using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Entities.Search
{
    public class SearchModel
    {
        public string TypeSearch{get;set;}

        public string ExactKeyWord { get; set; }

        public string KeyWordSearch { get; set; }

        //بین عبارت جست و جو Or میگذاریم
        public string AnyWord { get; set; }

        //جستوجو فاقد این عبارت باشد
        public string NoneWord { get; set; }

        //تاریخ import شی
        public DateTime? ImportDateOf { get; set; }

        public DateTime? ImportDateUntil { get; set; }
        //صعودی و نزولی سرچ
        public int SortOrder { get; set; }

        public string SortOrderType { get; set; }

        #region Just Doc
        public string Language { get; set; }
        //نوع سند مورد جست و جو را انتخاب میکنیم مانند PDF ,Doc ,...
        public string FileType { get; set; }

        //مشخص میکند در عنوان یا محتوا جست و حو انجام شود
        public string SearchIn { get; set; }

        public List<string> Topic { get; set; }

        //از فراداه ها تاریخ ساخت را بررسی میکنیم
        public DateTime? CreationDateOF { get; set; }

        public DateTime? CreationDateUntil { get; set; }

        //حجم فایل 
        public double FileSizeOF { get; set; }

        public double FileSizeUntil { get; set; }

        public long From { get; set; }
        public long To { get; set; }

        #endregion
    }
}
