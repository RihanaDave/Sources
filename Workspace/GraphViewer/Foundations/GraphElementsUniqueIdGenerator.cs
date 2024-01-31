using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Graph.GraphViewer.Foundations
{
    // تولید کننده‌ی شناسه یکتای داخلی برای گره ها و یال ها
    public static class GraphElementsUniqueIdGenerator
    {
        /// <summary>
        /// شمارنده شناسه یکتای اختصاص داده نشده؛ این شمارنده با هر بار اختصاص شناسه یکتا، به اضافه یک می شود
        /// </summary>
        private static int _unassignedUniqueID = 0;
        /// <summary>
        /// شناسه ای یکتا -که تا زمان فراخوانی این عملگر برگردانده نشده است- را برمی گرداند
        /// </summary>
        /// <returns>A not before given ID</returns>
        internal static int GenerateUniqueID()
        {
            return _unassignedUniqueID++;
        }
    }
}
