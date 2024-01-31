using GPAS.FilterSearch;
using System;

namespace GPAS.Workspace.Presentation.Controls.FilterSearchCriterias.EventArguments
{
    public class BooleanOperatorEventArgs : EventArgs
    {
        public BooleanOperatorEventArgs(BooleanOperator booleanOperator)
        {
            BooleanOperator = booleanOperator;
        }

        public BooleanOperator BooleanOperator
        {
            protected set;
            get;
        }
    }
}
