using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Controls.Browser
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
                RaisePropertyChangeEvent("Caption");
            }
        }



        public static List<PieChartItem> ConstructTestData()
        {
            List<PieChartItem> assetClasses = new List<PieChartItem>();

            assetClasses.Add(new PieChartItem() { Title = "Cash", Percent = 99.99 });
            assetClasses.Add(new PieChartItem() { Title = "Bonds", Percent = 0.01 });
            //assetClasses.Add(new AssetClass() { Class = "Real Estate", Benchmark = 10 });

            //assetClasses.Add(new AssetClass(){Class="Real Estate", Fund=13.24, Total=2.40, Benchmark=0.04});
            //assetClasses.Add(new AssetClass(){Class="Foreign Currency", Fund=16.44, Total=16.44, Benchmark=8.05});
            //assetClasses.Add(new AssetClass(){Class="Stocks; Domestic", Fund=27.57, Total=27.57, Benchmark=38.24});
            //assetClasses.Add(new AssetClass(){Class="Stocks; Foreign", Fund=50.03, Total=50.03, Benchmark=30.93});

            return assetClasses;
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
