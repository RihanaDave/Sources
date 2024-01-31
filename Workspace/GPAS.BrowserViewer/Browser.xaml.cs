using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GPAS.BrowserViewer
{
    /// <summary>
    /// Interaction logic for Browser.xaml
    /// </summary>
    public partial class Browser : UserControl , INotifyPropertyChanged
    {
        
        public Browser()
        {
            InitializeComponent();
            DataContext = this;
        }
        public void ShowBrowserProperties(List<BrowserViewerProperties> propertiesToShow)
        {
            typeValueListview.ItemsSource = propertiesToShow;
        }
        public void ShowBrowserObjectInformation(BitmapImage objectIcon, string objectDisplayName, string objectTypeURI)
        {
            Icon = objectIcon;
            ObjectDisplayName = objectDisplayName;
            ObjectTypeURI = objectTypeURI;
        }
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
        private string objectDisplayName;
        public string ObjectDisplayName
        {
            get { return this.objectDisplayName; }
            set
            {
                if (this.objectDisplayName != value)
                {
                    this.objectDisplayName = value;
                    this.NotifyPropertyChanged("ObjectDisplayName");
                }
            }
        }
        private string objectTypeURI;
        public string ObjectTypeURI
        {
            get { return this.objectTypeURI; }
            set
            {
                if (this.objectTypeURI != value)
                {
                    this.objectTypeURI = value;
                    this.NotifyPropertyChanged("ObjectTypeURI");
                }
            }
        }
        public void ClearBrowserViewerObjectInformation()
        {
            Icon = new BitmapImage();
            ObjectDisplayName = "";
            ObjectTypeURI = "";
        }
        public void ClearBrowserProperties()
        {
            typeValueListview.ItemsSource = new List<BrowserViewerProperties>();
        }
        public event PropertyChangedEventHandler PropertyChanged;
        public void NotifyPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
    }
    public class BrowserViewerProperties : INotifyPropertyChanged
    {
        private string type;
        public string Type
        {
            get { return this.type; }
            set
            {
                if (this.type != value)
                {
                    this.type = value;
                    this.NotifyPropertyChanged("Type");
                }
            }
        }

        private string value;
        public string Value
        {
            get { return this.value; }
            set
            {
                if (this.value != value)
                {
                    this.value = value;
                    this.NotifyPropertyChanged("Value");
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
    // از کلاس زیر برای تغییر رنگ سطرهای زوج و فرد در نمایش لیستی استفاده می شود
    public class BackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            ListViewItem item = (ListViewItem)value;
            ListView listView =
                ItemsControl.ItemsControlFromItemContainer(item) as ListView;
            int index =
                listView.ItemContainerGenerator.IndexFromContainer(item);

            if (index % 2 == 0)
            {
                return Brushes.White;
            }
            else
            {
                return Brushes.WhiteSmoke;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }        
    }
    // از کلاس زیر برای مقداردهی طول یا عرض یک عضو از واسط کاربری به مقدار بخشی از 
    // عضو دیگر استفاده می شود
    public class PercentageConverter : MarkupExtension, IValueConverter
    {
        private static PercentageConverter _instance;

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToDouble(value,CultureInfo.InvariantCulture) *
                   System.Convert.ToDouble(parameter, CultureInfo.InvariantCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return _instance ?? (_instance = new PercentageConverter());
        }
    }
}
