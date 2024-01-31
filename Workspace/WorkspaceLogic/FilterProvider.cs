using GPAS.FilterSearch;
using GPAS.PropertiesValidation;
using GPAS.Workspace.Entities;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace GPAS.Workspace.Logic
{
    /// <summary>
    /// منطق اعمال فیلتر بر مجموعه‌ای از اشیا محیط کاربری را اعمال می‌کند
    /// </summary>
    public class FilterProvider
    {
        /// <summary>
        /// زیرمجموعه‌ای از اشیا ورودی را براساس معیارهای فیلتر تعیین شده تفکیک کرده و برمی‌گرداند
        /// </summary>
        public static async Task<IEnumerable<KWObject>> ApplyFilterOnAsync(IEnumerable<KWObject> objectsToFilter, Query filterSearchQuery)
        {
            if (objectsToFilter == null)
                throw new ArgumentNullException("objectsToFilter");
            if (filterSearchQuery == null)
                throw new ArgumentNullException("filterSearchQuery");

            if (!objectsToFilter.Any() || filterSearchQuery.IsEmpty())
                return new List<KWObject>();

            return await DataAccessManager.Search.FilterSearch.ApplyFilterOnAsync(objectsToFilter,filterSearchQuery);
        }  
    }
}