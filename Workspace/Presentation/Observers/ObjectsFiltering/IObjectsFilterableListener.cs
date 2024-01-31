using GPAS.FilterSearch;
using System.Threading.Tasks;

namespace GPAS.Workspace.Presentation.Observers
{
    /// <summary>
    /// واسط پیاده‌سازی گوش دهنده‌های دارای قابلیت اعمال فیلتر روی اشیا
    /// </summary>
    public interface IObjectsFilterableListener
    {
        Query CurrentFilter
        {
            get;
        }
        Task ApplyFilter(ObjectsFilteringArgs filterToApply);
    }
}