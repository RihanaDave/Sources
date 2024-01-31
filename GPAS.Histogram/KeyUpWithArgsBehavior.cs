using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Input;

namespace GPAS.Histogram
{
    public class KeyUpWithArgsBehavior : Behavior<UIElement>
    {
        public ICommand KeyUpCommand
        {
            get { return (ICommand)GetValue(KeyUpCommandProperty); }
            set { SetValue(KeyUpCommandProperty, value); }
        }

        public static readonly DependencyProperty KeyUpCommandProperty =
            DependencyProperty.Register("KeyUpCommand", typeof(ICommand), typeof(KeyUpWithArgsBehavior), new UIPropertyMetadata(null));


        protected override void OnAttached()
        {
            AssociatedObject.KeyUp += AssociatedObjectKeyUp;
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            AssociatedObject.KeyUp -= AssociatedObjectKeyUp;
            base.OnDetaching();
        }

        private void AssociatedObjectKeyUp(object sender, KeyEventArgs e)
        {
            KeyUpCommand?.Execute(e.Key);
        }
    }
}
