using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.PieChartViewer
{
    public class PieChartItem : INotifyPropertyChanged
    {

        private String title;

        public String Title
        {
            get { return title; }
            set
            {
                title = value;
                RaisePropertyChangeEvent("Title");
            }
        }

        private double percent;

        public double Percent
        {
            get { return percent; }
            set
            {
                percent = value;
                RaisePropertyChangeEvent("Percent");
            }
        }      

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangeEvent(String propertyName)
        {
            if (PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));

        }

        #endregion
    }
}
