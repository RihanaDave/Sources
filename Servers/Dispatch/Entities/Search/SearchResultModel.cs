using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch.Entities.Search
{
    [DataContract]
    public class SearchResultModel
    {
        //نوع نتیجه جستوجو
        [DataMember]
        public string KeyWordSearched { get; set; }

        [DataMember]
        public string Type { get; set; }

        [DataMember]
        public string Image { get; set; }

        #region Just Doc
        //نام فایل
        [DataMember]
        public string FileName { get; set; }

        //بخشی از متن که کلیدواژه جست و جو در آن هست
        [DataMember]
        public List<string> PartOfText { get; set; }

        //تاریخ انتشار
        [DataMember]
        public DateTime PublishDate { get; set; }

        //حجم فایل
        [DataMember]
        public double FileSize { get; set; }

        //کلمات مرتبط
        [DataMember]
        public List<string> RelatedWord { get; set; }

        //تعداد کلمات پیدا شده در سند
        [DataMember]
        public int CountOfFind { get; set; }

        #endregion
    }
}
