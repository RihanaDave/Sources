using GMap.NET;
using GMap.NET.WindowsPresentation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;

namespace GPAS.MapViewer
{
    public class SimpleMarker : GMap.NET.WindowsPresentation.GMapMarker// , INotifyPropertyChanged
    {
        public class MouseClickOnMarkerEventArgs
        {
            public MouseClickOnMarkerEventArgs(SimpleMarker selectedMarker)
            {
                if (selectedMarker == null)
                    throw new ArgumentNullException("selectedMarker");

                SelectedMarker = selectedMarker;
            }

            public SimpleMarker SelectedMarker
            {
                private set;
                get;
            }
        }
        
        public event EventHandler<MouseClickOnMarkerEventArgs> MouseClickOnMarker;
        
        protected virtual void OnMouseClickOnMarker(SimpleMarker selectedMarker)
        {
            if (selectedMarker == null)
                throw new ArgumentNullException("selectedMarker");
            
            if (MouseClickOnMarker != null)
                MouseClickOnMarker(this, new MouseClickOnMarkerEventArgs(selectedMarker));
        }
        public SimpleMarker(PointLatLng pos)
            :base(pos)
        {
            var s = new SimpleMarkerShape();
            s.Tag = this;
            //s.MouseDown += S_MouseDown;
            s.DataContext = this;
            Shape = s;                                                
        }

        //private void S_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        //{
        //    if (IsSelected)
        //    {
        //        DeselectMarker(this);
        //    }
        //    else
        //    {
        //        SelectMarker(this);
        //    }
        //    OnMouseClickOnMarker(this);
        //}

        private bool isSelected;
        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                if (this.isSelected != value)
                {
                    this.isSelected = value;
                    //this.NotifyPropertyChanged("IsSelected");
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        public static void SelectMarker(SimpleMarker simpleMarker)
        {
            simpleMarker.IsSelected = true;
            (simpleMarker.Shape as SimpleMarkerShape).IsHighlighted = true;
        }

        public static void DeselectMarker(SimpleMarker simpleMarker)
        {
            simpleMarker.IsSelected = false;
            (simpleMarker.Shape as SimpleMarkerShape).IsHighlighted = false;
        }
        //public event PropertyChangedEventHandler PropertyChanged;

        //public void NotifyPropertyChanged(string propName)
        //{
        //    if (this.PropertyChanged != null)
        //        this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        //}
        
        private double weight;
        public double Weight
        {
            get { return this.weight; }
            set
            {
                if (this.weight != value)
                {
                    this.weight = value;
                    //this.NotifyPropertyChanged("weight");
                    OnPropertyChanged("weight");
                }
            }
        }
    }
}
