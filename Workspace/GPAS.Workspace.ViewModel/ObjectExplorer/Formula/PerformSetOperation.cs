using GPAS.Workspace.ViewModel.ObjectExplorer.ObjectSet;
using System.Windows;

namespace GPAS.Workspace.ViewModel.ObjectExplorer.Formula
{
    public class PerformSetOperation : FormulaBase
    {
        public ObjectSetBase JoinedSet
        {
            get { return (ObjectSetBase)GetValue(JoinedSetProperty); }
            set { SetValue(JoinedSetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for JoinedSet.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty JoinedSetProperty =
            DependencyProperty.Register("JoinedSet", typeof(ObjectSetBase), typeof(PerformSetOperation), new PropertyMetadata(null));



        public SetAlgebraOperator Operator
        {
            get { return (SetAlgebraOperator)GetValue(OperatorProperty); }
            set { SetValue(OperatorProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Operator.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty OperatorProperty =
            DependencyProperty.Register("Operator", typeof(SetAlgebraOperator), typeof(PerformSetOperation), new PropertyMetadata(SetAlgebraOperator.Unknown));
    }
}
