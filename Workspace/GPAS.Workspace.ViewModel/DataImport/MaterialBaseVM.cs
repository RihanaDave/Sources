using GPAS.DataImport.Material.SemiStructured;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.ViewModel.DataImport
{
    public class MaterialBaseVM : INotifyPropertyChanged
    {
        public MaterialBaseVM() { }

        public MaterialBaseVM(bool isSelected, string title)
        {
            IsSelected = isSelected;
            Title = title;
        }
        private bool isSelected;
        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                if (this.isSelected != value)
                {
                    this.isSelected = value;
                    this.NotifyPropertyChanged("IsSelected");
                }
            }
        }

        public BitmapImage Icon { get; set; }

        private string title;
        public string Title
        {
            get { return this.title; }
            set
            {
                if (this.title != value)
                {
                    this.title = value;
                    this.NotifyPropertyChanged("Title");
                }
            }
        }                        

        public MaterialBase relatedMaterialBase { get; set; }
        public ItemToImportVM relatedItemToImport { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
}
