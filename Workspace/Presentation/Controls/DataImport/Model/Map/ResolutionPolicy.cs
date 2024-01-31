using System.ComponentModel;

namespace GPAS.Workspace.Presentation.Controls.DataImport.Model.Map
{
    public enum ResolutionPolicy
    {
        /// <summary>
        /// درصورت وجود ویژگی های همنام حین ادغام دو شئ، مقدار ویژگی شئ جدید را نگه داشته و سایر ویژگی های همنام را حذف می‌کند.
        /// </summary>
        [Description("Set new")]
        SetNew,
        /// <summary>
        /// درصورت وجود ویژگی های همنام حین ادغام دو شئ، مقدار ویژگی شئ قدیممی را نگه داشته و سایر ویژگی های همنام را حذف می‌کند.
        /// </summary>
        [Description("Keep old")]
        KeepOld,
    }
}
