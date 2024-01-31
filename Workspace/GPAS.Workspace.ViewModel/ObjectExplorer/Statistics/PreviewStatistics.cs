using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.ViewModel.ObjectExplorer.Statistics
{
    public class PreviewStatistics : DependencyObject
    {

        public BitmapImage Icon
        {
            get { return (BitmapImage)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(BitmapImage), typeof(PreviewStatistics), new PropertyMetadata(null));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(PreviewStatistics), new PropertyMetadata(""));

        public long ObjectsCount
        {
            get { return (long)GetValue(ObjectsCountProperty); }
            set { SetValue(ObjectsCountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ObjectsCount.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ObjectsCountProperty =
            DependencyProperty.Register("ObjectsCount", typeof(long), typeof(PreviewStatistics), new PropertyMetadata((long)0));

        public ObservableCollection<PreviewStatistic> Content
        {
            get { return (ObservableCollection<PreviewStatistic>)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Content.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register("Content", typeof(ObservableCollection<PreviewStatistic>), typeof(PreviewStatistics), new PropertyMetadata(null));
    }
}
