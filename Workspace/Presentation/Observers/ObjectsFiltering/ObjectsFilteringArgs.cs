using GPAS.FilterSearch;

namespace GPAS.Workspace.Presentation.Observers
{
    /// <summary>
    /// آرگومان پاس دادن شرایط اعمال فیلتر روی اشیا
    /// </summary>
    public class ObjectsFilteringArgs
    {
        /// <summary>
        /// فیلتری که فراخواننده می‌خواهد اعمال کند
        /// </summary>
        public Query FilterToApply = new Query();
    }
}