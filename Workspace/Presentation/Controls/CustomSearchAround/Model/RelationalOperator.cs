using System.ComponentModel;

namespace GPAS.Workspace.Presentation.Controls.CustomSearchAround.Model
{
    public enum RelationalOperator
    {
        [Description("=")]
        Equals = 0,

        [Description(">")]
        GreaterThan = 1,

        [Description("<")]
        LessThan = 2,

        [Description(">=")]
        GreaterThanOrEquals = 3,

        [Description("<=")]
        LessThanOrEquals = 4,

        [Description("!=")]
        NotEquals = 5,
        Like = 6
    }
}
