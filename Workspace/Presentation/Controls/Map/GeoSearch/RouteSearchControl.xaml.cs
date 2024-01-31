using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace GPAS.Workspace.Presentation.Controls.Map.GeoSearch
{
    /// <summary>
    /// Interaction logic for RouteSearchControl.xaml
    /// </summary>
    public partial class RouteSearchControl : INotifyPropertyChanged
    {
        public event EventHandler<BufferChangedEventArgs> BufferChanged;
        protected virtual void OnBufferChanged(double buffer, Scales scale)
        {
            if (buffer < 0)
                throw new ArgumentNullException("buffer");            

            BufferChanged?.Invoke(this, new BufferChangedEventArgs(buffer, scale));
        }

        protected SolidColorBrush acceptBrush = new SolidColorBrush();
        protected SolidColorBrush rejectBrush = new SolidColorBrush();

        private int points;
        public int Points
        {
            get { return this.points; }
            set
            {
                if (this.points != value)
                {
                    this.points = value;
                    this.NotifyPropertyChanged("Points");
                }
            }
        }
        private double length;
        public double Length
        {
            get { return this.length; }
            set
            {
                if (this.length != value)
                {
                    this.length = value;
                    this.NotifyPropertyChanged("Length");
                }
            }
        }

        public Scales SelectedScale { get; set; }

        private string buffer;
        public string Buffer
        {
            get { return this.buffer; }
            set
            {
                if (this.buffer != value)
                {
                    this.buffer = value;
                    this.NotifyPropertyChanged("Buffer");
                }
            }
        }

        private bool canSearch;
        public bool CanSearch
        {
            get { return canSearch; }
            set
            {
                if (canSearch != value)
                {
                    canSearch = value;
                    NotifyPropertyChanged("CanSearch");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
            CheckSearchAbility();
        }

        private void CheckSearchAbility()
        {
            double bufferParsedValue;
            CanSearch = (Points > 1) && (Length > 0) && TryValidateBufferEnteredValue(out bufferParsedValue);
        }

        private bool TryValidateBufferEnteredValue(out double bufferParsedValue)
        {
            var lim = (SelectedScale == Scales.m) ? MapViewer.AdvanceRoute.CeillingRadius : MapViewer.AdvanceRoute.CeillingRadius / 1000;
            return (double.TryParse(Buffer, out bufferParsedValue) && bufferParsedValue > 0 && bufferParsedValue <= lim);
        }
        
        public List<ShowingScale> scales
        {
            get { return (List<ShowingScale>)GetValue(scalesProperty); }
            set { SetValue(scalesProperty, value); }
        }

        // Using a DependencyProperty as the backing store for scales.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty scalesProperty =
            DependencyProperty.Register(nameof(scales), typeof(List<ShowingScale>), typeof(RouteSearchControl),
                new PropertyMetadata(null));
        

        public List<string> GetValueScales { get; set; }
        public RouteSearchControl()
        {
            InitializeComponent();
            DataContext = this;
            CanSearch = false;
            InitBufferControls();
        }
        internal PaletteHelper palletHepler = new PaletteHelper();
        private void InitBufferControls()
        {
            GetValueScales = new List<string>();
            ITheme them=palletHepler.GetTheme();
            bufferTextBox.Background = Brushes.Transparent;
            acceptBrush = new SolidColorBrush(them.PrimaryMid.Color);
            rejectBrush = Brushes.OrangeRed;
            scales = GenerateScalesToShow();
            SelectedScale = Scales.m;
           
            if (scales.Count > 0)
            {
                scaleComboBox.SelectedIndex = 0;
            }
            this.Buffer = "300";
        }

        public event EventHandler<EventArgs> Canceled;
        protected void OnCanceled()
        {
            if (Canceled != null)
                Canceled(this, EventArgs.Empty);
        }
        
        private List<ShowingScale> GenerateScalesToShow()
        {
            List<ShowingScale> result = new List<ShowingScale>();
            string[] scalesList = Enum.GetNames(typeof(Scales));
            foreach (Scales currentScale in Enum.GetValues(typeof(Scales)))
            {
                result.Add(new ShowingScale()
                {
                    Scale = currentScale
                });
            }
            return result;
        }
        public event EventHandler<EventArgs> SearchRequested;
        protected void OnSearchRequested()
        {
            if (SearchRequested != null)
                SearchRequested(this, EventArgs.Empty);
        }
        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            OnSearchRequested();
        }

        private void scaleComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (scaleComboBox.SelectedIndex != -1)
            {
                SelectedScale = ((Scales)scaleComboBox.SelectedItem);
                RaiseBufferChangedEvent();
            }
        }

        private void TextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            RaiseBufferChangedEvent();
        }

        public double RaiseBufferChangedEvent()
        {
            double bufferParsedValue = 0.0;
            if (TryValidateBufferEnteredValue(out bufferParsedValue))
            {
                TextFieldAssist.SetUnderlineBrush(bufferTextBox, acceptBrush);
                CanSearch = true;
                if (scaleComboBox.SelectedItem == null)
                {
                    OnBufferChanged(bufferParsedValue, Scales.m);
                }
                else
                {
                   OnBufferChanged(bufferParsedValue, ((Scales)scaleComboBox.SelectedItem));
                }
                
            }
            else
            {
                TextFieldAssist.SetUnderlineBrush(bufferTextBox, rejectBrush);
                CanSearch = false;
            }
            return bufferParsedValue;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            OnCanceled();
        }

        public void Reset()
        {
            CanSearch = false;
            InitBufferControls();
        }
    }

    public enum Scales
    {
        [EnumMember]
        m = 0,
        [EnumMember]
        km = 1,
    }
    public class ShowingScale : INotifyPropertyChanged
    {
        private Scales scale;
        public Scales Scale
        {
            get { return this.scale; }
            set
            {
                if (this.scale != value)
                {
                    this.scale = value;
                    this.NotifyPropertyChanged("Scale");
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
