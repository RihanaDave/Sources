using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GPAS.BranchingHistoryViewer.ViewModel
{
    public class ConnectionLink : FrameworkElement
    {
        public BitmapImage Icon
        {
            get { return (BitmapImage)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(BitmapImage), typeof(ConnectionLink), new PropertyMetadata(null));

        public string Description
        {
            get { return (string)GetValue(DescriptionProperty); }
            set { SetValue(DescriptionProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Description.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DescriptionProperty =
            DependencyProperty.Register("Description", typeof(string), typeof(ConnectionLink), new PropertyMetadata(""));

        public ObjectBase From
        {
            get { return (ObjectBase)GetValue(FromProperty); }
            set { SetValue(FromProperty, value); }
        }

        // Using a DependencyProperty as the backing store for From.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty FromProperty =
            DependencyProperty.Register("From", typeof(ObjectBase), typeof(ConnectionLink), new PropertyMetadata(null, OnSetFromChanged));

        private static void OnSetFromChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var cl = d as ConnectionLink;
            cl.OnSetFromChanged(e);
        }

        private void OnSetFromChanged(DependencyPropertyChangedEventArgs e)
        {
            SetDirect();
        }

        public ObjectBase To
        {
            get { return (ObjectBase)GetValue(ToProperty); }
            set { SetValue(ToProperty, value); }
        }

        // Using a DependencyProperty as the backing store for To.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ToProperty =
            DependencyProperty.Register("To", typeof(ObjectBase), typeof(ConnectionLink), new PropertyMetadata(null, OnSetToChanged));

        private static void OnSetToChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var cl = d as ConnectionLink;
            cl.OnSetToChanged(e);
        }

        private void OnSetToChanged(DependencyPropertyChangedEventArgs e)
        {
            SetDirect();
        }

        public void SetDirect()
        {
            if (From != null && To != null && From != To)
            {
                IsDirect = From.VerticalDept == To.VerticalDept;
            }
            else
            {
                IsDirect = true;
            }
        }

        public bool IsDirect
        {
            get { return (bool)GetValue(IsDirectProperty); }
            set { SetValue(IsDirectProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsDirect.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsDirectProperty =
            DependencyProperty.Register("IsDirect", typeof(bool), typeof(ConnectionLink), new PropertyMetadata(true));
    }
}
