using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Controls.Map.GeoSearch
{
    /// <summary>
    /// Interaction logic for CircleSearchControl.xaml
    /// </summary>
    public partial class CircleSearchControl : INotifyPropertyChanged
    {
        private double centerPointLat;
        public double CenterPointLat
        {
            get { return this.centerPointLat; }
            set
            {
                if (this.centerPointLat != value)
                {
                    this.centerPointLat = value;
                    this.NotifyPropertyChanged("CenterPointLat");
                }
            }
        }

        private double centerPointLng;
        public double CenterPointLng
        {
            get { return this.centerPointLng; }
            set
            {
                if (this.centerPointLng != value)
                {
                    this.centerPointLng = value;
                    this.NotifyPropertyChanged("CenterPointLng");
                }
            }
        }

        private double radious;
        public double Radious
        {
            get { return this.radious; }
            set
            {
                if (this.radious != value)
                {
                    this.radious = value;
                    this.NotifyPropertyChanged("Radious");
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
            CanSearch = (!double.IsNaN(CenterPointLat))
                && (!double.IsNaN(CenterPointLng))
                && (!double.IsNaN(Radious) && Radious > 0);
        }

        public CircleSearchControl()
        {
            InitializeComponent();
            DataContext = this;
            CanSearch = false;
            CenterPointLat = CenterPointLng = Radious = double.NaN;
        }
        public event EventHandler<EventArgs> Canceled;
        protected void OnCanceled()
        {
            if (Canceled != null)
                Canceled(this, EventArgs.Empty);
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

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            OnCanceled();
        }
    }
}
