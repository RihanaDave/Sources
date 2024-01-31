using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.ViewModel.DataImport
{
    public class ItemToImportVM : INotifyPropertyChanged
    {
        public ItemToImportVM()
        {
            ItemMaterials = new List<MaterialBaseVM>();
        }
        public long TotalRowsCount { get; set; }
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
        public string ItemPath { get; set; }
        public string ItemName { get; set; }
        // Unstructured file Fields
        public bool IsUnstructured { get; set; }
        public string DocumentTypeUri { get; set; }
        public MaterialType ItemType { get; set; }
        public List<MaterialBaseVM> ItemMaterials { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }

    public enum MaterialType
    {
        Csv,
        Excel,
        Eml,
        Access
    }
}
