using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.LanguageSpectrumViewer
{
    internal class ShowingLanguage : INotifyPropertyChanged
    {
        private string detectedLanguage;
        public string DetectedLanguage
        {
            get { return this.detectedLanguage; }
            set
            {
                if (this.detectedLanguage != value)
                {
                    this.detectedLanguage = value;
                    this.NotifyPropertyChanged(nameof(DetectedLanguage));
                }
            }
        }
        private int priority;
        public int Priority
        {
            get { return this.priority; }
            set
            {
                if (this.priority != value)
                {
                    this.priority = value;
                    this.NotifyPropertyChanged(nameof(Priority));
                }
            }
        }

        private int? score;
        public int? Score
        {
            get { return this.score; }
            set
            {
                if (this.score != value)
                {
                    this.score = value;
                    this.NotifyPropertyChanged(nameof(Score));
                }
            }
        }

        private string transparent;
        public string Transparent
        {
            get { return this.transparent; }
            set
            {
                if (this.transparent != value)
                {
                    this.transparent = value;
                    this.NotifyPropertyChanged(nameof(Transparent));
                }
            }
        }

        private string description;
        public string Description
        {
            get { return description; }
            set
            {
                if (description != value)
                {
                    description = value;
                    NotifyPropertyChanged(nameof(Description));
                }
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
