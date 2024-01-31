namespace GPAS.LanguageSpectrumViewer
{
    public class LanguageDetail
    {
        public string LanguageTitle { get; set; }
        public int Percentage { get; set; }

        public override string ToString()
        {
            return string.Format("{0} : {1}%"
                , string.IsNullOrWhiteSpace(LanguageTitle) ? "(Untitled)" : LanguageTitle
                , Percentage);
        }
    }
}
