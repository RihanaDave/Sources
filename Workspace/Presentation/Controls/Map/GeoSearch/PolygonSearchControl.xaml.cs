using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace GPAS.Workspace.Presentation.Controls.Map.GeoSearch
{
    /// <summary>
    /// Interaction logic for PolygonSearchControl.xaml
    /// </summary>
    public partial class PolygonSearchControl : INotifyPropertyChanged
    {
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
        private double perimeter;
        public double Perimeter
        {
            get { return this.perimeter; }
            set
            {
                if (this.perimeter != value)
                {
                    this.perimeter = value;
                    this.NotifyPropertyChanged("Perimeter");
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
        
        private bool isDrawnPolygonCrossed;
        public bool IsDrawnPolygonCrossed
        {
            get { return isDrawnPolygonCrossed; }
            set
            {
                if (isDrawnPolygonCrossed != value)
                {
                    isDrawnPolygonCrossed = value;
                    NotifyPropertyChanged("isDrawnPolygonCrossed");
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
            CanSearch = (Points > 2) && (Perimeter > 0) && (!IsDrawnPolygonCrossed);
        }

        public PolygonSearchControl()
        {
            InitializeComponent();
            DataContext = this;
            CanSearch = false;
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

        //public event EventHandler<System.Drawing.Point[]> SearchRequested;
        //protected void OnSearchRequested(System.Drawing.Point[] polygonPoints)
        //{
        //    if (SearchRequested != null)
        //        SearchRequested(this, polygonPoints);
        //}

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
