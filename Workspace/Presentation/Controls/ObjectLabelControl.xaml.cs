using GPAS.Workspace.Entities;
using GPAS.Workspace.Logic;
using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.Presentation.Controls
{
    /// <summary>
    /// منطق تعامل با ObjectLabelControl.xaml
    /// </summary>
    public partial class ObjectLabelControl : INotifyPropertyChanged
    {
        #region مدیریت رخداد        
        public event EventHandler<EventArgs> ObjectChoosen;
        protected virtual void OnObjectChoosen()
        {
            if (ObjectChoosen != null)
                ObjectChoosen(this, EventArgs.Empty);
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        #endregion

        #region متغیرهای سراسری
        private BitmapImage icon;
        public BitmapImage Icon
        {
            get { return this.icon; }
            set
            {
                if (this.icon != value)
                {
                    this.icon = value;
                    this.NotifyPropertyChanged("Icon");
                }
            }
        }

        private string objectTitle;
        public string ObjectTitle
        {
            get { return this.objectTitle; }
            set
            {
                if (this.objectTitle != value)
                {
                    this.objectTitle = value;
                    this.NotifyPropertyChanged("ObjectTitle");
                }
            }
        }
        #endregion

        /// <summary>
        /// سازنده کلاس
        /// </summary>
        public ObjectLabelControl()
        {
            InitializeComponent();
            Init();
            IsEvenPlaced = true;
        }

        private void Init()
        {
            DataContext = this;
        }

        public void ShowObjectInformation(BitmapImage objectIcon, string objectTitle)
        {
            Icon = objectIcon;
            ObjectTitle = objectTitle;
        }

        /// <summary>
        /// این ویژگی نشان می دهد که آیا کنترل در جایگاه زوج قرار گرفته است یا فرد؛
        /// این ویژگی به صورت پیش فرض مقدار صحیح دارد تا در صورت عدم مقدار دهی، برچسب عادی نمایش داده شود
        /// </summary>
        public bool IsEvenPlaced
        {
            get;
            private set;
        }
        /// <summary>
        /// مقداردهی ویژگی نشاندهنده زوج یا فرد بودن جایگاه این کنترل در استفاده کننده
        /// </summary>
        /// <param name="isEvenPlaced"></param>
        public void SetEvenOrOddPlacement(bool isEvenPlaced)
        {
            IsEvenPlaced = isEvenPlaced;
            SetNormalAppearance();
        }
        /// <summary>
        /// شئی که این کنترل در حال نمایش آن است
        /// </summary>
        public new KWObject Content
        {
            get;
            private set;
        }
        /// <summary>
        /// مقداردهی شئ در حال نمایش توسط کنترل
        /// </summary>
        /// <param name="contentToSet"></param>
        public void SetContent(KWObject contentToSet)
        {
            if (contentToSet == null)
                throw new ArgumentNullException("contentToSet");

            Content = contentToSet;
            ShowObjectInformation(new BitmapImage(ObjectManager.GetIconPath(Content)), Content.GetObjectLabel());
        }

        //private static readonly Brush oddRowBackground = new SolidColorBrush(Colors.GhostWhite);
        //private static readonly Brush evenRowBackground = new SolidColorBrush(Colors.White);
        private static readonly Brush pointedRowBackground = new SolidColorBrush(Colors.Gray);
        private static readonly Brush selectedRowBackground = new SolidColorBrush(Colors.Gray);

        private void mainGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            SetPointedAppearance();
        }
        private void mainGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            SetNormalAppearance();
        }
        private void mainGrid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SetSelecetedAppearance();
        }
        private void mainGrid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            SetDeselectedAppearance();
        }

        private void SetNormalAppearance()
        {
            // mainGrid.Background = IsEvenPlaced ? evenRowBackground : oddRowBackground;
            mainGrid.Background = new SolidColorBrush(Colors.Transparent);
        }
        private void SetPointedAppearance()
        { mainGrid.Background = pointedRowBackground; }
        private bool pointedBeforeSelectAppearance = false;
        private void SetSelecetedAppearance()
        {
            pointedBeforeSelectAppearance = mainGrid.Background == pointedRowBackground;
            mainGrid.Background = selectedRowBackground;
        }
        private void SetDeselectedAppearance()
        {
            if (pointedBeforeSelectAppearance)
                SetPointedAppearance();
            else
                SetNormalAppearance();
        }

        private void coverLabel_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OnObjectChoosen();
        }
    }
}
