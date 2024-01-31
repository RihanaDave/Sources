using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.SearchServer.Entities
{
    public class SearchModel
    {
        //نوع جستجو را مشخص میکند مثل جستوجو در اسناد یا همه منابع 
        public string TypeSearch { get; set; }

        //کلید واژه جست و جو CaseSensetive
        //دقیقا همانند عبارت باید جستوجو صورت گیرد
        public string ExactKeyWord { get; set; }

        //کلید واژه جستوجو
        public string KeyWordSearch { get; set; }

        //بین عبارت جست و جو Or میگذاریم
        public string AnyWord { get; set; }

        //جستوجو فاقد این عبارت باشد
        public string NoneWord { get; set; }

        //تاریخ import شی
        public DateTime? ImportDateOf { get; set; }

        public DateTime? ImportDateUntil{ get; set; }

        //صعودی و نزولی سرچ
        public int SortOrder { get; set; }

        //صعودی و نزولی سرچ
        public string SortOrderType { get; set; }

        #region Just Doc
        //زبان فایل مورد جست و جو
        // TODO: ااز حالت string خارج شود
        public string Language { get; set; }

        //نوع سند مورد جست و جو را انتخاب میکنیم مانند PDF ,Doc ,...
        //نوع سند مورد جست و جو را انتخاب میکنیم مانند PDF ,Doc ,...
        public string FileType { get; set; }

        //مشخص میکند در عنوان یا محتوا جست و حو انجام شود
        public string SearchIn { get; set; }

        //موضوع
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
