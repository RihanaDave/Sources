using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.ViewModel.ObjectExplorer.Statistics
{
    public class PreviewStatistic : FrameworkElement
    {
        static PreviewStatistic()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(PreviewStatistic), new FrameworkPropertyMetadata(typeof(PreviewStatistic)));
        }

        public static readonly string ObjectTypesSuperCategoryTitle = "Object Types";

        public static readonly string PropertyTypesSuperCategoryTitle = "Property Type";

        public static readonly string LinkTypesSuperCategoryTitle = "Link Types";

        public static readonly string LinkedObjectTypesSuperCategoryTitle = "Linked Object Types";


        public static readonly RoutedEvent TitleChangedEvent =
          EventManager.RegisterRoutedEvent("TitleChanged", RoutingStrategy.Bubble,
          typeof(TextChangedEventHandler), typeof(PreviewStatistic));

        public event TextChangedEventHandler TitleChanged
        {
            add { AddHandler(TitleChangedEvent, value); }
            remove { RemoveHandler(TitleChangedEvent, value); }
        }

        public static readonly RoutedEvent SuperCategoryChangedEvent =
          EventManager.RegisterRoutedEvent("SuperCategoryChanged", RoutingStrategy.Bubble,
          typeof(TextChangedEventHandler), typeof(PreviewStatistic));

        public event TextChangedEventHandler SuperCategoryChanged
        {
            add { AddHandler(SuperCategoryChangedEvent, value); }
            remove { RemoveHandler(SuperCategoryChangedEvent, value); }
        }

        public BitmapImage Icon
        {
            get { return (BitmapImage)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(BitmapImage), typeof(PreviewStatistic), new PropertyMetadata(null));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Title.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(PreviewStatistic), new PropertyMetadata("", OnSetTitleChanged));

        private static void OnSetTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PreviewStatistic).OnSetTitleChanged(e);
        }

        private void OnSetTitleChanged(DependencyPropertyChangedEventArgs e)
        {
            OnTitleChanged();
        }

        public virtual void OnTitleChanged()
        {
            TextChangedEventArgs args = new TextChangedEventArgs(PreviewStatistic.TitleChangedEvent, UndoAction.Undo);
            RaiseEvent(args);
        }

        public string TypeURI { get; set; }

        public long Count
        {
            get { return (long)GetValue(CountProperty); }
            set { SetValue(CountProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Count.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CountProperty =
            DependencyProperty.Register("Count", typeof(long), typeof(PreviewStatistic), new PropertyMetadata((long)0));

        public string Category
        {
            get { return (string)GetValue(CategoryProperty); }
            set { SetValue(CategoryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Category.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CategoryProperty =
            DependencyProperty.Register("Category", typeof(string), typeof(PreviewStatistic), new PropertyMetadata(""));

        public string SuperCategory
        {
            get { return (string)GetValue(SuperCategoryProperty); }
            set { SetValue(SuperCategoryProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SuperCategory.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SuperCategoryProperty =
            DependencyProperty.Register("SuperCategory", typeof(string), typeof(PreviewStatistic), new PropertyMetadata("", OnSetSuperCategoryChanged));

        private static void OnSetSuperCategoryChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as PreviewStatistic).OnSetSuperCategoryChanged(e);
        }

        private void OnSetSuperCategoryChanged(DependencyPropertyChangedEventArgs e)
        {
            OnSuperCategoryChanged();
        }

        public void OnSuperCategoryChanged()
        {
            TextChangedEventArgs args = new TextChangedEventArgs(PreviewStatistic.SuperCategoryChangedEvent, UndoAction.Undo);
            RaiseEvent(args);
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsSelected.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(PreviewStatistic), new PropertyMetadata(false));
    }
}