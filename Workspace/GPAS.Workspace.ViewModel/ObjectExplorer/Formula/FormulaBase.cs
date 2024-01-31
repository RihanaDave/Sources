using System;
using System.Windows;
using System.Windows.Media.Imaging;

namespace GPAS.Workspace.ViewModel.ObjectExplorer.Formula
{
    public abstract class FormulaBase : FrameworkElement
    {
        public BitmapImage Icon
        {
            get { return (BitmapImage)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(BitmapImage), typeof(FormulaBase), new PropertyMetadata(null));

        public long ObjectsCountDifference
        {
            get { return (long)GetValue(ObjectsCountDifferenceProperty); }
            set { SetValue(ObjectsCountDifferenceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ObjectsCountDifference.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ObjectsCountDifferenceProperty =
            DependencyProperty.Register("ObjectsCountDifference", typeof(long), typeof(FormulaBase), new PropertyMetadata((long)0));

        public bool IsInActiveSetSequence
        {
            get { return (bool)GetValue(IsInActiveSetSequenceProperty); }
            set { SetValue(IsInActiveSetSequenceProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsInActiveSetSequence.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsInActiveSetSequenceProperty =
            DependencyProperty.Register("IsInActiveSetSequence", typeof(bool), typeof(FormulaBase), new PropertyMetadata(false));
    }
}
