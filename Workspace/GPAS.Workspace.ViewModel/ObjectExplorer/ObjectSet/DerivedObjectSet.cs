using GPAS.Workspace.ViewModel.ObjectExplorer.Formula;
using GPAS.Workspace.ViewModel.ObjectExplorer.VisualizationPanel;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GPAS.Workspace.ViewModel.ObjectExplorer.ObjectSet
{
    public class DerivedObjectSet : ObjectSetBase
    {
        public DerivedObjectSet()
        {            
        }

        public ObjectSetBase ParentSet
        {
            get { return (ObjectSetBase)GetValue(ParentSetProperty); }
            set { SetValue(ParentSetProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ParentSet.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ParentSetProperty =
            DependencyProperty.Register("ParentSet", typeof(ObjectSetBase), typeof(DerivedObjectSet), new PropertyMetadata(null));

        public FormulaBase AppliedFormula
        {
            get { return (FormulaBase)GetValue(AppliedFormulaProperty); }
            set { SetValue(AppliedFormulaProperty, value); }
        }

        // Using a DependencyProperty as the backing store for AppliedFormula.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty AppliedFormulaProperty =
            DependencyProperty.Register("AppliedFormula", typeof(FormulaBase), typeof(DerivedObjectSet), new PropertyMetadata(null));        
    }
}
