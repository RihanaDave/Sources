using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Entities.Search
{
    public class SearchResultModel
    {
        //نوع نتیجه جستوجو
        public string Type { get; set; }

        public string Image { get; set; }

        #region Just Doc
        //نام فایل
        public string FileName { get; set; }

        //بخشی از متن که کلیدواژه جست و جو در آن هست
        public List<string> PartOfText { get; set; }

        //تاریخ انتشار
        public DateTime PublishDate { get; set; }

        //حجم فایل
        public double FileSize { get; set; }

        //کلمات مرتبط
        public List<string> RelatedWord { get; set; }

        //تعداد کلمات پیدا شده در سند
        public int CountOfFind { get; set; }


        #endregion
    }
}
