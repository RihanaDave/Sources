using GPAS.Dispatch.Entities.Concepts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPAS.Workspace.Entities.Investigation.UnpublishedChanges
{
    public class CachedObjectMetadata
    {
        public KObject CachedObject;
        public bool IsLocallyResolved;
        public bool IsPublished;
        public bool IsNotUploadedSourceDocument;
        /// <summary>
        /// آرایه‌ی اشیائی که به صورت محلی با این شئ ادغام شده‌اند (این شئ میزبان ادغام
        /// آن‌هاست) و تغییرات این ادغام هنوز منتشر نشده است.
        /// 
        /// کاربرد این فیلد برای «توابع بازیابی» اجزای اشیاء است تا در صورت نیاز اجزای
        /// اشیاء محلی ادغام شده نیز بازیابی شوند
        /// </summary>
        public KObject[] ObjectsWhereLocallyResolvedTo;
    }
}
