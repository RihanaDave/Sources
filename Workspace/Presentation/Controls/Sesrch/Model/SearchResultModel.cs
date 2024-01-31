using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.Controls.Sesrch.Model
{
    public class SearchResultModel : BaseModel
    {
        private long id;

        public long Id
        {
            get => id;
            set
            {
                SetValue(ref id, value);
            }
        }

        private string keyWordSearched;
        //نوع نتیجه جستوجو
        public string KeyWordSearched
        {
            get => keyWordSearched;
            set
            {
                SetValue(ref keyWordSearched, value);
            }
        }

        private string exactKeyWord;
        //نوع نتیجه جستوجو
        public string ExactKeyWord
        {
            get => exactKeyWord;
            set
            {
                SetValue(ref exactKeyWord, value);
            }
        }

        private string type;
        //نوع نتیجه جستوجو
        public string Type
        {
            get => type;
            set
            {
                SetValue(ref type, value);
            }
        }

        private ImageSource image;
        //عکسهای مربوط به هر نوع در Ontology
        public ImageSource Image
        {
            get => image;
            set
            {
                SetValue(ref image, value);
            }
        }

        #region Just Doc
        private string fileName;
        //نام فایل
        public string FileName
        {
            get => fileName;
            set
            {
                SetValue(ref fileName, value);
            }

        }

        private ObservableCollection<string> partOfText = new ObservableCollection<string>();
        //بخشی از متن که کلیدواژه جست و جو در آن هست
        public ObservableCollection<string> PartOfText
        {
            get => partOfText;
            set
            {
                SetValue(ref partOfText, value);
            }
        }

        private ObservableCollection<string> partOfTextToPreView = new ObservableCollection<string>();
        public ObservableCollection<string> PartOfTextToPreView
        {
            get => partOfTextToPreView;
            set
            {
                SetValue(ref partOfTextToPreView, value);
            }
        }

        private DateTime publishDate;
        //تاریخ انتشار
        public DateTime PublishDate
        {
            get => publishDate;
            set
            {
                SetValue(ref publishDate, value);
            }
        }

        private double fileSize;
        //حجم فایل
        public double FileSize
        {
            get => fileSize;
            set
            {
                SetValue(ref fileSize, value);
            }
        }

        private List<string> relatedWord;
        //کلمات مرتبط
        public List<string> RelatedWord
        {
            get => relatedWord;
            set
            {
                SetValue(ref relatedWord, value);
            }
        }

        private int countOfFind;
        //تعداد کلمات پیدا شده در سند
        public int CountOfFind
        {
            get => countOfFind;
            set
            {
                SetValue(ref countOfFind, value);
            }
        }

        private FlowDocument paragraph;
        //تعداد کلمات پیدا شده در سند
        public FlowDocument ParagraphToSearchResult
        {
            get => paragraph;
            set
            {
                SetValue(ref paragraph, value);
            }
        }

        private string resultInDocument;
        public string ResultInDocument
        {
            get => resultInDocument;
            set
            {
                SetValue(ref resultInDocument, value);
            }
        }
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

        #endregion
    }
}
