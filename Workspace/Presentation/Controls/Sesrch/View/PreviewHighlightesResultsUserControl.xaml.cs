using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace GPAS.Workspace.Presentation.Controls.Sesrch.View
{
    /// <summary>
    /// Interaction logic for PreviewHighlightesResultsUserControl.xaml
    /// </summary>
    public partial class PreviewHighlightesResultsUserControl : UserControl
    {
        public PreviewHighlightesResultsUserControl()
        {
            InitializeComponent();
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for KeyWord.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register(nameof(Title), typeof(string), typeof(PreviewHighlightesResultsUserControl), new PropertyMetadata(string.Empty, OnSetTitleChanged));
        private static void OnSetTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PreviewHighlightesResultsUserControl)d).OnSetBindableTitleChanged(e);
        }
        private void OnSetBindableTitleChanged(DependencyPropertyChangedEventArgs e)
        {
            FileName.Text = Title;
            DisplayPreview();
        }
        public string KeyWord
        {
            get { return (string)GetValue(KeyWordProperty); }
            set { SetValue(KeyWordProperty, value); }
        }

        // Using a DependencyProperty as the backing store for KeyWord.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty KeyWordProperty =
            DependencyProperty.Register(nameof(KeyWord), typeof(string), typeof(PreviewHighlightesResultsUserControl), new PropertyMetadata(string.Empty, OnSetKeyWordChanged));
        private static void OnSetKeyWordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PreviewHighlightesResultsUserControl)d).OnSetBindableKeyWordChanged(e);
        }
        private void OnSetBindableKeyWordChanged(DependencyPropertyChangedEventArgs e)
        {
            PrepareForShowResult();
            DisplayPreview();
        }



        public string ExactKeyWord
        {
            get { return (string)GetValue(ExactKeyWordProperty); }
            set { SetValue(ExactKeyWordProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ExactKeyWord.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ExactKeyWordProperty =
            DependencyProperty.Register(nameof(ExactKeyWord), typeof(string), typeof(PreviewHighlightesResultsUserControl), new PropertyMetadata(string.Empty, OnSetExactKeyWordChanged));
        private static void OnSetExactKeyWordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PreviewHighlightesResultsUserControl)d).OnSetBindableExactKeyWordChanged(e);
        }
        private void OnSetBindableExactKeyWordChanged(DependencyPropertyChangedEventArgs e)
        {
            PrepareForShowResult();
            //DisplayPreview();
        }



        public int MaxHighlites
        {
            get { return (int)GetValue(MaxHighlitesProperty); }
            set { SetValue(MaxHighlitesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ReturnCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty MaxHighlitesProperty =
            DependencyProperty.Register(nameof(MaxHighlites), typeof(int), typeof(PreviewHighlightesResultsUserControl), new PropertyMetadata(0, OnSetMaxHighlitesChanged));
        private static void OnSetMaxHighlitesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PreviewHighlightesResultsUserControl)d).OnSetBindableMaxHighlitesChanged(e);
        }
        private void OnSetBindableMaxHighlitesChanged(DependencyPropertyChangedEventArgs e)
        {
            PrepareForShowResult();
        }



        public ObservableCollection<string> Highlightes
        {
            get { return (ObservableCollection<string>)GetValue(HighlightesProperty); }
            set { SetValue(HighlightesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty HighlightesProperty =
            DependencyProperty.Register(nameof(Highlightes), typeof(ObservableCollection<string>), typeof(PreviewHighlightesResultsUserControl), new PropertyMetadata(null, OnSetHighliteChanged));

        private static void OnSetHighliteChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PreviewHighlightesResultsUserControl)d).OnSetBindableDocumentChanged(e);
        }

        private void OnSetBindableDocumentChanged(DependencyPropertyChangedEventArgs e)
        {
            PrepareForShowResult();
        }

        private void PrepareForShowResult()
        {
            rtx_highlightes.Document.Blocks.Clear();
            var blockes = CreateDocument();
            rtx_highlightes.Document.Blocks.AddRange(blockes);

        }

        private void DisplayPreview()
        {
            if (string.IsNullOrEmpty(Title) && string.IsNullOrEmpty(KeyWord))
            {
                //grd_Previvew.Visibility = Visibility.Collapsed;
                txb_Show.Visibility = Visibility.Visible;
            }
            else
            {
                //grd_Previvew.Visibility = Visibility.Visible;
                txb_Show.Visibility = Visibility.Collapsed;

            }
        }
        private IEnumerable<Paragraph> CreateDocument()
        {
            List<Paragraph> paragraphs = new List<Paragraph>();
            if (Highlightes != null)
            {
                //for (int i = 0; i < Highlightes.Count && i < MaxHighlites; i++)
                for (int i = 0; i < Highlightes.Count ; i++)
                {
                    paragraphs.Add(CreateParagraph(Highlightes[i]));
                }
            }
            return paragraphs;

        }

        private Paragraph CreateParagraph(string highlight)
        {
            Paragraph paragraph = new Paragraph();

            MatchCollection collection = Regex.Matches(highlight, "\\<b\\>(?<Part>.*?)\\<\\/b\\>");

            int curIndex = 0;
            foreach (Match match in collection)
            {
                var firstLine = highlight.Substring(curIndex, match.Index - curIndex);
                paragraph.Inlines.Add(new Run(firstLine.Replace("<b>", "").Replace("</b>", "")));
                //paragraph.Inlines.Add(new Label() { Content = firstLine.Replace("<b>", "").Replace("</b>", "") });

                curIndex = match.Index;

                var secondLine = highlight.Substring(curIndex, match.Length);
                paragraph.Inlines.Add(new Run(secondLine.Replace("<b>", "").Replace("</b>", ""))
                {
                    FontWeight = FontWeights.Bold
                });

                //paragraph.Inlines.Add(new Label() { Content = secondLine.Replace("<b>", "").Replace("</b>", ""), FontWeight = FontWeights.Bold});

                curIndex = curIndex + match.Length;
            }

            paragraph.Inlines.Add(new Run(highlight.Substring(curIndex, highlight.Length - curIndex)));
            //paragraph.Inlines.Add(new Label() { Content = highlight.Substring(curIndex, highlight.Length - curIndex) });
            curIndex = highlight.Length;

            return paragraph;



            //Paragraph paragraph = new Paragraph();

            //string pattern = CreatePattern();

            //Regex regex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.IgnoreCase);
            //MatchCollection matchs = regex.Matches(highlight);

            //int curIndex = 0;
            //foreach (Match match in matchs)
            //{
            //    paragraph.Inlines.Add(new Run(highlight.Substring(curIndex, match.Index - curIndex)));
            //    curIndex = match.Index;
            //    paragraph.Inlines.Add(new Run(highlight.Substring(curIndex, match.Length))
            //    {
            //        FontWeight = FontWeights.Bold
            //    });

            //    curIndex = curIndex + match.Length;
            //}

            //paragraph.Inlines.Add(new Run(highlight.Substring(curIndex, highlight.Length - curIndex)));
            //curIndex = highlight.Length;

            //return paragraph;
        }

        private string CreatePattern()
        {
            if (KeyWord.Contains("*"))
                return KeyWord + "([\\w]+)?";

            return @"\b" + KeyWord + @"\b";
        }
        //private IEnumerable<Paragraph> CreateDocument()
        //{
        //    List<Paragraph> paragraphs = new List<Paragraph>();
        //    if (Highlightes != null)
        //    {
        //        for (int i = 0; i < Highlightes.Count && i < MaxHighlites; i++)
        //        {
        //            paragraphs.Add(CreateParagraph(Highlightes[i]));
        //        }
        //    }
        //    return paragraphs;

        //}

        //private Paragraph CreateParagraph(string highlight)
        //{
        //    Paragraph paragraph = new Paragraph();

        //    var patterns = CreatePattern();
        //    foreach (var item in patterns)
        //    {
        //        Regex regex = new Regex(item, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        //        MatchCollection matchs = regex.Matches(highlight);


        //        int curIndex = 0;
        //        foreach (Match match in matchs)
        //        {
        //            paragraph.Inlines.Add(new Run(highlight.Substring(curIndex, match.Index - curIndex)));
        //            curIndex = match.Index;
        //            paragraph.Inlines.Add(new Run(highlight.Substring(curIndex, match.Length))
        //            {
        //                FontWeight = FontWeights.Bold
        //            });

        //            curIndex = curIndex + match.Length;
        //        }

        //        paragraph.Inlines.Add(new Run(highlight.Substring(curIndex, highlight.Length - curIndex)));
        //        curIndex = highlight.Length;
        //    }
        //    return paragraph;
        //}

        //private List<string> CreatePattern()
        //{
        //    List<string> ls = new List<string>();

        //    var words = new List<string>();
        //    var wrd = GetWords(KeyWord);
        //    foreach (var item in wrd)
        //    {
        //        if (!string.IsNullOrEmpty(item))
        //        {
        //            words.Add(item);
        //        }
        //    }
        //    if (!string.IsNullOrEmpty(KeyWord))
        //    {
        //        words.Add(KeyWord);
        //    }
        //    if (!string.IsNullOrEmpty(ExactKeyWord))
        //    {
        //        words.Add(ExactKeyWord);
        //    }
        //    foreach (var item in words)
        //    {
        //        if (item.Contains("*"))
        //            ls.Add("\\b" + item.Replace("*", "([\\w]*)"));

        //        ls.Add(@"\b" + item + @"\b");
        //    }
        //    return ls;
        //}
        //private List<string> GetWords(string input)
        //{
        //    MatchCollection matches = Regex.Matches(input, @"\b[\w']*\b");

        //    var words = from m in matches.Cast<Match>()
        //                where !string.IsNullOrEmpty(m.Value)
        //                select TrimSuffix(m.Value);

        //    return words.ToList();

        //}

        //static string TrimSuffix(string word)
        //{
        //    int apostropheLocation = word.IndexOf('\'');
        //    if (apostropheLocation != -1)
        //    {
        //        word = word.Substring(0, apostropheLocation);
        //    }

        //    return word;
        //}

    }
}
