using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace GPAS.LanguageSpectrumViewer
{
    /// <summary>
    /// Interaction logic for ShowLanguagesControl.xaml
    /// </summary>
    public partial class LanguageSpectrumViewer
    {
        ObservableCollection<ShowingLanguage> ShowingLanguages = new ObservableCollection<ShowingLanguage>();
        
        public static readonly DependencyProperty MinimumFontSizeProperty =
            DependencyProperty.Register("MinimumFontSize", typeof(double), typeof(LanguageSpectrumViewer));
        public double MinimumFontSize
        {
            get { return (double)GetValue(MinimumFontSizeProperty); }
            set { SetValue(MinimumFontSizeProperty, value); }
        }

        public static readonly DependencyProperty MaximumFontSizeProperty =
            DependencyProperty.Register("MaximumFontSize", typeof(double), typeof(LanguageSpectrumViewer));
        public double MaximumFontSize
        {
            get { return (double)GetValue(MaximumFontSizeProperty); }
            set { SetValue(MaximumFontSizeProperty, value); }
        }

        public LanguageSpectrumViewer()
        {
            InitializeComponent();
            DataContext = this;
            newLanguage.ItemsSource = ShowingLanguages;
        }
        public void ShowLanguages(List<LanguageDetail> languagesStatistics)
        {
            ShowingLanguages.Clear();
            IEnumerable<LanguageDetail> orderedLanguages = languagesStatistics.OrderBy(o => o.Percentage).Reverse();
            foreach (LanguageDetail item in languagesStatistics)
            {
                ShowingLanguage language = new ShowingLanguage()
                {
                    DetectedLanguage = string.IsNullOrWhiteSpace(item.LanguageTitle) ? "(Untitled)" : item.LanguageTitle,
                    Priority = ConvertRange(MinimumFontSize, MaximumFontSize, item.Percentage),
                    Score = item.Percentage,
                    Transparent = ConvertTransparent(item.Percentage),
                    Description = item.ToString()
                };
                ShowingLanguages.Add(language);
            }
            ShowLanguagePieChart(languagesStatistics);
        }

        private void ShowLanguagePieChart(List<LanguageDetail> languagesStatistics)
        {
            throw new NotImplementedException();
        }

        private static int ConvertRange(double newStart, double newEnd, int? value)
        {
            double scale = (newEnd - newStart) / (100 - 0);
            return (int)(newStart + ((value - 0) * scale));
        }

        private string ConvertTransparent(int? value)
        {
            double number = (double)value;
            double min = 0;
            double max = 100;

            if (number >= min && number <= max)
            {
                double range = (max - min) / 2;
                number -= max - range;
                double factor = 255 / range;
                double red = number < 0 ? number * factor : 255;
                double green = number > 0 ? (range - number) * factor : 255;
                double blue = number > 0 ? (range - number) * factor : 255;
                Color color = Color.FromRgb((byte)red, 0, 0);
                return color.ToString();
            }
            else
            {
                return "red";
            }
        }
    }
}
