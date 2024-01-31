using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch.Entities.Search
{
    [DataContract]
    public class SearchModel
    {
        [DataMember]
        public string TypeSearch { get; set; }
        [DataMember]
        public string ExactKeyWord { get; set; }
        [DataMember]
        public string KeyWordSearch { get; set; }
        [DataMember]
        //بین عبارت جست و جو Or میگذاریم
        public string AnyWord { get; set; }
        [DataMember]
        //جستوجو فاقد این عبارت باشد
        public string NoneWord { get; set; }
        [DataMember]
        //تاریخ import شی
        public DateTime? ImportDateOf { get; set; }
        [DataMember]
        public DateTime? ImportDateUntil { get; set; }
        //صعودی و نزولی سرچ
        [DataMember]
        public int SortOrder { get; set; }
        [DataMember]
        public string SortOrderType { get; set; }

        #region Just Doc
        [DataMember]
        public string Language { get; set; }
        //نوع سند مورد جست و جو را انتخاب میکنیم مانند PDF ,Doc ,...
        [DataMember]
        public string FileType { get; set; }

        //مشخص میکند در عنوان یا محتوا جست و حو انجام شود
        [DataMember]
        public string SearchIn { get; set; }
        [DataMember]
        public List<string> Topic { get; set; }

        //از فراداه ها تاریخ ساخت را بررسی میکنیم
        [DataMember]
        public DateTime? CreationDateOF { get; set; }

        [DataMember]
        public DateTime? CreationDateUntil { get; set; }

        //حجم فایل 
        [DataMember]
        public double FileSizeOF { get; set; }
        [DataMember]
        public double FileSizeUntil { get; set; }
        [DataMember]
        public long From { get; set; }
        [DataMember]
        public long To { get; set; }
        #endregion
    }
}
