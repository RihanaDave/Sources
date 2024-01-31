using System.ComponentModel;

namespace GPAS.Workspace.Presentation
{
    /// <summary>
    /// نوع شمارشی کاربردهای تعریف شده در نرم‌افزار
    /// </summary>
    public enum PresentationApplications
    {
        [Description("Home")]
        Home,
        [Description("Browser Application")]
        Browser,
        [Description("Graph Application")]
        Graph,
        [Description("Map Application")]
        Map,
        [Description("Big-Data Search Application")]
        BigDataSearch,
        [Description("DataSource Application")]
        DataSource,
        [Description("Image Analysis Application")]
        ImageAnalysis,
        [Description("Object Explorer Application")]
        ObjectExplorer,
        [Description("Data Import Application")]
        DataImport,
        [Description("Data Publish Application")]
        DataPublish,
        [Description("Investigation")]
        Investigation,
        [Description("Settings")]
        Settings,
        [Description("Log Out")]
        LogOut
    }
}
