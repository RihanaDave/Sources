using System.Windows;
using System.Windows.Controls;

namespace GPAS.Graph.GraphViewer.Foundations
{
    public class Image : UserControl
    {
        public ImageDetails Details
        {
            set
            {
                // از آنجایی که تنها استفاده کننده‌ی این ویژگی، شرط زیر را برای همه‌ی
                // گره‌ها بررسی می‌کند، برای جلوگیری از سربار پردازشی، این شرط غیرفعال
                // شده است
                //if ((ImageDetails)GetValue(DetailsProperty) == value)
                //    return;
                SetValue(DetailsProperty, value);
            }
        }
        public static readonly DependencyProperty DetailsProperty =
            DependencyProperty.Register(nameof(Details), typeof(ImageDetails), typeof(Image));
        public static ImageDetails GetDetails(DependencyObject obj)
        {
            return (ImageDetails)obj.GetValue(DetailsProperty);
        }
        public static void SetDetails(DependencyObject obj, object value)
        {
            obj.SetValue(DetailsProperty, value);
        }
    }
}
