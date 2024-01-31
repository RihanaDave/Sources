using GPAS.Workspace.ViewModel.ObjectExplorer.Formula;
using System.Windows;

namespace GPAS.Workspace.Presentation.Windows.ObjectExplorer
{
    /// <summary>
    /// Interaction logic for SetAlgebraWindow.xaml
    /// </summary>
    public partial class SetAlgebraWindow
    {
        public SetAlgebraWindow()
        {
            InitializeComponent();
            Init();
        }

        private void Init()
        {
            Result.IsDialogCanceled = true;
            Result.UserChoice = SetAlgebraOperator.Unknown;
        }

        SetAlgebraDialogResult Result = new SetAlgebraDialogResult();

        public static SetAlgebraDialogResult Show(Point position,DependencyObject owner)
        {
            var win = new SetAlgebraWindow()
            {
                Owner = Window.GetWindow(owner)
            };

            win.ShowDialog();
            return win.Result;
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            Result.IsDialogCanceled = true;
            Result.UserChoice = SetAlgebraOperator.Unknown;
            Close();
        }

        private void btnApplyFormula_Click(object sender, RoutedEventArgs e)
        {
            Result.IsDialogCanceled = false;
            Result.UserChoice = SetAlgebraOperator.Unknown;
            Close();
        }

        private void btnUnion_Click(object sender, RoutedEventArgs e)
        {
            Result.IsDialogCanceled = false;
            Result.UserChoice = SetAlgebraOperator.Union;
            Close();
        }

        private void btnIntersection_Click(object sender, RoutedEventArgs e)
        {
            Result.IsDialogCanceled = false;
            Result.UserChoice = SetAlgebraOperator.Intersection;
            Close();
        }

        private void btnSubtractRight_Click(object sender, RoutedEventArgs e)
        {
            Result.IsDialogCanceled = false;
            Result.UserChoice = SetAlgebraOperator.SubtractRight;
            Close();
        }

        private void btnSubtractLeft_Click(object sender, RoutedEventArgs e)
        {
            Result.IsDialogCanceled = false;
            Result.UserChoice = SetAlgebraOperator.SubtractLeft;
            Close();
        }

        private void btnExclusiveOr_Click(object sender, RoutedEventArgs e)
        {
            Result.IsDialogCanceled = false;
            Result.UserChoice = SetAlgebraOperator.ExclusiveOr;
            Close();
        }

        private void MainBorder_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            OnMouseLeftButtonDown(e);
            DragMove();
        }
    }
}
