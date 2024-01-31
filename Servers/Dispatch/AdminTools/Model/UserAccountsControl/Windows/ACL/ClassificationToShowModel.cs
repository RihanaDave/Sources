using GPAS.AccessControl;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Dispatch.AdminTools.Model.UserAccountsControl.Windows.ACL
{
    public class ClassificationToShowModel : TreeViewItemBaseModel
    {
        //public ObservableCollection<ClassificationToShowModel> classifications { get; set; }
        //public ClassificationToShowModel()
        //{
        //    classifications = new ObservableCollection<ClassificationToShowModel>();
        //}
        private bool isLeafClassification;
        public bool IsLeafClassification
        {
            get => isLeafClassification;
            set
            {
                isLeafClassification = value;
                OnPropertyChanged();
            }
        }

        private bool isRootClassification;
        public bool IsRootClassification
        {
            get => isRootClassification;
            set
            {
                isRootClassification = value;
                OnPropertyChanged();
            }
        }

        private string title;
        public string Title
        {
            get => title;
            set
            {
                title = value;
                OnPropertyChanged();
            }
        }

        private Permission classPermission;
        public Permission ClassPermission
        {
            get => classPermission;
            set
            {
                classPermission = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<ClassificationToShowModel> Classifications { get; set; }
        public ClassificationToShowModel()
        {
            Classifications = new ObservableCollection<ClassificationToShowModel>();
        }

    }
}
